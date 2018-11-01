using models;
using System;
using System.Collections.Generic;
using Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace QServer.Upgrader
{
    public interface IShemaColumn<T> where T : DObject
    {
        int Index { get; }
        string Name { get; }
        void SetValue(T data, object value, Context context);
        object GetValue(T data, Context context);
        bool IsReference { get; }
    }
    public interface IShemaTable<T> where T : DObject
    {
        int ColumnLength { get; }
        IShemaColumn<T> this[int columnIndex] { get; }
        IShemaColumn<T> this[string columnName] { get; }
        
    }


    public class ShemaDPColumn<T> : IShemaColumn<T> where T : DataRow, new()
    {
        public ShemaDPColumn(DProperty property,int index)
        {
            Property = property;
            Index = index;
        }
        public string Name => Property.Name;

        public bool IsReference => Property.Converter.IsComplex;

        public DProperty Property { get; }

        public int Index { get; }

        public object GetValue(T data, Context context) => data.get<object>(Property.Index);

        public void SetValue(T data, object value,Context context)
        {
            var ct = Property.Converter;
            object _val = null;

            if (ct is BasicConverter t)
                _val = t.ToCsValue(value);
            else if (ct is ComplexConverter s)
            {
                s.ToCsValue(context, Property, data, value);
            }
            DProperty.db2dtrw(context, Property, data, value);
        }
    }

    public class ShemaDBTable<T> : IShemaTable<T> where T : DataRow, new()
    {
        private readonly List<IShemaColumn<T>> list = new List<IShemaColumn<T>>();
        public ShemaDBTable(DataTable<T> table)
        {
            Table = table;
        }

        public IShemaColumn<T> this[int columnIndex] => (columnIndex<list.Count?list[columnIndex]:null);

        public IShemaColumn<T> this[string columnName]
        {
            get
            {
                foreach (var item in list)
                    if (item?.Name == columnName) return item;
                return null;
            }
        }

        public int ColumnLength => list.Count;

        public DataTable<T> Table { get; }
        public bool Add(IShemaColumn<T> column)
        {
            var c = this[column.Name];
            if (c != null) return false;
            for (int i = list.Count; i <= column.Index; i++)
                list.Add(null);
            list[column.Index] = column;
            return true;
        }


    }
    public abstract class DataImporter
    {
        public abstract bool IsExist(string[] data);
    }

    public class Products
    {
        public Products()
        {

        }
        public void Parse(Database database, string file)
        {

        }

        private static Dictionary<string, string> translator = new Dictionary<string, string>()
        {
            ["Name"] = nameof(models.Product.Name),
            ["Qte"] = nameof(models.Product.Qte),
            ["PrixA"] = nameof(models.Product.PSel),
            ["PrixV"] = "Value",
            ["Dim"] = nameof(models.Product.Dimention),
            ["SerN"] = nameof(models.Product.SerieName),
            ["Categ"] = nameof(models.Product.Category),
            ["Desc"] = nameof(models.Product.Description)
        };
        static char[] separator = new[] { ',', ';', '>', '\t' };
        private static string getDatabase(ref string[] args)
        {
            var t = args[0];
            var xt = new string[args.Length - 1];
            for (int i = 0; i < xt.Length; i++)
                xt[i] = args[i + 1];

            args = xt;
            return t;
        }
        static void Main(Database database,string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(QServer.FingerPrint.Value());
                return;
            }
            Resource.DatabasePath = getDatabase(ref args);
            var fileName = $"temp_{DateTime.Now.Ticks}";

            ((models.MySqlManager)database.DB).Backup(fileName, out var e);
            for (int i = 0; i < args.Length; i++)
                if (!parseCsv(args[i], database))
                {
                    Console.WriteLine("*************************************************");
                    Console.WriteLine("             Un Expected Error Happend .");
                    Console.WriteLine($"             {args[i]} .");
                    Console.WriteLine("*************************************************");
                    tpx:

                    Console.WriteLine("Do you would to continue (y or n)");
                    var rx = Console.ReadLine();
                    if (rx == "no")
                        break;
                    else if (rx != "yes")
                        goto tpx;
                }
            tp:
            Console.Write("\r\nDo you want to keep all Transaction (yes or no)");
            var r = Console.ReadLine();
            if (r == "no")
            {
                if (database.DB.Restore(fileName, out var we))
                    Console.WriteLine("The Database successfully Backed");
                else Console.WriteLine(we?.Message);
            }
            else if (r == "yes")
            {
                Console.WriteLine("You CSV Success fully Saved .");
            }
            else goto tp;
        }

        private static bool parseCsv(string file, models.Database database)
        {
            file = file.Trim(' ', '\'', '"');
            var upd = new DatabaseUpdator(database);
            var l = upd.Update(database.Products, nameof(database.Products));
            var x = upd.Update(database.Categories, "Categories");
            var prr = DObject.GetProperties<Category>();
            if (file != null)
            {
                Resource.DatabasePath = "bachour";
                var c = new Context(true, database, null);
                var f = File.ReadAllText(file);

                var lns = f.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);


                var cols = lns[0].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                var props = ExtractShema(cols, out var ni);
                if (ni == -1)
                {
                    Console.WriteLine($"The file {file} has no column named Name");
                    return false;
                }
                if (props == null) return false;
                for (int i = 1; i < lns.Length; i++)
                {
                    var ln = lns[i];

                    parseProduct(c, ni, props, ln);
                }
                return true;
            }
            return false;
        }
        static int id = 100;
        private static void parseProduct(Context c, int nameProp, DProperty[] props, string ln)
        {
            var vals = ln.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < vals.Length; i++)
                vals[i] = vals[i].Trim();

            if (nameProp >= vals.Length || vals[nameProp] == "") { Console.WriteLine($"The Line {ln} was skiped because has no name"); return; }

            var obj = GetProduct(vals[nameProp],c.Database) ?? new models.Product() { Id = id++ };

            for (int j = 0; j < vals.Length; j++)
            {
                var val = vals[j];
                if (j >= props.Length) { Console.WriteLine($"The Value {val} Of line {j} is out of range"); continue; }
                var pr = props[j];
                var ct = DProperty.GetDbConverter(pr.Type);
                object _val = null;
                if (pr.Name == nameof(models.Product.Category))
                {
                    _val = GetCategory(val, c.Database);
                }
                else if (ct is BasicConverter t)
                    _val = t.ToCsValue(val);
                else if (ct is ComplexConverter s)
                {
                    Console.WriteLine("This Value Its save ==> cannot be Introduced");
                }
                obj[pr.Name] = _val;
            }
            if (c.Database.Save(obj, false))
                c.Database.Products.Add(obj);
            else
            {
                Console.WriteLine($"Check the line {ln} has an error");
            }


        }


        private static Dictionary<string, Category> catHistory = new Dictionary<string, Category>();

        private static Category GetCategory(string s,Database database)
        {
            var os = s;
            s = s.ToLower().Trim();
            if (s == "") return null;
            if (catHistory.TryGetValue(s, out var cat)) return cat;
            var categories = database.Categories;
            foreach (var kv in categories.AsList())
            {
                cat = kv.Value as Category;
                if (string.Compare(cat.Name?.Trim(), s, true) == 0) goto ret;
            }
            var t = new List<Category>();
            foreach (var kv in categories.AsList())
            {
                cat = kv.Value as Category;
                var cn = cat.Name?.Trim().ToLower() ?? "";
                var st = false;

                if (cn.Contains(s))
                {
                    if (!st) Console.WriteLine("Choose Category "); else st = true;
                    t.Add(cat);
                    Console.WriteLine($"Option {t.Count}: {cn}");
                }
            }
            if (t.Count == 0)
            {
                save:

                if (database.Save(cat = new Category() { Id = models.DataRow.NewGuid(), Name = os }, false))
                {
                    database.Categories.Add(cat);
                }
                else
                {
                    Console.WriteLine($"The Category ** {os} ** Cannot be acceptted");
                    Console.Write("Do you want rename it");
                    var r = Console.ReadLine();
                    if (r == "yes")
                    {
                        Console.Write($"Enter The Name of {os}  :");
                        cat.Name = Console.ReadLine();
                        goto save;
                    }
                    goto ret;
                }
                goto ret;
            }
            db:
            Console.Write($"Choose From 1-{t.Count}");
            if (!int.TryParse(Console.ReadLine(), out var _ch)) goto db;
            if (_ch < 1 || _ch > t.Count) goto db;

            cat = t[_ch - 1];
            ret:
            catHistory.Add(s, cat);
            return cat;
        }
        private static models.Product GetProduct(string s,Database database)
        {
            return null;
            var os = s;
            s = s.ToLower().Trim();
            var categories = database.Products;

            foreach (var kv in categories.AsList())
            {
                var cat = kv.Value as models.Product;
                if (string.Compare(cat.Name?.Trim(), s, true) == 0) return cat;
            }
            return null;
        }
        private static DProperty[] ExtractShema(string[] cols, out int NameIndex)
        {
            var ot = new DProperty[cols.Length];
            var props = DObject.GetProperties<models.Product>();
            NameIndex = -1;
            for (int i = 0; i < cols.Length; i++)
            {
                var co = cols[i];
                var col = co;
                translator.TryGetValue(col, out var tcol);
                var prop = GetProp(col, tcol);
                if (prop == null)
                {
                    Console.WriteLine($"Column {col} doesn't exist ");
                    var revTrans = reverse(translator);
                    foreach (var p in props)
                    {
                        if (p.Index > 1)
                        {
                            revTrans.TryGetValue(p.Name, out var ncol);
                            Console.WriteLine($"Column {p.Index} : {ncol ?? p.Name}");
                        }
                    }
                    return ot;
                }
                else
                {
                    if (prop.Name == nameof(models.Product.Name)) NameIndex = i;
                    ot[i] = prop;
                }
            }
            return ot;
        }
        private static DProperty[] props;
        private static DProperty GetProp(string name, string orName)
        {
            new models.Product();
            props = props ?? (props = DObject.GetProperties<models.Product>());

            foreach (var prop in props)
            {
                if (prop.Index >= 0)
                    if (string.Compare(prop.Name, name, true) == 0 || string.Compare(prop.Name, orName, true) == 0)
                    {
                        return prop;
                    }
            }
            return null;

        }
        private static Dictionary<P, T> reverse<T, P>(Dictionary<T, P> dictionary)
        {
            var dic = new Dictionary<P, T>(dictionary.Count);
            foreach (var kv in dictionary)
                dic.Add(kv.Value, kv.Key);
            return dic;
        }
    }
}
