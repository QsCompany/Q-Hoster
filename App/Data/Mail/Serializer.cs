using Server;
using Json;
using models;
using System;

namespace Serializers
{
    public class MailSerializer : DataRowTypeSerializer
    {
        public MailSerializer() : base("models.Mail")
        {
        }

        public override DataTable Table => null;

        public override bool CanBecreated => true;

        protected override JValue CreateItem(Context c, JValue jv) => new models.Mail(c, jv);
    }
    public class MailsSerializer : Typeserializer
    {
        public MailsSerializer()
            : base("models.Mails")
        {
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew) => new models.Factures(c, jv);
        public override Object FromJson(Context c, JValue jv) => new Mails(c, jv);

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
