using System;
using Json;
using models;

namespace Server
{
    abstract public class DataRowTypeSerializer:Typeserializer
    {
        public DataRowTypeSerializer(string name)
            : base(name)
        {
        }

        public static Database data => Database.__Default;
        public abstract DataTable Table { get; }
        public abstract bool CanBecreated { get; }
        protected abstract JValue CreateItem(Context c, JValue jv);

        public override Object FromJson(Context c, JValue jv)
        {
            var id = getId(jv);
            var t = Table[id];
            if (!c.IsLocal && !CanBecreated)
                return t;
            return t ?? CreateItem(c, jv);
        }

        public override JValue Swap(Context c, JValue jv,bool requireNew)
        {
            if (c.store.TryGetValue(jv, out var ret)) return ret;
            if (requireNew) return CreateItem(c, jv);
            var id = getId(jv);
            var tbl = Table;
            JValue t = null;
            if (tbl != null)
                t = tbl[id];
            t= t ?? CreateItem(c, jv);
            return t;
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
