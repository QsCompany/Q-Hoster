using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Agents), typeof(AgentsSerializer))]
    public class Agents : DataTable<Agent>
    {
        public Agents(DataRow owner)
            : base(owner)
        {

        }

        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            c.Owner.set(c.Property.Index, d);
        }

        public Agents(Context c, JValue jv) : base(c, jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }
        

        public override JValue CreateItem(long id)
        {
            return new Agent { Id = id };
        }
        
    }

}