using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Pictures), typeof(PicturesSerializer))]
    public class  Pictures : DataTable<Picture>
    {
        public Pictures(DataRow owner)
            : base(owner)
        {

        }

        public Pictures(Context c,JValue jv):base(c,jv)
        {
        }
        public override JValue Parse(JValue json)
        {
            return json;   
        }

        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            c.Owner.set(c.Property.Index, d);
        }
    }
}