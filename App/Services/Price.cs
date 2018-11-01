using Json;
using models;
using QServer.Core;
using Server;

namespace Api
{
    public class FakePrice : Service
    {
        public FakePrice()
            : base("FakePrice")
        {

        }
        public new static bool RequireNew(string s, JObject k) { return true; }
        public override bool Create(RequestArgs args)
        {
            var t = new models.FakePrice()
            {
                Id = models.DataRow.NewGuid(),
            };
            args.Send(t);
            return true;
        }
        public override bool Get(RequestArgs args)
        {
            if (!args.User.IsAgent) return false;
            var id = args.Id;
            if (id == -1) return false;
            args.JContext.Add(typeof(models.FakePrice), new PriceParameter { Job = args.User.Client.Abonment, IsAdmin = args.User.IsAgent });
            args.Send(args.Database.FakePrices[id]);
            return true;
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
            //var c = MemoryMappedFile.CreateOrOpen("test", 1200, MemoryMappedFileAccess.ReadWriteExecute);
        }
        public override bool Post(RequestArgs args)
        {
            if (!args.User.IsAgent) return false;
            args.JContext.RequireNew = RequireNew;
            var e = args.BodyAsJson as models.FakePrice;
            if (e == null) return args.SendError("Connection Intercepted");
            return e.Save(args);
        }
        public override bool Delete(RequestArgs args)
        {
            var art = args.Database.FakePrices[args.Id];
            if (art == null) return args.SendError("L'Article N'Exist pas");
            var f = art.Facture;
            if (f?.IsLocked(args) == false)
                return art.Delete(args);
            else return args.SendError($"La facture est opened by {args.Client.Name ?? args.Client.FullName}");
        }
    }
    
    public class  Price : Service
    {
        public Price()
            : base("Price")
        {
        }
        public new static bool RequireNew(string s, JObject k) => true;

        public override bool CheckAccess(RequestArgs args) => args.User.IsAgent;

        public override bool Post(RequestArgs args)
        {
            args.JContext.RequireNew = RequireNew;
            var price = args.BodyAsJson as models.FakePrice;
            if (price == null) return args.SendFail();
            var prd = price.Product;
            if (prd == null) args.SendError("This Product is not exist", false);
            price.Qte = 0;
            if (!price.CheckPrices(args)) return false;
            
            var deb = models.Price.DPPSel;
            var fn = models.Price.DPWValue;
            var x = prd.SaveStat();
            for (int i = deb; i <= fn; i++)
                prd.set(i, price.get<object>(i));
            if (!args.Database.Save(prd, true))
            {
                prd.Restore(x);
                return args.SendError(CodeError.fatal_error);
            }
            x = null;
            args.Send(prd);
            return true;
        }
    }
}