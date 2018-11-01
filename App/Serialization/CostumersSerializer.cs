using System;
using Json;
using models;

namespace Server
{
    public class  CostumersSerializer : Typeserializer
    {
        public CostumersSerializer()
            : base("models.Costumers")
        {

        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new Costumers(c, jv);
        }
        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }

        public override Object FromJson(Context c, JValue jv)
        {
            return new Costumers(c, jv);
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