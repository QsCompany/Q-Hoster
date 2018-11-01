using System;
using QServer.Core;
using Server;
using Json;

namespace models
{
    abstract public partial class FactureBase
    {
        public int Factor => (int)Type < 4096 ? Transaction == TransactionType.Avoir ? -1 : 1 : 1;
        public bool IsValidable => (int)Type < 4096;

        public void SetFactureType(BonType bonType, TransactionType transaction)
        {
            if ((int)bonType > 4096) transaction = TransactionType.Neutre;
            else
            {
                if (transaction == TransactionType.Neutre)
                    transaction = TransactionType.Vente;
            }
            set(DPTransaction, transaction);
            set(DpType, bonType);
        }

        public virtual bool IsAccessibleBy(User user,bool force,out string msg)
        {
            if (LockedBy != null && LockedBy != user.Client)
            {
                if (force && user.IsAgent) { msg = null; return true; }
                msg = $"Cette facture est Ouvert par {LockedBy.Name } : { LockedBy.FullName}";
                return false;
            }
            msg = null;
            return true;
        }

        public bool CLose(RequestArgs args)
        {
            if (LockedBy == null) return args.SendSuccess();
            var forceClose = args.GetParam("Force") == "true";
            var lockedBy = LockedBy;
            if (args.Client == lockedBy)
                goto close;
            if (args.User.IsAgent || forceClose)
            {
                if (args.User.IsAgent)
                    goto close;
                else if (args.Client == lockedBy)
                    goto close;
                else if (LockedAt < (DateTime.Now - TimeSpan.FromMinutes(15)).Ticks)
                    goto close;
                return args.SendError("This Facture Is Open by " + lockedBy.FirstName + " " + lockedBy.LastName);
            }

            goto fail;

            close:
            LockedBy = null;
            if (args.Database.Save(this, true))
                return args.SendSuccess();
            else LockedBy = lockedBy;

            fail:
            return args.SendError(CodeError.fatal_error);
        }

        public bool Open(RequestArgs args)
        {
            if (LockedBy == args.Client) return args.SendSuccess();
            var forceOpen = args.GetParam("Force") == "true";
            var lockedBy = LockedBy;
            var lockedAt = LockedAt;
            if (lockedBy == null) goto open;

            if (forceOpen)
            {
                if (args.User.IsAgent)
                    goto open;
                else if (LockedAt < (DateTime.Now - TimeSpan.FromMinutes(15)).Ticks)
                    goto open;
                return args.SendError("This Facture Is Open by " + lockedBy.FirstName + " " + lockedBy.LastName);
            }
            goto fail;

            open:
            LockedBy = args.Client;
            LockedAt = DateTime.Now.Ticks;
            if (args.Database.Save(this, true))
                return args.SendSuccess();
            else
            {
                LockedBy = lockedBy;
                LockedAt = lockedAt;
            }
            fail:
            return args.SendError(CodeError.fatal_error);
        }

        public bool IsLocked(RequestArgs args) => args.Client != LockedBy;

        public bool Operation(RequestArgs args)
        {
            var oper = args.GetParam("Operation");
            switch (oper)
            {
                case "Open":
                    Open(args);
                    return true;
                case "Close":
                    CLose(args);
                    return true;
                case "IsOpen":
                    if (LockedBy == null) args.SendSuccess();
                    else args.SendFail();
                    return true;
                case "Benifit":
                    var t = this.CalcBenifit();
                    args.Send(t.ToString());
                    return true;
            }
            return false;
        }

        public abstract IBenifit CalcBenifit();
    }

    abstract public partial class FactureBase : DataRow, IHistory
    {
        public new static int __LOAD__(int dp) { return DPLockedBy; }

        public readonly static int DPRef = Register<FactureBase, string>("Ref", PropertyAttribute.None, null, null, "nvarchar(10)");
        public static readonly int DpTotal = Register<FactureBase, float>("Total"); public float Total
        {
            get => get<float>(DpTotal);
            set => set(DpTotal, value);
        }
        public static readonly int DpDateLivraison = Register<FactureBase, DateTime>("DateLivraison");
        public static readonly int DpDate = Register<FactureBase, DateTime>("Date");
        public static readonly int DPEditeur = Register<FactureBase, Client>("Editeur", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetClient(c));
        public static readonly int DPValidator = Register<FactureBase, Agent>("Validator", PropertyAttribute.AsId, null, (d, f) => ((Database)d).GetAgent(f));
        public static readonly int DPObservation = Register<FactureBase, string>("Observation", PropertyAttribute.None, null, null, "nvarchar(255)");
        public static readonly int DPLockedBy = Register<FactureBase, Client>("LockedBy", PropertyAttribute.Private, null, (d, c) => ((Database)d).GetClient(c));
        public static readonly int DPLockedAt = Register<FactureBase, long>("LockedAt", PropertyAttribute.Private, null);        
        public static readonly int DpType = Register<FactureBase, BonType>("Type");
        public static readonly int DPValidated = Register<FactureBase, bool>("IsValidated", PropertyAttribute.NonModifiableByHost);
        public static readonly int DPNArticles = Register<FactureBase, int>("NArticles", PropertyAttribute.None, null);

        public static int DPTransaction = Register<FactureBase, TransactionType>("Transaction");
        public TransactionType Transaction { get => get<TransactionType>(DPTransaction);  }

        public string Ref { get { return get<string>(DPRef); } set { set(DPRef, value); } }

        public DateTime DateLivraison
        {
            get => get<DateTime>(DpDateLivraison);
            set => set(DpDateLivraison, value);
        }

        public DateTime Date
        {
            get => get<DateTime>(DpDate);
            set => set(DpDate, value);
        }
        
        public Client Editeur
        {
            get => get<Client>(DPEditeur);
            set => set(DPEditeur, value);
        }
        
        public Agent Validator
        {
            get => get<Agent>(DPValidator);
            set => set(DPValidator, value);
        }
        
        public string Observation
        {
            get => get<string>(DPObservation);
            set => set(DPObservation, value);
        }
        
        public Client LockedBy
        {
            get => get<Client>(DPLockedBy);
            set => set(DPLockedBy, value);
        }
        
        public long LockedAt
        {
            get => get<long>(DPLockedAt);
            set => set(DPLockedAt, value);
        }
        
        public BonType Type
        {
            get => get<BonType>(DpType);
        }
        
        public bool IsValidated
        {
            get => get<bool>(DPValidated);
            set => set(DPValidated, value);
        }
        
        public int NArticles { get { return get<int>(DPNArticles); } set { set(DPNArticles, value); } }
        
        protected FactureBase(Context c, JValue jv) : base(c, jv)
        {
        }

        protected FactureBase()
        {
        }

        public abstract void CopyFacture(FactureBase newFacture);
        public abstract Dealer Partner { get; }
    }

    public struct IBenifit
    {
        public float Total;
        public float Benifit;
        public override string ToString() => $"{{\"Total\":{Total},\"Benifit\":{Benifit}}}";
    }
}
