using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Json;

namespace models
{
    
    public class History : models.DataRow
    {
        public readonly static int DPQte = Register<History, float>("Qte", PropertyAttribute.None, null);
        public float Qte { get { return get<float>(DPQte); } set { set(DPQte, value); } }

        public readonly static int DPPSel = Register<History, float>("PSel", PropertyAttribute.None, null);
        public float PSel { get { return get<float>(DPPSel); } set { set(DPPSel, value); } }

        public readonly static int DPFournisseur = Register<History, Fournisseur>("Fournisseur", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetFournisseur(c));
        public Fournisseur Fournisseur { get { return get<Fournisseur>(DPFournisseur); } set { set(DPFournisseur, value); } }

        public readonly static int DPDate = Register<History, DateTime>("Date", PropertyAttribute.None, null);
        public DateTime Date { get { return get<DateTime>(DPDate); } set { set(DPDate, value); } }

        public readonly static int DPFactureId = Register<History, long>("FactureId", PropertyAttribute.None, null);

        public History()
        {

        }
        public History(FakePrice c)
        {
            Qte = c.Qte;
            PSel = c.PSel;
            FactureId = c.Facture.Id;
            //Fournisseur=
        }

        public long FactureId { get { return get<long>(DPFactureId); } set { set(DPFactureId, value); } }
    }

    public class Histories : DataTable<History>
    {
        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            
        }
        public Histories(Database database, long id) : base(null)
        {
            var f = database.FakePrices.AsList();
            for (int i = f.Length - 1; i >= 0; i--)
            {
                var c = (FakePrice)f[i].Value;
                if (c.Product?.Id == id) Add(new History(c));
            }
        }
        public Histories():base(null)
        {

        }
        protected Histories(Context c, JValue jv) : base(c, jv)
        {
        }

        protected Histories(DataRow owner) : base(owner)
        {
        }
    }
}
