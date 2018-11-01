using Server;
using System;
using Json;
using System.Collections.Generic;
using System.Linq;
namespace Api
{
    public class  EtatTransfers : Service
    {
        public EtatTransfers() : base("EtatTransfers")
        {
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
        public override bool Get(RequestArgs args)
        {
            var id = args.Id;
            if ((args.GetParam("IsAchat") ?? "").ToLower() !="true")
                this.GetClientEtat(args, id);
            else this.GetFournisseurEtat(args, id);
            return true;
        }

        private void GetFournisseurEtat(RequestArgs args, long id)
        {
            var d = args.Database;
            var frn = d.Fournisseurs[id];
            if (frn == null) { args.SendError("The Fournisseur Doen't Exist"); return; }
            var t = new models.EtatTransfers(frn);
            
            var factures = d.SFactures.AsList();
            for (int i = factures.Length - 1; i >= 0; i--)
            {
                var fact = (models.SFacture)factures[i].Value;
                if (fact.IsValidable)
                    if (fact.Fournisseur?.Id == id)
                    {
                        var x = new models.EtatTransfer(fact);
                        t.Add(x);
                        t.TotalEntree += x.MontantEntree;
                    }
            }
            var versments = d.SVersments.AsList();
            for (int i = versments.Length - 1; i >= 0; i--)
            {
                var ver = (models.SVersment)versments[i].Value;
                if (ver.Fournisseur?.Id == id)
                {
                    var x = new models.EtatTransfer(ver);
                    t.Add(x);
                    t.TotalSortie += x.MontantSortie;
                }
            }
            args.JContext.Add(typeof(models.EtatTransfers), new DataTableParameter(typeof(models.EtatTransfers)) { SerializeItemsAsId = false });
            args.Send(t);
        }

        private void GetClientEtat(RequestArgs args, long id)
        {
            var d = args.Database;
            var client = d.Clients[id];
            if (client == null) { args.SendError("The Client Doen't Exist"); return; }
            var t = new models.EtatTransfers(client) { IsVente = true };
            var factures = d.Factures.AsList();
            for (int i = factures.Length - 1; i >= 0; i--)
            {
                var fact = (models.Facture)factures[i].Value;
                if (fact.IsValidable)
                    if (fact.Client?.Id == id)
                    {
                        var x = new models.EtatTransfer(fact);
                        t.Add(x);
                        t.TotalSortie += x.MontantSortie;
                    }
            }
            var versments = d.Versments.AsList();
            for (int i = versments.Length - 1; i >= 0; i--)
            {
                var ver = (models.Versment)versments[i].Value;
                if (ver.Client?.Id == id)
                {
                    var x = new models.EtatTransfer(ver);
                    t.Add(x);
                    t.TotalEntree += x.MontantEntree;
                }
            }
            args.JContext.Add(typeof(models.EtatTransfers), new DataTableParameter(typeof(models.EtatTransfers)) { SerializeItemsAsId = false, FullyStringify = true });
            args.Send(t);
        }
    }
}
namespace models
{

    public enum TransferType
    {
        Versment,
        Facture
    }


    public class EtatTransfer : DataRow
    {
        public new static int __LOAD__(int dp) => DataRow.__LOAD__(DPTransactionId);
        public readonly static int DPType = Register<EtatTransfer, TransferType>("Type", PropertyAttribute.None);
        public TransferType Type { get { return get<TransferType>(DPType); } set { set(DPType, value); } }
        public readonly static int DPDate = Register<EtatTransfer, DateTime>("Date");
        public DateTime Date { get { return get<DateTime>(DPDate); } set { set(DPDate, value); } }
        public readonly static int DPMontantEntree = Register<EtatTransfer, float>("MontantEntree", PropertyAttribute.None, null);
        public float MontantEntree { get { return get<float>(DPMontantEntree); } set { set(DPMontantEntree, value); } }
        public readonly static int DPMontantSortie = Register<EtatTransfer, float>("MontantSortie", PropertyAttribute.None, null);
        public float MontantSortie { get { return get<float>(DPMontantSortie); } set { set(DPMontantSortie, value); } }
        public readonly static int DPTransactionId = Register<EtatTransfer, long>("TransactionId", PropertyAttribute.None, null);

        public string Transaction => Type == TransferType.Versment ? "Versment" : "Achat";

        public float Montant => MontantEntree + MontantSortie;
        public float SoldActual { get; set; }
        public string Ref { get; set; }

        public EtatTransfer()
        {

        }
        public EtatTransfer(Facture fact)
        {
            this.TransactionId = fact.Id;
            this.Type = TransferType.Facture;
            this.Date = fact.Date;
            this.MontantSortie = fact.Total;
        }
        public EtatTransfer(Versment ver)
        {
            this.TransactionId = ver.Id;
            this.Type = TransferType.Versment;
            this.Date = ver.Date;
            this.MontantEntree = ver.Montant;
        }


        public EtatTransfer(SFacture fact)
        {
            this.TransactionId = fact.Id;
            this.Type = TransferType.Facture;
            this.Date = fact.Date;
            this.MontantEntree = fact.Total;
        }
        public EtatTransfer(SVersment ver)
        {
            this.TransactionId = ver.Id;
            this.Type = TransferType.Versment;
            this.MontantSortie = ver.Montant;
            this.Date = ver.Date;
        }

        public long TransactionId { get { return get<long>(DPTransactionId); } set { set(DPTransactionId, value); } }
    }
    [QServer.Core.HosteableObject(typeof(Api.EtatTransfers), null)]
    public class EtatTransfers : DataTable<EtatTransfer>
    {
        public new static int __LOAD__(int dp) => DataTable<EtatTransfer>.__LOAD__(DPIsVente);
        public readonly static int DPTotalEntree = Register<EtatTransfers, float>("TotalEntree", PropertyAttribute.None, null);
        public float TotalEntree { get { return get<float>(DPTotalEntree); } set { set(DPTotalEntree, value); } }

