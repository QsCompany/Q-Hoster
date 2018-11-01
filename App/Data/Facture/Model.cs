using System;
using System.Collections.Generic;
using System.Text;
using Json;
using QServer.Core;
using Server;

namespace models
{

    [QServer.Core.HosteableObject(typeof(Api.Facture), typeof(FactureSerializer))]
    public partial class Facture : FactureBase
    {
        public new static int __LOAD__(int dp) => DpArticles;

        public static int DpClient = Register<Facture, Client>("Client", PropertyAttribute.AsId | PropertyAttribute.NonModifiableByHost, null, (d, p) => ((Database)d).GetClient(p));

        public Client Owner
        {
            get => get<Client>(DpClient);
            set => set(DpClient, value);
        }
        public Client Client { get => get<Client>(DpClient); set => set(DpClient, value); }
        public override bool IsAccessibleBy(User user, bool force, out string msg)
        {
            if (!user.IsAgent)
                if (Client != user.Client) { msg = "Vous n'a pas l'authorité d'access a cet facture"; return false; }

            return base.IsAccessibleBy(user, force, out msg);
        }

        public static bool IsAccessibleBy(RequestArgs args, bool forceOpen, out string msg, Facture f = null)
        {
            f = f ?? args.Database.Factures[args.Id];
            if (f == null)
            {
                msg = "Je Pense Que La facture est Supprimer";
                return false;
            }
            return f.IsAccessibleBy(args.User, forceOpen, out msg);
        }

        public static int DpVendeur = Register<Facture, Agent>("Vendeur", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetAgent(c)); public Agent Vendeur
        {
            get => get<Agent>(DpVendeur);
            set => set(DpVendeur, value);
        }

        public static int DpArticles = Register<Facture, Articles>("Articles", PropertyAttribute.Private, null, (d, c) => ((Database)d).GetArticles(c));
        public Articles Articles
        {
            get => get<Articles>(DpArticles);
            set => set(DpArticles, value);
        }

        public override Dealer Partner => get<Client>(DpClient);

        public static readonly int DPAbonment = Register<Facture, Abonment>("Abonment", PropertyAttribute.Private, null);
        public Abonment Abonment { get => get<Abonment>(DPAbonment); set => set(DPAbonment, value); }

        public readonly static int DPPour = Register<Facture, Projet>("Pour", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetProjet(c));
        public Projet Pour { get { return get<Projet>(DPPour); } set { set(DPPour, value); } }

        public Facture()
        {
            Articles = new Articles(this);
            //Versments = new Versments(this);
        }

