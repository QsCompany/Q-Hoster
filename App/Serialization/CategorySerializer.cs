using System;
using Json;
using models;

namespace Server
{
    public class  CategorySerializer : DataRowTypeSerializer
    {
        public CategorySerializer()
            : base("models.Category")
        {

        }


        public override bool CanBecreated => false;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new Category(c, jv);
        }
        public override DataTable Table => data.Categories;

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
}