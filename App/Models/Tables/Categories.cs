using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Categories), typeof(CategoriesSerializer))]
    public class  Categories : DataTable<Category>
    {
        public Categories(DataRow owner)
            : base(owner)
        {

        }
        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            c.Owner.set(c.Property.Index, d);
        }

        public Categories(Context c,JValue jv):base(c,jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }
        
    }
}