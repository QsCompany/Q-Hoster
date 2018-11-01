namespace Server
{
    /*
    public class  PersonSerializer : DataRowTypeSerializer
    {
        public PersonSerializer()
            : base("models.Client")
        {

        }

        public override bool CanBecreated
        {
            get { return true; }
        }
        protected override models.JValue CreateItem(Context c, JValue jv)
        {
            return jv == null ? new models.Client() : new models.Client(c, jv);
        }
        public override models.DataTable Table
        {
            get { return data.Persons; }
        }

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
    }*/
}