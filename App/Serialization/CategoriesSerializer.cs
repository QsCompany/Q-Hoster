using System;
using Json;
using models;

namespace Server
{
    public class  CategoriesSerializer : Typeserializer
    {
        public CategoriesSerializer ()
            : base("models.Categories")
        {

        }
        public override JValue ToJson(Context c,object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new Categories(c,jv);
        }
        public override Object FromJson(Context c,JValue jv)
        {
            return new Categories(c,jv);
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