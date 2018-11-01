using System;
using Json;
using models;

namespace Server
{
    public class  AgentSerializer : DataRowTypeSerializer
    {
        public AgentSerializer()
            : base("models.Agent")
        {

        }

        public override bool CanBecreated => false;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return  new Agent(c,jv);
        }
        public override DataTable Table => data.Agents;

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