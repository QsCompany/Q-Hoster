using System;
using Json;
using models;

namespace Server
{
    public class  SignupSerializer : DataRowTypeSerializer
    {
        public SignupSerializer()
            : base("models.Signup")
        {

        }

        public override bool CanBecreated => true;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new Login(c, jv);
        }
        public override DataTable Table => null;

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