using System;
using Json;
using models;

namespace Server
{
    public class  ProductSerializer : DataRowTypeSerializer
    {
        public ProductSerializer()
            : base("models.Product")
        {

        }
        public override bool CanBecreated => false;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return jv == null ? new Product() : new Product(c, jv);
        }
        public override DataTable Table => data.Products;

        public override JValue ToJson(Context c, object ov)
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
    
}