        public Facture(Context c, JValue jv) : base(c, jv)
        {
            if (Articles == null) Articles = new Articles(this);
        }
        public bool Check(RequestArgs args, Facture ofact)
        {
            if (ofact.IsValidated) return args.SendError(CodeError.facture_isfrozen);
            var ow = Owner;
            if (!args.User.IsAgent)
                Owner = ow = args.Client;
            else if (ow == null)
                return args.SendError("The Client must be setted", false);

            var db = args.Database;
            var dbarts = db.Articles;
            var oarts = ofact.Articles;
            var dbprods = db.Products;
            var dpp = Price.GetDPPrice(ow.Abonment);

            var tt = 0f;
            if (Articles == null || Articles.Count == 0) return args.SendError(CodeError.facture_empty);
            var arts = Articles;
            foreach (KeyValuePair<long, DataRow> p in ofact.Articles.AsList())
            {
                var oldArt = p.Value as Article;
                var newArt = arts[oldArt.Id];
                if (newArt == null)
                {
                    if (!db.Delete(oldArt))
                        return args.SendError("Un Expected Error When deleting an article " + oldArt.Product);
                    dbarts.Remove(oldArt);
                    oarts.Remove(oldArt);
                }
            }

            var cm = new Dictionary<long, bool>();
            foreach (var pair in arts.AsList())
            {
                var newArt = pair.Value as Article;
                var oldArt = oarts[newArt.Id];
                if (cm.ContainsKey(newArt.Product.Id)) return args.SendError(CodeError.duplicated_article);
                if (!newArt.Check(args, dpp)) return false;
                newArt.Facture = ofact;
                var isOld = oldArt != null;
                if (!isOld)
                    arts.Add(newArt);
                else
                    oldArt.CopyFrom(newArt);
                if (!db.Save(isOld ? oldArt : newArt, isOld)) return args.SendError(CodeError.fatal_error);
                cm.Add(newArt.Product.Id, false);
                tt += newArt.Qte * newArt.Product.GetPrice(dpp);
            }
            NArticles = arts.Count;
            ofact.NArticles = oarts.Count;
            var v = 0f;
            //if (Versments != null)
            //{
            //    foreach (var vr in Versments.AsList())
            //        v += ((Versment)vr.Value).Montant;
            //}
            Total = tt;
            //Sold = tt - v;
            Date = DateTime.Now;
            return true;
        }
        public bool Check(RequestArgs args)
        {
            var ow = Owner;
            if (!args.User.IsAgent)
                Owner = args.Client;
            else if (Owner == null)
                return args.SendError("The Client must be setted", false);

            ow = Owner;
            var db = args.Database;
            var dbarts = db.Articles;
            var dbprods = db.Products;


            var dpp = Price.GetDPPrice(ow.Abonment);
            var tt = 0f;
            if (Articles == null) return args.SendError(CodeError.propably_hacking);

            var isadm = args.User.IsAgent;
            var cm = AppSetting.Default.AllowDuplicateArticles ? null : new Dictionary<long, bool>();
            foreach (var pair in Articles.AsList())
            {
                var a = pair.Value as Article;
                var prc = a.Product;
                if (prc == null) { Articles.Remove(a); continue; }
                if (!AppSetting.Default.AllowDuplicateArticles)
                {
                    if (cm.ContainsKey(a.Product.Id)) return args.SendError(CodeError.duplicated_article);
                    cm.Add(a.Product.Id, false);
                }
                a.Price = isadm ? a.Price : prc.DPrice;
                a.Facture = this;
                tt += a.Qte * (a.Price/*.GetPrice(dpp)*/);
            }

            Total = tt;
            NArticles = Articles.Count;
            if (Editeur == null) Editeur = args.User.Client;
            return true;
        }

        
        internal string GetUpdates()
        {
            var e = Articles.AsList();
            var b = new StringBuilder(e.Length * 25);
            b.Append("{\"__service__\":\"facture_count_updater\",\"dropRequest\":true,\"sdata\":{\"id\":").Append(Id.ToString()).Append(",\"articles\":{");

            var st = true;
            for (int i = e.Length - 1; i >= 0; i--)
            {
                var p = e[i].Value as Article;
                b.Append(st ? "\"" : ",\"").Append(p.Id.ToString()).Append("\":").Append(p.Qte);
                st = false;
            }
            b.Append("}}")
                .Append(",\"iss\":").Append("true}");
            return b.ToString();
        }

        public class UpdateRevage
        {
            public Stat Stat;
            public FakePrice Price;
            public float Count;
            public UpdateRevage(Stat stat, FakePrice p, float count)
            {
                Price = p;
                Count = count;
                Stat = stat;
            }
        }

        internal void GetDelete(RequestArgs args)
        {
            throw new NotImplementedException();
        }

        internal bool Delete(RequestArgs args)
        {
            if (args.Database.Delete(this))
            {
                if (IsValidable)
                {
                    var c = Client;
                    c.MontantTotal -= Total;
                    args.Database.StrictSave(c, true);
                }
                args.Database.Factures.Remove(this);
                return true;
            }
            return args.SendFail();
        }

        public override int Repaire(Database db)
        {
            var t = 0f;
            foreach (var l in Articles.AsList())
            {
                var a = (Article)l.Value;
                t += a.Total;
            }
            if (Total != t)
            {
                Total = t;
                return db.Save(this, true) ? 0 : 1;
            }
            return 0;

        }

