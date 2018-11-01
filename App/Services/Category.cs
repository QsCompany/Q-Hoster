using Json;
using models;
using QServer.Core;
using Server;

namespace Api
{
    public class  Category : Service
    {
        public Category()
            : base("Category")
        {

        }
        private new static bool RequireNew(string n, JObject x) { return n == "models.Category"; }
        public override bool Post(RequestArgs args)
        {
            bool isNew;
            args.JContext.RequireNew = RequireNew;
            var cat = args.BodyAsJson as models.Category;
            if (cat == null) { return args.SendFail(); }
            if (!cat.Check(args, out isNew)) return false;
            var d = args.Database;

            if (d.Save(cat, !isNew))
            {
                if (isNew)
                    d.Categories.Add(cat);
                else d.Categories[cat.Id].CopyFrom(cat);
                return args.SendSuccess();
            }
            return args.SendError(CodeError.UknownError);
        }
        public override bool Delete(RequestArgs args)
        {

            var d = args.Database;
            var ocat = d.Categories[args.Id];
            if (ocat != null)
            {
                if (d.Delete(ocat))
                    d.Categories.Remove(ocat.Id);
                else { return args.SendError("UnExpected Error !!");  }
            }
            return args.SendSuccess();

        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
    }
}