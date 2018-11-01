using System;
using Json;
using models;

namespace Server
{
    public class  PictureSerializer : DataRowTypeSerializer
    {
        public PictureSerializer()
            : base("models.Picture")
        {

        }

        public override bool CanBecreated => false;

        public override DataTable Table => throw new NotImplementedException();

        protected override JValue CreateItem(Context c, JValue jv)
        {
            throw new NotImplementedException();
        }

        //protected override JValue CreateItem(Context c, JValue jv)
        //{
        //    return new Picture(c, jv);
        //}
        //public override DataTable Table => data.Pictures;

        //public override JValue ToJson(Context c,object ov)
        //{
        //    return null;
        //}
        //public override void Stringify(Context c,object p)
        //{
        //    throw new NotImplementedException();
        //}

        //public override void SimulateStringify(Context c,object p)
        //{
        //    throw new NotImplementedException();
        //}
    }

    
}