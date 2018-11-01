using models;
using Server;
using System;

namespace Api
{
    public class Article : Service
    {
        public Article()
            : base("Article")
        {

        }
        public override bool Get(RequestArgs args)
        {
            args.JContext.Add(typeof(models.Clients), new DObjectParameter(typeof(models.Clients)) { DIsFrozen = true });
            var id = args.GetParam("Id");
            if (id != null)
            {
                args.Send(args.Database.Articles[long.Parse(id)]);
                return true;
            }
            var fid = args.GetParam("Path");
            if (fid == null) return false;
            var path = fid.Split('/');
            if (path.Length != 2) return false;
            var f = long.Parse(path[0]);
            var a = long.Parse(path[1]); return true;
        }
        public override bool Create(RequestArgs args)
        {
            var c = new models.Article { Id = DataRow.NewGuid() };
            args.Send(c);
            return true;
        }
        public override bool Delete(RequestArgs args)
        {
            var art = args.Database.Articles[args.Id];
            if (art == null) return args.SendError("Argument null exception");
            var f = art.Facture;
            if (f?.IsLocked(args) == false)
                return art.Delete(args);
            else return args.SendError($"La facture est opened by {args.Client.Name ?? args.Client.FullName}");
        }
        public override bool Post(RequestArgs args)
        {
            var art = args.BodyAsJson as models.Article;
            if (art == null) return args.SendError("Argument null exception");
            var f = art.Facture;
            if (f == null) return args.SendError("La Facture est Suprimer");
            if (f.LockedBy == null) return args.SendError("La Facture est fermer ,Ouvert la");
            if (f.IsLocked(args) == false)
                return art.Save(args);
            else return args.SendError($"La facture est opened by {f.LockedBy.Name ?? f.LockedBy.FullName}");
        }
        public override bool CheckAccess(RequestArgs args)
        {
            var c = args.Client;
            args.JContext.RequireNew = RequireNew;
            var art = args.BodyAsJson as models.Article;
            return true;
        }

        new static bool RequireNew(string x, Json.JObject v)
        {
            return x == "models.Article";
        }
    }
}