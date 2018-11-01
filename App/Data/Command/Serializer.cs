using Json;
using models;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Serializers
{
    public class CommandSerializer : DataRowTypeSerializer
    {
        public CommandSerializer()
            : base("models.Command")
        {
        }

        public override bool CanBecreated => true;

        public override DataTable Table => data.Commands;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new Facture(c, jv);
        }
    }
    public class CommandsSerializer : Typeserializer
    {

        public CommandsSerializer()
            : base("models.Commands")
        {
        }
        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new models.Commands(c, jv);
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

    public class CArticleSerializer : DataRowTypeSerializer
    {
        public CArticleSerializer()
            : base("models.CArticle")
        {
        }

        public override bool CanBecreated => true;

        public override DataTable Table => data.CArticles;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new models. CArticle(c, jv);
        }
    }
    public class CArticlesSerializer : Typeserializer
    {

        public CArticlesSerializer()
            : base("models.CArticles")
        {
        }
        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new models.CArticles(c, jv);
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
