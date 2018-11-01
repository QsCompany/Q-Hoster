using System;
using models;



namespace Server
{
    public class Trans
    {
        //private Logicom Logicom;
        private Database database = Database.__Default;

        public void initProducts()
        {

            try
            {

                
                //Logicom = Logicom.Default;
            }
            catch (Exception e)
            {

            }
            //return;
            //GetLot(null);
        }
        /*
        private void GetTiers(TIERS tiers)
        {
            foreach (var tier in Logicom.Data.TIERS)
            {
                var x = new Client()
                {
                    Name = tier.TIERS_FR,
                    FirstName = (tier.TIERS_FR ?? "").Split(' ').FirstOrDefault(),
                    LastName = (tier.TIERS_FR ?? "").Split(' ').LastOrDefault(),
                    Address = tier.ADRESSE_FR,
                    Observation = Encoding.UTF8.GetString(tier.DESCRIPTION),
                };
            }
        }
        private Product GetLot(ARTICLES _art)
        {
            if (!initLots)
            {
                GetPicture(0);
                initLots = true;
                int i = 0;
                foreach (var art in Logicom.Data.ARTICLES)
                {
                    var p = art.Lot;
                    i++;
                    //if (i > 200) break;
                    if (art == null) continue;
                    var x = new Product
                    {
                        Name = art.ARTICLE_FR,
                        Category = GetCategory(art.KEY_ART_FAM),
                        LastModified = art.DATE_MODIF.Ticks,
                        Description = art.DESCRIPTION_FR,
                        Id = art.KEY_ARTICLE,
                        WPrice = (float)p.PRIX_GROS,
                        HWPrice = (float)p.PRIX_DEMI_GROS,
                        DPrice = (float)p.PRIX_DETAIL,
                        PPrice = (float)p.PRIX_SUPER_GROS,
                        PSel = (float)p.DERNIER_PRIX_ACHAT,
                        Qte = (float)p.QTE_TOTALE,
                        Picture = GetPicture(art.KEY_ARTICLE)
                    };
                    //GetPrices(x);
                    database.Products.Add(x);
                    if (!database.StrictSave(x, false)) { Console.WriteLine("Fail insert :  " + x.Name); }
                }
            }
            return _art == null ? null : database.Products[_art.Lot.KEY_LOT];
        }

        Dictionary<int, List<Picture>> pictures;
        private Picture GetPicture(int key)
        {
            List<Picture> lst;
            if (pictures == null)
            {
                pictures = new Dictionary<int, List<Picture>>();
                var dir = new DirectoryInfo("./images");
                if (!dir.Exists) dir.Create();
                foreach (var op in Logicom.Data.PHOTOS)
                {
                    Picture pic;
                    string s;
                    using (var file = File.OpenWrite(System.IO.Path.Combine(dir.FullName, s = op.KEY_PHOTO.ToString() + ".png")))
                        file.Write(op.PHOTO, 0, op.PHOTO.Length);
                    if (!pictures.TryGetValue(op.KEY_ARTICLE, out lst)) pictures.Add(op.KEY_ARTICLE, lst = new List<Picture>());
                    lst.Add(pic = new Picture() { Id = op.KEY_PHOTO, ImageUrl = "/_/Picture/" + s });
                    database.StrictSave(pic, false);
                }
            }
            if (pictures.TryGetValue(key, out lst)) return lst[0];
            return null;
        }
        float getPrice(Price p, Abonment a)
        {
            var pr = p.GetPrice(a);
            if (pr == 0)
            {
                for (int i = (int)a - 1; i >= 0; i--)
                {
                    pr = p.GetPrice((Abonment)i);
                    if (pr != 0) return pr;
                }
                for (int i = (int)a + 1; i < 4; i++)
                {
                    pr = p.GetPrice((Abonment)i);
                    if (pr != 0) return pr;
                }
                if (p.PSel != 0) return p.PSel;
                return 0;
            }
            return pr;
        }
        private Category ChaudFroid = new Category { Name = "Chaud et Froid", Id = 0 };
        private bool initCategory;
        private bool initLots;
        private bool initPrd = false;
        private Category GetCategory(long id)
        {
            if (!initCategory)
            {
                foreach (var _artFam in Logicom.Data.ARTICLES_FAMILLES)
                {
                    var c = new Category
                    {
                        Id = _artFam.KEY_ART_FAM,
                        Name = _artFam.ART_FAMILLE_FR,
                    };
                    database.Categories.Add(c);
                }
                initCategory = true;
                database.Categories.Add(ChaudFroid);
                database.Save(ChaudFroid, false);
            }
            
            return database.Categories[id];
        }
        */
    }
    public class Common
    {

        private static char[] seps = new char[] { '.', '/' };
        public static DateTime ParseDate(string v)
        {
            if (v == "" || v == null || v == "null") return DateTime.Now;
            var t = v.Split(seps);
            return new DateTime(int.Parse(t[2]), int.Parse(t[1]), int.Parse(t[0]));
        }
        public static float ParseFloat(string s)
        {
            if (s == "" || s == null || s == "nulll") return 0;
            return float.TryParse(s.Replace(',', '.'), out var a) ? a : 0f;
        }
        public static int ParseInt32(string s)
        {
            if (s == "" || s == null || s == "nulll") return 0;
            return int.TryParse(s.Replace(',', '.'), out var a) ? a : 0;
        }
        public static long ParseLong(string s)
        {
            if (s == "" || s == null || s == "nulll") return 0;
            return long.TryParse(s.Replace(',', '.'), out var a) ? a : 0l;
        }
        public static short ParseInt16(string s)
        {
            if (s == "" || s == null || s == "null") return 0;
            return short.TryParse(s.Replace(',', '.'), out var a) ? a : (short)0;
        }
        public static bool ParseBoolean(string s)
        {
            switch (s)
            {
                case "1":
                    return true;
                case "0":
                case "":
                case null:
                case "null":
                    return false;
            }
            return bool.TryParse(s, out var a) ? a : false;
        }

        public static string ParseString(string s)
        {
            if (s == "") return s;
            if ( s == null || s == "nulll") return null;
            return Json.JString.ReadString(s);
        }
    }
    public abstract class CsvConverter : Common
    {
        public QFactUpgrade Db { get; }
        public Csv Table { get; }
        public static int ROWID = 1;
        public CsvConverter(QFactUpgrade db, string name)
        {
            Db = db;
            Table = db.tables[name];
        }

        public abstract void LoadBasics(string[] row);
        public abstract void LoadDependencies(string[] row);

        public abstract T CreateWrapper<T>(string[] data) where T : CsvReader;
    }

    public abstract class CsvReader:Common
    {
        protected QFactUpgrade __db__;
        protected string __tableName__;
        protected int __key__;
        protected string[] __data__;
        public Csv CsvTable => __db__[__tableName__];
        public CsvConverter CsvConverter => __db__.GetConverter(__tableName__);

        protected CsvReader(string tableName, int keyIndex)
        {
            __tableName__ = tableName;
            __key__ = keyIndex;
        }

        public virtual CsvReader Initailize(QFactUpgrade db, string[] data)
        {
            __db__ = db;
            __data__ = data;
            return this;
        }
        public abstract DataRow Parse();

        public string GetKeyValue() => __data__?[__key__];

        public virtual models.DataRow ToDataRow()
        {
            return null;
        }

    }
}