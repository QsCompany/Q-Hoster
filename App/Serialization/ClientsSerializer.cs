using System;
using Json;
using models;

namespace Server
{
    public class  ClientsSerializer : Typeserializer
    {
        public ClientsSerializer ()
            : base("models.Clients")
        {

        }
        public override JValue ToJson(Context c,object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new Clients(c,jv);
        }
        public override Object FromJson(Context c,JValue jv)
        {
            return new Clients(c,jv);
        }

        public override void Stringify(Context c,object p)
        {
            throw new NotImplementedException();
        }

        public override void SimulateStringify(Context c,object p)
        {
            throw new NotImplementedException();
        }
    }
    public class  FournisseursSerializer : Typeserializer
    {
        public FournisseursSerializer()
            : base("models.Fournisseurs")
        {

        }
        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new Clients(c, jv);
        }
        public override Object FromJson(Context c, JValue jv)
        {
            return new Clients(c, jv);
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