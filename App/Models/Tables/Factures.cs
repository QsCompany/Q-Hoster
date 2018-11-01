using System;
using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Factures), typeof(FacturesSerializer))]
    public class  Factures : DataTable<Facture>
    {
        public Factures(DataRow owner)
            : base(owner)
        {

        }
        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            ((Database)d).GetClient(c);
        }

        public Factures(Context c, JValue jv) : base(c, jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }
        //protected override bool OnRowAdding(DataRow row)
        //{
        //    base.OnRowAdding(row);
        //    var t = (Facture)row;
        //    var d = Owner as Database;
        //    if (d != null)
        //        if (t.Owner == null)
        //            if (Database.IsUploading) return true;
        //            else throw new Exception("The Person must have an Owner befor setted in database");
        //        else t.Owner.Factures.Add(t);
        //    else if (t.Owner == null)
        //        t.Owner = Owner as Client;
        //    else return t.Owner == Owner;
        //    return true;
        //}


        //protected override bool OnRowRemoving(DataRow old, DataRow value)
        //{
        //    base.OnRowRemoving(old, value);
        //    var fct = (Facture)old;
        //    if (Owner is Database)
        //    {
        //        fct.Owner.Factures.Remove(fct);
        //        fct.Owner = null;
        //    }
        //    else if (Owner is Client)
        //        fct.Owner = null;
        //    OnRowAdding(value);
        //    return true;
        //}

    }
}