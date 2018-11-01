using System;
using System.Collections.Generic;
using Json;
using QServer.Core;
using Server;

namespace models
{
    [HosteableObject(typeof(Api.Article), typeof(ArticleSerializer))]
    public partial class  Article : DataRow
    {
        public new static int __LOAD__(int dp) => DpCount;
        public static int DPFacture = Register<Article, Facture>("Owner", PropertyAttribute.DeserializeById | PropertyAttribute.SerializeAsId, null, (d, c) => ((Database)d).GetFacture(c));
        public static int DPProduct = Register<Article, Product>("Product", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetProduct(c));

        public static int DPPrice = Register<Article, float>("Price");
        public static int DpCount = Register<Article, float>("Count");

        public static int DPPSel = Register<Article, float>("PSel");
        public float PSel { get => get<float>(DPPSel); set => set(DPPSel, value); }

        public static int DPProductName = Register<Article, string>("ProductName", Server.PropertyAttribute.None, null, null, "NVARCHAR(50)");
        public string ProductName { get => get<string>(DPProductName); set => set(DPProductName, value); }

        public static int DPOPrice = Register<Article, float>("OPrice");
        public float OPrice { get => get<float>(DPOPrice); set => set(DPOPrice, value); }

        public string Label
        {
            get
            {
                var p = get<string>(DPProductName);
                return string.IsNullOrWhiteSpace(p) ? Product?.Label : p;
            }
        }

        public Facture Facture { get => get<Facture>(DPFacture); set => set(DPFacture, value); }
        public Product Product { get => get<Product>(DPProduct); set => set(DPProduct, value); }
        
        public float Qte { get => get<float>(DpCount); set => set(DpCount, value); }
        public string Ref => Product.Ref;
        public float Price { get => get<float>(DPPrice); set => set(DPPrice, value); }

        public float Total => Qte * Price;
        public float GetTotalBenifice() => Qte * (Price - PSel);

        public float Order { get; set; }

        public bool IsValid => Price != 0;

        public override string ToString() => Qte + " " + Product;
        public Article(Context c, JValue j):base(c,j)
        {
            
        }

        public Article()
        {
        }

        public override JValue Parse(JValue json) => json;

        internal bool Check(RequestArgs args, int priceIndex)
        {
            if (Product == null) return args.SendError("L'article doit a un produit");
            
            var t = Product.GetPrice(priceIndex);
            if (args.User.IsAgent) return true;

            else
            {
                if (Price < t) Price = t;
                if (Qte <= 0) return args.SendError("Quantity D'article " + Product.Name + " faut il soit superieur a 0");
            }
            return true;
        }
        internal bool Check(RequestArgs args, out bool isNew)
        {
            var priceIndex = models.Price.GetDPPrice(args.Client.Abonment);
            isNew = false;
            if (Product == null) return args.SendError("L'article doit a un produit");
            var org = args.Database.Articles[Id];
            isNew = org == null;
            if (!isNew)
            {
                if (org.Facture != Facture) return args.SendError("Delete Article and reset the operation");
            }
            var t = Product.GetPrice(priceIndex);

            if (args.User.IsAgent) return true;
            else
            {
                if (Price < t) Price = t;
                if (Qte <= 0) return args.SendError("Quantity D'article " + Product.Name + " faut il soit superieur a 0");
            }
            return true;
        }

        public override int Repaire(Database db)
        {
            if (Product == null)
            { MyConsole.WriteLine("Fatal Error : Le Produit est Supprimer"); return 1; }
            if (Facture == null)
            {
                MyConsole.WriteLine("Fatal Error : La Facture est Supprimer");
                return 1;
            }
            return 0;
        }
        public static Product Unknown = new Product() { Id = -1, Name = "Mybe deleted Product" };
    }
    [HosteableObject(typeof(Api.Articles), typeof(ArticlesSerializer))]
    public class Articles : DataTable<Article>
    {
        public Article[] List
        {
            get
            {
                var t = new Article[Count];
                var x = AsList();
                for (int i = x.Length - 1; i >= 0; i--)
                {
                    t[i] = (Article)(x[i].Value);
                    t[i].Order = i;
                }
                return t;

            }
        }
        public Articles(DataRow owner)
            : base(owner)
        {

        }
        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            (d as Database).GetFacture(c);
        }

        public Articles(Context c, JValue jv)
            : base(c, jv)
        {
        }

        public Articles(KeyValuePair<long, DataRow>[] arts, DataRow owner = null) : base(owner)
        {
            foreach (var p in arts)
                base[p.Key] = p.Value as Article;
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }

        protected override bool OnRowAdding(DataRow row)
        {
            base.OnRowAdding(row);
            var t = (Article)row;
            if (Owner is Database)
            {
                if (Database.IsUploading) return true;
                if (t.Facture == null)
                    throw new Exception("The Article must have an Owner befor setted in database");
                t.Facture.Articles.Add(t);
            }
            else if (t.Facture == null)
                t.Facture = Owner as Facture;
            return true;
        }

