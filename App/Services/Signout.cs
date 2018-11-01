using Server;

namespace Api
{
    public class  Signout : Service
    {
        public Signout()
            : base("Signout")
        {

        }
        public override bool Post(RequestArgs args)
        {
            return false;
        }

        public override void Put(RequestArgs args)
        {
        }
    }
}