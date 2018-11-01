using System;
using Json;
using models;
using Server;
namespace Serializers
{
    public class SFactureSerializer : DataRowTypeSerializer
    {
        public SFactureSerializer()
            : base("models.SFacture")
        {

        }
        public override bool CanBecreated => false;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return jv == null ? new SFacture() : new SFacture(c, jv);
        }
        public override DataTable Table => data.SFactures;

        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }
    }
    public class SFacturesSerializer : Typeserializer
    {
        public SFacturesSerializer()
            : base("models.SFactures")
        {

        }
        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new SFactures(c, jv);
        }
        public override Object FromJson(Context c, JValue jv)
        {
            return new SFactures(c, jv);
        }

        public override void Stringify(Context c, object p)
        {
            throw new NotImplementedException();
        }

        public override void SimulateStringify(Context c, object p)
        {

        }
    }
    public class FakePricesSerializer : Typeserializer
    {
        public FakePricesSerializer()
            : base("models.FakePrices")
        {

        }
        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new FakePrices(c, jv);
        }
        public override Object FromJson(Context c, JValue jv)
        {
            return new FakePrices(c, jv);
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