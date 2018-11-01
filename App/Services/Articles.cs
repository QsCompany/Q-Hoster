using System.Collections.Generic;
using models;
using Server;

namespace Api
{
    public class  Articles :Service
    {
        public Articles ():base("Articles")
        {

        }
        public override bool Get(RequestArgs args)
        {
            var fid = args.Id;
            if (fid == -1)
            {
                var list_id = args.GetParam("list");
                if (list_id != null)
                {
                    var ids = list_id.Split(',');
                    var e = new models.Articles(args.Client);
                    foreach (var sid in ids)
                    {
                        var id = long.Parse(sid);
                        var art = args.Database.Articles[id];
                        if (art != null)
                            e.Add(art);
                    }
                    args.JContext.Reset();
                    args.JContext.Add(typeof(models.Articles), new DataTableParameter(typeof(models.Articles)) { SerializeItemsAsId = false, DIsFrozen = !true, ToType = typeof(DataTable) });
                    args.JContext.Add(typeof(models.Article), new ArticleParameter(typeof(models.Article), false) { DIsFrozen = !true });
                    args.JContext.Add(typeof(models.Product), new ArticleParameter(typeof(models.Product), false, true) { DIsFrozen = !true });

                    args.Send(e);
                    return true;
                }
                args.JContext.Add(typeof(models.Articles), new DataTableParameter(typeof(models.Articles)) { SerializeItemsAsId = false, DIsFrozen = true, ToType = typeof(DataTable) });
                args.JContext.Add(typeof(models.Article), new ArticleParameter(typeof(models.Article), true) { DIsFrozen = true });

                var client = args.User.Client;
                var list = new models.Articles(client);
                foreach (KeyValuePair<long, DataRow> facture in args.Database.Factures.AsList())
                    if (client.Id == ((models.Facture)facture.Value).Client?.Id)
                        foreach (var article in ((models.Facture)facture.Value).Articles.AsList())
                            list.Push((models.Article)article.Value);
                    
                args.Send(list);
                return true;
            }
            else
            {
                args.JContext.Add(typeof(models.Articles), new DataTableParameter(typeof(models.Articles)) { SerializeItemsAsId = false });
                args.JContext.Add(typeof(models.Article), new ArticleParameter(typeof(models.Article)));
                var f = args.Database.Factures[fid];
                if (f == null) args.SendFail();
                else
                {
                    if (!args.User.IsAgent && f.Client?.Id != args.Client.Id) args.SendFail();
                    else args.Send(f.Articles);
                }
                return true;
            }
            
        }


        public override bool SUpdate(RequestArgs args)
        {
            if (args.User.IsAgent)
                return args.Database.Articles.SendUpdates(args);
            else return args.SendFail();
        }
    }

    
    public class  FakePrices : Service
    {
        public FakePrices() : base(nameof(FakePrices))
        {

        }
        public override bool Get(RequestArgs args)
        {
            var fid = args.Id;
            if (fid == -1)
            {
                var list_id = args.GetParam("list");
                if (list_id != null)
                {
                    var ids = list_id.Split(',');
                    var e = new models.Articles(args.Client);
                    foreach (var sid in ids)
                    {
                        var id = long.Parse(sid);
                        var art = args.Database.Articles[id];
                        if (art != null)
                            e.Add(art);
                    }
                    args.JContext.Reset();
                    args.JContext.Add(typeof(models.Articles), new DataTableParameter(typeof(models.Articles)) { SerializeItemsAsId = false, DIsFrozen = !true, ToType = typeof(DataTable) });
                    args.JContext.Add(typeof(models.Article), new ArticleParameter(typeof(models.Article), false) { DIsFrozen = !true });
                    args.JContext.Add(typeof(models.Product), new ArticleParameter(typeof(models.Product), false, true) { DIsFrozen = !true });
                    args.Send(e);
                    return true;
                }
                args.JContext.Add(typeof(models.Articles), new DataTableParameter(typeof(models.Articles)) { SerializeItemsAsId = false, DIsFrozen = true, ToType = typeof(DataTable) });
                args.JContext.Add(typeof(models.Article), new ArticleParameter(typeof(models.Article), true) { DIsFrozen = true });
                //args.JContext.Add(typeof(models.Product), new DObjectParameter(true) { });
                var client = args.User.Client;
                var list = new models.Articles(client);
                foreach (KeyValuePair<long, DataRow> facture in args.Database.Factures.AsList())
                    foreach (var article in ((models.Facture)facture.Value).Articles.AsList())
                        list.Push((models.Article)article.Value);

                args.Send(list);
                return true;
            }
            var x = args.Database.SFactures[fid];
            if (x == null) args.SendFail();
            else
            args.Send(x.Articles); return true;
        }


        public override bool SUpdate(RequestArgs args)
        {
            if (args.User.IsAgent)
                return args.Database.Articles.SendUpdates(args);
            else return args.SendFail();
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
    }
}