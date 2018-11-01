using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;
using Json;
using Server;
using System.Data.Common;
using System.Xml;
using System;
using System.Data;
using System.Windows.Forms;
using System.Xml;
namespace models
{
    public partial class DArray<T> : IList<T>,IEnumerable<T>
    {
        public bool IsReadOnly => false;

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(this);
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        void ICollection<T>.Clear()
        {
            _list.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<T>(this);
        }
        class Enumerator<P> : IEnumerator,IEnumerator<P>
        {
            private int index = -1;
            private DArray<P> array;
            public Enumerator(DArray<P> array)
            {
                this.array = array;
            }
            public P Current => array[index];

            object IEnumerator.Current => array[index];

            public void Dispose()
            {
                array = null;
            }

            public bool MoveNext()
            {
                return ++index < array.Count;
            }

            public void Reset()
            {
                index = -1;
            }
        }
    }
    
    public partial class DArray<T> : DObject
    {
        public int Count => _list.Count;
        private List<T> _list = new List<T>();
        public T this[int index]
        {
            get => index < _list.Count ? _list[index] : _default;
            set {
                if (index < _list.Count)
                    _list[index] = value;
                else
                {
                    for (int i = _list.Count; i < index; i++)
                        _list.Add(_default);
                    _list[index] = value;
                }
            }
        }
        protected T _default = default(T);
        public override string ToString()
        {
            return "Has " + Count + " item";
        }
        
        public override void Stringify(Context c)
        {
            var indexer = c.GetIndexer(this);
            if (indexer.StringifyAsRef(c, out var start))
                return;

            var sb = c.GetBuilder();
            sb.Append('[');
            for (int i = 0; i < _list.Count; i++)
            {
                if (i != 0) sb.Append(',');
                c.Stringify(_list[i]);
            }
            sb.Append(']');
        }

        public override void SimulateStringify(Context c)
        {
            var indexer = c.GetIndexer(this);
            if (indexer.IsStringified)
            {
                indexer.SimulateStringify(c);
                return;
            }
            if (indexer.IsReferenced != false)
                if (!(indexer is EIndexer))
                    indexer.SimulateStringify(c);
            for (int i = 0; i < _list.Count; i++)
                c.SimulateStringify(_list[i]);
        }

        public void Add(T t) => _list.Add(t);
        public bool Remove(T t) => _list.Remove(t);
        public void Insert(int index,T value)
        {
            if (index <= _list.Count)
                _list.Insert(index, value);
            else
            {
                for (int i = _list.Count; i < index; i++)
                    _list.Add(_default);
                _list.Add(value);
            }
        }
        public void AddRange(IEnumerable<T> collection) => _list.AddRange(collection);
        public void InsertRange(int index,IEnumerable<T> collection)
        {

            if (index <= _list.Count)
                _list.InsertRange(index, collection);
            else
            {
                for (int i = _list.Count; i < index; i++)
                    _list.Add(_default);
                _list.AddRange(collection);
            }
        }
        internal void Clear() => _list.Clear();
    }

    public class DArray : DArray<object>
    {

    }



    abstract public class DataTable : DObject, IEnumerable
    {
        public bool IsHistory
        {
            set
            {
                if (value) deletedItems = new ConcurrentDictionary<long, DateTime>();
            }
        }
        public override string ToString()
        {
            return "Has " + Count + " item";
        }
        public virtual string Name => GetType().Name;
        private static Dictionary<Type, string> sqlTypes = new Dictionary<Type, string>();

        static DataTable()
        {
            sqlTypes.Add(typeof(Int32), "INT");
            sqlTypes.Add(typeof(DateTime), "DATETIME");
            sqlTypes.Add(typeof(Int64), "BIGINT");
            sqlTypes.Add(typeof(Decimal), "DECIMAL(15,2)");
            sqlTypes.Add(typeof(Int16), "SMALLINT");
            sqlTypes.Add(typeof(Char), "nchar(1)");
            sqlTypes.Add(typeof(Single), "FLOAT");
            sqlTypes.Add(typeof(Double), "REAL");
            sqlTypes.Add(typeof(String), "TEXT");
            sqlTypes.Add(typeof(Boolean), "BIT");
            sqlTypes.Add(typeof(Guid), "BIGINT");
        }

        public static void Register(Type nativeType, string sqlType)
        {
            sqlTypes.Add(nativeType, sqlType);
        }

