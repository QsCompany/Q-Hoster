using Server;

namespace Api
{
    public class  Costumers : Service
    {
        public Costumers()
            : base("Costumers")
        {
        }
        public override bool Get(RequestArgs args)
        {
            args.JContext.ResetParameterSerialiers();
            args.JContext.Add(typeof(models.Client), new DObjectParameter(typeof(models.Client)) { DIsFrozen = !true, FullyStringify = true });
            args.JContext.Add(typeof(models.Clients), new DataTableParameter(typeof(models.Clients)) { SerializeItemsAsId = false, DIsFrozen = !true, FullyStringify = true });
            args.Client.SetCookie(Name.ToLower() + "_lasttimeupdated", System.DateTime.Now, args.Server.ExpiredTime);
            args.Send(args.Database.Clients);
            return true;
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }

        public override bool SUpdate(RequestArgs args)
        {
            args.JContext.Add(typeof(models.Client), new DObjectParameter(typeof(models.Client)) { DIsFrozen = !true, FullyStringify = true });
            args.JContext.Add(typeof(models.Clients), new DataTableParameter(typeof(models.Clients)) { SerializeItemsAsId = false, DIsFrozen = !true, FullyStringify = true });
            return args.Database.Clients.SendUpdates(args);
        }
    }
}