using System;
using Json;
using models;

namespace Server
{
    public class  ArticlesSerializer : Typeserializer
    {
        public ArticlesSerializer ()
            : base("models.Articles")
        {

        }

        public override JValue Swap(Context c, JValue v, bool requireNew)
        {
            return new Articles(c,v);
        }

        public override JValue ToJson(Context c,object ov)
        {
            return null;
        }

        public override Object FromJson(Context c,JValue jv)
        {
            return new Articles(c,jv);
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