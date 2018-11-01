using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Versments), typeof(VersmentsSerializer))]
    public class  Versments : DataTable<Versment>
    {
        public Versments(DataRow owner)
            : base(owner)
        {

        }
        public Versments(Context c,JValue jv):base(c,jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }

        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            throw null;
        }                
        public float Total
        {
            get
            {
                float t = 0;
                foreach (var v in this.AsList())
                    t += ((Versment)v.Value).Montant;
                return t;
            }
        }
    }
    [QServer.Core.HosteableObject(typeof(Api.SVersments), null)]
    public class  SVersments : DataTable<SVersment>
    {
        public SVersments(DataRow owner)
            : base(owner)
        {

        }


        public SVersments(Context c, JValue jv)
            : base(c, jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }

        protected override void GetOwner(DataBaseStructure d, Path c) => ((Database)d).GetSFacture(c);

        public float Total
        {
            get
            {
                float t = 0;
                foreach (var v in this.AsList())
                    t += ((SVersment)v.Value).Montant;
                return t;
            }
        }
    }
}