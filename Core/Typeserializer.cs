using System;
using Json;

namespace Server
{
    abstract public class Typeserializer
    {
        
        public string Name;
        public Typeserializer(string name)
        {
            Name = name;
        }

        public virtual JValue ToJson(Context c, Object ov) { return ov as JValue; }
        public abstract Object FromJson(Context c, JValue jv);

        public abstract JValue Swap(Context c, JValue jv, bool requireNew);

        public abstract void Stringify(Context c, object p);

        public abstract void SimulateStringify(Context c, object p);

        protected static long getId(JValue v)
        {
            long id = 0L;
            if (v is JObject)
                v = ((JObject) v)["Id"];
            if (v is JNumber)
                id = (long)((JNumber)v).Value;
            return id;
        }
    }
}