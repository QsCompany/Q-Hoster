using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using models;
using Server;
using Agent = models.Agent;
using Client = models.Client;
using Login = models.Login;
using Console = MyConsole;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.NetworkInformation;


//[module: UnverifiableCode]

namespace QServer.Core
{
    public delegate void CommandCallback(TaskQueue<CommandsParam> queue, CommandsParam param);
    public class CommandsParam
    {
        public CommandsParam(RequestArgs args, CommandCallback callback, object param)
        {
            Args = args;
            Parms = param;
            Callback = callback;
        }
        public RequestArgs Args;
        public object Parms;
        public CommandCallback Callback;
    }


    public partial class Server : CriticalFinalizerObject
    {
        private readonly Dictionary<string, Service> services = new Dictionary<string, Service>();
        public readonly Dictionary<string, Typeserializer> Serializers = new Dictionary<string, Typeserializer>();

        public static string[] CreateCerts()
        {
            return new[]{
                "makecert -n \"CN=localhost\" -r -sv localhost.pvk localhost.cer",
                "makecert -sk localhost -iv localhost.pvk -n \"CN=localhost\" -ic localhost.cer localhostSSL.cer -sr localmachine -ss My",
                "netsh http add sslcert ipport=0.0.0.0:5000 certhash=9d1c9c8b192aae38e48d27e0c726a7ad849428b7 appid={3c7cc984-be62-4f16-9a5f-878d54666fa9}"
            };

        }

        internal void BlockClient(RequestArgs args)
        {
            MyConsole.WriteLine($"User '{args.User.Client.FullName}' est trying to spy to you");
            if (args.User.IsAdmin)
            {
                args.SendSuccess();
                return;
            }
            var client = args.Client;
            var user = args.User;
            client.IsActive = false;
            user.IsBlocked = true;
            user.IsLogged = false;
            args.Database.Save(client, true);
            args.Send(new byte[0]);
        }

        public HttpListener Listener = new HttpListener();

        private readonly Dictionary<string, Resource> _files = new Dictionary<string, Resource>();

        public Service GetService(string name) => services.TryGetValue(name.ToLower(), out Service s) ? s : null;

        public void AddService(Service service) => services.Add(service.Name.ToLower(), service);

        private bool getEntryPoint(out EntryPoint entry)
        {
            if (_files.TryGetValue(Resource.EntryPoint?.ToLower() ?? "", out var file) || _files.TryGetValue("/index.html", out file))
            {
                entry = new EntryPoint(file);
                return true;
            }
            entry = null;
            return false;
        }

