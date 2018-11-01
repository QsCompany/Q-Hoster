using System;
using Json;
using models;
using Server;

namespace Api
{
    public class SFacture : Service
    {
        public SFacture()
            : base("SFacture")
        {

        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }

        public override bool Create(RequestArgs args)
        {
            object la = args.Client.GetCookie("facturCreation", false, out bool expired);
            if (!expired && la != null) { args.SendAlert("Wait", $"You must wait for : {((DateTime.Now - (DateTime)la).TotalSeconds.ToString()) } s", "OK", false); return false; }
            args.Client.SetCookie("facturCreation", DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(20));


            if (!args.GetParam("FId", out long clientID)) return args.SendFail();
            var fournisseur = args.Database.Fournisseurs[clientID];
            if (fournisseur == null) args.SendError("Fournisseur Not Exist");

            if (!args.GetParam("AId", out clientID))
                return args.SendFail();

            var agent = args.Database.Agents[clientID];
            if (agent == null) args.SendError("Agent Not Exist");

            if (!args.GetParam("Type", out int fType)) return args.SendFail();
            var bonType = (BonType)fType;

            if (!args.GetParam("TType", out fType)) return args.SendFail();
            var transaction = (TransactionType)fType;

            if (int.TryParse(bonType.ToString(), out var i)) return args.SendError("Facture Type is UnCorrect");
            

            var t = new models.SFacture
            {
                Fournisseur = fournisseur,
                Achteur = agent,
                LockedBy = args.Client,
                LockedAt = DateTime.Now.Ticks,
                Id = DataRow.NewGuid(),
                Editeur = args.Client,
                Date = DateTime.Now,
                IsValidated = ((int)bonType & 4096) == 4096 ? false : true,
                LastModified = DateTime.Now
            };
            
            var x = (bonType == BonType.Bon ? "F" : bonType.ToString())[0] + "" + (transaction == TransactionType.Vente ? "A" : transaction.ToString())[0];
            t.Ref = x + $"{(AppSetting.Default.FactureCounter++).ToString("D5")}";
            t.SetFactureType(bonType, transaction);
            if (args.Database.Save(t, false))
            {
                args.Database.SFactures.Add(t);
                args.JContext.Add(typeof(models.SFacture), new FactureParameter(args.User, typeof(models.SFacture)));
                args.Send(t);
            }
            else return args.SendFail();
            return true;
        }

        public override bool Get(RequestArgs args)
        {
            if (!models.SFacture.IsAccessibleBy(args, out var msg)) return args.SendError(msg);
            var id = args.Id;
            if (id == -1) return args.CodeError(404);
            var sf = args.Database.SFactures[id];
            if (sf == null) return args.SendError("This Facture Is not Exist", false);
            if ((sf as FactureBase).Operation(args)) return true;
            args.JContext.Add(typeof(models.Clients), new DObjectParameter(typeof(models.Clients)) { DIsFrozen = true });
            args.Send(sf);
            return true;
        }

        private new static bool RequireNew(string n, JObject x) => n == "models.FakePrice" || n == "models.SFacture";

        public override bool Post(RequestArgs args)
        {
            if (!models.SFacture.IsAccessibleBy(args, out var msg)) return args.SendError(msg);
            if (args.GetParam("Set", out string property)) return set(args, property);
            if (args.GetParam("SetInfo", out string isInfo)) return SetInfo(args);
            return args.SendError("Method Not Implimented +<br>" + System.Threading.CompressedStack.Capture().ToString());
        }

        private bool SetInfo(RequestArgs args)
        {
            var fid = args.Id;
            var f = args.Database.SFactures[args.Id];
            if (f == null) return args.SendError("La facture n'exist pas");
            args.JContext.RequireNew = RequireNew;
            var nf = args.BodyAsJson as models.SFacture;

            var x = f.SaveStat();
            f.Achteur = nf.Achteur;
            f.Date = nf.Date;
            f.DateLivraison = nf.DateLivraison;
            f.Editeur = nf.Editeur;
            f.Observation = nf.Observation;
            f.Validator = nf.Validator;
            nf.Dispose();
            if (args.Database.StrictSave(f, true))
                return args.SendSuccess();
            f.Restore(x);
            return args.SendFail();
        }

        private bool set(RequestArgs args,string property)
        {
            if (!args.GetParam("Id", out long fid)) return args.SendFail();
            if (!args.GetParam("value", out long cid)) return args.SendFail();

            var f = args.Database.SFactures[fid];
            if (f == null) return args.SendError("La facture n'exist pas");
            switch (property)
            {
                case nameof(models.SFacture.Fournisseur):
                    var c = args.Database.Fournisseurs[cid];
                    if (c == null) return args.SendError("ParamNullExeption");
                    QServer.Complex.SFactureManager.ChangeFournisseur(args, f, c);
                    break;

                case nameof(models.SFacture.Achteur):
                    var agent = args.Database.Agents[cid];
                    if (agent == null) return args.SendError("ParamNullExeption");
                    var old = f.Achteur;
                    f.Achteur = agent;
                    if (args.Database.StrictSave(f, true))
                        return args.SendSuccess();
                    f.Achteur = old;
                    return args.SendFail();

                default:
                    return args.SendFail();
            }
            return args.SendFail();
        }

        public override bool Delete(RequestArgs args)
        {
            var id = args.Id;
            if (id == -1) return args.SendError("The facture Is not exist");
            var orgFacture = args.Database.SFactures[id];
            if (orgFacture == null) return args.SendError("The facture Is not exist");
            if (orgFacture.Articles.Count > 0) return args.SendError("Delete Articles befor delete this facture");
            return QServer.Complex.SFactureManager.DeleteFacture(args, orgFacture);
        }

        private void SendConfirm(RequestArgs arg)
        {
            var id = Guid.NewGuid();
            arg.Client.SetCookie("post_sfacture", id, DateTime.Now + TimeSpan.FromMinutes(5));
            var t = new Negociation
            {
                Id = id,
                user = arg.User,
                OnResponse = OnResponse,
                Data = this,
                ExpireDate = DateTime.Now + TimeSpan.FromMinutes(5)
            };
        }

        private static void OnResponse(Negociation cj, RequestArgs ra)
        {
        }
    }
    public class SFactures : Service
    {
        public SFactures()
            : base("SFactures")
        {

        }
        public override bool Get(RequestArgs args)
        {
            args.Client.SetCookie(Name.ToLower() + "_lasttimeupdated", System.DateTime.Now, args.Server.ExpiredTime);
            args.JContext.Add(typeof(models.SFactures), new DataTableParameter(typeof(models.SFactures)) { SerializeItemsAsId = false, DIsFrozen = false });
            args.JContext.Add(typeof(models.SFacture), new FactureParameter(args.User, typeof(models.SFacture)) { DIsFrozen = true });

            args.Send(args.Database.SFactures);
            return true;
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
        public override bool SUpdate(RequestArgs args)
        {
            args.Database.SFactures.Remove(0);
            return args.Database.SFactures.SendUpdates(args);
        }
    }
}