        public static string CreateTable(string tableName, DProperty[] ps)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CREATE TABLE `{0}` (", tableName);
            var start = true;
            var srs = "";
            foreach (var item in ps)
            {
                if (srs.Contains("[" + item.Name + "]")) 
                {

                }
                srs += "[" + item.Name + "]";
            }
            foreach (var property in ps)
            {
                if (property.IsOptional) continue;
                if (start) start = false;
                else sb.Append(',');
                string sqlType = property.DBType;
                if (string.IsNullOrEmpty(sqlType))
                    if (property.Type.IsEnum)
                        sqlType = "INT";
                    else if (property.Type == typeof(Rectangle))
                        sqlType = "NVARCHAR(16)";
                    else
                        sqlType =
                            typeof(DataRow).IsAssignableFrom(property.Type) || typeof(DataTable).IsAssignableFrom(property.Type)
                                ? "BIGINT" : sqlTypes[property.Type];
                var sqlName = property.Name;
                var isKey = sqlName == "Id";
                sb.AppendFormat("`{0}` {1} ", sqlName, sqlType);
                if (isKey) sb.Append(" NOT NULL PRIMARY KEY ");
            }
            sb.Append(')');
            return sb.ToString();
        }

        public static void Read()
        {

        }
        private readonly ConcurrentDictionary<long, DataRow> Values = new ConcurrentDictionary<long, DataRow>();
        public static int DPOwner = Register<DataTable, DataRow>("Owner", PropertyAttribute.NonSerializable | PropertyAttribute.NonModifiableByHost | PropertyAttribute.ReadOnly, null, (d, c) => { (c.Owner as DataTable).GetOwner(d, c); });

        private KeyValuePair<long, DataRow>[] _list = new KeyValuePair<long, DataRow>[0];
        private bool Changed;
        public int Count => Values.Count;

        protected abstract void GetOwner(DataBaseStructure d, Path c);

        public DataRow this[long id]
        {
            get
            {
                Values.TryGetValue(id, out DataRow e);
                return e;
            }
            set
            {
                if (!Check(value)) throw null;
                Changed = true;
                if (OnRowAdding(value))
                    Values.AddOrUpdate(id, value, (n, old) => old != value && OnRowRemoving(old, value) ? value : old);
            }
        }
        public abstract DataRow Get(long id, bool createNewIfNotExist);
        public KeyValuePair<long, DataRow>[] AsList()
        {
            if (Changed) { _list = Values.ToArray(); Changed = false; }
            return _list;
        }
        public override void Stringify(Context c) => Stringify(c, null);
        public  void Stringify(Context c, Func<DataRow, bool> selector = null)
        {
            var indexer = c.GetIndexer(this);
            if (indexer.StringifyAsRef(c, out var start))
                return;
            var sb = c.GetBuilder();
            var siasid = true;
            var param = c.GetParameter(GetType(), typeof(DataTable)) as DataTableParameter;
            if (param != null) siasid = param.SerializeItemsAsId;

            base.StringifyDProperties(c, ref start);
            var type = GetType();
            var typeName = type == typeof(JArray) ? "sys.List" : type.FullName;
            if (!start) sb.Append(',');
            sb.Append("\"__type__\":\"").Append(typeName).Append("\",\"__list__\":[");
            start = true;
            foreach (var kv in Values)
            {
                var v = kv.Value;
                if (selector?.Invoke(v) == false) continue;
                if (v == null) continue;
                if (start) start = false;
                else sb.Append(',');
                if (siasid)
                    c.Stringify(v.Id);
                else
                    v.Stringify(c);
            }
            sb.Append(']');
            if (param != null && param.IsFrozen(this).HasValue)
                sb.Append(",\"IsFrozen\":").Append(true.ToString().ToLower());
            sb.Append("}");
        }

        public override void SimulateStringify(Context c)
        {
            var indexer = c.GetIndexer(this);
            if (indexer.IsStringified)
            {
                indexer.SimulateStringify(c);
                return;
            }
            if (indexer.IsReferenced != false)
                if (!(indexer is EIndexer))
                    indexer.SimulateStringify(c);
            foreach (var kv in Values)
            {
                var v = kv.Value;
                v.SimulateStringify(c);
            }
        }
        public DataRow Owner
        {
            get => get<DataRow>(DPOwner);
            set => set(DPOwner, value);
        }


        public DataTable(DataRow owner)
        {
            set(DPOwner, owner);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            var x = Values.ToArray();
            return x.GetEnumerator();
        }

        protected void Add(DataRow t)
        {
            this[t.Id] = t;
        }
        protected abstract bool Check(DataRow t);

        protected virtual bool OnRowAdding(DataRow row) { return true; }
        protected virtual bool OnRowRemoving(DataRow old, DataRow value) { return true; }

