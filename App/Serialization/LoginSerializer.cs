using System;
using Json;
using models;

namespace Server
{
    public class  LoginSerializer : DataRowTypeSerializer
    {
        public LoginSerializer()
            : base("models.Login")
        {

        }

        public override bool CanBecreated => true;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new Login(c, jv);
        }
        private static Logins l = new Logins(null);
        public override DataTable Table => l;

        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }

        public override void Stringify(Context c, object p)
        {
            throw new NotImplementedException();
        }

        public override void SimulateStringify(Context c, object p)
        {
            throw new NotImplementedException();
        }
    }
}