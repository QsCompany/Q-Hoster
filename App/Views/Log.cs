using QServer.Properties;
using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Drawing;
using Server;
using System.Collections.Generic;
using System.Diagnostics;

using static DEBUGGER;
static class DEBUGGER
{
    public static bool DisableCache = !false;
}

namespace QServer
{
    public partial class Log : Form
    {
        public MenuItem Dossiers;
        public static void SetDossier(string s)
        {
            try
            {
                Log.Console.Dossiers.Text = s;
            }
            catch { }
        }
        public event Func<string, Action, bool> ExecuteCommand;
        private NotifyIcon trayIcon;
        public Log()
        {
            InitTryIcon();
            try
            {
                InitializeComponent();
                Program.OnBeforeExit += ((a) => { _exit = true; a(); });
                Server.Program.OnExit += Application_ApplicationExit;
                Visible = false;
            }
            catch { }
        }

        private void InitTryIcon()
        {
            try
            {
                trayIcon = new NotifyIcon()
                {
                    Icon = Resources.Logo,
                    ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Dossiers",new []
                    {
                        Dossiers = new MenuItem("Dossiers",DossiersClicked)
                    }),
                    new MenuItem("Settings",new []
                    {
#if DEBUG
                        new MenuItem("Upgrade EasyLife",UpgEasyLifeClicked),
                        new MenuItem("Change Hosting Dir",ChangeHostDir),
#endif
                        new MenuItem("Change Shared Dir",ChangeSharedPath),
                    }),

                    new MenuItem(),
                    new MenuItem("Open", Show),
                    new MenuItem(),
                    new MenuItem("Repaire",Repaire),
                    new MenuItem("Create Backup", Backup),
                    new MenuItem("Restore Backup", Restore),
                    new MenuItem(),
                    new MenuItem("Exit", Exit),
                    new MenuItem("Restart", RestartClicked),
                    new MenuItem("Collect DLLS",CollectDlls)
#if DEBUG
                    ,
                    new MenuItem($"Cache Status is : {(DisableCache ? "Enabled" : "Disabled")}",CacheStatus)
#endif
                }),
                    Visible = true,Text="Q-Shop App"
                };
                trayIcon.Text = "QShop";
                Icon = Resources.Logo;
                trayIcon.BalloonTipText = "QShop Server";
                trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                trayIcon.BalloonTipTitle = "QsCompany";
                var Visible = true;
                trayIcon.Click += (s, e) =>
                {
                    if (Visible)
                    {
                        WindowState = FormWindowState.Minimized;
                        TopMost = false;
                        Visible = false;
                    }
                    else
                    {
                        Visible = true;
                        //TopMost = true;
                        WindowState = FormWindowState.Maximized;
                    }
                };
            }
            catch { }
        }
#if DEBUG
        private void CacheStatus(object sender, EventArgs e)
        {
            var s = (MenuItem)sender;
            DisableCache = !DisableCache;
            s.Text = $"Cache Status is : {(DisableCache ? "Enabled" : "Disabled")}";
        }
#endif
        private void Invoke(string cmd)
        {
            foreach (Func<string, Action,bool> cll in ExecuteCommand.GetInvocationList())
                try
                {
                    if (cll?.Invoke(cmd, Empty) == true) return;
                }
                catch
                {

                }
            
        }
        private void ChangeSharedPath(object sender, EventArgs e)
        {
            Invoke("changesharedpath");
        }
#if DEBUG
        private void ChangeHostDir(object sender, EventArgs e)
        {
            Invoke("changeResourcePath");
        }
#endif
        private void CollectDlls(object sender, EventArgs e)
        {
            Invoke("collectddls");
        }

        private void UpgEasyLifeClicked(object sender, EventArgs e)
        {
#if DEBUG
            Invoke("easylife_upgrade");            
#endif
        }

        private void Repaire(object sender, EventArgs e)
        {
            Invoke("repaire");
        }

        private void Empty() { }
        private void Restore(object sender, EventArgs e)
        {
            Invoke("restore");
        }

        private void DossiersClicked(object sender, EventArgs e)
        {
            Invoke("dossiers");
        }
        private void Backup(object sender, EventArgs e)
        {
            Invoke("backup");
        }

        private void RestartClicked(object sender, EventArgs e)
        {
            Restart();
        }

        internal void Restart()
        {
            Invoke("restart");
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Hide();
            BeginInvoke(new Action(() =>
            {
                IsInitailized = true;
                foreach (var a in actions)
                    AWrite(a.Item1, a.Item2);
                actions.Clear();
            }));
            
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            trayIcon.Visible = false;
            base.OnFormClosed(e);
        }
        private void Application_ApplicationExit()
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }

        public void Show(object sender, EventArgs e)  { Show(); }

        private bool _exit;
        private void Exit(object sender, EventArgs e)
        {
            ExecuteCommand?.Invoke("exit", Empty);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_exit)
            {
                //WindowState = FormWindowState.Minimized;
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_exit)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
        private bool IsInitailized = false;
        public static void Write(string s)
        {
            if (Console._exit) return;
            if (Console.IsInitailized)
                Console.textBox1.Invoke(new Action<string, bool>(Console.AWrite), s, false);
            else Console.actions.Add(new Tuple<string, bool>(s, false));
        }
        private List<Tuple<string, bool>> actions = new List<Tuple<string, bool>>();
        public static void WriteLn(string s)
        {
            if (Console._exit) return;
            if (Console.IsInitailized)
                Console.textBox1.Invoke(new Action<string, bool>(Console.AWrite), s, true);
            else Console.actions.Add(new Tuple<string, bool>(s, true));
        }
        private void AWrite(string x, bool newLine)
        {
            if (_exit) return;
            textBox1.Text += string.Concat(x?.ToString() ?? "", newLine ? Environment.NewLine : "");
        }
        private static Log _ilog;
        public static Log Console {[DebuggerNonUserCode] get => _ilog ?? (_ilog = new Log()); }
        private static Size size = Console.Size;
        static Log()
        {
            
            var _log = Console;
            _log.ShowInTaskbar = false;
            _log.Show();
            _log.Size = new Size();
            System.Threading.Timer r = null;
            r = new System.Threading.Timer((t) =>
            {
                _log.BeginInvoke(new Action<bool>((b) =>
                {
                    _log.Size = size;
                }), true);
                r.Dispose();
            }, null, 1000, Timeout.Infinite);
        }

        private void reloadResourceToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
    }
}
