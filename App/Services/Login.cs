using models;
using Server;

namespace Api
{
    public class  Login : Service
    {
        public static bool IPost(RequestArgs args,models.Login e)
        {
            if(args.GetParam("validate",out string val))
                return args.Server.ValidateUser(args, e);

            if (args.GetParam("remove",out val))
                return args.Server.DeleteUser(args, e);

            if (args.GetParam("lock", out val))
                return args.Server.LockUser(args, e);


            return args.SendFail();
        }
        public Login()
            : base("Login")
        {
        }
        public override bool Post(RequestArgs args)
        {
            args.JContext.RequireNew = (ef, o) => true;
            var e = args.BodyAsJson as models.Login;
            return IPost(args, e);
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
        public override bool Get(RequestArgs args)
        {
            var id = args.Id;
            if (id == -1) return args.SendFail();
            var f = args.Database.Logins[id];
            if (f == null) return args.SendFail();
            else
                args.Send(f);
            return true;
        }
    }
}