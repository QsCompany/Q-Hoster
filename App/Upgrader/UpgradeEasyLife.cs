using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using models;
using System.Windows.Forms;

namespace Shop
{
    public static class Electricite
    {
        public class Resistivite
        {
            public const float Cuivre = 0.01786f;
            public const float Aluminium = Cuivre * 0.59f;
        }
        public static float biasTension = 3;
        public static float LongueurCableRequis(float puissance,float longueurCable)
        {
            return Resistivite.Cuivre * 2 * longueurCable * puissance / (220 * 3 / 100) * 220;
        }
    }
}
#if DEBUG
namespace QHostTrans
{
    public partial class PME
    {
        models.Database db ;
        public Csv Produits;
        public Csv Fournis;
        public Csv Famille;
        //public Csv Factp2;
        public Csv Client;
        public Csv Bon2;
        public Csv Bon1;
        public Csv Bon_a2;
        public Csv Bon_a1;
        public Csv AgentGPC;

        public Csv Carnet_C;
        public Csv Carnet_F;

        public void Transf_Familles()
        {
            if (Famille.Count == 0 || Famille.ColumnCount == 0) return;
            var rid = Famille.ColumnIndex("RecordId");
            var rnm = Famille.ColumnIndex("FAMILLE");
            for (Famille.Reset(); Famille.Next();)
            {
                var x = Famille.Current;
                var ops = db.Categories.Get(long.Parse(x[rid]), true) as models.Category;
                ops.Name = x[rnm];
                if (!db.Save(ops, false)) Stop(null);
            }
        }
        

