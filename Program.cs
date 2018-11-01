using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Threading;
using models;
using Path = System.IO.Path;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Console = MyConsole;
namespace Server
{


    public partial class Program
    {
        public static string certhash = "52b3728de735ff137bfb878a8e47cf6b138a952a";
        public static string appid = "3c7cc984-be62-4f16-9a5f-878d54666fa9";

        internal static void _Main()
        {
            Main(null);
        }

        public static string Server = string.Format("netsh http add sslcert ipport=0.0.0.0:8443 certhash={0} appid={{{1}}}", certhash, appid);
        static byte[] key = new byte[32] { 234, 23, 196, 234, 69, 238, 92, 244, 50, 110, 70, 181, 109, 139, 252, 209, 146, 174, 40, 140, 129, 41, 58, 89, 102, 193, 99, 194, 178, 192, 239, 152 };

        private static Stack<Facture> factures = new Stack<Facture>();

        [STAThread]
        public static void Main(string[] args)
        {
            new QServer.UrlDownloader();
            try
            {
                SqlServerTypes.Utilities.LoadNativeAssemblies(new FileInfo(Application.ExecutablePath).Directory.FullName);
            }
            catch { }
            try
            {
                SqlServerTypes.Utilities1.LoadNativeAssemblies(new FileInfo(Application.ExecutablePath).Directory.FullName);
            }
            catch (Exception)
            {
            }
            Application.EnableVisualStyles();

            if (!CheeckInstance())
                return;
            processArgs(args);
            Resource.ResourcePath = getFullPath(Resource.ResourcePath);
            Console.WriteLine("Resource Path : " + Resource.ResourcePath);
            QServer.Core.Server s = new QServer.Core.Server();
            OnExit += () => s.Dispose(null);
            OnBeforeExit += (a) => { autoCallback(s.database, 3); a(); };
            QServer.Log.Console.ExecuteCommand += (e, c) =>
            {
                switch (e)
                {
                    case "collectddls":
                        {
                            var zee = QServer.PrgTest.CollectModules(Process.GetCurrentProcess());
                            return true;
                        }

                    case "changesharedpath":
                        {
                            using (var o = new FolderBrowserDialog { SelectedPath = Resource.SharedPath })
                            {
                                if (o.ShowDialog(QServer.Log.Console) == DialogResult.OK)
                                {
                                    var d = new DirectoryInfo(o.SelectedPath);
                                    if (!d.Exists) return true;
                                    Resource.SharedPath = d.FullName;
                                }
                            }
                            return true;
                        }

#if DEBUG
                    case "easylife_upgrade":
                        PME.Upgrade(s);
                        return true;
#endif
                    case "changeResourcePath":
                        {
                            using (var o = new FolderBrowserDialog { SelectedPath = Resource.ResourcePath })
                            {
                                if (o.ShowDialog(QServer.Log.Console) == DialogResult.OK)
                                {
                                    var d = new DirectoryInfo(o.SelectedPath);
                                    if (!d.Exists) return true;
                                    Resource.ResourcePath = d.FullName;
                                    s.ReloadResources();
                                }
                            }
                            return true;
                        }
                    default:
                        return false;
                }
            };
            if (UpdateDatabase(null, out var db) && s.Start(Resource.Addresses, db))
            {
                new Trans().initProducts();
                new SQL.SQLDatabase(db.DB.SQL);
            }
            else { Exit(null, true, true); return; }

            
            Application.Run();
            return;

        }
    
        public static Database StartServer()
        {
            QServer.Core.Server s = new QServer.Core.Server();
            OnBeforeExit += x1 => s.Stop(() => x1());
            OnExit += () => s.Dispose(null);

            OnBeforeExit += (a) => { autoCallback(s.database, 3); a(); };

            if (UpdateDatabase(null, out var db) && s.Start(Resource.Addresses, db))
            {
                new Trans().initProducts();
                Application.Run();
            }
            else Exit(null, true, true);
            return db;
        }

        static string[] split(string s)
        {
            var cls = new string[6];
            var pind = -1;
            for (int i = 0; i < cls.Length; i++)
            {
                if (pind + 1 == s.Length)
                {
                    cls[i] = "";
                    break;
                }
                if (s[pind + 1] == '"')
                {
                    var e = s.IndexOf("\",", pind + 1);
                    if (e == -1)
                        e = s.Length - 1;
                    cls[i] = s.Substring(pind + 2, e - pind - 2);
                    pind = e + 1;
                    continue;
                }
                var ind = s.IndexOf(',', pind + 1);
                if (ind == -1)
                {
                    cls[i] = s.Substring(pind + 1);
                    break;
                }
                cls[i] = s.Substring(pind + 1, ind - pind - 1);
                pind = ind;
            }
            return cls;
        }