        public void Set(DataTable factures)
        {
            Values.Clear();
            foreach (KeyValuePair<Guid, DataRow> facture in factures)
                Values.GetOrAdd(facture.Value.Id, facture.Value);
            Changed = true;
        }


        protected bool remove(DataRow art)
        {   
            var t = Values.TryRemove(art.Id, out art);
            if (deletedItems != null) deletedItems.AddOrUpdate(art.Id, DateTime.Now, (k, v) => v);
            Changed |= t;
            return t;
        }
        public Message Remove(long id)
        {
            Changed |= Values.TryRemove(id, out DataRow msg);
            if (deletedItems != null) deletedItems.AddOrUpdate(id, DateTime.Now, (x, v) => { return v; });
            return msg as Message;
        }
        public ConcurrentDictionary<long, DateTime> deletedItems;
        
        public abstract JValue CreateItem(long id);

        public void AddNewRow(Database d, object[] ps)
        {
            var t = Get((long)ps[DataRow.DpId], true);
            var e = t.GetProperties();
            for (int i = 0; i < ps.Length; i++)
            {
                var x = e[i];
                var val = ps[i];
                if (val == DBNull.Value) val = null;
                if (typeof(DataRow).IsAssignableFrom(x.Type))
                {
                    if (val != null)
                    {
                        d[x.Type].Get(((IConvertible)val).ToInt64(null), true);
                        d.ItemConstraints.Add(new Path { Id = ((IConvertible)val).ToInt64(null), Owner = t, Property = x });
                    }
                }
                else if (typeof(DataTable).IsAssignableFrom(x.Type))
                {
                    d.ListConstraints.Add(new Path { Id = val == null ? -1 : ((IConvertible)val).ToInt64(null), Owner = t, Property = x });
                }
                else
                {
                    var c = x.Converter;
                    if (c.IsComplex)
                        ((ComplexConverter)x.Converter).ToCsValue(null, x, t, val);
                    else t.set(x.Index, ((BasicConverter)x.Converter).ToCsValue(val));
                }
            }
            Add(t);
        }
        public static bool isHandled;
        internal void Read(Database d, SqlDataReader sqlDataReader)
        {
            var t = new object[sqlDataReader.FieldCount];
            while (sqlDataReader.Read())
            {
                sqlDataReader.GetValues(t);
                AddNewRow(d, t);
            }
        }

        internal void Save(DbConnection sql)
        {
            using (var cmd = sql.CreateCommand())
             foreach (var pair in AsList())
                {
                    cmd.Parameters.Clear();
                    int i = 0;
                    var r = pair.Value;
                    try
                    {
                        cmd.CommandText = r.GetInsert(cmd,ref i);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            cmd.CommandText = r.GetUpdate(cmd,ref i);
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            MyConsole.WriteLine(e.Message);
                        }
                    }
                }
        }

        public void Clear()
        {
            this.Values.Clear();
            this.Changed = true;
        }

        public abstract Type GetItemType();

        public DataTable Read(DatabaseUpdator d, DbDataReader p, bool dispose)
        {
            if (p == null) return this;
            try
            {
                var ps = new object[p.FieldCount];
                
                while (p.Read())
                {
                    p.GetValues(ps);
                    var row = Get((long)ps[DataRow.DpId], true);
                    d.UpdateRow(row, ps);
                    this[row.Id] = row;
                }
            }
            catch (Exception)
            {
            }
            if (dispose)
                p.Close();
            return this;
        }
        public DataRow ReadScalar(DatabaseUpdator d,DbDataReader p,bool dispose)
        {
            if (p == null) return null;
            DataRow row = null;
            try
            {
                var ps = new object[p.FieldCount];
                if (p.Read())
                {
                    p.GetValues(ps);
                    row = Get((long)ps[DataRow.DpId], true);
                    d.UpdateRow(row, ps);
                }
            }
            catch (Exception)
            {
            }
            if (dispose)
                p.Close();
            return row;
        }
        public bool SendUpdates(RequestArgs args)
        {
            Context c = args.JContext;
            var s = UpdateLastAccess(args);
            if (s < new DateTime(621360004000000000)) s = default;
            var e = AsList();
            var x = deletedItems?.ToArray();
            var b = c.GetBuilder();
            b.Append("{\"__service__\":\"updater\",\"date\":" + StringifyString(DateTime.Now.ToLocalTime().ToString()) + ",\"table\":\"" + Name.ToLowerInvariant() + "\",\"dropRequest\":true,\"sdata\":");
            b.Append('{');
            var st = true;
            for (int i = e.Length - 1; i >= 0; i--)
            {
                var pr = e[i].Value;
                if (pr.LastModified > s)
                {
                    if (st) { b.Append("\""); st = false; }
                    else b.Append(",\"");
                    b.Append(pr.LastModified - s).Append("\":");
                    pr.Stringify(c);
                }
            }
            if (x != null)
            {
                for (int i = x.Length - 1; i >= 0; i--)
                {
                    var t = x[i];
                    if (t.Value > s)
                    {
                        if (st) { b.Append("\""); st = false; }
                        else b.Append(",\"");
                        b.Append(t.Value - s).Append("\":").Append(t.Key);
                    }
                }
            }
            b.Append("},\"iss\":").Append("true}");
            args.Send(b.ToString());
            return true;
        }
        private DateTime UpdateLastAccess(RequestArgs args)
        {
            var s = getDateFromRaw(args);
            args.Client.SetCookie(Name + "_lasttimeupdated", DateTime.Now, args.Server.ExpiredTime);
            return s;
        }

