using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Json;
using QServer.Services;
using Server;
using System.IO;

namespace models
{
    public static class cache
    {
        private static Stack<StringBuilder> sbs = new Stack<StringBuilder>();

        public static StringBuilder Cache
        {
            get => sbs.Count == 0 ? new StringBuilder(1000) : sbs.Pop().Clear();
            set => sbs.Push(value);
        }
    }

    public enum Stat
    {
        New = 0,
        Modified = 1,
        Saved = 2,
        Delete = 4,
        Deleted = 8
    }

    public delegate  void OnPropertyChanged(DataRow d, DProperty p);


    


    abstract public class DataRow : DObject,IHistory
    {

        public virtual int Repaire(Database db)
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            
            if (ReferenceEquals(this, obj)) return true;
            var t = obj as DataRow;
            if (t == null) return false;
            return GetType() == obj.GetType() && t.Id == Id;
        }
        public new static int __LOAD__(int dp) => DObject.__LOAD__(DpId);
        public StringBuilder GetTypeScript()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine($"export public class {TableName} extends data.QShopRow {{");
            return b;
        }
        
        public OnPropertyChanged PropertyChanged;
        protected override void OnPropertyChanged(DProperty dp) => PropertyChanged?.Invoke(this, dp);

        private static readonly DateTime Defaultdatetime = new DateTime(0);
        protected static DateTime ToDateTime(JString s)
        {
            if (s == null) return Defaultdatetime;
            DateTime d;
            if (DateTime.TryParse(s, out d)) return d;
            return Defaultdatetime;
        }

        public static int DpId = Register<DataRow, long>("Id"); public long Id { get => get<long>(DpId);
            set => set(DpId, value);
        }
        public readonly static int DPLastModified = Register<DataRow, DateTime>("LastModified", PropertyAttribute.None, test);

        private static object test(DObject obj, object _old, object _new)
        {
            return null;
        }

        public DateTime LastModified { get { return get<DateTime>(DPLastModified); } set { this._values[DPLastModified] = value; } }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
        public override void Stringify(Context c)
        {
            var j = c.GetParameter(GetType(), typeof(DataRowParameter)) as DataRowParameter;
            if (j != null)
                if (j.SerializeAsId)
                {
                    c.Stringify(Id);
                    return;
                }
            base.Stringify(c);
        }

        private static Random r = new Random();
        
        public virtual JValue Parse(JValue json)
        {
            return json;
        }
        public static Random mr = new Random((int)(DateTime.Now.Ticks & int.MaxValue));
        private static byte[] lid = new byte[8];
        static DataRow()
        {
            lid = BitConverter.GetBytes(DateTime.Now.Ticks);
        }
        private static long ix = GuidService.GetGuid();
        private static long end = ix + 10000 - 100;
        public static long NewGuid()
        {
            if (ix > end)
            {
                lock (r)
                    if (ix > end)
                    {
                        ix = GuidService.GetGuid();
                        end = ix + 10000 - 100;
                    }
            }
            return ++ix;
        }
        protected DataRow()
        {
            Id = NewGuid();
            CreatedRaws.Add(this);
        }

        protected DataRow(Context c, JValue jv)
            : base(c, jv)
        {
            if (Id == 0) Id = NewGuid();
        }

        public virtual string TableName => Database.GetPlur(GetType().Name);

        public string GetCreateTable()
        {
            var s = new StringBuilder(1024).AppendFormat("CREATE TABLE `{0}` (", TableName);
            s.Append("`Id` BIGINT NOT NULL PRIMARY KEY");
            var ps = GetProperties();
            for (int i = 1; i < ps.Length; i++)
            {
                var p = ps[i];
                s.AppendFormat(",`{0}` {1}", p.Name, p.DBType);
            }
            s.Append(')');
            return s.ToString();
        }
        public static bool CreateColumn(DProperty property,Database database,DataTable table)
        {
            var s = $"ALTER TABLE `{table.Name}` ADD COLUMN `{property.Name}` {property.DBType};";
            return database.Exec(s);
        }
        public static List<DataRow> CreatedRaws = new List<DataRow>();

        public void StringifyProperty(Context c, string name, object value)
        {
            var s = c.GetBuilder();
            s.Append('"' + name + "\":");
            c.Stringify(value);
        }
        public void StringifyProperty(Context c, string name, JValue value)
        {
            var s = c.GetBuilder();
            s.Append('"' + name + "\":");
            value.Stringify(c);
        }

        struct TypeDBInfo
        {
            public string Name;
            public string Insert;
        }
        private static Dictionary<Type, TypeDBInfo> heads = new Dictionary<Type, TypeDBInfo>();


        private static string GetInsert(Type type,string tableName)
        {
            TypeDBInfo h;
            if (!heads.TryGetValue(type, out h))
                heads.Add(type, h = new TypeDBInfo());
            else if (h.Insert != null)
                return h.Insert;
            if (h.Name == null) h.Name = tableName ?? Database.GetPlur(type.Name);
            var isb = cache.Cache.Append("INSERT INTO `").Append(h.Name).Append("` (");
            var s = true;
            foreach (var property in GetProperties(type))
            {
                if (property.IsOptional) continue;
                if (s) s = false;
                else isb.Append(',');
                isb.Append('`').Append(property.Name).Append('`');
            }
            isb.Append(") VALUES (");
            return isb.ToString();
        }

        public string GetUpdate(System.Data.Common.DbCommand c,ref int pi)
        {
            var isb = cache.Cache;
            isb.Clear().Append("Update `").Append(TableName).Append("` SET ");
            var s = true;
            var prpts = GetProperties();
            object v;
            for (int i = 1; i < prpts.Length; i++)
            {
                var property = prpts[i];
                if (property.IsOptional) continue;
                if (s) s = false;
                else isb.Append(',');
                v = _values[property.Index];
                if (v != null && property.Type == typeof(string))
                {
                    var r = (string)v;
                    if (r.Length > property.Length) r = r.Substring(0, property.Length);
                    var p = c.CreateParameter();
                    p.ParameterName = (++pi).ToString();                    
                    p.Value = r;
                    isb.Append('`').Append(property.Name).Append("`=@").Append(p.ParameterName);
                    c.Parameters.Add(p);
                }
                else
                    isb.Append('`').Append(property.Name).Append("`=").Append((v == null ? "null" : property.Converter.ToDbValue(v)));
            }
            return isb.AppendFormat(" WHERE `Id`={0}", (v = _values[0]) == null ? "null" : prpts[0].Converter.ToDbValue(v)).ToString();
        }

        public object GetDbValue(DProperty dp)
        {
            object v;
            return (v = get(dp.Index)) == null ? "null" : dp.Converter.ToDbValue(v);
        }

        public virtual string GetDelete(System.Data.Common.DbCommand c)
        {
            return "DELETE FROM " + TableName + " WHERE `Id`=" + Id + "";
        }
        public virtual string GetInsert(System.Data.Common.DbCommand c,ref int pi)
        {
            var sbv = cache.Cache.Append(GetInsert(GetType(), TableName));
            var ps = GetProperties();
            var s = true;
            object v;
            foreach (var property in ps)
            {
                if (property.IsOptional) continue;
                if (s) s = false;
                else
                    sbv.Append(',');

                v = _values[property.Index];

                if (v == null)
                    v = "NULL";
                else if (property.Type == typeof(String))
                {
                    var r = (string)v;
                    if (r.Length > property.Length) r = r.Substring(0, property.Length);
                    var p = c.CreateParameter();
                    p.ParameterName = (++pi).ToString();
                    p.Value = r;
                    c.Parameters.Add(p);
                    sbv.Append('@');
                    v = p.ParameterName;
                }
                else
                {
                    v = property.Converter.ToDbValue(v);
                    if (v is IConvertible)
                    {
                        if (v is float || v is double || v is decimal)
                            v = v.ToString().Replace(",", ".");
                    }
                }
				sbv.Append(v);
				
			}
            sbv.Append(')');
            return sbv.ToString();
        }
        
        private Stack<System.Data.Common.DbParameter> parametres = new Stack<System.Data.Common.DbParameter>();
        private System.Data.Common.DbParameter CreateSqlParam(System.Data.Common.DbCommand cmd)
        {
            if (parametres.Count == 0) return cmd.CreateParameter();
            else return parametres.Pop();
        }
        private void Dispose(System.Data.Common.DbCommand cmd)
        {
            var p = cmd.Parameters;
            var l = p.Count;
            for (int i = 0; i <l ; i++)
                parametres.Push(p[i]);
            p.Clear();
        }

        //internal static long Parse(string p)
        //{
        //    return long.Parse(p);
        //}

        internal static bool TryParse(string rp, out long g)
        {
            return long.TryParse(rp, out g);
        }

        public override int GetHashCode()
        {
            var hashCode = -1377217700;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TableName);
            return hashCode;
        }
    }
}