        public void Transf_Produits()
        {
            Produits.Reset();
            if (Produits.Count == 0 || Produits.ColumnCount == 0) return;
            var dti = Produits.ColumnIndex("DATE_INI");
            var rid = Produits.ColumnIndex("RecordId");
            var rp = Produits.ColumnIndex("PRODUIT");
            var rf = Produits.ColumnIndex("FAMILLE");
            var rcb = Produits.ColumnIndex("CODE_BARRE");
            var rd = Produits.ColumnIndex("DETAIL");
            var rpd = Produits.ColumnIndex("PRIX_D");
            var rpg = Produits.ColumnIndex("PRIX_G");
            var rpa = Produits.ColumnIndex("PRIX_A");
            var rs = Produits.ColumnIndex("STOCK");
            var rtp = Produits.ColumnIndex("TYPE_PRODUIT");
            var rt = Produits.ColumnIndex("TVA");
            var ops = db.CreateOperations();
            for (Produits.Reset(); Produits.Next();)
            {
                var x = Produits.Current;
                var id = (long)ParseFloat(x[rid]);
                var prod = db.Products[id];
                var update = prod != null;
                prod = db.Products.Get(id, true) as models.Product;
                prod.Name = Produits.Current[rp];
                prod.Category = GetFamile(Produits.Current[rf]);
                prod.Codebare = Produits.Current[rcb];
                prod.Description = Produits.Current[rd];
                prod.DPrice = ParseFloat(Produits.Current[rpd]);
                prod.PPrice = ParseFloat(Produits.Current[rpg]);
                prod.PSel = ParseFloat(Produits.Current[rpa]);
                prod.Qte = ParseFloat(Produits.Current[rs]);
                prod.Sockable = Produits.Current[rtp] == "P";
                prod.TVA = ParseFloat(Produits.Current[rt]);
                prod.Picture = getFile(prod.Codebare);
                
                ops.Add(update ? SqlOperation.Update : SqlOperation.Insert, prod);
                if (ops.Length == 100)
                {
                    if (db.Save(ops) != true) Stop(ops);
                    ops = db.CreateOperations();
                }
            }
            if (db.Save(ops) != true) Stop(ops);
        }
        public static string getFile(string s)
        {
            string m = string.Join("-", s.Split(System.IO.Path.GetInvalidFileNameChars())) + ".jpg";
            var f = new FileInfo(@".\PHOTO\" + m);
            if (!f.Exists) return null;
            return m;
        }
        public void Transf_Agent()
        {
            if (AgentGPC.Count == 0 || AgentGPC.ColumnCount == 0) return;
            var rid = AgentGPC.ColumnIndex("RecordId");
            AgentGPC.Reset();
            while (AgentGPC.Next())
            {
                var x = AgentGPC.Current;
                var id = (long)ParseFloat(x[rid]);
                //if (db.Agents[id] != null) continue;
                var agent = db.Agents.Get(id, true) as Agent;
                agent.Name = AgentGPC["POSTE"];
                agent.Username = "@" + agent.Name;
                agent.Pwd = agent.Name;
                if (!db.Save(agent, false))
                {
                }
            }
        }
        public void Transf_Fournisseur()
        {
            if (Fournis.Count == 0 || Fournis.ColumnCount == 0) return;
            var rid = Fournis.ColumnIndex("RecordId");
            Fournis.Reset();
            while (Fournis.Next())
            {
                var x = Fournis.Current;
                var id = (long)ParseFloat(x[rid]);
                //if (db.Fournisseurs[id] != null) continue;
                var fourn = db.Fournisseurs.Get(id, true) as models.Fournisseur;
                fourn.Name = Fournis["FOURNIS"];
                fourn.Address = Fournis["ADRESSE"];
                fourn.Tel = Fournis["TEL"];
                fourn.MontantTotal = ParseFloat(Fournis["MONTANT"]);
                fourn.VersmentTotal = ParseFloat(Fournis["VERSER"]);
                fourn.Ville = Fournis["COMMUNE"];
                fourn.CodePostal = Fournis["CODE_POSTAL"];
                fourn.Email = Fournis["EMAIL"];
                fourn.Mobile = Fournis["TEL"];
                fourn.Observation = Fournis["NOTES"];
                if (!db.Save(fourn, false))
                {
                }
            }
        }
        public void Transf_Clients()
        {
            db.Save(server.Admin, false);
            
            if (Client.Count == 0 || Client.ColumnCount == 0) return;
            var rid = Client.ColumnIndex("RecordId");
            Client.Reset();
            var ops = db.CreateOperations();
            var code_client = Client.ColumnIndex("Code_Client");
            var code_id = Client.ColumnIndex("RecordId");
            var ref_client= Client.ColumnIndex("Ref_Client");
            var client_name = Client.ColumnIndex("Client");
            var client_addr = Client.ColumnIndex("ADRESSE");
            var client_tel = Client.ColumnIndex("TEL");
            var client_mantant = Client.ColumnIndex("MONTANT");
            var client_versement = Client.ColumnIndex("VERSER");

            var Ville = Client.ColumnIndex("COMMUNE");
            var CodePostal = Client.ColumnIndex("CODE_POSTAL");
            var Email = Client.ColumnIndex("EMAIL");
            var Mobile = Client.ColumnIndex("TEL");
            var Observation = Client.ColumnIndex("NOTES");

            while (Client.Next())
            {
                var x = Client.Current;
                var id = ParseLong(x[rid]);
                var client = db.Clients[id];
                var update = client != null;
                client = client ?? db.Clients.Get(id, true) as Client;
                client.Ref = x[code_client];
                client.FirstName = x[client_name];
                client.Name = x[client_name];
                client.Address = x[client_addr];
                client.Tel = x[client_tel];
                client.MontantTotal = ParseFloat(x[client_mantant]);
                client.VersmentTotal = ParseFloat(x[client_versement]);
                client.Ville = x[Ville];
                client.CodePostal = x[CodePostal];
                client.Email = x[Email];
                client.Mobile = x[Mobile];
                client.Observation = x[Observation];

                ops.Add(update ? SqlOperation.Update : SqlOperation.Insert, client);
                if (ops.Length == 100)
                {
                    if (db.Save(ops) != true) Stop(ops);
                    ops = db.CreateOperations();
                }
            }
            if (db.Save(ops) != true) Stop(ops);
        }


        public void Transf_SFacture()
        {
            if (Bon_a1.Count == 0 || Bon_a1.ColumnCount == 0) return;
            var rid = Bon_a1.ColumnIndex("RecordId");
            var rnb = Bon_a1.ColumnIndex("NUM_BON");
            var rcf = Bon_a1.ColumnIndex("CODE_FRS");
            var rf = Bon_a1.ColumnIndex("FOURNIS");
            var rd = Bon_a1.ColumnIndex("DATE");
            var rm = Bon_a1.ColumnIndex("MONTANT");
            var rnp = Bon_a1.ColumnIndex("NBR_P");
            var ru = Bon_a1.ColumnIndex("UTILISATEUR");

            var ops = db.CreateOperations();
            Bon_a1.Reset();
            while (Bon_a1.Next())
            {
                var x = Bon_a1.Current;
                long.TryParse(x[rid], out var id);
                var update = db.SFactures[id] != null;
                var bon = db.SFactures.Get(id, true) as models.SFacture;
                bon.Ref = "TBA" + x[rnb];
                bon.Fournisseur = GetFournisseur(x[rcf], x[rf]);
                bon.Date = ParseDate(x[rd]);
                bon.Total = ParseFloat(x[rm]);
                bon.NArticles = (int)ParseFloat(x[rnp]);
                bon.Achteur = GetAgent(x[ru]);
                bon.SetFactureType(BonType.Bon, TransactionType.Vente);

                ops.Add(update ? SqlOperation.Update : SqlOperation.Insert, bon);
                if (ops.Length == 100)
                {
                    if (db.Save(ops) != true) Stop(ops);
                    ops = db.CreateOperations();
                }
            }
            if (db.Save(ops) != true) Stop(ops);
        }
        public void Transf_SArticles()
        {
            if (Bon_a2.Count == 0 || Bon_a2.ColumnCount == 0) return;
            var rid = Bon_a2.ColumnIndex("RecordId");
            var rnb = Bon_a2.ColumnIndex("NUM_BON");
            var rpa = Bon_a2.ColumnIndex("PRIX_ACHAT");
            var rq = Bon_a2.ColumnIndex("QTE");
            var rp = Bon_a2.ColumnIndex("PRODUIT");
            var rcb = Bon_a2.ColumnIndex("CODE_BARRE");


            var ops = db.CreateOperations();
            Bon_a2.Reset();
            while (Bon_a2.Next())
            {
                var x = Bon_a2.Current;
                long.TryParse(x[rid], out var id);
                var update = db.FakePrices[id] != null;
                var rvg = db.FakePrices.Get(id, true) as FakePrice;
                rvg.Facture = GetSFacture(x[rnb]);

                rvg.PSel = ParseFloat(x[rpa]);
                rvg.Qte = ParseFloat(x[rq]);
                var p = rvg.Product = GetProduct(x[rp], x[rcb]) ?? CreateProduit(x[rcb], x[rp], rvg.PSel, rvg.PSel * 1.3f, rvg.Qte);
                if (p == null)
                {

                }
                rvg.DPrice = p.DPrice;
                rvg.PPrice = p.PPrice;
                rvg.HWPrice = p.HWPrice;
                rvg.WPrice = p.WPrice;
                rvg.LastModified = DateTime.Now;

                ops.Add(update ? SqlOperation.Update : SqlOperation.Insert, rvg);
                if (ops.Length == 100)
                {
                    if (db.Save(ops) != true) Stop(ops);
                    ops = db.CreateOperations();
                }
            }
            if (db.Save(ops) != true) Stop(ops);
        }

        public void Transf_Facture()
        {
            if (Bon1.Count == 0 || Bon1.ColumnCount == 0) return;
            var rid = Bon1.ColumnIndex("RecordId");
            var rnb = Bon1.ColumnIndex("NUM_BON");
            var rcc = Bon1.ColumnIndex("CODE_CLIENT");
            var rc = Bon1.ColumnIndex("CLIENT");
            var rd = Bon1.ColumnIndex("DATE");
            var rm = Bon1.ColumnIndex("TTC");
            var rnp = Bon1.ColumnIndex("NBR_P");
            var ru = Bon1.ColumnIndex("UTILISATEUR");
            var ro = Bon1.ColumnIndex("OBSERVATIONS");
            var rvrs = Bon1.ColumnIndex("VERSER");
            var rtype = Bon1.ColumnIndex("MODE_RG");

            var ops = db.CreateOperations();
            Bon1.Reset();
            var vi = 0;
            while (Bon1.Next())
            {
                var x = Bon1.Current;
                if(!long.TryParse(x[rid], out var id))
                {

                }
                var update = db.Factures[id] != null;
                var bon = db.Factures.Get(id, true) as Facture;
                bon.Ref = "TBV" + x[rnb].PadLeft(5, '0');
                bon.Client = GetClient(x[rcc], x[rc]);
                if (bon.Client == null)
                {

                }
                bon.Date = ParseDate(x[rd]);
                bon.Total = ParseFloat(x[rm]);
                bon.NArticles = (int)ParseFloat(x[rnp]);
                bon.Validator = GetAgent(x[ru]);
                bon.SetFactureType(BonType.Bon, TransactionType.Vente);
                bon.Observation = x[ro];
                var mverser = ParseFloat(x[rvrs]);
                if (mverser != 0 && x[rtype] == "ESPECE")
                {
                    var vers = new models.Versment()
                    {
                        Id = DataRow.NewGuid(),
                        Facture = bon,
                        Client = bon.Client,
                        Date = bon.Date,
                        Montant = mverser,
                        Observation = "Versment",
                        Ref = "VBV" + ++vi,
                        Type = VersmentType.Espece
                    };
                    ops.Add(SqlOperation.Insert, vers);
                }
                ops.Add(update ? SqlOperation.Update : SqlOperation.Insert, bon);
                if (ops.Length >= 100)
                {
                    
                    if (db.Save(ops) != true) Stop(ops);
                    ops = db.CreateOperations();
                }
            }
            if (db.Save(ops) != true) Stop(ops);
        }
        public void Transf_Articles()
        {
            if (Bon2.Count == 0 || Bon2.ColumnCount == 0) return;
            if (Bon2.Count == 0 || Bon2.ColumnCount == 0) return;
            var rid = Bon2.ColumnIndex("RecordId");
            var rnb = Bon2.ColumnIndex("NUM_BON");
            var rpa = Bon2.ColumnIndex("PRIX_ACHAT");
            var rpu = Bon2.ColumnIndex("PRIX_UNITE");
            var rq = Bon2.ColumnIndex("QTE");
            var rp = Bon2.ColumnIndex("PRODUIT");
            var rcb = Bon2.ColumnIndex("CODE_BARRE");


            var ops = db.CreateOperations();
            Bon2.Reset();
            while (Bon2.Next())
            {
                var x = Bon2.Current;
                var id = long.Parse(x[rid]);
                Article art = db.Articles[id];
                Facture fct = GetFacture(x[rnb]);
                if (fct == null)
                    fct = CreatePerdedFacture(x[rnb]);
                var update = art != null;
                if (art == null)
                    db.Articles.Add(art = new Article() { Facture = fct, Id = id });
                art.Facture = fct;
                art.Qte = ParseFloat(x[rq]);
                art.Price = ParseFloat(x[rpu]);
                art.PSel = ParseFloat(x[rpa]);
                art.Product = GetProduct(x[rp], x[rcb]) ?? CreateProduit(x[rcb], x[rp], art.PSel, art.Price, art.Qte);
                if (art.Product == null)
                {
                }
                
                art.LastModified = DateTime.Now;
                ops.Add(update ? SqlOperation.Update : SqlOperation.Insert, art);
                if (ops.Length == 100)
                {
                    if (db.Save(ops) != true) Stop(ops);
                    ops = db.CreateOperations();
                }
            }
            if (db.Save(ops) != true) Stop(ops);
        }

        private void Stop(DatabaseOperationGroup ops)
        {
            if (ops == null)
            {
                return;
            }
            foreach (DatabaseOperation op in ops)
            {
                if (db.Save(op)!=true)
                {
                    if (op.Operation == SqlOperation.Delete) continue;
                    op.Operation = op.Operation == SqlOperation.Insert ? SqlOperation.Update : SqlOperation.Insert;
                    if (db.Save(op) != true)
                    {
                        MyConsole.WriteLine("Grave Error occured when save " + op.Data);
                    }
                }
            }
        }

        public void Transf_Versment()
        {
            if (Carnet_F.Count == 0 || Carnet_F.ColumnCount == 0) return;
            Carnet_C.Reset();
            var rid = Carnet_C.ColumnIndex("RecordId");
            var rcc = Carnet_C.ColumnIndex("CODE_CLIENT");
            var rc = Carnet_C.ColumnIndex("CLIENT");
            var ru = Carnet_C.ColumnIndex("UTILISATEUR");
            var rnb = Carnet_C.ColumnIndex("NUM_BON");
            var rv = Carnet_C.ColumnIndex("VERSEMENTS");
            var ro = Carnet_C.ColumnIndex("REMARQUES2");
            var rvt = Carnet_C.ColumnIndex("MODE_RG");
            var rd = Carnet_C.ColumnIndex("DATE");
            var ops = db.CreateOperations();
            while (Carnet_C.Next())
            {
                if (Carnet_C.Current[rvt] == "A TERME") continue;

                long.TryParse(Carnet_C.Current[rid], out var id);
                var client = GetClient(Carnet_C.Current[rcc], Carnet_C.Current[rc]);
                var agent = GetAgent(Carnet_C.Current[ru]);
                var facture = GetFacture(Carnet_C.Current[rnb]);
                var ver = db.Versments[id];
                var update = ver != null;
                if (ver == null)
                {
                    ver = new Versment() { Id = id };
                    db.Versments[ver.Id] = ver;
                }
                ver.Cassier = agent;
                ver.Client = client;
                ver.Date = ParseDate(Carnet_C.Current[rd]);
                ver.Facture = facture;
                ver.Montant = ParseFloat(Carnet_C.Current[rv]);
                ver.Observation = Carnet_C.Current[ro];
                ver.Pour = ver.Client;
                ver.Ref = "TV" + id.ToString().PadLeft(5, '0');
                ver.Type = VersmentType.Espece;

                ops.Add(update ? SqlOperation.Update : SqlOperation.Insert, ver);
                if (ops.Length == 100)
                {
                    if (db.Save(ops) != true) Stop(ops);
                    ops = db.CreateOperations();
                }
            }
            if (db.Save(ops) != true) Stop(ops);
        }
        public void Transf_SVersment()
        {
            if (Carnet_F.Count == 0 || Carnet_F.ColumnCount == 0) return;
            Carnet_F.Reset();
            var rid = Carnet_F.ColumnIndex("RecordId");
            var rcf = Carnet_F.ColumnIndex("CODE_FRS");
            var rf = Carnet_F.ColumnIndex("FOURNIS");
            var ru = Carnet_F.ColumnIndex("UTILISATEUR");
            var rnb = Carnet_F.ColumnIndex("NUM_BON");
            var rv = Carnet_F.ColumnIndex("VERSEMENTS");
            var ro = Carnet_F.ColumnIndex("REMARQUES2");
            var rvt = Carnet_F.ColumnIndex("MODE_RG");
            var rd = Carnet_F.ColumnIndex("DATE");
            var ops = db.CreateOperations();
            while (Carnet_F.Next())
            {
                if (Carnet_F.Current[rvt] == "A TERME") continue;

                long.TryParse(Carnet_F.Current[rid], out var id);
                var frn = GetFournisseur(Carnet_F.Current[rcf], Carnet_F.Current[rf]);
                var agent = GetAgent(Carnet_F.Current[ru]);
                var facture = GetSFacture(Carnet_F.Current[rnb]);
                var ver = db.SVersments[id];
                var update = ver != null;
                if (ver == null)
                {
                    ver = new SVersment() { Id = id };
                    db.SVersments[ver.Id] = ver;
                }
                ver.Cassier = agent;
                ver.Fournisseur = frn;
                ver.Date = ParseDate(Carnet_F.Current[rd]);
                ver.Facture = facture;
                ver.Montant = ParseFloat(Carnet_F.Current[rv]);
                ver.Observation = Carnet_F.Current[ro];
                ver.Ref = "TV" + id.ToString().PadLeft(5, '0');
                ver.Type = VersmentType.Espece;

                ops.Add(update ? SqlOperation.Update : SqlOperation.Insert, ver);
                if (ops.Length == 100)
                {
                    if (db.Save(ops) != true) Stop(ops);
                    ops = db.CreateOperations();
                }
            }
            if (db.Save(ops) != true) Stop(ops);
        }

        public void Clear()
        {
            var tables = new[] { "ARTICLES", "FACTURES", "FAKEPRICES", "FOURNISSEURS", "PRODUCTS", "SFACTURES", "VERSMENTS", "SVERSMENTS" };
            db.CreateEntity();
            foreach (var tbl in tables)
            {
                if (!db.Exec($"TRUNCATE TABLE `{tbl}`"))
                {

                }
            }
            db.Articles.Clear();
            db.Factures.Clear();
            db.FakePrices.Clear();
            db.Fournisseurs.Clear();
            db.Products.Clear();
            db.SFactures.Clear();
            db.Versments.Clear();
            db.SVersments.Clear();
        }

        internal PME Update()
        {
            Transf_Familles();
            Transf_Produits();
            Transf_Agent();
            Transf_Fournisseur();
            Transf_Clients();

            Transf_SFacture();
            Transf_SArticles();

            Transf_Facture();
            Transf_Articles();

            Transf_Versment();
            return this;
        }

        public PME(string dataSource)
        {
            var files = new[] { "produit.csv" , "fournis.csv", "famille.csv",
                "client.csv","Bon2.csv","Bon1.csv","bon_a2.csv","bon_a1.csv","GPC.csv",
                "CARNET_C.csv","CARNET_F.csv"};
            void create(string file)
            {
                var dir = dataSource;
                if (!new DirectoryInfo(dir).Exists)
                    Directory.CreateDirectory(dir);
                var f = dir + file;
                try
                {
                    if (!new FileInfo(f).Exists)
                        File.Create(f).Close();
                }
                catch (Exception)
                {
                }
            }
            foreach (var file in files)
                create(file);

            Produits = new Csv(System.IO.Path.Combine(dataSource, "produit.csv"));
            Fournis = new Csv(System.IO.Path.Combine(dataSource, "fournis.csv"));
            Famille = new Csv(System.IO.Path.Combine(dataSource, "famille.csv"));
            //Factp2 = new Csv(System.IO.Path.Combine(dataSource, "factp2.csv"));
            Client = new Csv(System.IO.Path.Combine(dataSource, "client.csv"));
            Bon2 = new Csv(System.IO.Path.Combine(dataSource, "Bon2.csv"));
            Bon1 = new Csv(System.IO.Path.Combine(dataSource, "Bon1.csv"));
            Bon_a2 = new Csv(System.IO.Path.Combine(dataSource, "bon_a2.csv"));
            Bon_a1 = new Csv(System.IO.Path.Combine(dataSource, "bon_a1.csv"));
            AgentGPC = new Csv(System.IO.Path.Combine(dataSource, "GPC.csv"));

            Carnet_C = new Csv(System.IO.Path.Combine(dataSource, "CARNET_C.csv"));
            Carnet_F = new Csv(System.IO.Path.Combine(dataSource, "CARNET_F.csv"));
        }
        public QServer.Core.Server server;
        public static PME Upgrade(QServer.Core.Server server)
        {            
            OpenFileDialog f = new OpenFileDialog()
            {
                AutoUpgradeEnabled = true,
                Title = "Selectioner Le Dossier de backup"
            };
            f.ShowDialog();
            if (string.IsNullOrWhiteSpace( f.FileName)) return null;
            var dir = new FileInfo(f.FileName).Directory;
            if (MessageBox.Show("Est vous etez sur de mise a jour le logiciel ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                switch (MessageBox.Show("Voulez-vous supprimer tout les donnees ou non", "Suppression", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                {
                    case DialogResult.Yes:
                        if (MessageBox.Show("Est vous etez sur de supprimer tout les donneés ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            return new PME(dir.FullName).Upgrade(server, true);
                        return null;
                    case DialogResult.No:
                        return new PME(dir.FullName).Upgrade(server, false);
                    case DialogResult.Cancel:
                        return null;
                }
            }
            return null;
        }

        private PME Upgrade(QServer.Core.Server server,bool dropeveryThings = false)
        {
            this.server = server;
            this.db = server.database;
            if (dropeveryThings)
                Clear();
            return Update();
        }
    }
    public partial class PME
    {
        private Facture CreatePerdedFacture(string numBon)
        {
            //var id = (long)parseFloat(v);
            var client = GetClient("DELETED_FACTURE", "DELETED FACTURE");
            if (client == null)
            {
                client = db.Clients[-909090];
                if (client == null)
                {
                    client = new models.Client() { Id = -909090, Abonment = Abonment.Detaillant, FirstName = "DELETED", LastName = "FACTURE", Name = "DELETED FACTURE", Observation = "This Compte is must be revalidated" };
                    db.Clients.Add(client);
                    db.Save(client, false);
                }
            }
            var f = new Facture() { Ref = "TBV" + numBon.PadLeft(5, '0'), Id = DataRow.NewGuid(), Client = client, Observation = "Recupered Facture (DELETED FACTURE)" };
            db.Factures.Add(f);
            db.Save(f, false);
            factures.Add(f.Ref, f);
            return f;
        }

        private Product CreateProduit(string codebarre, string name, float pachat, float pvent, float qte)
        {
            var p = db.Products.CreateItem(DataRow.NewGuid()) as Product;
            db.Products.Add(p);

            p.Name = name;
            p.Qte = qte;
            var r = p.PSel = pachat;
            p.DPrice = pvent;
            p.PPrice = pvent;
            p.HWPrice = pvent;
            p.WPrice = pvent;
            p.Codebare = codebarre;
            if (!db.Save(p, false)) Stop(null);
            return p;
        }
    }
    public partial class PME
    {
        private Dictionary<string, SFacture> sfactures;
        private Dictionary<string, Facture> factures;
        private Dictionary<string, Product> products;
        private Dictionary<string, Product> cproducts;

        private Dictionary<string, Client> clients;
        private Dictionary<string, Client> cclients;
        public SFacture GetSFacture(string cd)
        {
            if (sfactures == null)
            {
                sfactures = new Dictionary<string, SFacture>();
                foreach (var f in db.SFactures.AsList())
                {
                    var v = f.Value as SFacture;
                    sfactures.Add(v.Ref, v);
                }
            }
            sfactures.TryGetValue("TBA" + cd, out var x);
            if (x == null)
            {

            }
            return x;
        }
        public Facture GetFacture(string numBon)
        {
            numBon = numBon.PadLeft(5, '0');
            if (factures == null)
            {
                factures = new Dictionary<string, Facture>();
                foreach (var f in db.Factures.AsList())
                {
                    var v = f.Value as Facture;
                    if (!factures.ContainsKey(v.Ref))
                        factures.Add(v.Ref, v);
                    else
                    {

                    }
                }
            }
            factures.TryGetValue("TBV" + numBon, out var x);
            return x;
        }
        public Product GetProduct(string name, string codebarre)
        {
            if (products == null)
            {
                products = new Dictionary<string, Product>();
                cproducts = new Dictionary<string, Product>();
                foreach (var f in db.Products.AsList())
                {
                    var v = f.Value as Product;
                    if (!products.ContainsKey(v.Name))
                        products.Add(v.Name, v);
                    if (!string.IsNullOrWhiteSpace(v.Codebare) && !cproducts.ContainsKey(v.Codebare))
                        cproducts.Add(v.Codebare, v);
                }
            }
            if (!cproducts.TryGetValue(codebarre, out var x))
                products.TryGetValue(name, out x);
            return x;
        }
        public Category GetFamile(string name)
        {
            foreach (var c in db.Categories.AsList())
            {
                var v = c.Value as models.Category;
                if (v.Name == name) return v;
            }
            return null;
        }
        public Fournisseur GetFournisseur(string frRef, string name)
        {
            foreach (var c in db.Fournisseurs.AsList())
            {
                var v = c.Value as Fournisseur;
                if (v.Name == name) return v;
            }
            Fournis.Reset();
            var i = Fournis.ColumnIndex("CODE_FRS");
            while (Fournis.Next())
                if (Fournis.Feild(i) == frRef) return db.Fournisseurs[(long)ParseFloat(Fournis.Field("RecordId"))];
            return null;
        }
        
        public Client GetClient(string code_client, string name)
        {
            if (clients == null)
            {
                clients = new Dictionary<string, Client>();
                cclients = new Dictionary<string, Client>();
                goto update;
            }
            else if (clients.Count < db.Clients.Count) goto update;

            _continue:
            if (!cclients.TryGetValue(code_client, out var x))
                clients.TryGetValue(name, out x);
            if (x == null)
            {
                var c = new models.Client() { Id = DataRow.NewGuid(), Name = name, Ref = code_client, FirstName = name };
                if(db.Save(c, false))
                {
                    clients[name] = c;
                    cclients[code_client] = c;
                    db.Clients.Add(c);
                    return c;
                }
                return null;
            }
            return x;

            update:
            foreach (var f in db.Clients.AsList())
            {
                var v = f.Value as Client;
                if (v.Name != null)
                    if (!clients.ContainsKey(v.Name))
                        clients.Add(v.Name, v);
                if (v.Ref != null)
                    if (!cclients.ContainsKey(v.Ref))
                        cclients.Add(v.Ref, v);
            }
            goto _continue;
        }
        //public Client GetFournisseur(string frRef, string name)
        //{
        //    foreach (var c in db.Clients.AsList())
        //    {
        //        var v = c.Value as models.Client;
        //        if (v.Name == name) return v;
        //    }
        //    Client.Reset();
        //    var i = Client.ColumnIndex("CODE_CLIENT");
        //    while (Client.Next())
        //        if (Client.Feild(i) == frRef) return db.Clients[(long)ParseFloat(Client.Field("RecordId"))];
        //    return null;
        //}
        public Agent GetAgent(string name)
        {
            foreach (var c in db.Agents.AsList())
            {
                var v = c.Value as models.Agent;
                if (v.Name == name || v.Username == "@" + name) return v;
            }
            return null;
        }
        private char[] seps = new char[] { '.', '/' };
        private DateTime ParseDate(string v)
        {
            if (v == "" || v == null) return DateTime.Now;
            var t = v.Split(seps);
            return new DateTime(int.Parse(t[2]), int.Parse(t[1]), int.Parse(t[0]));
        }
        static float ParseFloat(string s)
        {
            if (s == ""||s==null) return 0;

            if (float.TryParse(s.Replace(',', '.'), out var a))
                return a;
            return 0;
        }
        static long ParseLong(string s)
        {
            if (s == "" || s == null) return 0;

            if (ulong.TryParse(s.Replace(',', '.'), out var a))
                return (long)a;
            return 0;
        }
    }
    
}
#endif

public class Csv
{
    public Dictionary<string, int> indx = new Dictionary<string, int>();
    private List<string[]> rows = new List<string[]>();
    public List<string[]> GetRows() => rows;
    public string KeyName { get; set; }
    public int ColumnCount => indx.Count;
    public Csv(string file, string tableName = null)
    {
        TableName = tableName;
        var tt = ReadAllLines(file);
        if (tt.Length == 0) return;
        var cols = split(tt[0]);
        int l = cols.Length;
        for (int i = 0; i < l; i++)
            indx.Add(cols[i].ToUpperInvariant(), i);
        for (int i = 1; i < tt.Length; i++)
        {
            var s = tt[i];
            cols = split(tt[i], l);
            if (cols != null)
                rows.Add(cols);
        }
    }
    public int Count => rows.Count;
    public string this[int row, string col] => rows[row][indx[col.ToUpperInvariant()]];
    public string this[int row, int col] => rows[row][col];
    public string[] this[int row] => rows[row];
    public string ColumnName(int index)
    {
        foreach (var x in indx)
            if (x.Value == index) return x.Key;
        return "";
    }
    public int ColumnIndex(string name)
    {
        if (indx.TryGetValue(name.ToUpperInvariant(), out var index)) return index;
        if (indx.TryGetValue(name, out  index)) return index;

        return -1;
    }

    public int Index { get; private set; } = -1;
    public void Reset() => Index = -1;
    public bool Next() => ++Index < rows.Count;
    public string[] Current => Index < rows.Count ? rows[Index] : null;

    public string TableName { get; }

    public string Field(string name) => Index < rows.Count ? rows[Index][indx[name.ToUpperInvariant()]] : null;
    public string Feild(int index) => Index < rows.Count ? rows[Index][index] : null;
    public string this[string colName] => Index < rows.Count ? rows[Index][indx[colName.ToUpperInvariant()]] : null;

    public static string[] ReadAllLines(string file)
    {
        if (!new FileInfo(file).Exists) return new string[0];
        var s = File.ReadAllText(file, System.Text.Encoding.Default);
        var t = new List<string>();
        var pi = 0;
        var inq = false;
        int i = 0;
        for (i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c == '\\') continue;
            if (c == '"') inq = !inq;
            if (inq) continue;
            if (c == '\r')
            {
                t.Add(s.Substring(pi, i - pi));
                if (s[i + 1] == '\n')
                    i++;
                pi = i + 1;
            }
        }
        if (i - pi > 0)
            t.Add(s.Substring(pi, i - pi));
        return t.ToArray();
    }
    static string[] split(string s)
    {
        if (s.IndexOf(separator) == -1) throw null;
        var cls = new List<string>(20);
        var pind = -1;
        int i = 0;
        while (true)
        {
            i++;
            if (pind + 1 == s.Length)
            {
                cls.Add("");
                break;
            }
            if (pind + 1 > s.Length)
                break;

            if (s[pind + 1] == '"')
            {
                var e = s.IndexOf("\"" + separator, pind + 1);
                if (e == -1)
                    e = s.Length - 1;
                cls.Add(s.Substring(pind + 2, e - pind - 2));
                pind = e + 1;
                continue;
            }
            var ind = s.IndexOf(separator, pind + 1);
            if (ind == -1)
            {
                cls.Add(s.Substring(pind + 1));
                break;
            }
            cls.Add(s.Substring(pind + 1, ind - pind - 1));
            pind = ind;
        }
        return cls.ToArray();
    }
    static string[] split(string s, int l)
    {
        var cls = new string[l];
        var pind = -1;
        int i = 0;
        for (; i < l; i++)
        {
            if (pind + 1 == s.Length)
            {
                cls[i] = "";
                break;
            }
            if (pind + 1 > s.Length)
                break;

            if (s[pind + 1] == '"')
            {
                var e = s.IndexOf("\"" + separator, pind + 1);
                if (e == -1)
                    e = s.Length - 1;
                cls[i] = s.Substring(pind + 2, e - pind - 2);
                pind = e + 1;
                continue;
            }
            var ind = s.IndexOf(separator, pind + 1);
            if (ind == -1)
            {
                cls[i] = s.Substring(pind + 1);
                break;
            }
            cls[i] = s.Substring(pind + 1, ind - pind - 1);
            pind = ind;
        }
        if (i >= l - 1)
            return cls;
        return null;
    }
    static char separator = ';';
    private Dictionary<string, string[]> _dic;
    public Csv IndexWith(string keyName = null)
    {
        KeyName = keyName ?? KeyName;
        if (KeyName == null) return this;
        var t = ColumnIndex(KeyName);
        if (t == -1) throw new Exception("the key is not exist");
        (_dic ?? (_dic = new Dictionary<string, string[]>())).Clear();
        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            _dic[r[t] ?? ""] = r;
        }
        return this;
    }
    public string[] GetByKey(string key)
    {
        string[] v = null;
        return _dic?.TryGetValue(key, out v) != null ? v : null;
    }
}