using Json;
using Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using models;

namespace QServer.Serialization
{
    public class TicketSerializer : DataRowTypeSerializer
    {
        public TicketSerializer()
            : base("models.Ticket")
        {

        }

        public override bool CanBecreated => false;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new Ticket(c, jv);
        }
        public override models.DataTable Table => null;

        public override JValue ToJson(Context c, object ov)
        {
            return null;
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
    public class TicketsSerializers : Typeserializer
    {
        public TicketsSerializers()
            : base("models.Tickets")
        {
        }
        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }

        public override Object FromJson(Context c, JValue jv)
        {
            return new Tickets(c, jv);
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew) => new Tickets(c, jv);

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
