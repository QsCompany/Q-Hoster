using Json;
using models;
using Server;
namespace models
{
    public class Messages : DataTable<Message>
    {
        public Messages(DataRow owner)
            : base(owner)
        {

        }
        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            ((Database)d).GetClient(c);
        }

        public Messages(Context c, JValue jv)
            : base(c, jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }
    }
}
namespace QServer.Serialization
{
    /*
    public class  MessageSerializer : DataRowTypeSerializer
    {
        public MessageSerializer()
            : base("models.Message")
        {

        }

        public override bool CanBecreated
        {
            get { return false; }
        }
        protected override models.JValue CreateItem(Context c, JValue jv)
        {
            return new models.Message(c, jv);
        }
        public override models.DataTable Table
        {
            get { return data.Pictures; }
        }
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
    */


    public class  MessageSerializer : DataRowTypeSerializer
    {
        private static DataTable<Message> messages = new Messages(null);
        public override DataTable Table => messages;

        public override bool CanBecreated => true;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new Message(c, jv);
        }

        public override JValue ToJson(Context c, object ov)
        {
            return ov as Message;
        }

        public override void Stringify(Context c, object p)
        {
            (p as Message).Stringify(c);
        }

        public override void SimulateStringify(Context c, object p)
        {
            (p as Message).SimulateStringify(c);
        }

        public MessageSerializer()
            : base("models.Message")
        {
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew)
        {
            var id = getId(jv);
            Message msg = messages[id];
            if (id == -1) return null;
            if (msg!=null)
            {
                DObject.FromJson(msg, c, jv);
                return msg;
            }
            return new Message(c, jv);
            //return base.Swap(c, jv, requireNew);
        }

        public static void Register(Message msg)
        {
            messages[msg.Id] = msg;
        }

        public static Message UnRegister(long id)
        {
            return messages.Remove(id);
        }
        public static Message GetRegistration(long id)
        {
            return messages[id];
        }
    }
    /*
    public class  MessageSerializer : DataRowTypeSerializer
    {
        public MessageSerializer()
            : base("Common.Message")
        {
        }
        public override Json.JValue ToJson(Context c, object ov)
        {
            return (models.Message) ov;
        }

        private static Dictionary<Guid, models.Message> m = new Dictionary<Guid, models.Message>();

        public static void Register(models. Message msg)
        {
            if (m.ContainsKey(msg.Id)) m[msg.Id] = msg;
            else m.Add(msg.Id, msg);
        }

        public static models.Message UnRegister(Guid id)
        {
            models.Message msg;
            if (m.TryGetValue(id,out msg)) m.Remove(id);
            return msg;
        }

        public override object FromJson(Context c, Json.JValue jv)
        {
            return Swap(c, jv, false);
        }
        public override Json.JValue Swap(Context c, Json.JValue jv, bool requireNew)
        {
            var id = getId(jv);
            models.Message msg;
            if (id == null) return null;
            if (m.TryGetValue(id, out msg))
            {
                msg.FromJson(c, jv);
                return msg;
            }
            return new models.Message(c, jv);
        }
        public override void Stringify(Context c, object p)
        {
            (p as models.Message).Stringify(c);
        }

        public override void SimulateStringify(Context c, object p)
        {
            (p as models.Message).SimulateStringify(c);
        }
    }
    */
}