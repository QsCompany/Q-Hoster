using System;
using System.Collections.Generic;
using System.Text;
using Json;
using models;
using QServer.Serialization;
using Server;

namespace Api
{
    public class  Client : Service
    {
        public static bool ThereAnyTel(Database d, string tel)
        {
            
            foreach (var m in d.Clients.AsList())
            {
                var t = m.Value as models.Client;
                if (t.Tel == tel) return true;
            }
            return false;
        }
        public static bool IPost(RequestArgs arg, models.Client per)
        {
            var isOld = false;
            if (per == null)
                return arg.SendError("costumer_argnull");
            var oper = arg.Database.Clients[per.Id];
            isOld = oper != null;
            if (!per.Check(out List<models.Client.Message> ss))
            {
                StringBuilder r = new StringBuilder();
                foreach (var m in ss)
                {
                    r.Append("<p><h4 style='color:Yellow'>")
                        .Append(m.Title)
                        .Append("</h4><h5 class='msg-body' style='padding-left:20px'>")
                        .Append(m.Body)
                        .Append("</h5></p>");
                }
                return arg.SendError(r.ToString(), false);
            }
            if (oper != null)
            {
                oper.Name = per.Name;
                oper.FirstName = per.FirstName;
                oper.LastName = per.LastName;
                oper.Tel = per.Tel;
                oper.Email = per.Email;
                oper.Job = per.Job;
                oper.Abonment = per.Abonment;
                oper.Picture = per.Picture;
                oper.WorkAt = per.WorkAt;
                oper.Picture = per.Picture;
            }
                

            if (!arg.Database.Save(oper ?? per, isOld)) return arg.SendError(QServer.Core.CodeError.fatal_error);
            if (oper == null)
                arg.Database.Clients.Add(per);
            if (!isOld) arg.Database.Clients.Add(per);
            return true;
        }

        public Client()
            : base("Client")
        {

        }
        public override bool Create(RequestArgs args)
        {
            var c = new models.Client()
            {
                Id = models.DataRow.NewGuid(),
                Abonment= Abonment.Detaillant,
                Name="MyName"
            };
            args.Send(c);
            return base.Create(args);
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
        public override bool Get(RequestArgs args)
        {
            if (args.GetParam("Id", out string _id) && _id == "undefined") { args.Send(args.Client); return true; }
            args.JContext.Add(typeof(models.Clients), new DObjectParameter(typeof(models.Clients)) { DIsFrozen = true });

            var id = args.Id;
            if (id == -1) return args.SendFail();
            var p = args.Database.Clients[id];
            if (p == null)
                return args.SendFail();
            else
                args.Send(p);
            return false;
        }
        public override bool Post(RequestArgs args)
        {
            
            args.JContext.RequireNew = (type, t) => type == "models.Client";
            var per = args.BodyAsJson as models.Client;
            if (!IPost(args, per)) return false;
            return args.SendSuccess();
        }

        public override bool Delete(RequestArgs args)
        {
            return args.SendError( "Lèoption est nest pas implementer");
            bool? l;
            if ((l = PromptPassword(args)) != null) return (bool)l;
            var id = args.Id;
            var d = args.Database;
            var client = d.Clients[id];
            if (client == null) return args.SendFail();
            foreach (var o in args.Database.Agents.AsList())
            {
                var a = o.Value as models.Agent;
                if (a.Client?.Id == client.Id) return args.SendError("Ce Client ne peut pas supprimer");
            }
            foreach (var o in args.Database.Logins.AsList())
            {
                var a = o.Value as models.Agent;
                if (a.Client?.Id == client.Id) return args.SendError("Ce Client ne peut pas supprimer");
            }
            client.IsActive = false;
            if (!args.Database.StrictSave(client, true)) return args.SendFail();
            return args.SendSuccess();

            if (args.Server.Admin.Id == id) return args.SendAlert("Fatal Error","this client cannot be deleted .","Je Comprend", false);

            if (id == -1) return false;
            {
                var lst = d.Factures.AsList();
                for (int i = lst.Length - 1; i >= 0; i--)
                {
                    var x = (models.Facture)lst[0].Value;
                    if (x.Client?.Id == id) return args.SendAlert("FireWall", $"This Client has factures {x.Ref}==&gt; then cannot be deleted ;", "Ok", false);
                }
            }
            {
                var lst = d.Versments.AsList();
                for (int i = lst.Length - 1; i >= 0; i--)
                {
                    var x = (models.Versment)lst[0].Value;
                    if (x.Client?.Id == id) {
                        args.SendAlert("FireWall", $"This Client has Versments {x?.Ref}==&gt; then cannot be deleted ;", "Ok", false); return false;
                    }

                }
            }
            
            if (client == null) return args.SendError("This Client Does'nt Exist", false);
            if (!d.Delete(client)) return args.SendError(QServer.Core.CodeError.DatabaseError);
            d.Clients.Remove(client);
            return args.SendSuccess();
        }
        private bool? PromptPassword(RequestArgs args)
        {
            var codeerror = false;
            bool exp;

            var m = args.Client.GetCookie("delete_client", true, out exp) as models.Message;
            deb:
            if (codeerror || m == null || exp)
            {
                object info = 0;
                if (codeerror)
                {
                    info = (args.Client.GetCookie("uncorrect_password_count", false) ?? 1);
                    if (info != null)
                    {
                        int a = (int)info;
                        if (a > 3) { return args.SendError("You cannot delete clients pandant 2 days"); }
                        info = a + 1;
                    }
                    args.Client.SetCookie("uncorrect_password_count", info, DateTime.Now + TimeSpan.FromDays(2));
                }
                if (m != null && m.Action == "close")
                    return args.SendFail();

                var data = new JObject { { "value", new JString("enter your password for delete this client") } };
                args.Client.SetCookie("delete_client",
                    m =
                    args.SendConfirm("Confirm --Expired Within 30 s " + (codeerror ? "<h4 style='color:red'>Password Uncorrect (" + info + ")</h4>" : exp ? " (Last Request Expired)" : ""), "Enter Your Password    :<input type='text' db-bind='value' db-job='input' db-check='username' db-twoway='3'></input>", "DELETE",
                            "CLOSE", true, data),
                    DateTime.Now + TimeSpan.FromSeconds(15));
                MessageSerializer.Register(m);
                return true;
            }
            if (m.Action != "ok")
                return args.SendFail();

            if (m.Data != null && (m.Data["value"] as JString) != args.User.Password)
            {
                codeerror = true;
                goto deb;
            }
            return null;
        }
    }

