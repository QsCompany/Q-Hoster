using Json;
using Server;

namespace models
{
    public class  Logins : DataTable<Login>
    {
        public Logins(DataRow owner)
            : base(owner)
        {
        }

        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            c.Owner.set(c.Property.Index, d);
        }
        public Logins(Context c, JValue jv)
            : base(c, jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }

        
        public new Login this[string username]
        {
            get
            {
                var lst = AsList();
                for (int i = 0; i < lst.Length; i++)
                {
                    var v = (Login)lst[i].Value;
                    if (v.Username == username) return v;
                }
                return null;
            }
        }
    }
}