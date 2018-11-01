using System;
using Json;
using models;

namespace Server
{
    public class  ArticleSerializer : DataRowTypeSerializer
    {
        public ArticleSerializer()
            : base("models.Article")
        {

        }

        public override bool CanBecreated => true;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new Article(c, jv);
        }
        public override DataTable Table => data.Articles;

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