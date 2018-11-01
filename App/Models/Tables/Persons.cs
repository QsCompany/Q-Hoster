namespace models
{
    /*
    public class  Persons : DataTable<Person>
    {
        public Persons(DataRow owner)
            : base(owner)
        {
            Clients
            if (!(owner is Client || owner is Database)) throw new System.Exception("False Owner");
        }

        public Persons(Context c,JValue jv):base(c,jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }

        protected override bool OnRowAdding(DataRow row)
        {
            var t = (Person)row;
            if (Owner is Database)
            {
                if (t.Owner == null)
                    if (Database.IsUploading) return true;
                    else
                        throw new System.Exception("The Person must have an Owner befor setted in database");
                t.Owner.Costumers.Add(t);
            }
            else if (t.Owner == null)
                t.Owner = Owner as Client;
            else return t.Owner == Owner;
            return true;
        }
        protected override bool OnRowRemoving(DataRow old, DataRow value)
        {

            var fct = (Person)old;
            if (Owner is Database)
            {
                fct.Owner.Costumers.Remove(fct);
                fct.Owner = null;
            }
            else if (Owner is Client)
                fct.Owner = null;
            OnRowAdding(value);
            return true;
        }
    }*/
}