        public override IBenifit CalcBenifit()
        {

            var c = this.Articles.AsList();
            float b = 0;
            float t = 0;
            for (int i = 0; i < c.Length; i++)
            {
                var x = (Article)c[i].Value;
                b += x.Qte * (x.Price - x.PSel);
                t += x.Qte * x.Price;
            }
            return new IBenifit() { Total = t, Benifit = b };
        }

        public static int DPFreezed = Register<Facture, bool>("Freezed");
        public bool Freezed { get => get<bool>(DPFreezed); set => set(DPFreezed, value); }
        
    }
}

namespace models
{
    public partial class Facture
    {
        public override void CopyFacture(FactureBase _newFacture)
        {
            var newFacture = (Facture)_newFacture;
            Client = newFacture.Client;
            Vendeur = newFacture.Vendeur;
            Editeur = newFacture.Editeur;
            Validator = newFacture.Validator;

            Total = newFacture.Total;
            NArticles = Articles.Count;
            Abonment = newFacture.Abonment;
            DateLivraison = newFacture.DateLivraison;
            Date = newFacture.Date;
            Observation = newFacture.Observation;
            Pour = newFacture.Pour;
            Ref = newFacture.Ref;
        }
        
        public static void TransferVersments(RequestArgs args, Facture orgfact, Facture newfact)
        {
            if (newfact == null || orgfact == null) return;
            Client newClient = newfact.Client;
            Client orgClient = orgfact.Client;

            if (newClient == orgClient) return;
            var d = args.Database;
            var versments = args.Database.Versments;
            var hasVers = orgfact.HasVersments(ref versments, out var ttl);

            newClient.MontantTotal += newfact.Total;
            orgClient.MontantTotal -= orgfact.Total;

            if (hasVers)
            {
                newClient.VersmentTotal += ttl;
                orgClient.VersmentTotal -= ttl;
                
            }
            d.Save(newClient, true);
            d.Save(orgClient, true);
            if (hasVers)
                foreach (KeyValuePair<long, DataRow> v in versments)
                {
                    var _v = (Versment)v.Value;
                    _v.Client = newClient;
                    d.Save(_v, true);
                }
        }
        private static bool ConfirmDelleting(RequestArgs args, Facture orgFacture)
        {
            Versments c = args.Database.Versments;
            var cookieName = "delete_facture_" + orgFacture.Id;
            var cvv = args.Client.GetCookie(cookieName + "confirmed", true, out var expired);
            var deletedConfirmed = Equals(true, cvv);

            if (deletedConfirmed && orgFacture.GetVersments(ref c)) 
            {
                return ConfirmDelletingWVersm(args, orgFacture, c);
            }
            
            var cookie = args.Client.GetCookie(cookieName, true, out  expired) as Message;
            JObject @ref;
            if (cookie == null) goto reconf;
            else
            {
                if (expired) return args.SendError("The Confirmation must donnot take more than 15s");
                else
                {

                    if (cookie.Action != "ok") return args.SendFail();
                    @ref = cookie.Data as JObject;
                    if (@ref == null || (@ref["Id"] as JNumber)?.ToInt64(null) != orgFacture.Id || (@ref["Ref"] as JString) != orgFacture.Ref) return args.SendFail();
                    args.Client.SetCookie(cookieName + "confirmed", true, DateTime.Now + TimeSpan.FromSeconds(15));
                    return orgFacture.GetVersments(ref c) ? ConfirmDelletingWVersm(args, orgFacture, c) : true;
                }
            }
            reconf:
            @ref = new JObject() { { "Ref", (JString)orgFacture.Ref }, { "Id", (JNumber)orgFacture.Id } };
            var m = args.SendConfirm("Confirm", 
                "<section><h1>You are about to deleting the facture <span db-bind='Ref' db-job='label' style='color:red'>Reference</span></h1>" +
                "<br><h3>Do you want realy to delete this facture</h3></section>", "Delete", "Cancel", false, @ref);
            QServer.Serialization.MessageSerializer.Register(m);

           

            args.Client.SetCookie(cookieName, m, DateTime.Now + TimeSpan.FromSeconds(15));
            return false;
        }

