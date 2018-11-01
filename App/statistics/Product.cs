using models;
using QServer.Core;
using Server;
using System;

namespace Statistics.Models
{
    class ProductSelledByClient:DObject
    {
        public static int DPArticleId = Register<ProductSelledByClient, long>("ArticleId");
        public long ArticleId { get => get<long>(DPArticleId); set => set(DPArticleId, value); }

        public static int DPQte = Register<ProductSelledByClient, float>("Qte");
        public float Qte { get => get<float>(DPQte); set => set(DPQte, value); }
    }
}
namespace Statistics
{
    [Service]
    public class FactureStat : Service
    {
        public FactureStat() : base("fstat")
        {
            
        }
        public override bool Get(RequestArgs args)
        {
            switch (args.Path[1])
            {
                default:
                    break;
            }
            return true;
        }
    }

    [Service]
    partial class ProductStat:Service
    {
        public ProductStat() : base("pstat")
        {
        }        
        public override bool Get(RequestArgs args)
        {
            switch (args.Path[1]?.ToLowerInvariant())
            {
                case "articlespurchased":
                    args.JContext.Add(typeof(models.Articles), new DataTableParameter(typeof(models.Articles)) { SerializeItemsAsId = false, FullyStringify = true });
                    args.Send(GetArticlesPurchased(args));
                    return true;
                case "lastarticlepurchased":
                    var c = GetLastArticlePurchasedByClient(args);
                    if (args.HasParam("price"))
                        args.Send(c?.Price);
                    else args.Send(c);
                    return true;
                case "articlessolded":
                    args.JContext.Add(typeof(FakePrices), new DataTableParameter(typeof(models.FakePrices)) { SerializeItemsAsId = false, FullyStringify = true });
                    args.Send(GetArticlesSolded(args));
                    return true;
                case "lastarticlesolded":
                    var v = GetLastArticleSoldedByFrn(args);
                    if (args.HasParam("price"))
                        args.Send(v?.PSel);
                    else args.Send(v);
                    return true;
                default:
                    return args.SendFail();
            }
        }
    }

    partial class ProductStat
    {
        public Articles GetArticlesPurchased(RequestArgs args)
        {
            var info = new Filters.Info();
            var art = (Article)null;
            if (!info.Init(args)) return new Articles(null);
            var ret = new Articles(info.Client);
            foreach (var kv in args.Database.Articles.AsList())
                    if (info.Check(art = (Article)kv.Value))
                        ret.Add(art);
            return ret;
        }
        public FakePrices GetArticlesSolded(RequestArgs args)
        {
            var ret = new FakePrices(null);
            var info = new Filters.Info();
            var art = (FakePrice)null;

            if (info.Init(args))
                foreach (var kv in args.Database.FakePrices.AsList())
                    if (info.Check(art = (FakePrice)kv.Value))
                        ret.Add(art);
            return ret;
        }
        
        public Article GetLastArticlePurchasedByClient(RequestArgs args)
        {
            Article ret = null;
            var info = new Filters.Info();
            if (info.Init(args))
                foreach (var kv in args.Database.Articles.AsList())
                {
                    var art = (Article)kv.Value;
                    if (info.Check(art))
                        if (ret == null) ret = art;
                        else if (ret.Facture.Date < art.Facture.Date) ret = art;
                }
            return ret;
        }
        
        private FakePrice GetLastArticleSoldedByFrn(RequestArgs args)
        {
            FakePrice ret = null;
            var info = new Filters.Info();
            if (info.Init(args))
                foreach (var kv in args.Database.FakePrices.AsList())
                {
                    var art = (FakePrice)kv.Value;
                    if (info.Check(art))
                        if (ret == null) ret = art;
                        else if (ret.Facture?.Date < art.Facture?.Date) ret = art;
                }
            return ret;
        }
    }

}
namespace Statistics.Filters
{
    public interface IFilter<T>
    {
        bool Init(RequestArgs args);
        bool Check(T data);
    }

    class Info : IFilter<Article>
    {
        public Product Product;
        public Client Client;
        private bool check0;
        private Fournisseur Fournisseur;

        public bool Check0(Article data) => (Product == null || data.Product == Product) && (Client == null || data.Facture?.Client == Client);
        public bool Check0(FakePrice data) => (Product == null || data.Product == Product) && (Fournisseur == null || data.Facture?.Fournisseur == Fournisseur);
        public bool Init0(RequestArgs args)
        {
            Product = args.Database.Products[args.Id];
            if (args.GetParam("CID", out long cid))
                Client = args.Database.Clients[cid];
            if (args.GetParam("FID", out cid))
                Fournisseur = args.Database.Fournisseurs[cid];

            return check0 = (Product != null || Client != null || Fournisseur != null);
        }


        public DateTime From;
        public DateTime To;
        public bool ByFacture;
        private bool check1;


        public bool Check1(Article article)
        {
            var t = ByFacture ? article.Facture?.Date ?? default : article.LastModified;
            return t.Ticks >= From.Ticks && t.Ticks < To.Ticks;
        }
        public bool Check1(FakePrice article)
        {
            var t = ByFacture ? article.Facture?.Date ?? default : article.LastModified;
            return t >= From && t <= To;
        }

        public bool Init1(RequestArgs args)
        {
            bool a, b;
            if (args.GetParam("Befor", out DateTime Before))
            {
                ByFacture = true;
                From = default;
                To = new DateTime(Before.Ticks - 1, DateTimeKind.Utc);
                return check1 = true;
            }
            if (!(a = args.GetParam("From", out  From))) From = DateTime.MinValue;
            if (!(b = args.GetParam("To", out  To))) To = DateTime.MaxValue;
            return check1 = a || b;
        }

        public bool Init(RequestArgs args) => Init0(args) | Init1(args);

        public bool Check(Article data) => (check0 && Check0(data)) && (check1 && Check1(data));

        public bool Check(FakePrice data) => (check0 && Check0(data)) && (check1 && Check1(data));
    }
}

