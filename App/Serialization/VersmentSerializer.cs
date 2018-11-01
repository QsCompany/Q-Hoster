using System;
using Json;
using models;

namespace Server
{
    public class  VersmentSerializer : DataRowTypeSerializer
    {
        public VersmentSerializer()
            : base("models.Versment")
        {

        }

        public override bool CanBecreated => false;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return jv == null ? new Versment() : new Versment(c, jv);
        }
        public override DataTable Table => data.Versments;

        public override JValue ToJson(Context c,object ov)
        {
            return null;
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
    public class  SVersmentSerializer : DataRowTypeSerializer
    {
        public SVersmentSerializer()
            : base("models.SVersment")
        {

        }

        public override bool CanBecreated => false;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return jv == null ? new SVersment() : new SVersment(c, jv);
        }
        public override DataTable Table => data.SVersments;

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