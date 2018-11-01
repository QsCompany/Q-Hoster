using models;
using Server;

namespace Api
{
    public class  Prices : Service
    {
        public Prices()
            : base("Prices")
        {

        }
        public override bool Get(RequestArgs args)
        {
            var id = args.Id;
            if (id == -1)
            {
                if (args.User.IsAgent)
                {
                    args.JContext.Add(typeof(FakePrices), new DataTableParameter(typeof(FakePrices)) { SerializeItemsAsId = false, FullyStringify = true });
                    args.JContext.Add(typeof(models.FakePrice), new PriceParameter { Job= args.User.Client.Abonment, IsAdmin = args.User.IsAgent });
                    args.Send(args.Database.FakePrices);
                }
                return true;
            }

            args.JContext.Add(typeof(models. FakePrices), new DataTableParameter(typeof(models. FakePrices)) { SerializeItemsAsId = false });
            args.JContext.Add(typeof(models.FakePrice), new PriceParameter { Job = args.User.Client.Abonment, IsAdmin = args.User.IsAgent });
            var f = args.Database.SFactures[id];
            if (f == null) args.SendFail();
            else args.Send(f.Articles);
            return true;
        }

    }



}