using System;
using Json;

namespace Server
{
    public class  GuidSerializer : Typeserializer
    {
        public GuidSerializer()
            : base(typeof(Guid).FullName)
        {

        }
        public override JValue ToJson(Context c, object p)
        {
            return new JString(p == null ? "null" : p is Guid ? p.ToString() : "undefinned");
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return jv;
        }
        public override Object FromJson(Context c, JValue jv)
        {
            if (jv is JString) return Guid.Parse((JString)jv);
            return Guid.Empty;
        }

        public override void Stringify(Context c, object p)
        {
            c.GetBuilder().Append(p == null ? "null" : p is Guid ? p.ToString().ToUpperInvariant() : "undefinned");
        }

        public override void SimulateStringify(Context c, object p)
        {
        }
    }
}