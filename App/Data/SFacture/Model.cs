using System;
using System.Collections.Generic;
using Json;
using models;
using QServer.Core;
using Server;
using System.Text;

namespace models
{
    [HosteableObject(typeof(Api.SFacture), typeof(Serializers.SFactureSerializer))]
    public partial class SFacture : FactureBase
    {
        public new static int __LOAD__(int dp) => FactureBase.__LOAD__(DPFournisseur);
        public static int DPFournisseur = Register<SFacture, Fournisseur>("Fournisseur", PropertyAttribute.AsId| PropertyAttribute.NonModifiableByHost, null, (d, f) => ((Database)d).GetFournisseur(f));
        public static int DPAchteur = Register<SFacture, Agent>("Achteur", PropertyAttribute.AsId, null, (d, f) => ((Database)d).GetAgent(f));
        protected override void OnPropertyChanged(DProperty dp)
        {
            if (dp.Index == 17)
            {

            }
            base.OnPropertyChanged(dp);
        }

        public static int DPArticles = Register<SFacture, FakePrices>("Articles", PropertyAttribute.Private, null, (d, c) =>
        {

            var sfact = (SFacture)c.Owner;
            var arts = sfact.Articles;
            if (arts == null) sfact.Articles = arts = new FakePrices(sfact);
            c.Owner.set(c.Property.Index, arts);
            foreach (KeyValuePair<long, DataRow> fp in ((Database)d).FakePrices)
            {
                var v = fp.Value as FakePrice;
                if (v.Facture == sfact)
                    arts.Add(v);
            }
        });
        public override bool IsAccessibleBy(User user, bool force, out string msg)
        {
            if (!user.IsAgent)
            {
                msg = "Vous n'a pas l'authorité d'access a cet facture";
                return false;
            }
            return base.IsAccessibleBy(user, force, out msg);
        }
        public static bool IsAccessibleBy(RequestArgs args, out string msg,SFacture f=null)
        {
            f = f ?? args.Database.SFactures[args.Id];
            if (f == null)
            {
                msg = "Je Pense Que La facture est Supprimer";
                return false;
            }
            return f.IsAccessibleBy(args.User, args.GetParam("Force", out bool b), out msg);
        }
        public static bool IsAccessibleBy(RequestArgs args, out string msg)
        {
            var f = args.Database.SFactures[args.Id];
            if (f == null)
            {
                msg = "Je Pense Que La facture est Supprimer";
                return false;
            }
            return f.IsAccessibleBy(args.User, args.GetParam("Force", out bool b), out msg);
        }
        #region Properties
        public Fournisseur Fournisseur
        {
            get => get<Fournisseur>(DPFournisseur);
            set => set(DPFournisseur, value);
        }
        public Agent Achteur
        {
            get => get<Agent>(DPAchteur);
            set => set(DPAchteur, value);
        }


        public FakePrices Articles
        {
            get => get<FakePrices>(DPArticles);
            set => set(DPArticles, value);
        }


        #endregion
        public bool Check(RequestArgs args)
        {
            if (Articles == null) { Articles = new FakePrices(this); }
            else Articles.Owner = this;
            var of = args.Database.SFactures[Id];
            var IsNew = of == null;
            if (Validator == null) Validator = args.User.Agent;
            if (Validator == null || Fournisseur == null || Achteur == null)
                return args.SendError("Fournisseaur AND Validator AND Achteur <h1>MUST BE SETTED</h1>", false);

            if (!(IsNew ? CheckArticlesAsNew(args) : CheckArticlesAsOld(args, of)))
                return false;
            NArticles = Articles.Count;
            return true;
        }
        private bool CheckArticlesAsNew(RequestArgs arg)
        {
            var t = 0f;
            foreach (KeyValuePair<long, DataRow> p in Articles)
            {
                var v = (FakePrice)p.Value;
                var oa = arg.Database.FakePrices[p.Key];
                if (v.Product == null) return arg.SendError("You Must Select an article for this item");
                if (v.Facture != null && v.Facture != this)
                    return arg.SendError(CodeError.fatal_error);
                v.Facture = oa?.Facture ?? this;
                if (!v.CheckPrices(arg)) return false;
                t += v.Qte * v.PSel;
            }
            Total = t;
            return true;
        }
        private bool CheckArticlesAsOld(RequestArgs arg, SFacture of)
        {
            var t = 0f;
            foreach (KeyValuePair<long, DataRow> p in Articles.AsList())
            {
                var v = (FakePrice)p.Value;
                if (!v.CheckPrices(arg)) return false;
                if (v.Facture != null && v.Facture.Id != of.Id)
                    return arg.SendError($"The Revage {v.Product.Name} Is not from this facture");
                var oart = of.Articles[p.Key];
                if (of.IsValidated)
                    if (oart != null && oart.Product != v.Product)
                    {

                        arg.SendAlert("Protocol", "Ce Article est deja Valide <br> si vous voulez changer le product supprimer le et entre un article a nouveau<br>"
                            + v.Product.Name + " " + v.Product.Dimention + " " + v.Product.SerieName
                            + "L'origin est  <br>" + oart.Product.Name + " " + oart.Product.Dimention + " " + oart.Product.SerieName
                            , "Je Comprend", false);
                        return false;
                    }
                v.Facture = of;
                t += v.Qte * v.PSel;
            }
            Total = t;
            return true;
        }

