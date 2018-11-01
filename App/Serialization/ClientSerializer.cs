using System;
using Json;
using models;

namespace Server
{
    public class  ClientSerializer : DataRowTypeSerializer
    {
        public ClientSerializer()
            : base("models.Client")
        {

        }

        public override bool CanBecreated => true;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return jv == null ? new Client() : new Client(c, jv);
        }
        public override DataTable Table => data.Clients;

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
    public class  FournisseurSerializer : DataRowTypeSerializer
    {
        public FournisseurSerializer()
            : base("models.Fournisseur")
        {

        }

        public override bool CanBecreated => true;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return jv == null ? new Fournisseur  () : new Fournisseur(c, jv);
        }
        public override DataTable Table => data.Fournisseurs;

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
}