        private void InitializeWeb()
        {
            //getIp((ip) =>
            //{

            //    var user = "ammisaidkaidi";
            //    var pwd = "023942332as";
            //    var hostname = "qapp.ddns.net";
            //    var req = $"http://{user}:{pwd}@dynupdate.no-ip.com/nic/update?hostname={hostname}&myip={ip}";
            //    var userAgent = "QServer/v1.0 ammisaidkaidi.gmail.com ";
            //    var httpClient = new HttpClient();DObject
            //    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, req);
            //    request.Headers.Date = DateTime.Now.Subtract(new TimeSpan(10, 0, 0));
            //    request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.167 Safari/537.36");
            //    httpClient.SendAsync(request).ContinueWith((resp) =>
            //    {
            //        var r = resp.Result.StatusCode;
            //        if(r== HttpStatusCode.Unauthorized) {
            //            Console.WriteSeparator('#', "failed to change the ip address of site", "#");
            //        }else Console.WriteSeparator('\u2550', "failed to change the ip address of site", "\u2550");
            //    });
            //});

        }
        //HttpClient httpClient = new HttpClient();
        private void getIp(Action<string> calback)
        {
            //httpClient.GetStringAsync("https://api.ipify.org").ContinueWith((a) =>
            //{
            //    calback?.Invoke(a.Result);
            //});
        }
        public void ReloadResources()
        {
            Console.WriteLine("START Loading Manifest");
            var d = new DirectoryInfo(Resource.ResourcePath);
            var e = new ResourceLoader(d.FullName);
#if DEV
            if (e.Manifest.Exists) e.Manifest.Delete();
#endif
            _files.Clear();
            e.Load();

            foreach (var rs in e.resources)
                if (rs != null)
                {
                    if (rs.FileInfo.Extension == "") continue;
                    if (_files.ContainsKey(rs.RawUrl) == false)
                    {
                        _files.Add(rs.RawUrl, rs);
#if DEV
                        Console.WriteLine(rs.RawUrl);
#endif
                    }
                    else Console.WriteLine("Error code 404 : {0x" + rs.filePath.GetHashCode().ToString("x") + "}");
                }

            if (!_files.ContainsKey("/") && getEntryPoint(out var entry))
                _files.Add("/", entry);
            Console.WriteLine("END Loading Manifest");
        }
        private void Initailize()
        {
            InitializeWeb();
            
            HosteableObjectAttribute.LoadAssembly(System.Reflection.Assembly.GetExecutingAssembly(), this);
        }
        private void createDir(FileInfo output)
        {
            if (output.Exists) output.Delete();
            var dir = output.Directory;
            if (!dir.Exists)
                try
                {
                    dir.Create();
                }
                catch (Exception)
                {
                    Log.WriteLn("Error occurred when creating Directory " + dir.FullName);
                }
        }
        public void TransferDirPathTo(string dir, bool commpressed)
        {
            var d = new DirectoryInfo(Resource.ResourcePath);
            var e = new ResourceLoader(d.FullName);
            e.Load();
            foreach (var rs in e.resources)
            {
                if (rs == null) continue;
                if (rs.FileInfo.Name == "Manifest")
                {
                    continue;
                }
                try
                {

                    FileInfo output = new FileInfo(System.IO.Path.Combine(dir, commpressed ? rs.RelativeGZipFile : rs.RelativeFile));
                    createDir(output);
                    File.WriteAllBytes(output.FullName, rs.ReadZipBuffer());
                    rs.filePath = output.FullName;
                }
                catch (Exception ee)
                {
                    Log.WriteLn("Unexpected error :" + ee);
                }
            }
            e.SaveTo(dir);
        }
        private int connect(string[] addresses,bool first=true)
        {
            addresses = addresses != null && addresses.Length > 0 ? addresses : new[] { "https://+:443/", "http://+:80/" };
            int connected = 0;
            List<string> nconnected = new List<string>();
            foreach (var add in addresses)
            {
                try 
                {
                    Listener.Prefixes.Add(add);
                    Console.WriteLine("\t {0}", add);
                    connected++;
                }
                catch (Exception)
                {
                    nconnected.Add(add);
                }

            }
            if (nconnected.Count == 0)
                return connected;
            if (first)
            {
                var nsucc = 0;
                var computerName = Environment.GetEnvironmentVariable("COMPUTERNAME");
                var x = nconnected.ToArray();nconnected.Clear();
                foreach (var add in x)
                {
                    var p = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = $@"http add urlacl url={add} user={computerName}\Administrator listen=yes",
                            CreateNoWindow = false,
                            ErrorDialog = false,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = true,
                        }
                    };
                    p.Start();
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        nconnected.Add(add);
                    }
                    else nsucc++;
                }
                if (nconnected.Count > 0)
                {
                    return connect(nconnected.ToArray(), false);
                }
                System.Windows.Forms.MessageBox.Show("The server is not initialized please check your fireWall");
                return 0;
            }
            return 0;
        }

        private bool Initialize(string[] addresses)
        {
            addresses = addresses != null && addresses.Length > 0 ? addresses : new[] { "https://+:443/", "http://+:80/" };
            Initailize();
            try
            {
                Console.WriteLine("-----Connecting-----------------");
                Listener.Realm = "http";
                Listener.Start();
                Console.WriteLine("The Server Listen Into :");
                if (connect(addresses) != 0)
                    Listener.BeginGetContext(Callback, this);
                else
                    return false;

                Console.WriteLine("--------------------------------");
                Console.WriteLine("-----Loading Logings------------");
                authenticationManager.UpdateLogins(this.database);
                Console.WriteLine(new string('*', Console.BufferWidth - 1));
                Console.WriteLine("\r\n\r\n-----Finnish  !!!!--------------");
                Console.WriteLine(new string('*', Console.BufferWidth));

                this.ReloadResources();

            }
            catch (Exception eo)
            {
                Console.WriteLine("Error");
                Console.WriteLine(eo.Message);
                Thread.Sleep(000);
                return false;
            }
            
            Console.WriteLine("--------Connect Now-------------");
            return true;

        }

        public Context NewContext(bool isLocal, Database database) => new Context(isLocal, database, this);

        public Server()
        {
            authenticationManager = new AuthenticationManager(this);
            apiTaskQuee = new TaskQueue<HttpListenerContext>(this.ProcessQuee, OnProcessQueeError);
            paquetQueue = new TaskQueue<IAsyncResult>(this.PacketProcess, OnProcessPacketError);
            CommandsQueue = new TaskQueue<CommandsParam>(this.CommandProcesser, this.OnCommandProcesserError);
            InitializeCommands();
        }

        public void Stop(Action callback)
        {
            this._pause = true;
            this.paquetQueue.ContinueWith((pq) =>
            {
                apiTaskQuee.ContinueWith(aq =>
                {
                    CommandsQueue.ContinueWith(cq =>
                    {
                        Listener.Stop();
                        Console.WriteLine("Server Listener Stoped Successfully");
                        try { callback(); } catch { }
                        _pause = false;
                    });
                });
            });
        }

        public Database database;
        public bool Start(string[] addresses,Database database)
        {
            this.database = database;
            //Log.Console.ExecuteCommand += Console_ExecuteCommand;
            return Initialize(addresses);
        }
        private bool _pause;
      
        private void GetFile(RequestArgs args)
        {
            var res = _files[args.Url.ToLowerInvariant()];
            res.Reponse(args);
        }

        private void Api(RequestArgs args)
        {
            if (args.Service != null)
                args.Service.Exec(args);
        }

        public void Dispose(Action callback = null)
        {
            Console.WriteLine("Server Is Disposing");
            try
            {
                Listener.Stop();
                Listener.Close();
                authenticationManager.Dispose();
                _files.Clear();
                services.Clear();
                Serializers.Clear();
                paquetQueue.Dispose();
                CommandsQueue.Dispose();
                apiTaskQuee.Dispose();                
            }
            catch { Console.WriteLine("Server Is Disposing :UnExpected Error Occured"); }
            callback?.Invoke();
        }

        private void Callback(IAsyncResult ar)
        {

            var c = TaskQueue<IAsyncResult>.CurrentTask;
            if (ar.CompletedSynchronously)
            {
                if (apiTaskQuee.Count > 200)
                    OnTropCharche(ar);
                else if (_pause)
                    this.ServerIsPaused(ar);
                else
                    paquetQueue.Add(ar);
            }
            if (!_pause)
                Listener.BeginGetContext(Callback, this);
        }

        private void ServerIsPaused(IAsyncResult ar)
        {

            try
            {
                //var x = Listener.EndGetContext(ar);
                //x.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                //RequestArgs.SendAlertError(x, "Wait ...", "Le serveur est en Wait Stat");
            }
            catch (Exception e)
            {

            }
        }

        void OnTropCharche(IAsyncResult ar)
        {
            try
            {
                var x = Listener.EndGetContext(ar);
                RequestArgs.SendAlertError(x, "Wait ...", "Le serveur est trop charger");
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

        private bool OnRequest(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "OPTIONS")
            {
                AuthenticationManager.RespondOptions(context);
                return true;
            }
            //string addhttps = $"netsh http add sslcert ipport=0.0.0.0:443 certhash={certHash} appid={{{appid}}}";
            //https://www.duckdns.org/update?domains=qapp&token=88d5b589-b0e8-4e3f-9dcc-a1a8b226fd4c&ip=105.98.213.55&verbose=true

            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            //context.Response.AddHeader("Access-Control-Expose-Headers", "id");
            var user = authenticationManager.CheckAuth(context, out bool logged);
            if (user != null || logged)
            {
                var serviceArgs = RequestArgs.NewRequestArgs(context, this, user);

                if (serviceArgs.Service == null)
                {
                    serviceArgs.SendCode(HttpStatusCode.OK);
                }
                else if (serviceArgs.Service.CanbeDelayed(serviceArgs))
                {
                    CommandsQueue.Add(new CommandsParam(serviceArgs, ExecuteCommand, this));
                    return false;
                }
                else
                    using (serviceArgs)
                    {
                        Api(serviceArgs);
                        return !serviceArgs.IsBusy;
                    }
                
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            return true;
        }

        private static void ExecuteCommand(TaskQueue<CommandsParam> queue, CommandsParam param)
        {
            var x = (Server)param.Parms;
            x.Api(param.Args);
        }
        public Client Admin => authenticationManager.Admin;
        
        public static void Send(HttpListenerContext context, byte[] x)
        {
            var r = context.Response;
            r.AddHeader("content-length", x.Length.ToString());
            Stream s = r.OutputStream;
            s.Write(x, 0, x.Length);
            s.Close();
        }
        
        public void AddSerializer(Typeserializer serializer) => Serializers.Add(serializer.Name, serializer);
        public bool ValidateUser(RequestArgs args, Login e) => authenticationManager.ValidateUser(args, e);
        public bool DeleteUser(RequestArgs args, Login e) => authenticationManager.DeleteUser(args, e);
        public bool LockUser(RequestArgs args, Login e) => authenticationManager.LockUser(args, e);
        public bool SignupAgent(RequestArgs args, Agent c) => authenticationManager.SignupAgent(args, c);
        public bool Logout(Client client, RequestArgs args) => authenticationManager.Logout(client, args);

        private TaskQueue<HttpListenerContext> apiTaskQuee;
        private TaskQueue<IAsyncResult> paquetQueue;
        public TaskQueue<CommandsParam> CommandsQueue;
        public AuthenticationManager authenticationManager;
        public static byte[] True = Encoding.UTF8.GetBytes("true");
        public static byte[] False = Encoding.UTF8.GetBytes("false");
        public const string SGuidService = "{{\"__service__\":\"guid\",\"sdata\":{0}}}";
        internal readonly DateTime ExpiredTime = DateTime.Now + TimeSpan.FromDays(31);
        internal readonly DateTime StartTime=DateTime.Now;
    }
    partial class Server
    {
        static List<int> dd;
        
        class PacketStat
        {
            public HttpListenerContext context;
            public RequestArgs args;
            public PacketStat(HttpListenerContext context)
            {
                this.context = context;
            }
        }
        private void PacketProcess(Operation< IAsyncResult> value)
        {
            HttpListenerContext context = Listener.EndGetContext(value.Value);
            value.Stat = context;
            var raw = context.Request.RawUrl;

            if (context.Request.HttpMethod == "OPTIONS")
            {
                AuthenticationManager. RespondOptions(context);
                return;
            }
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Expose-Headers", "id");
            if (raw.Length > 2)
                if (raw[1] == '~')
                {
                    authenticationManager.PublicApi(context, raw);
                    goto fr;
                }
                else if (raw[1] == '_' && raw[2] == '/')
                {
                    apiTaskQuee.Add(context);
                    return;
                }
            if (_files.TryGetValue(context.Request.Url.LocalPath.ToLowerInvariant(), out var file))
            {
                value.Stat = new PacketStat(context);
                if (file.RequireAuth)
                {
                    using (((PacketStat)value.Stat).args = RequestArgs.NewRequestArgs(context, this, null))
                        file.Reponse(((PacketStat)value.Stat).args);
                }
                else file.Reponse(context);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            fr:
            context.Response.Close();
        }

        public void ProcessQuee(Operation<HttpListenerContext> sync)
        {
            HttpListenerContext context = sync.Value;
            if (OnRequest(context))
                context.Response?.Close();
        }
        private void OnProcessPacketError(Operation<IAsyncResult> c, Exception e)
        {

        }
        public void OnProcessQueeError(Operation< HttpListenerContext> sync, Exception e)
        {
            HttpListenerContext context = sync.Value;
            if (context != null)
            {
                //if (e.HResult != -2147467259)
                //    RequestArgs.SendAlertError(context, "Code Error : 0x" + e.HResult.ToString("x"), e.Message);
                context.Response.StatusCode =(int) HttpStatusCode.PreconditionFailed;
                context.Response.Close();
            }
        }
    }

    partial class Server
    {

        private void CommandProcesser(Operation<CommandsParam> value)
        {
            value.Value.Callback(CommandsQueue, value.Value);
        }

        private void OnCommandProcesserError(Operation<CommandsParam> value, Exception e)
        {
            value.Value.Args.context.Response.Close(False, false);
            value.Value.Args.Dispose();
            
            value.Value.Callback = null;
            value.Value.Parms = null;
        }
        public void AddToCriticalOperation(RequestArgs args, CommandCallback callback, object param) => CommandsQueue.Add(new CommandsParam(args, callback, param));
    }
    public partial class Server
    {
        void ResumeServer()
        {
            _pause = false;
            Listener.Start();
            Listener.BeginGetContext(Callback, this);
        }
        bool CanContinueExecutingOperation()
        {
            if (_pause)
                if (MessageBox.Show("L'Operation est en cours d'execution\r\n  voulez-continue execution l'operation", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2) == DialogResult.Yes) return true;
                else return false;
            return true;
        }
        public void InitializeCommands()
        {
            
            CommandsQuee.Default["exit"] = () =>
            {
                if (!CanContinueExecutingOperation()) return;
                Stop(() => Program.Exit(null, true, true));
            };
            CommandsQuee.Default["restart"] = () =>
            {
                if (!CanContinueExecutingOperation()) return;
                Stop(() => Restart(null));
            };
            CommandsQuee.Default["restore"] = () =>
            {
                if (!CanContinueExecutingOperation()) return;
                Stop(() => {
                    
                    fb.CheckFileExists = true;
                    if (fb.ShowDialog() == DialogResult.OK)
                    {
                        var mysql = database.DB as MySqlManager;
                        var iss = mysql.InternalRestore(new FileInfo(fb.FileName), out var exp);
                    }
                });
                ResumeServer();
            };
            CommandsQuee.Default["backup"] = () =>
            {
                if (!CanContinueExecutingOperation()) return;
                Stop(() => {
                    fb.CheckFileExists = false;
                    if (fb.ShowDialog() == DialogResult.OK)
                    {
                        var mysql = database.DB as MySqlManager;
                        var iss = mysql.InternalBackup(new FileInfo(fb.FileName), out var exp);
                    }
                });
                ResumeServer();
            };
            CommandsQuee.Default["repaire"] = () =>
            {
                if (!CanContinueExecutingOperation()) return;

                Stop(() => {
                    if (MessageBox.Show("Le database va se reparer .vous comfirmer ça .", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        return ;
                    var mysql = database.DB as MySqlManager;
                    Repaire(mysql, null);
                });
                ResumeServer();
            };
        }
        //private bool Console_ExecuteCommand(string cmd,Action callback)
        //{
        //    return false;
        //    if ("changeResourcePath|collectddls".Contains(cmd)) return false;
        //    _pause = true;
        //    var rslt = false;
        //    Stop(() =>
        //    {
        //        try
        //        {
        //            Console.WriteLine("Begin Executing Command");
        //            rslt = ExecuteCriticalCommand(cmd, database.DB as MySqlManager, callback);
        //            Console.WriteLine("La Command Executed");
        //        }
        //        catch
        //        {

        //        }
        //        finally
        //        {
        //            _pause = false;
        //        }
        //        if (cmd != "restart" && cmd != "exit")
        //        {
        //            _pause = false;
        //            Listener.Start();
        //            Listener.BeginGetContext(Callback, this);
        //        }
        //    });
        //    return rslt;
        //}
        OpenFileDialog fb = new OpenFileDialog() { DefaultExt = "", Multiselect = false, Title = "Selectioner un ficher de Resturation", CheckPathExists = true, RestoreDirectory = true, AutoUpgradeEnabled = true };
        private bool ExecuteCriticalCommand(string cmd, MySqlManager mysql,Action callback)
        {
            if (!string.IsNullOrWhiteSpace(Resource.BackupDir))
                fb.InitialDirectory = Resource.BackupDir;
            bool iss = false;
            Exception exp = null;
            switch (cmd)
            {
                case "restore":
                    fb.CheckFileExists = true;
                    if (fb.ShowDialog() == DialogResult.OK)
                        iss = mysql.InternalRestore(new FileInfo(fb.FileName), out exp);
                    break;
                case "backup":
                    fb.CheckFileExists = false;
                    if (fb.ShowDialog() == DialogResult.OK)
                        iss = mysql.InternalBackup(new FileInfo(fb.FileName), out exp);
                    break;
                case "repaire":
                    if (MessageBox.Show("Le database va se reparer .vous comfirmer ça .", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        return true;
                    Repaire(mysql, callback);
                    return true;
                case "restart":
                    Restart(callback);
                    return true;
                case "exit":
                    Program.Exit(callback, true, true);
                    return true;
                case "dossiers":
                    //var t = new SelectDatabase(database.DB.SQL);
                    //if (t.ShowDialog() == DialogResult.OK)
                    //{

                    //}
                    return true;
                default: return false;
            }

            if (string.IsNullOrWhiteSpace(Resource.BackupDir) && !string.IsNullOrWhiteSpace(fb.FileName) && new FileInfo(fb.FileName).Exists)
                Resource.BackupDir = new FileInfo(fb.FileName).Directory.FullName;
            if (iss)
                MessageBox.Show("L' Operation Réusie", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("L' Operation Echoué", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return true;
        }

        private void Repaire(MySqlManager mysql, Action callback)
        {
            var db = database;

            int err = 0;
            
            err += db.Versments.Repaire(database) + db.SVersments.Repaire(database) +
                db.Factures.Repaire(database) + db.SFactures.Repaire(database) +
                db.Clients.Repaire(database) + db.Fournisseurs.Repaire(database) +
                db.Logins.Repaire(database) + db.Agents.Repaire(database) + db.Products.Repaire(database);
            if (err == 0)
            {
                MessageBox.Show("All Errors are now corrected", "Filicitation", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                MessageBox.Show($"We Found ---- {err}s --- Error  not corrected \r\n Contacter QCompany for more information", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool Restart(Action action)
        {
            if (MessageBox.Show("Confirmation", "Vous vouler vous de Redemarer le server", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false
                };
                Program.Exit(() => { action?.Invoke(); }, true, true);
                using (Process process = Process.Start(psi))
                    process.Close();
                
            }
            return true;
        }
    }
}