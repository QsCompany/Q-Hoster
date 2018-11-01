using Json;
using QServer.Core;
using Server;
using System;

namespace models
{

    [HosteableObject(null,typeof(Serializers.CommandSerializer))]
    public partial class Command : DataRow, IHistory
    {
        public new static int __LOAD__(int dp)
        {
            DataRow.__LOAD__(DPDate);
            CArticles.__LOAD__(CArticle.DPQte);
            return Commands.__LOAD__(0);
        }


        public static int DPArticles = Register<Command, CArticles>("Articles", PropertyAttribute.Private,null, (d, c) =>
        {

            var cmd = (Command)c.Owner;
            var arts = cmd.Articles;
            if (arts == null) cmd.Articles = arts = new CArticles(cmd);
            c.Owner.set(c.Property.Index, arts);
            foreach (System.Collections.Generic.KeyValuePair<long, DataRow> fp in ((Database)d).CArticles)
            {
                var v = fp.Value as CArticle;
                if (v.Command == cmd)
                    arts.Add(v);
            }
        });
        public CArticles Articles { get => get<CArticles>(DPArticles); set => set(DPArticles, value); }

        public static int DPDate = Register<Command, DateTime>("Date");
        public DateTime Date { get => get<DateTime>(DPDate); set => set(DPDate, value); }

        public static int DPIsOpen = Register<Command, Boolean>("IsOpen");
        public Boolean IsOpen { get => get<Boolean>(DPIsOpen); set => set(DPIsOpen, value); }

        public static int DPLockedBy = Register<Command, Agent>("LockedBy", PropertyAttribute.Private,null, (d, f) => ((Database)d).GetAgent(f));
        public Agent LockedBy { get => get<Agent>(DPLockedBy); set => set(DPLockedBy, value); }

        public virtual bool IsAccessibleBy(User user, bool force, out string msg)
        {
            if (this.IsOpen)
                if (LockedBy != null && LockedBy != user.Agent)
                    if (!force)
                    {
                        msg = $"Cette facture est Ouvert par {LockedBy.Name } : { LockedBy.Name}";
                        return false;
                    }
            msg = null;
            return true;
        }


        public Command()
        {
            Articles = new CArticles(this);
        }

    }
}

namespace models
{
    [HosteableObject(null, typeof(Serializers.CArticleSerializer))]
    public class CArticle : DataRow
    {

        public static int DPCommand = Register<CArticle, Command>("Command", PropertyAttribute.AsId,null, (d, f) => ((Database)d).GetCommand(f));
        public Command Command { get => get<Command>(DPCommand); set => set(DPCommand, value); }

        public static int DPFournisseur = Register<CArticle, Fournisseur>("Fournisseur",PropertyAttribute.AsId,null, (d, f) => ((Database)d).GetFournisseur(f));
        public Fournisseur Fournisseur { get => get<Fournisseur>(DPFournisseur); set => set(DPFournisseur, value); }

        //public static int DPProduct = Register<CArticle, Product>("Product", PropertyAttribute.AsId,null, (d, f) => d.GetProduct(f));
        //public Product Product { get => get<Product>(DPProduct); set => set(DPProduct, value); }
        public static int DPProduct = Register<CArticle, Product>("Product", PropertyAttribute.None, null, (d, f) => ((Database)d).GetProduct(f));
        public Product Product { get => get<Product>(DPProduct); set => set(DPProduct, value); }

        public static int DPQte = Register<CArticle, float>("Qte");
        public float Qte { get => get<float>(DPQte); set => set(DPQte, value); }
        
        public static int DPPrice = Register<CArticle, float>("Price");
        public float Price { get => get<float>(DPPrice); set => set(DPPrice, value); }


        public static int DPPriceMin = Register<CArticle, float>("PriceMin");
        public float PriceMin { get => get<float>(DPPriceMin); set => set(DPPriceMin, value); }


        public static int DPPriceMax = Register<CArticle, float>("PriceMax");
        public float PriceMax { get => get<float>(DPPriceMax); set => set(DPPriceMax, value); }


        public static int DPProductName = Register<CArticle, string>("ProductName", Server.PropertyAttribute.None, null, null, "NVARCHAR(50)");
        public string ProductName { get => get<string>(DPProductName); set => set(DPProductName, value); }

        public static int DPDateSel = Register<CArticle, DateTime>("DateSel");

        public string Name
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ProductName)) return this.ProductName;
                var p = Product;
                if (p == null) return "Le Produit est Supprimé";
                return   $"{(p.Name ?? "").Trim()} {(p.Dimention ?? "").Trim()} {(string.IsNullOrWhiteSpace(p.SerieName) ? "" : " - " + p.SerieName.Trim())} ";
            }
        }

        public string CategoryName => Product?.Category?.Name;

        public string FournisseurName => Fournisseur?.Name;
        public CArticle(Context c, JValue jv) : base(c, jv)
        {
        }

        public CArticle()
        {
        }

        public DateTime DateSel { get => get<DateTime>(DPDateSel); set => set(DPDateSel, value); }

    }
}

namespace models
{
    public class Commands:DataTable<Command>
    {
        public new static int __LOAD__(int dp) => DataTable<Command>.__LOAD__(Command.DPDate);

        private readonly Database database;

        public Commands(Context c,JValue jv):base(c,jv)
        {

        }
        public Commands(Database database) : base(database)
        {
            this.database = database;
        }
        protected override void GetOwner(DataBaseStructure d, Path c) => c.Owner.set(c.Property.Index, database);
    }
    public class CArticles : DataTable<CArticle>
    {
        public new static int __LOAD__(int dp) => DataTable<Command>.__LOAD__(CArticle.DPQte);
        public Command Command { get => (Command)get(DPOwner); set => set(DPOwner, value); }
        
        public CArticles(Command owner) : base(owner)
        {
        }
        public CArticles(Context c, JValue jv) : base(c, jv)
        {

        }

        public CArticles(Database database) : base(database)
        {
        }

        protected CArticles(DataRow owner) : base(owner)
        {
        }

        protected override void GetOwner(DataBaseStructure d, Path c) => ((Database)d).GetCommand(c);
    }
}