    public class Fournisseur : Service
    {
        public Fournisseur()
            : base("Fournisseur")
        {

        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
        public override bool Create(RequestArgs args)
        {
            var t = new models.Fournisseur()
            {
                Id = models.DataRow.NewGuid(),
                Name = "FournisseurName",
                Ref = Guid.NewGuid().ToString()
            };
            args.Send(t);
            return true;
        }
        public override bool Get(RequestArgs args)
        {
            var t = args.Database.Fournisseurs;
            var id = args.Id;
            if (id == -1) args.SendFail();
            var p = t[id];
            if (p == null)
                return args.SendFail();
            args.Send(p);
            return true;
        }
        public override bool Post(RequestArgs args)
        {

            args.JContext.RequireNew = (type, t) => type == "models.Fournisseur";
            models.Fournisseur per = args.BodyAsJson as models.Fournisseur, oper;
            if (per == null)
                return args.SendError("costumer_argnull");
            if (!per.Check(args, oper = args.Database.Fournisseurs[per.Id])) return false;
            if (oper != null)
            {
                //if (per.Avatar != null)
                //    if (!args.Database.Save(per.Avatar, oper.Avatar != null)) args.SendError("AvatarError");
                oper.Email = per.Email;
                oper.Tel = per.Tel;
                oper.Address = per.Address;
                oper.Name = per.Name;
                oper.Avatar = per.Avatar;

            }
            if (args.Database.Save(oper ?? per, oper != null))
                if (oper == null) args.Database.Fournisseurs.Add(per);
                else;
            else return args.SendFail();
            return args.SendSuccess();
        }

        public override bool Delete(RequestArgs args)
        {
         
            bool? l;
            if ((l = PromptPassword(args)) != null) return (bool)l;
            var id = args.Id;
            var d = args.Database;
            if (id == -1) return false;
            var frn = d.Fournisseurs[id];
            frn.IsActive = false;
            if (!args.Database.StrictSave(frn, true)) return args.SendFail();
            return args.SendSuccess();


            {
                var lst = d.SFactures.AsList();
                for (int i = lst.Length - 1; i >= 0; i--)
                {
                    var x = (models.SFacture)lst[0].Value;
                    if (x.Fournisseur?.Id == id) { args.SendAlert("FireWall", $"This Fournisseur has factures {x.Id}==&gt; then cannot be deleted ;", "Ok", false); return false; }
                }
            }
            {
                
                var lst = d.SVersments.AsList();
                for (int i = lst.Length - 1; i >= 0; i--)
                {
                    var x = (models.SVersment)lst[0].Value;
                    if (x.Fournisseur?.Id == id) { args.SendAlert("FireWall", $"This Fournisseur has factures {x.Id}==&gt; then cannot be deleted ;", "Ok", false); return false; }
                }
            }
            
            if (!d.Delete(frn)) return args.SendError(QServer.Core.CodeError.DatabaseError);
            d.Fournisseurs.Remove(frn);
            return args.SendSuccess();
        }
        private bool? PromptPassword(RequestArgs args)
        {
            var codeerror = false;
            bool exp;

            var m = args.Client.GetCookie("delete_fournisseur", true, out exp) as models.Message;
            deb:
            if (codeerror || m == null || exp)
            {
                object info = 0;
                if (codeerror)
                {
                    info = (args.Client.GetCookie("uncorrect_password_count", false) ?? 1);
                    if (info != null)
                    {
                        int a = (int)info;
                        if (a > 3) { return args.SendError("You cannot delete fournisseur pandant 2 days"); }
                        info = a + 1;
                    }
                    args.Client.SetCookie("uncorrect_password_count", info, DateTime.Now + TimeSpan.FromDays(2));
                }
                if (m != null && m.Action == "close")
                    return args.SendFail();

                var data = new JObject {
                    {"value" , (JString)"Achour Brahim" }
                };
                args.Client.SetCookie("delete_fournisseur",
                    m =
                    args.SendConfirm("Confirm --Expired Within 30 s " + (codeerror ? "<h4 style='color:red'>Password Uncorrect (" + info + ")</h4>" : exp ? " (Last Request Expired)" : ""), "Enter Your Password    :<input type='text' db-bind='value' db-job='input' db-check='username' db-twoway='3'></input>", "DELETE",
                            "CLOSE", true, data),
                    DateTime.Now + TimeSpan.FromSeconds(15));
                MessageSerializer.Register(m);
                return true;
            }
            if (m.Action != "ok")
                return args.SendFail();

            if (m.Data != null && (m.Data["value"] as JString) != args.User.Password)
            {
                codeerror = true;
                goto deb;
            }
            return null;
        }
    }
}