        protected override bool OnRowRemoving(DataRow old, DataRow value)
        {
            base.OnRowRemoving(old, value);
            var fct = (Article)old;
            if (Owner is Database)
            {
                fct.Facture.Articles.Remove(fct.Id);
                fct.Facture = null;
            }
            else if (Owner is Client)
                fct.Facture = null;
            OnRowAdding(value);
            return true;
        }
    }

    partial class Article
    {
        public bool SaveAndValidate_New(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            var factor = f.Factor;
            this.PSel = Product.PSel;
            if (args.Database.Save(this, false))
            {
                f.Articles.Add(this);
                args.Database.Articles.Add(this);
                Product.Qte -= Qte * factor;
                args.Database.Save(Product, true);
                f.NArticles++;
                if (updateFF)
                {

                    f.Total += Total * factor;
                    args.Database.Save(f, true);


                    f.Client.MontantTotal += Total * factor;
                    return args.Database.Save(f.Client, true);
                }
                else args.Database.StrictSave(f, true);
                return true;
            }
            else return args.SendError(CodeError.fatal_error, false);
        }

        public bool SaveAndReValidate_Old(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            var factor = f.Factor;
            var old = f.Articles[Id];
            var oldPrd = old.Product;
            var oldQte = old.Qte * factor;
            var oldTot = old.Total * factor;
            var qte = Qte * factor;
            var def = (Total - old.Total) * factor;

            if (old.Facture != Facture) return args.SendError("Fatal Error (Restart QShop)", false);
            //if (oldPrd == Product && def == 0) return args.SendSuccess();
            if (args.Database.Save(this, true))
            {
                oldPrd.Qte += oldQte;
                Product.Qte -= qte;
                args.Database.Save(Product, true);
                if (oldPrd != Product)
                    args.Database.Save(oldPrd, true);

                old.CopyFrom(this);

                if (updateFF)
                {
                    f.Total += def;
                    args.Database.Save(f, true);

                    f.Client.MontantTotal += def;
                    return args.Database.Save(f.Client, true);
                }
                return true;
            }
            else return args.SendError(CodeError.fatal_error, false);
        }

        public bool Save_New(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            PSel = Product.PSel;
            if (args.Database.Save(this, false))
            {
                f.Articles.Add(this);
                args.Database.Articles.Add(this);
                f.NArticles++;
                if (updateFF)
                {
                    f.Total += Total * Facture.Factor;
                    return args.Database.Save(f, true);
                }
                else args.Database.StrictSave(f, true);
            }
            else return args.SendError(CodeError.fatal_error, false);
            return true;
        }

        public bool Save_Old(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            if (args.Database.Save(this, true))
            {
                var old = f.Articles[Id];
                var def = (Total - old.Total) * Facture.Factor;
                old.CopyFrom(this);
                if (updateFF)
                {
                    f.Total += def;
                    return args.Database.Save(f, true);
                }
                return true;

            }
            else return args.SendError(CodeError.fatal_error, false);
        }

        public bool DeleteUnValidate(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            if (args.Database.Delete(this))
            {
                f.Articles.Remove(this);
                args.Database.Articles.Remove(this);
                f.NArticles--;
                if (updateFF)
                {
                    f.Total -= Total * f.Factor;
                    return args.Database.Save(f, true);
                }
                else args.Database.StrictSave(f, true);
            }
            return true;
        }

        public bool DeleteValidate(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            var factor = f.Factor;
            if (args.Database.Delete(this))
            {
                f.Articles.Remove(this);
                args.Database.Articles.Remove(this);
                f.NArticles--;
                if (updateFF)
                {
                    f.Total -= Total * factor;
                    args.Database.Save(f, true);

                    f.Client.MontantTotal -= Total * factor;
                    args.Database.Save(f.Client, true);
                }
                else args.Database.StrictSave(f, true);
                Product.Qte += Qte * factor;
                return args.Database.Save(Product, true);
            }
            return true;
        }

        public bool Save(RequestArgs args)
        {
            if (!Check(args, out var isNew)) return false;
            var fact = Facture;
            if (Facture == null) return args.SendError("La facture N'exist pas");
            if (!fact.IsAccessibleBy(args.User, false, out var msg)) return args.SendError(msg);
            if (fact.IsValidated)
                if (isNew)
                    return SaveAndValidate_New(args);
                else
                    return SaveAndReValidate_Old(args);
            else
                if (isNew)
                return Save_New(args);
            else
                return Save_Old(args);
        }

        public bool Delete(RequestArgs args)
        {
            //if (!Check(args, out var isNew)) return false;

            var fact = Facture;
            if (Facture == null) return args.SendError("La facture N'exist pas");
            if (args.Database.Articles[args.Id] == null)
                return fact.Articles.Remove(this) & false;
            if (fact.IsValidated)
                return DeleteValidate(args);
            else return DeleteUnValidate(args);
        }
    }
}