        private DateTime getDateFromRaw(RequestArgs args)
        {
            var date = args.GetParam("Date");
            if (date != null)
                if (DateTime.TryParse(date.Replace("%20", " "), out var d)) return d;
                else if (long.TryParse(date, out var l)) return l.FromJSDate();
            var t = args.Client.GetCookie(Name + "_lasttimeupdated", false);
            if (t is DateTime)
                return (DateTime)t;
            else
                return args.Server.StartTime;
        }
    }
    public class cc<T>
    {
        public string PropertyName;
        public Type Type;
        public Func<T, object> getValue;
        public cc(string PropertyName, Func<T, object> getValue, Type type)
        {
            this.PropertyName = PropertyName;
            this.getValue = getValue;
            Type = type;
        }
    }
    abstract public class DataTable<T> : DataTable, ISerializable where T : DataRow, new()
    {
        public override Type GetItemType() { return typeof(T); }
        public new static int __LOAD__(int dp) => DataTable.__LOAD__(DPOwner);
        public List<T> GetNewItems(DataTable<T> sec, ref List<T> oldItems)
        {
            if (oldItems == null) oldItems = new List<T>();
            var x = new List<T>(sec.Count);
            var e = sec.AsList();
            for (int i = 0; i < e.Length; i++)
            {
                var c = e[i].Value as T;
                if (this[c.Id] == null) x.Add(c);
                else oldItems.Add(c);
            }
            return x;
        }
        public virtual int Repaire(Database db)
        {
            int err = 0;
            var l = AsList();
            for (int i = 0; i < l.Length; i++)
            {
                try
                {
                    err += l[i].Value.Repaire(db);
                }
                catch
                {
                    err += 1;
                }
            }
            return err;
        }
        public override DataRow Get(long id, bool createIfNotExist)
        {
            var n = base[id];
            if (n == null  && createIfNotExist)
            {
                n = CreateNew();
                n.Id = id;
                base[id] = n;
            };
            return n;
        }
        protected virtual T CreateNew()
        {
            return new T();
        }
        public override JValue CreateItem(long id)
        {
            return new T { Id = id };
        }
        const int a = 4 | 1;
        public bool Remove(T art)
        {
            return remove(art);
        }
        protected override bool Check(DataRow t)
        {
            return t is T;
        }
        public new T this[long id]
        {

            get => (T)base[id];
            set => base[id] = value;
        }

        public System.Data.DataTable ToDataset(params cc<T>[] props)
        {
            var dt = new System.Data.DataTable(Name);

            var prs = DObject.GetProperties<T>();
            foreach (var pr in prs)
                dt.Columns.Add(new DataColumn(pr.Name, pr.Type));
            foreach (var pr in props)
                dt.Columns.Add(new DataColumn(pr.PropertyName, pr.Type));


            foreach (var row in this.AsList())
                dt.Rows.Add(
                    fillRow((T)row.Value, props,
                    fillRow((T)row.Value, prs, dt.NewRow())));
            return dt;
        }

        public System.Data.DataTable ToCostumizedDataset(params cc<T>[] props)
        {
            var dt = new System.Data.DataTable(Name);
            foreach (var pr in props)
                dt.Columns.Add(new DataColumn(pr.PropertyName, pr.Type));
            foreach (var row in AsList())
                dt.Rows.Add(fillRow((T)row.Value, props, dt.NewRow()));
            return dt;
        }
        public static System.Data.DataTable ToCostumizedDataset(T[] table, params cc<T>[] props)
        {
            var dt = new System.Data.DataTable();
            foreach (var pr in props)
                dt.Columns.Add(new DataColumn(pr.PropertyName, pr.Type));
            for (int i = 0; i < table.Length; i++)
            {
                var row = table[i];
                dt.Rows.Add(fillRow((T)row, props, dt.NewRow()));
            }
            return dt;
        }
        static System.Data.DataRow fillRow(T row, DProperty[] prs, System.Data.DataRow xml)
        {
            for (int i = 0; i < prs.Length; i++)
                xml[i] = row.Get(i);
            return xml;
        }

        static System.Data.DataRow fillRow(T row, cc<T>[] prs, System.Data.DataRow xml)
        {
            for (int i = 0; i < prs.Length; i++)
                xml[prs[i].PropertyName] = prs[i].getValue(row);
            return xml;
        }
        protected DataTable(Context c, JValue jv)
            : base(null)
        {
            JArray list;
            var ja = jv as JObject;
            if (ja != null)
                list = (JArray)ja["__list__"];
            else if (jv is JArray)
                list = (JArray)jv;
            else return;

            foreach (var jValue in list._values)
            {
                var i = (T)jValue;
                if (i != null)
                    base[i.Id] = i;
            }
        }

        protected DataTable(DataRow owner)
            : base(owner)
        {
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }

        public virtual JValue Parse(JValue json) { return json; }

        public void Push(T item)
        {
            base[item.Id] = item;
        }
        public void Add(T t)
        {
            this[t.Id] = t;
        }
        //protected  bool OnRowAdding(DataRow row)
        //{
        //    if (Owner is Database)
        //    {
        //        //row.PropertyChanged = RowPropertyChanged;
        //        TableRowsChanged(row, true);
        //    }
        //    return true;
        //}
        //protected  bool OnRowRemoving(DataRow old, DataRow @new)
        //{
        //    if (Owner is Database)
        //    {
        //        if (old != null)
        //        {
        //            //old.PropertyChanged = null;
        //            //TableRowsChanged(old, false);
        //        }
        //        if (@new != null)
        //        {
        //            //@new.PropertyChanged = RowPropertyChanged;
        //            //TableRowsChanged(old, true);
        //        }
        //    }
        //    return true;
        //}

        //protected virtual void RowPropertyChanged(DataRow d, DProperty p)
        //{
        //    //if (isHandled) return;
        //    //changes.Add(d);
        //}
        //protected void TableRowsChanged(DataRow src, bool isAdded_Removed)
        //{
        //    //if (isHandled) return;
        //    //(isAdded_Removed ? added : removed).Add(src);
        //    //changes.Add(src);
        //}
        //private long getDateFromRaw(RequestArgs args)
        //{
        //    var date = args.GetParam("Date");
        //    if (date != null)
        //        if (DateTime.TryParse(date.Replace("%20", " "), out var d)) return d.Ticks;
        //        else if (long.TryParse(date, out var l)) return l;
        //    var t = args.Client.GetCookie(Name + "_lasttimeupdated", false);
        //    if (t is long)
        //        return (long)t;
        //    else
        //        return args.Server.StartTime;
        //}
        //private long UpdateLastAccess(RequestArgs args)
        //{
        //    var s = getDateFromRaw(args);
        //    args.Client.SetCookie(Name + "_lasttimeupdated", DateTime.Now, args.Server.ExpiredTime);
        //    return s;
        //}
        //public bool SendUpdates(RequestArgs args)
        //{
        //    Context c = args.JContext;
        //    long s = UpdateLastAccess(args);
        //    if (s < 621360004000000000) s = 0;
        //    var e = AsList();
        //    var x = deletedItems?.ToArray();
        //    var b = c.GetBuilder();
        //    b.Append("{\"__service__\":\"updater\",\"date\":"+ StringifyString(DateTime.Now.ToLocalTime().ToString())+",\"table\":\"" + Name.ToLowerInvariant() + "\",\"dropRequest\":true,\"sdata\":");
        //    b.Append('{');
        //    var st = true;
        //    for (int i = e.Length - 1; i >= 0; i--)
        //    {
        //        var pr = e[i].Value;
        //        if (pr.LastModified > s)
        //        {
        //            if (st) { b.Append("\""); st = false; }
        //            else b.Append(",\"");
        //            b.Append(pr.LastModified - s).Append("\":");
        //            pr.Stringify(c);
        //        }
        //    }
        //    if (x != null)
        //    {
        //        for (int i = x.Length - 1; i >= 0; i--)
        //        {
        //            var t = x[i];
        //            if (t.Value > s)
        //            {
        //                if (st) { b.Append("\""); st = false; }
        //                else b.Append(",\"");
        //                b.Append(t.Value - s).Append("\":").Append(t.Key);
        //            }
        //        }
        //    }
        //    b.Append("},\"iss\":").Append("true}");
        //    args.Send(b.ToString());
        //    return true;
        //}

    }
}