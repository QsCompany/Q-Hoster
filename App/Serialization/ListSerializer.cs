using System;
using Json;

namespace Server
{
    public class  ListSerializer:Typeserializer
    {
        public ListSerializer()
            : base("sys.List")
        {
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return jv is JArray ? jv : (jv as JObject)["__list__"];
        }
        public override JValue ToJson(Context c,object ov)
        {
            return null;
        }

        public override object FromJson(Context c,JValue jv)
        {
            return jv is JArray ? jv : (jv as JObject)["__list__"];
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
}