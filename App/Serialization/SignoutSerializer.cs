namespace Server
{/*
    public class  SignoutSerializer : DataRowTypeSerializer
    {
        public SignoutSerializer()
            : base("models.Signout")
        {
        }

        public override bool CanBecreated
        {
            get { return true; }
        }
        protected override models.JValue CreateItem(Context c, JValue jv)
        {
            return new models.Signout(c, jv);
        }
        public override models.DataTable Table
        {
            get { return data.Signouts; }
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
}