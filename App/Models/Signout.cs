using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Signout),null)]
    public class  Signout : DataRow
    {
        public new static int __LOAD__(int dp) => DPUser;
        public static int DPUser = Register<Signout, Client>("User");
        private Signout()
        {

        }
        public Signout(Context c, JValue jv) : base(c, jv)
        {
        }
        public Client Client { get => get<Client>(DPUser);
            set => set(DPUser, value);
        }


        public override JValue Parse(JValue json)
        {
            return json;
        }
    }
}