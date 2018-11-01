using System;
using Json;
using models;

namespace Server
{
    public class  JobSerializer : Typeserializer
    {
        public JobSerializer()
            : base(typeof(Job).FullName)
        {

        }
        public override JValue ToJson(Context c, object p)
        {
            return new JString(p == null ? "null" : p is Guid ? p.ToString().ToUpperInvariant() : "undefinned");
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return jv;
        }
        public override Object FromJson(Context c, JValue jv)
        {
            if (jv is JString) return Enum.Parse(typeof(Job), (JString)jv);
            if (jv is JNumber) return (Job)((int)((JNumber)jv).Value);
            return Job.Detaillant;
        }

        public override void Stringify(Context c, object p)
        {
            var r = p == null ? -1 : (int)(Job)p;
            c.GetBuilder().Append(r);
        }

        public override void SimulateStringify(Context c, object p)
        {
        }
    }
    public class  AbonmentSerializer : Typeserializer
    {
        public AbonmentSerializer()
            : base(typeof(Abonment).FullName)
        {

        }
        public override JValue ToJson(Context c, object p)
        {
            return new JString(p == null ? "null" : p is Guid ? p.ToString().ToUpperInvariant() : "undefinned");
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return jv;
        }
        public override Object FromJson(Context c, JValue jv)
        {
            if (jv is JString) return Enum.Parse(typeof(Abonment), (JString)jv);
            if (jv is JNumber) return (Abonment)((int)((JNumber)jv).Value);
            return Abonment.Detaillant;
        }

        public override void Stringify(Context c, object p)
        {
            var r = p == null ? (int)Abonment.Detaillant : (int)(Abonment)p;
            c.GetBuilder().Append(r);
        }

        public override void SimulateStringify(Context c, object p)
        {
        }
    }
}