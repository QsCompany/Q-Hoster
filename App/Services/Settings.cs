using Json;
using QServer.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    [QServer.Core.HosteableObject(typeof(Settings))]
    class Settings : Service
    {
        public Settings() : base("Settings")
        {
        }
        public override bool CheckAccess(RequestArgs args)
        {
            var t = args.Client.GetCookie("Open", false, out var expired) as string;
            var t1 = args.Client.GetCookie("Open1", false, out var expired1) as string;
            if (t1 == null) return false;
            if (expired && !expired1)
                return args.User.IsAgent && t == args.User?.Agent?.Id.ToString();
            return false;
        }
        public override void Exec(RequestArgs args)
        {
            models.Client client = args.Client;
            string cookieName = client.Id.ToString();
            var messg = "backup_" + cookieName + "WV";
            switch (args.Method.ToUpper())
            {
                case "GET":
                    if (this.CheckAccess(args)) goto case "fail";
                    client.SetCookie("Open", args.User.Agent.Id.ToString(), DateTime.Now + TimeSpan.FromSeconds(5));
                    client.SetCookie("Open1", args.User.Agent.Id.ToString(), DateTime.Now + TimeSpan.FromSeconds(60));
                    break;
                case "POST":
                    var cc = client.GetCookie("Open1", false, out var experid);      
                    if (cc == null || experid) { args.SendError("Expired"); return; }
                    var c = client.GetCookie(cookieName, true, out bool expired);
                    if (c == null)
                    {
                        
                        var m = args.SendConfirm("Backup", "<section><div db-cmd='template' as='templates.backup'></div></section>", "Create", "Cancel", false, new JObject()
                        {
                            {"FileName",new JString(DateTime.Now.ToString("Backup dd_MM_yy_HH_mm")) },{"Password",(JString)"your password" },
                        });

                        client.SetCookie(messg, m, DateTime.Now + TimeSpan.FromSeconds(60));
                        client.SetCookie(cookieName, cookieName, DateTime.Now + TimeSpan.FromSeconds(60));
                        MessageSerializer.Register(m);
                        return;
                    }
                    else
                    {                        
                        var cookie = args.Client.GetCookie(messg, false, out expired) as models.Message;
                        if (cookie == null) { c = null; goto case "POST"; }
                        else if(expired) { c = null; goto case "POST"; }
                        if (cookie.Action == "ok")
                        {
                            var pwd = cookie.Data["Password"] as JString;
                            if (pwd == null || pwd != args.User.Password)
                                args.SendAlert("Password Error", "You are entred An Error Password");
                            else if (!args.Database.DB.CreateBackup((cookie.Data["FileName"] as JString)?.Value, out var e))
                                args.SendAlert("Backup Error", e.Message, "Close", false);
                            else args.SendAlert("Backup", "<section style='background:green'><h1>The Data wass success fully backed</h1></div>", "Close", true);
                        }
                        cookie.Dispose();
                    }
                    
                    break;
                case "PUT":
                    if (!this.CheckAccess(args)) goto case "fail";

                    break;
                case "fail":
                    args.SendFail();
                    return;
            }
        }

    }
}
