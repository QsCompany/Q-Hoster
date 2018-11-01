using Json;
using models;

namespace Server
{

    public class  FakePriceSerializer : DataRowTypeSerializer
    {
        public FakePriceSerializer()
            : base("models.FakePrice")
        {

        }

        public override bool CanBecreated => false;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return jv == null ? new FakePrice() : new FakePrice(c, jv);
        }
        public override DataTable Table => data.FakePrices;
    }

    public class  PriceSerializer : DataRowTypeSerializer
    {
        public PriceSerializer()
            : base("models.Price")
        {

        }
        public override bool CanBecreated => false;
        protected override JValue CreateItem(Context c, JValue jv) => jv == null ? new Price() : new Price(c, jv);
        public override DataTable Table => store;
        private static DataTable<Price> store = new Prices(null);
    }
    
}