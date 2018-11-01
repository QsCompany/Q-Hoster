using QServer.Core;
using Server;

namespace Api
{
    public class  Clients :TableService
    {
        public Clients ():base("Clients")
        {

        }
        public override bool Get(RequestArgs args)
        {
            if (!args.User.IsAgent) return args.SendError(CodeError.access_restricted);
            args.JContext.Add(typeof(models.Clients), new DataTableParameter(typeof(models.Clients)) { SerializeItemsAsId = false });
            args.Client.SetCookie(Name.ToLower() + "_lasttimeupdated", System.DateTime.Now, args.Server.ExpiredTime);
            args.Send(args.Database.Clients);
            return true;
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
    }

    
    public class  Fournisseurs : Service
    {
        public Fournisseurs()
            : base("Fournisseurs")
        {
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
        public override bool Get(RequestArgs args)
        {            
            args.JContext.Add(typeof(models.Fournisseurs), new DataTableParameter(typeof(models.Fournisseurs)) { SerializeItemsAsId = false });
            args.Client.SetCookie(Name.ToLower() + "_lasttimeupdated", System.DateTime.Now, args.Server.ExpiredTime);
            args.Send(args.Database.Fournisseurs);
            return true;
        }

        public override bool SUpdate(RequestArgs args)
        {
            return args.Database.Fournisseurs.SendUpdates(args);
        }
    }
}

/*
 * 
 * 
 */
