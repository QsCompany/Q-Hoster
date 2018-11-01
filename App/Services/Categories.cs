using Server;

namespace Api
{
    public class Categories : TableService
    {
        public Categories() : base("Categories")
        {

        }
        public override bool Get(RequestArgs args)
        {
            args.Client.SetCookie(Name.ToLower() + "_lasttimeupdated", System.DateTime.Now, args.Server.ExpiredTime);
            args.JContext.Add(typeof(models.Categories), new DataTableParameter(typeof(models.Category)) { SerializeItemsAsId = false, FullyStringify = true });
            args.Send(args.Database.Categories);
            return true;
        }

        //public override bool SUpdate(RequestArgs args)
        //{
        //    if (args.User.IsAgent)
        //        return args.Database.Categories.SendUpdates(args);
        //    else return args.SendFail();
        //}
    }
}