using System;
using Json;
using models;

namespace Server
{
    public class  PicturesSerializer : Typeserializer
    {
        public PicturesSerializer ()
            : base("models.Pictures")
        {

        }
        public override JValue ToJson(Context c,object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return new Pictures(c,jv);
        }
        public override Object FromJson(Context c,JValue jv)
        {
            return new Pictures(c,jv);
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