        public readonly static int DPTotalSortie = Register<EtatTransfers, float>("TotalSortie", PropertyAttribute.None, null);
        public float TotalSortie { get { return get<float>(DPTotalSortie); } set { set(DPTotalSortie, value); } }

        public readonly static int DPIsVente = Register<EtatTransfers, bool>("IsVente", PropertyAttribute.None, null);
        private Client client;

        public bool IsVente { get { return get<bool>(DPIsVente); } set { set(DPIsVente, value); } }

        public EtatTransfers(DataRow owner) : base(owner)
        {
        }

        protected EtatTransfers(Context c, JValue jv) : base(c, jv)
        {
        }

        protected override void GetOwner(DataBaseStructure d, Path c)
        {
        }

        public EtatTransfers() : base(null)
        {
        }
        public EtatTransfers(Database db, Fournisseur frn) : base(frn)
        {
            if (frn == null) { return; }
            var id = frn.Id;
            IsVente = false;
            var factures = db.SFactures.AsList();
            for (int i = factures.Length - 1; i >= 0; i--)
            {
                var fact = (models.SFacture)factures[i].Value;
                if (fact.IsValidable)
                    if (fact.Fournisseur?.Id == id)
                    {
                        var x = new EtatTransfer(fact);
                        this.Add(x);
                        this.TotalEntree += x.MontantEntree;
                    }
            }
            var versments = db.SVersments.AsList();
            for (int i = versments.Length - 1; i >= 0; i--)
            {
                var ver = (models.SVersment)versments[i].Value;
                if (ver.Fournisseur?.Id == id)
                {
                    var x = new EtatTransfer(ver);
                    Add(x);
                    TotalSortie += x.MontantSortie;
                }
            }
        }

        public EtatTransfers(Database d,Client client):base (client)
        {
            if (client == null) {  return; }
            var id = client.Id;
            IsVente = true;
            var factures = d.Factures.AsList();
            for (int i = factures.Length - 1; i >= 0; i--)
            {
                var fact = (models.Facture)factures[i].Value;
                if (fact.IsValidable)
                    if (fact.Client?.Id == id)
                    {
                        var x = new EtatTransfer(fact);
                        Add(x);
                        TotalSortie += x.MontantSortie;
                    }
            }
            var versments = d.Versments.AsList();
            for (int i = versments.Length - 1; i >= 0; i--)
            {
                var ver = (models.Versment)versments[i].Value;
                if (ver.Client?.Id == id)
                {
                    var x = new EtatTransfer(ver);
                    Add(x);
                    TotalEntree += x.MontantEntree;
                }
            }
        }


        public static EtatTransfer[] GetEtatTransfers(Database db, Fournisseur frn, out float TotalEntree, out float TotalSortie)
        {
            TotalEntree = 0;
            TotalSortie = 0;
            if (frn == null) { return null; }
            var @this = new List<EtatTransfer>();
            var id = frn.Id;
            var factures = db.SFactures.AsList();
            for (int i = factures.Length - 1; i >= 0; i--)
            {
                var fact = (models.SFacture)factures[i].Value;
                if (fact.IsValidable)
                    if (fact.Fournisseur?.Id == id)
                    {
                        var x = new EtatTransfer(fact);
                        @this.Add(x);
                        TotalEntree += x.MontantEntree;
                    }
            }
            var versments = db.SVersments.AsList();
            for (int i = versments.Length - 1; i >= 0; i--)
            {
                var ver = (models.SVersment)versments[i].Value;
                if (ver.Fournisseur?.Id == id)
                {
                    var x = new EtatTransfer(ver);
                    @this.Add(x);
                    TotalSortie += x.MontantSortie;
                }
            }
            return OrderByDate(@this);
        }

        public static EtatTransfer[] GetEtatTransfers(Database d, Client client,out float TotalFactures,out float TotalVersments) 
        {
            TotalFactures = 0;
            TotalVersments = 0;
            if (client == null) { return null; }
            var id = client.Id;
            var @this = new List<EtatTransfer>();
            var factures = d.Factures.AsList();
            for (int i = factures.Length - 1; i >= 0; i--)
            {
                var fact = (Facture)factures[i].Value;
                if (fact.IsValidable)
                    if (fact.Client?.Id == id)
                    {
                        var x = new EtatTransfer(fact) { Ref = fact.Ref };

                        @this.Add(x);
                        TotalFactures += x.MontantSortie;
                    }
            }
            var versments = d.Versments.AsList();
            for (int i = versments.Length - 1; i >= 0; i--)
            {
                var ver = (Versment)versments[i].Value;
                if (ver.Client?.Id == id)
                {
                    var x = new EtatTransfer(ver) { Ref = ver.Ref };
                    @this.Add(x);
                    TotalVersments += x.MontantEntree;
                }
            }
            return OrderByDate(@this);
        }
        public static EtatTransfer[] OrderByDate(List<EtatTransfer> lst) => lst.OrderBy((a) => a.Date.Ticks).ToArray();

        public static void CalcSoldActual(EtatTransfer[] lst)
        {
            if (lst == null) return;
            var tot = 0f;
            for (int i = 0; i < lst.Length; i++)
            {
                var x = lst[i];
                if (x.Type == TransferType.Facture)
                    tot += x.MontantEntree + x.MontantSortie;
                else tot -= x.MontantEntree + x.MontantSortie;
                x.SoldActual = tot;
            }
        }
    }
}
