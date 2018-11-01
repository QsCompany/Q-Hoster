using System;
using Json;
using models;

namespace Server
{
    public class FactureSerializer : DataRowTypeSerializer
    {
        public FactureSerializer()
            : base("models.Facture")
        {
        }

        public override bool CanBecreated => true;

        public override DataTable Table => data.Factures;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new models.Facture(c, jv);
        }
    }
}


namespace Server
{
    public class FacturesSerializer : Typeserializer
    {
        public FacturesSerializer()
            : base("models.Factures")
        {
        }
        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new models.Factures(c, jv);
        }
        public override Object FromJson(Context c, JValue jv)
        {
            return new models.Factures(c, jv);
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