        private static bool ConfirmDelletingWVersm(RequestArgs args, Facture orgFacture, Versments c)
        {
            var cookieName = "delete_facture_" + orgFacture.Id + "WV";
            var cookie = args.Client.GetCookie(cookieName, true, out var expired) as Message;
            if (!expired)
                if (cookie != null)
                {
                    if (cookie.Action == "ok")
                    {
                        foreach (KeyValuePair<long, DataRow> kv in c)
                        {
                            var v = (Versment)kv.Value;
                            v.Facture = null;
                            if (!args.Database.Save(v, true))
                            {
                                foreach (KeyValuePair<long, DataRow> _kv in c)
                                {
                                    v = (Versment)_kv.Value;
                                    v.Facture = orgFacture;
                                    args.Database.Save(v, true);
                                }
                                return args.SendError(CodeError.fatal_error);
                            }
                        }
                        return true;
                    }
                    else
                    {

                        foreach (KeyValuePair<long, DataRow> kv in c)
                        {
                            var v = (Versment)kv.Value;
                            if (!args.Database.Delete(v))
                            {
                                foreach (KeyValuePair<long, DataRow> _kv in c)
                                {
                                    v = (Versment)_kv.Value;
                                    args.Database.Save(v, false);
                                    args.Database.Versments.Add(v);
                                }
                                return args.SendError(CodeError.fatal_error);
                            }
                            args.Database.Versments.Remove(kv.Key);
                        }
                        return true;
                    }
                }
                else goto reconf;
            else
                if (cookie != null) return args.SendError("The Confirmation must donnot take more than 15s");

            reconf:
            StringBuilder message = new StringBuilder().Append("<section><h2> This Versment has Verment :</h2><ol>");
            foreach (KeyValuePair<long, DataRow> v in c)
            {
                var t = (models.Versment)v.Value;
                message.Append($"<li>Date:{t.Date.ToShortDateString()} , Monatnt : {t.Montant }</li>");
            }
            message.Append("</ol><br><h2>Do you want to keep this versment or delete them</h2></section>");
            JObject @ref;
            @ref = new JObject() { { "Ref", (JString)orgFacture.Ref }, { "Id", (JNumber)orgFacture.Id } };
            var m = args.SendConfirm("Confirm",message.ToString(), "Transfer to client", "Delete Those versment", false, @ref);
            QServer.Serialization.MessageSerializer.Register(m);
            args.Client.SetCookie(cookieName, m, DateTime.Now + TimeSpan.FromSeconds(15));
            return false;
        }
        
        public float getVersmnet(Database database)
        {
            float ttl = 0f;
            var id = Id;
            var data = database.Versments.AsList();
            for (int i = data.Length - 1; i >= 0; i--)
            {
                var x = (Versment)data[i].Value;
                if (x.Facture?.Id == id)
                    ttl += x.Montant;
            }
            return ttl;
        }
        public bool hasVersment(Database database)
        {
            var id = Id;
            var data = database.Versments.AsList();
            for (int i = data.Length - 1; i >= 0; i--)
            {
                var x = (Versment)data[i].Value;
                if (x.Facture?.Id == id)
                    return true;
            }
            return false;
        }
        internal Versments GetVersments(Database database)
        {
            var id = Id;
            var t = new Versments(this);
            foreach (KeyValuePair<long, DataRow> x in database.Versments)
            {
                var sv = (models.Versment)x.Value;
                if (sv.Facture?.Id == id) t.Add(sv);
            }
            return t;
        }
        internal bool GetVersments(ref Versments source)
        {
            var id = Id;
            var t = new Versments(this);
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.Versment)x.Value;
                if (sv.Facture?.Id == id)
                    t.Add(sv);
                
            }
            source = t;
            return t.Count != 0;
        }
        internal bool HasVersments(ref Versments source,out float ttl)
        {
            var id = Id;
            var t = new Versments(this);
            ttl = 0f;
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.Versment)x.Value;
                if (sv.Facture?.Id == id)
                {
                    t.Add(sv);
                    ttl += sv.Montant;
                }
            }
            source = t;
            return t.Count != 0;
        }
    }
}