        public override JValue Parse(JValue json) => json;
        public SFacture(Context c, JValue jv)
            : base(c, jv)
        {
            if (Articles == null)
                this.Articles = new FakePrices(this);
        }
        public SFacture() => Articles = new FakePrices(this);

        internal bool GetSVersments(ref SVersments source)
        {
            var t = new SVersments(null);
            var id = Id;
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (SVersment)x.Value;
                if (sv.Facture?.Id == id) t.Add(sv);
            }
            source = t;
            return t.Count != 0;
        }

        internal bool GetVersments(ref SVersments source, out float ttl)
        {
            var t = new SVersments(null);
            var id = Id;
            ttl = 0f;
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (SVersment)x.Value;
                if (sv.Facture?.Id == id)
                {
                    t.Add(sv);
                    ttl += sv.Montant;
                }
            }
            source = t;
            return t.Count != 0;
        }

        internal bool hasVersment(Database database)
        {
            var t = database.SVersments;
            return HasVersments(ref t,out var tt);
        }
        internal bool HasVersments(ref SVersments source, out float ttl)
        {
            var id = Id;
            var t = new SVersments(this);
            ttl = 0f;
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.SVersment)x.Value;
                if (sv.Facture?.Id == id)
                {
                    t.Add(sv);
                    ttl += sv.Montant;
                }
            }
            source = t;
            return t.Count != 0;
        }

        internal bool Delete(RequestArgs args)
        {
            if (args.Database.Delete(this))
            {
                if (IsValidable)
                {
                    var c = Fournisseur;
                    c.MontantTotal -= Total;
                    args.Database.StrictSave(c, true);
                }
                args.Database.SFactures.Remove(this);
                return true;
            }
            return args.SendFail();
        }
        internal SVersments GetVersments(Database database)
        {
            var id = Id;
            var t = new SVersments(this);
            float ttl = 0f;
            foreach (KeyValuePair<long, DataRow> x in database.Versments)
            {
                var sv = (models.SVersment)x.Value;
                if (sv.Facture?.Id == id) t.Add(sv);
                ttl += sv.Montant;
            }
            return t;
        }

        public override int Repaire(Database db)
        {
            var t = 0f;
            foreach (var l in Articles.AsList())
            {
                var a = (FakePrice)l.Value;
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
            var b = 0f;
            var t = 0f;
            for (int i = 0; i < c.Length; i++)
            {
                var x = (FakePrice)c[i].Value;
                b += x.Qte * (x.DPrice - x.PSel);
                t += x.Qte * x.PSel;
            }
            return new IBenifit() { Total = t, Benifit = b };
        }
    }

    [HosteableObject(typeof(Api.SFactures), null)]
    public class SFactures : DataTable<SFacture>
    {
        public SFactures(DataRow owner)
            : base(owner)
        {
            if (!(owner is Fournisseur) && !(owner is Database)) throw null;
        }
        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            ((Database)d).GetFournisseur(c);
        }

        public SFactures(Context c, JValue jv)
            : base(c, jv)
        {
        }

    }

    public partial class SFacture
    {
        public static void TransferVersments(RequestArgs args, SFacture orgfact, SFacture newfact)
        {
            if (newfact == null || orgfact == null) return;
            var newFournisseur = newfact.Fournisseur;
            var orgFournisseur = orgfact.Fournisseur;

            if (newFournisseur == orgFournisseur) return;
            var d = args.Database;
            var versments = args.Database.SVersments;
            var hasVers = orgfact.GetVersments(ref versments, out var ttl);
            if (!hasVers) return;
            newFournisseur.VersmentTotal += ttl;
            d.Save(newFournisseur, true);

            orgFournisseur.VersmentTotal -= ttl;
            d.Save(orgFournisseur, true);

            foreach (KeyValuePair<long, DataRow> v in versments)
            {
                var _v = (SVersment)v.Value;
                _v.Fournisseur = newFournisseur;
                d.Save(_v, true);
            }
        }
        
        public override void CopyFacture(FactureBase _newFacture)
        {
            var newFacture = (SFacture)_newFacture;
            Fournisseur = newFacture.Fournisseur;
            Achteur = newFacture.Achteur;
            Editeur = newFacture.Editeur;
            Validator = newFacture.Validator;

            DateLivraison = newFacture.DateLivraison;
            Date = newFacture.Date;
            DateLivraison = newFacture.DateLivraison;
            Observation = newFacture.Observation;
            Total = newFacture.Total;
            Total = newFacture.Total;
            Ref = newFacture.Ref;
            NArticles = Articles.Count;

        }

        public override Dealer Partner => get<Fournisseur>(DPFournisseur);
        
        /// <summary>
        /// Prd Cannot be saved if any changed
        /// </summary>
        /// <param name="d"></param>
        /// <param name="prd"></param>
        /// <param name="price"></param>
        private static void DeletePrice(Database d, Product prd, FakePrice price)
        {

            if (!prd.getPrevious(price, out var prev)) return;
            if (prev == null || prd.Revage == price)
            {
                prd.Revage = price.Next;
            }
            else
            {
                prev.Next = price.Next;
                d.Save(prev, true);
            }
        }

       
        private static void AddPrice(Database d, Product prd, FakePrice price)
        {
            if (prd.Revage?.Id == price.Id) return;
            if (prd.getPrevious(price, out var prev)) return;
            if (prev == null)
            {
                price.Next = prd.Revage;
                prd.Revage = price;
                d.Save(prd, true);
                d.Save(price, false);
            }
            else
            {
            }
        }
        
    }

    public partial class SFacture
    {
        public static bool Delete(RequestArgs args, SFacture orgFacture)
        {
            if (!ConfirmDelleting(args, orgFacture)) return false;
            return args.SendFail();
        }

        private static bool ConfirmDelleting(RequestArgs args, SFacture orgFacture)
        {
            var c = args.Database.SVersments;
            var cookieName = "delete_facture_" + orgFacture.Id;
            var cvv = args.Client.GetCookie(cookieName + "confirmed", true, out var expired);
            var deletedConfirmed = Equals(true, cvv);

            if (deletedConfirmed && orgFacture.GetSVersments(ref c))
            {
                return ConfirmDelletingWVersm(args, orgFacture, c);
            }

            var cookie = args.Client.GetCookie(cookieName, true, out expired) as Message;
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
                    return orgFacture.GetSVersments(ref c) ? ConfirmDelletingWVersm(args, orgFacture, c) : true;
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

        private static bool ConfirmDelletingWVersm(RequestArgs args, SFacture orgFacture, SVersments c)
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
                            var v = (SVersment)kv.Value;
                            v.Facture = null;
                            if (!args.Database.Save(v, true))
                            {
                                foreach (KeyValuePair<long, DataRow> _kv in c)
                                {
                                    v = (SVersment)_kv.Value;
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
                            var v = (SVersment)kv.Value;
                            if (!args.Database.Delete(v))
                            {
                                foreach (KeyValuePair<long, DataRow> _kv in c)
                                {
                                    v = (SVersment)_kv.Value;
                                    args.Database.Save(v, false);
                                    args.Database.SVersments.Add(v);
                                }
                                return args.SendError(CodeError.fatal_error);
                            }
                            args.Database.SVersments.Remove(kv.Key);
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
            var m = args.SendConfirm("Confirm", message.ToString(), "Transfer to client", "Delete Those versment", false, @ref);
            QServer.Serialization.MessageSerializer.Register(m);
            args.Client.SetCookie(cookieName, m, DateTime.Now + TimeSpan.FromSeconds(15));
            return false;
        }
    }
}



