using System;
using Json;
using models;

namespace Server
{
    public class  QDataSerializer : Typeserializer
    {
        public QDataSerializer()
            : base("models.QData")
        {

        }
     
        public override JValue ToJson(Context c,object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            return jv == null ? new QData() : new QData(c,jv);
        }
        public override Object FromJson(Context c,JValue jv)
        {
            return jv == null ? new QData() : new QData(c,jv);
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