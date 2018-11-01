using Server;

namespace Api
{
    public class  Agents :TableService
    {
        public Agents ():base("Agents")
        {

        }
        public override bool Get(RequestArgs args)
        {
            args.Client.SetCookie(Name.ToLower() + "_lasttimeupdated", System.DateTime.Now, args.Server.ExpiredTime);
            args.JContext.Add(typeof(models.Agents), new DataTableParameter(typeof(models.Agents)) { SerializeItemsAsId = false });
            args.Send(args.Database.Agents);
            return true;
        }        
    }
}