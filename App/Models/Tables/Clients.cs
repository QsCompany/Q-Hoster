using Json;
using Server;

namespace models
{
    class total
    {
        public float f;
        public float v;
        public float s => f - v;
        public bool set(Dealer c)
        {
            if (c.MontantTotal == this.f && this.v == c.VersmentTotal) return false;
            c.MontantTotal = f;
            c.VersmentTotal = v;
            return true;
        }
    }
    [QServer.Core.HosteableObject(typeof(Api.Clients), typeof(ClientsSerializer))]
    public class  Clients : DataTable<Client>
    {
        public Clients(DataRow owner):base(owner)
        {

        }
        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            ((Database)d).GetClient(c);
        }

        public Clients(Context c,JValue jv):base(c,jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }

        public override int Repaire(Database db)
        {

            var clients = new System.Collections.Generic.Dictionary<long, total>();
            {
                var vs = db.Versments.AsList();
                for (int i = vs.Length - 1; i >= 0; i--)
                {
                    var v = (Versment)vs[i].Value;
                    var cid = v.Client?.Id;
                    if (cid.HasValue)
                    {
                        if (!clients.TryGetValue(cid.Value, out var t)) clients.Add(cid.Value, t = new total());
                        t.v += v.Montant;
                    }
                }
            }
            {
                var vs1 = db.Factures.AsList();
                for (int i = vs1.Length - 1; i >= 0; i--)
                {
                    var v = (Facture)vs1[i].Value;
                    var cid = v.Client?.Id;
                    if (cid.HasValue)
                    {
                        if (!clients.TryGetValue(cid.Value, out var t)) clients.Add(cid.Value, t = new total());
                        t.f += v.Total;
                    }
                }
            }
            var ops = db.CreateOperations();
            foreach (var kv in clients)
            {
                var c = db.Clients[kv.Key];
                if (kv.Value.set(c)) ops.Add(SqlOperation.Update, c);
            }
            return (db.Save(ops) == true ? 0 : 1);
        }

    }
    [QServer.Core.HosteableObject(typeof(Api.Fournisseurs), typeof(FournisseursSerializer))]
    public class  Fournisseurs : DataTable<Fournisseur>
    {
        public Fournisseurs(DataRow owner) : base(owner) { }
        protected override void GetOwner(DataBaseStructure d, Path c) => ((Database)d).GetFournisseur(c);

        public Fournisseurs(Context c, JValue jv) : base(c, jv) { }
        public override int Repaire(Database db)
        {

            var fournisseurs = new System.Collections.Generic.Dictionary<long, total>();

            var vs = db.SVersments.AsList();
            for (int i = vs.Length - 1; i >= 0; i--)
            {
                var v = (SVersment)vs[i].Value;
                var cid = v.Fournisseur?.Id;
                if (cid.HasValue)
                {
                    if (!fournisseurs.TryGetValue(cid.Value, out var t)) fournisseurs.Add(cid.Value, t = new total());
                    t.v += v.Montant;
                }
            }

            var vs1 = db.SFactures.AsList();
            for (int i = vs1.Length - 1; i >= 0; i--)
            {
                var v = (SFacture)vs1[i].Value;
                var cid = v.Fournisseur?.Id;
                if (cid.HasValue)
                {
                    if (!fournisseurs.TryGetValue(cid.Value, out var t)) fournisseurs.Add(cid.Value, t = new total());
                    t.f += v.Total;
                }
            }

            var ops = db.CreateOperations();
            foreach (var kv in fournisseurs)
            {
                var c = db.Fournisseurs[kv.Key];
                if (kv.Value.set(c)) ops.Add(SqlOperation.Update, c);
            }
            return (db.Save(ops) == true ? 0 : 1);
        }
    }
}