        public static bool UpdateDatabase(System.Data.Common.DbConnection conn, out Database db)
        {
            try
            {
                db = Database.__Default;
                Console.WriteLine("-----Loading Database-----------");
                Resource.DatabasePath = "QShopDatabase";
                QServer.Log.SetDossier(Resource.DatabasePath);
                Database.__Default.Load();
                Console.WriteLine(new string('*', Console.BufferWidth - 1));
                new FakePrice();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                db = null;
                return false;
            }

        }
        public static readonly Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        private static bool CheeckInstance()
        {

            if (mutex.WaitOne(TimeSpan.FromSeconds(15), true))
            {
                OnExit += () => { Console.WriteLine("Mutex Is Closing"); mutex.Close(); mutex.Dispose(); };
                if (!QServer.Core.WindowsService.StartService())
                {
                    System.Windows.Forms.MessageBox.Show("The Service MySQL Cannot be started please goto MySQL and make \r\n\t\tStartup type :(Automatique)\r\n\t\t and press button Apply \r\n\t\tand press button start\r\nrestart program");
                    Exit(null, true, true);
                    return false;
                }
                return true;
            }
            else {
                System.Windows.Forms.MessageBox.Show("Another Instance Connected");
                return false;
            }

        }
        public static event Action<Action> OnBeforeExit;
        public static event Action OnExit;
        private static bool exiting;
        public static void Exit(Action callback, bool threads, bool forms)
        {
            if (exiting) if (MessageBox.Show("Le Programme est en cours de fermer. \r\nVous le vous attender?") == DialogResult.OK) return;
            exiting = true;
            var i = 0;
            if (OnBeforeExit != null)
            {
                var dlgts = OnBeforeExit.GetInvocationList();
                foreach (var dlg in dlgts)
                {
                    try
                    {
                        ((Action<Action>)dlg)(() =>
                          {
                              i++;
                              if (i == dlgts.Length)
                                  finalExit(callback, threads, forms);
                          });
                    }
                    catch { try { Console.WriteLine("U Expected Error Occurred When Executing exit-handler"); } catch { } }
                }
            }
            else finalExit(callback, threads, forms);
        }
        private static void finalExit(Action callback, bool threads, bool forms)
        {
            Application.Exit();
            if (OnExit != null)
                try
                {
                    foreach (var x in OnExit.GetInvocationList())
                    {
                        try
                        {
                            ((Action)x)();
                        }
                        catch { try { Console.WriteLine("U n expected Error whil finalExiting"); } catch { } }
                    }
                }
                catch { }
            OnExit = null;
            /*
            foreach (ProcessThread th in Process.GetCurrentProcess().Threads.OfType<ProcessThread>().ToArray())
                try { th.Dispose(); } catch { }*/            
            try
            {
                callback?.Invoke();
            }
            catch { }
        }
        private static FileInfo SettingsFile = new FileInfo("./Settings");
        private static string getFullPath(string arg, bool isfile=false)
        {
            string p;
            arg = arg.Trim('"', '\'');
            if (arg == "$")
                p = Path.GetFullPath(".\\") + "\\";
            else
                p = Path.GetFullPath(arg);
            if (!isfile)
                if (!p.EndsWith("\\")) return p + "\\";
            return p;
            
        }
        private static IPEndPoint Parse(string s,out string msg)
        {
            var x = s.Split(':');
            int port=0;
            IPAddress ip = IPAddress.Any;
            if (x.Length != 2)
            {
                msg = "{s} Not conform to the format {{IPADDRESS:PORT}} // ex: 127.0.0.1:5000 ";
                return null;
            }
            if(!int.TryParse(x[1],out port))
            {
                msg = $"{s} Not conform to the format {{IPADDRESS:PORT}} // ex: 127.0.0.1:5000 // Port must be Integer";
                return null;
            }
            if(!IPAddress.TryParse(x[0],out ip))
            {
                msg = $"{s} Not conform to the format {{IPADDRESS:PORT}} // ex: 127.0.0.1:5000 // IPADDRESS must be of type IPADDRESS";
                return null;
            }
            msg = null;
            return new IPEndPoint(ip, port);
        }
#if DEV
        private static void processArgs(string[] args)
        {
            bool reset = false;
            //Resource.ResourcePath = getFullPath(@"C:\QShop\");
            if (args != null)
                foreach (var a in args)
                {
                    var i = a.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
                    if (i == -1) continue;
                    var c = a.Substring(0, i);
                    var arg = a.Substring(i + 1);
                    arg = arg.Trim('\'', '"');
                    switch (c)
                    {
                        case "--path":
                            Resource.ResourcePath = getFullPath(arg);
                            continue;
                        case "--dpath":
                            Resource.DatabasePath = arg;
                            continue;
                        case "--host":
                            Resource.SetAddresses(arg);
                            continue;
                        case "--entrypoint":
                            Resource.EntryPoint = arg;
                            continue;
                        case "--reset":
                            reset = true;
                            continue;
                        default:
                            Console.WriteLine($"INVALID ARGUMENT {a}");
                            break;
                    }
                }
            if (string.IsNullOrWhiteSpace(Resource.DatabasePath))
                Resource.DatabasePath = "qshopdatabase";

            if (string.IsNullOrWhiteSpace(Resource.ResourcePath))
                Resource.ResourcePath = getFullPath(@".\Files\");

            if (!reset && Directory.Exists(Resource.ResourcePath))
                return;
            else
            {
                db:
                OpenFileDialog gh;
                FolderBrowserDialog dialog = new FolderBrowserDialog() { Description = "Selectioner le program a lancer", SelectedPath = Resource.ResourcePath, ShowNewFolderButton = false };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (Directory.Exists(dialog.SelectedPath))
                    {
                        if(!new FileInfo( Path.Combine( dialog.SelectedPath, "Manifest")).Exists)
                        {
                            MessageBox.Show("Le dossier ce n'est pas une application valide", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            goto db;
                        }
                        Resource.ResourcePath = getFullPath(dialog.SelectedPath);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Server Path Was not found use --path:'Server_PATH' for registration");
                    Exit();
                }
                goto db;
            }
        }
#else

        private static void processArgs(string[] args)
        {
            Resource.DatabasePath = "qshopdatabase";
            Resource.ResourcePath =getFullPath(/*@"F:\Test\QInfinite\bin\" ?? */@"F:\Test\e-shop\bin\") ?? getFullPath(@".\Files\");

            if (Directory.Exists(Resource.ResourcePath))
                return;
            else
            {
                MessageBox.Show("Le Program de Q-Shop n'est pas exist");
                Application.Exit(new System.ComponentModel.CancelEventArgs() { Cancel = false });
                throw new Exception() { };
            }
        }
#endif


        public static void Exit()
        {
            Console.ReadKey(true);
            Process.GetCurrentProcess().Close();
            Thread.CurrentThread.Abort();
        }


        private static void autoCallback(Database database,int hours=6)
        {
            var t = Resource.LastTimeBackup;
            if ((DateTime.Now - t).TotalHours > hours)
            {
                Console.WriteLine("Auto-Backup Starded");
                var fi = GetBackupDir();
                
                if (fi == null) { MessageBox.Show("L'Enregestrement Automatique Echouer d'obtenir le chemin de sauvegardae .", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                Console.WriteLine("\t" + "Backup-dir: " + fi.FullName);
                if ((database.DB as MySqlManager).InternalBackup(fi, out var e))
                    Resource.LastTimeBackup = DateTime.Now;
                Console.WriteLine("Auto-Backup Ended");
            }
        }
        private static FileInfo GetBackupDir()
        {
            var dir = Resource.BackupDir;
            var lst = new[] {
                dir,
                Path.Combine(Environment.CurrentDirectory, "Backups"),
                Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"),"Desktop", "Backups")
            };
            var extDir = Path.Combine(DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString());
            DirectoryInfo fp = null;
            foreach (var d in lst)
            {
                try
                {
                    fp = new DirectoryInfo(Path.Combine(d, extDir));
                    if (fp.Exists) break;
                    fp.Create();
                    break;
                }
                catch (Exception e)
                {
                    fp = null;
                }
            }
            if (fp == null) return null;
            FileInfo fi;
            do
            {
                fi = new FileInfo(Path.Combine(fp.FullName, DateTime.Now.ToLocalTime().ToString().Replace(':', '_').Replace('/', '_').Replace(' ', '_') + ".QBackup"));
            } while (fi.Exists);
            return fi;
        }
    }

    static class Debugger
    {
        private static bool isDebugging;
        public static void StartDebugging()
        {
            StopDebugging();
            isDebugging = true;
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        public static void StopDebugging()
        {
            isDebugging = false;
            Application.ThreadException -= Application_ThreadException;
            AppDomain.CurrentDomain.FirstChanceException -= CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }
        private static bool @internal;
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (@internal) return;
            @internal = true;

            try
            {
                Console.WriteSeparator();
                Console.WriteLine("UnHandled Exception");
                Console.WriteLine("Exception Object : " + e.ExceptionObject?.ToString());
                Console.WriteLine("Exception IsTerminate : " + e.IsTerminating.ToString());
                Console.WriteSeparator();
            }
            catch { }
            @internal = false;
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (@internal) return;
            @internal = true;

            try
            {
                Console.WriteSeparator("Handled Exception", '-');
                Console.Write(e.Exception);
                Console.WriteSeparator('-');
            }
            catch { }
            @internal = false;

        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (@internal) return;
            @internal = true;
            try
            {
                Console.Write(e.Exception);
            }
            catch { }
            @internal = false;
        }
    }
    class CommandsQuee
    {
        public static CommandsQuee Default { get; } = new CommandsQuee();
        private static Dictionary<string, Action> commands = new Dictionary<string, Action>();
        public Action this[string index]
        {
            get => commands.TryGetValue(index.ToLowerInvariant(), out var v) ? v : null;
            set => commands[index.ToLowerInvariant()] = value;
        }
        public bool Execute(string cmd, Action callback)
        {
            var c = this[cmd];
            if (c == null) return false;
            c();
            return true;
        }
        public CommandsQuee() => QServer.Log.Console.ExecuteCommand += Execute;

        public static void Register(string cmd, Action action) => Default[cmd] = action;
    }
}        