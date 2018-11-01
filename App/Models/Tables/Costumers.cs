using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Costumers), typeof(CostumersSerializer))]
    public class  Costumers : Clients
    {
        public Costumers(DataRow owner)
            : base(owner)
        {
        }


        public Costumers(Context c, JValue jv)
            : base(c, jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }

        

        
    }
}