using System;
using System.Collections.Generic;
using System.Reflection;
using Json;
using models;
using System.Text;
using System.Collections;
namespace Server
{
    [Flags]
    public enum PropertyAttribute
    {
        None = 0,
        NonSerializable = 2,
        SerializeAsId = 4,
        NonModifiableByHost = 8,
        ReadOnly = 16,
        DeserializeById = 32,
        Private = NonModifiableByHost | NonSerializable,
        AsId = DeserializeById | SerializeAsId,
        Optional = 64,
        UnUploadable = 128
    }
    public delegate object Convert(object v);
    public delegate void ComplexConvert(Context c, DProperty p, DObject owner, object v);
    abstract public class Converter
    {
        public readonly Convert ToDbValue;
        public readonly Boolean IsComplex;
        public readonly String DBType;
        public Converter(Convert ToDbValue, String dbType, int len = -1)
        {
            DBType = dbType;
            this.ToDbValue = ToDbValue;
            IsComplex = GetType() != typeof(BasicConverter);
            Length = len == -1 ? DProperty.getSizeOf(dbType) : len;
        }
        public readonly int Length;
    }
    public class BasicConverter : Converter
    {
        public readonly Convert ToCsValue;
        public BasicConverter(String dbType, Convert ToCsValue, Convert ToDbValue) : base(ToDbValue, dbType)
        {
            this.ToCsValue = ToCsValue;
        }
    }
    public class ComplexConverter : Converter
    {
        public readonly ComplexConvert ToCsValue;
        public ComplexConverter(String dbType, ComplexConvert ToCsValue, Convert ToDbValue)
            : base(ToDbValue, dbType)
        {
            this.ToCsValue = ToCsValue;
        }
    }
    public delegate void Constraint(DataBaseStructure d, Path c);

    public class DBType
    {
        private static Dictionary<string, DBType> types = new Dictionary<string, DBType>();
        public static DBType GetDBType(string s)
        {
            var length = getSizeOf(s = s.ToUpperInvariant(), out var type);
            s = length > 0 ? $"{type}({length})" : type;
            if (!types.TryGetValue(s, out var db))
                types.Add(s, db = new DBType(type, length));
            return db;
        }
        public string Type { get; }
        public int Length { get; }
        public bool IsArray { get; }
        private DBType(string type, int length)
        {
            IsArray = HasLength(Type = type.ToUpperInvariant());
            Length = length;
        }
        private DBType(string dbType)
        {
            Length = getSizeOf(dbType.ToUpperInvariant(), out var type);
            IsArray = HasLength(Type = type);
        }
        public static int getSizeOf(string dbType, out string type)
        {
            var DBTYPE = new System.Text.RegularExpressions.Regex(@"(?<type>\w*)(\((?<length>\d+)\))?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var c = DBTYPE.Match(dbType).Groups;
            var v = c["length"];
            var t = c["type"];
            int l = -1;
            if (!t.Success) throw null;
            type = t.Value;
            if (v.Success)
                if (int.TryParse(v.Value, out l)) return l;
            switch (type.ToUpper().Trim())
            {
                case "CHAR":
                    return 1;
                case "VARCHAR":
                    return 1;
                case "NVARCHAR":
                    return 1;
                case "TINYTEXT":
                    return 255;
                case "TEXT":
                    return 65535;
                case "BLOB":
                    return 65535;
                case "MEDIUMTEXT":
                    return 16777215;
                case "MEDIUMBLOB":
                    return 16777215;
                case "LONGTEXT":
                    return int.MaxValue;
                case "LONGBLOB":
                    return int.MaxValue;
                default:
                    break;
            }
            return HasLength(type) ? getDefaultSize(type) : 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DBType)) return false;
            var type = (DBType)obj;
            return this == type;
        }

        public override int GetHashCode()
        {
            var hashCode = 1182903210;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            return hashCode;
        }
        public static implicit operator DBType(string s) => GetDBType(s);
        public static implicit operator string(DBType s) => s.ToString();

        public static bool operator ==(DBType x, DBType y)
        {
            if ((x.Type == "NVARCHAR" && y.Type == "VARCHAR") || (y.Type == "NVARCHAR" && x.Type == "VARCHAR")) { }
            else if (x.Type != y.Type) return false;
            
            if (!x.IsArray) return true;
            if (x.Length == y.Length) return true;
            if (x.Length < 2 && y.Length < 2) return true;
            return false;
        }
        private static int getDefaultSize(string type)
        {
            switch (type.ToUpper().Trim())
            {
                case "CHAR":
                    return 1;
                case "VARCHAR":
                    return 1;
                case "NVARCHAR":
                    return 1;
                case "TINYTEXT":
                    return 255;
                case "TEXT":
                    return 65535;
                case "BLOB":
                    return 65535;
                case "MEDIUMTEXT":
                    return 16777215;
                case "MEDIUMBLOB":
                    return 16777215;
                case "LONGTEXT":
                    return int.MaxValue;
                case "LONGBLOB":
                    return int.MaxValue;
                case "BIT":
                    return 1;
                default:
                    return 1;
            }
        }
        public static bool HasLength(string TYPE)
        {
            switch (TYPE)
            {
                case "BIT":
                case "CHAR":
                case "VARCHAR":
                case "NVARCHAR":
                case "BLOB":
                case "BINARY":
                case "NTEXT":
                    return true;
                default:
                    return false;
            }
        }

        public static bool operator !=(DBType x, DBType y) => !(x == y);
        public override string ToString()
        {
            if (!this.IsArray || Length < 2) return Type;
            return $"{Type}({Length})";
        }
    }

    public class DProperty
    {
        public readonly PropertyAttribute Attribute;
        public readonly string Name;
        public readonly int Index;
        public readonly Type Type;
        public readonly Type Owner;
        public readonly OnChanged OnChanged;
        public readonly Constraint OnUpload;
        private readonly string _dbType;
        internal readonly int _length;
        
        public static System.Text.RegularExpressions.Regex DBTYPE = new System.Text.RegularExpressions.Regex(@"(?<type>\w*)(?<length>\(\d+\))?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        public static int getSizeOf(string dbType)
        {
            DBTYPE = new System.Text.RegularExpressions.Regex(@"(?<type>\w*)(\((?<length>\d+)\))?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var c = DBTYPE.Match(dbType).Groups;
            var v = c["length"];
            var t = c["type"];
            int l = -1;
            if (v.Success)
                if (int.TryParse(v.Value, out l)) return l;
            if (!t.Success) throw null;
            var type = t.Value;
            switch (type.ToUpper().Trim())
            {
                case "CHAR":
                    return 1;
                case "VARCHAR":
                    return 1;
                case "NVARCHAR":
                    return 1;
                case "TINYTEXT":
                    return 255;
                case "TEXT":
                    return 65535;
                case "BLOB":
                    return 65535;
                case "MEDIUMTEXT":
                    return 16777215;
                case "MEDIUMBLOB":
                    return 16777215;
                case "LONGTEXT":
                    return int.MaxValue;
                case "LONGBLOB":
                    return int.MaxValue;
                default:
                    break;
            }
            return 0;
        }
        public DProperty(Type owner, Type propType, string propName, PropertyAttribute attribute, int index, OnChanged OnChanged, Constraint OnUpload, string dbType, int length = -1)
        {
            Owner = owner;
            Name = propName;
            Type = propType;
            Index = index;
            Attribute = attribute;
            this.OnChanged = OnChanged;
            this.OnUpload = OnUpload;
            _dbType = dbType;
            _length = dbType == null ? -1 : length == -1 ? getSizeOf(dbType) : length;
        }
        public static void db2dtrw(Context c, DProperty p, DObject o, object v)
        {
            if (v == null)
                o.set(p.Index, null);
            else if (v is JNumber)
                p.OnUpload(c.Database, new Path { Id = (long)((JNumber)v).Value, Owner = o, Property = p });
            else if (v is DataRow)
                o.set(p.Index, v);
            else
            {
                if (v is JSON x)
                {
                    if (c.Serializers.TryGetValue(x.Type ?? p.Type.FullName, out Typeserializer ts))
                    {
                        var vl = ts.Swap(c, x, false);
                        if (x.Ref != null)
                            x.Ref.Value = vl;
                        o.set(p.Index, vl);
                    }
                    else
                    {
                        throw null;
                    }
                }
                else
                {
                    Typeserializer ts;
                    if (c.Serializers.TryGetValue(p.Type.FullName, out ts))
                    {
                        var vl = ts.Swap(c, (JValue)v, false);
                        o.set(p.Index, vl);
                    }
                    throw null;
                }
            }
        }
        private static object dtrw2db(object p)
        {
            var t = (DataRow)p;
            if (t == null) return "null";
            return t.Id;
        }
        private static object dttb2db(object p)
        {
            var t = (DataTable)p;
            if (t == null || t.Owner == null) return "null";
            return t.Owner.Id;
        }
        private Converter todbv;
        internal Converter Converter
        {
            get
            {
                if (todbv == null)
                    todbv = GetDbConverter(Type);
                return todbv;
            }
        }
        private static Converter DataRowConverter = new ComplexConverter("BIGINT", db2dtrw, dtrw2db);
        private static Converter DataTableConverter = new ComplexConverter("BIGINT", db2dttb, dttb2db);
        private static BasicConverter JValueConverter = new BasicConverter("text", p => p as JValue, p => (p as JValue)?.ToString());
        private static void db2dttb(Context c, DProperty p, DObject owner, object v)
        {
            if (v == null || v is DataTable)
                owner.set(p.Index, v);
        }
        public static Converter GetDbConverter(Type Type)
        {
            if (typeof(DataRow).IsAssignableFrom(Type))
                return DataRowConverter;
            if (typeof(DataTable).IsAssignableFrom(Type))
                return DataTableConverter;
            if (typeof(JValue).IsAssignableFrom(Type))
                return JValueConverter;
            return _store[Type];
        }
        private static Dictionary<Type, Converter> _store = new Dictionary<Type, Converter>();
        private static Dictionary<DProperty, string> dbtypes = new Dictionary<DProperty, string>();
        public string DBType => _dbType ?? Converter.DBType;
        public int Length => _length == -1 ? Converter.Length : _length;
        public bool IsOptional => (Attribute & PropertyAttribute.Optional) == PropertyAttribute.Optional;
        static DProperty()
        {
            var numberToString = Context.FloatVirguleChar == '.' ? new Convert(p => p.ToString()) : new Convert(p => p.ToString().Replace(Context.FloatVirguleChar, '.'));

            _store.Add(typeof(string), new BasicConverter("NVARCHAR(25)", p => p == null ? null : p is JString ? ((JString)p).Value : ((IConvertible)p).ToString(), p => "\'" + p + "\'"));
            _store.Add(typeof(int), new BasicConverter("INT", p => p == null ? 0 : ((IConvertible)p).ToInt32(null), p => p));
            _store.Add(typeof(long), new BasicConverter("BIGINT", p => p == null ? 0L : ((IConvertible)p).ToInt64(null), p => p));
            _store.Add(typeof(bool), new BasicConverter("BIT", p => p == null ? false : p is bool ? p : p is JBool ? ((JBool)p).Value : (p is IConvertible) ? ((IConvertible)p).ToBoolean(null) : false, p => (bool)p ? "1" : "0"));
            _store.Add(typeof(float), new BasicConverter("FLOAT", p => p == null ? 0f : ((IConvertible)p).ToSingle(null), numberToString));
            _store.Add(typeof(double), new BasicConverter("DOUBLE", p => p == null ? 0.0 : ((IConvertible)p).ToDouble(null), numberToString));
            _store.Add(typeof(decimal), new BasicConverter("DECIMAL(15,2)", p => p == null ? new decimal() : ((IConvertible)p).ToDecimal(null), numberToString));
            _store.Add(typeof(DateTime), new BasicConverter("DATETIME", toData, p => { var d = (DateTime)p; return d.Year < 1900 ? "null" : "\'" + d.ToString(true ? "yyyy-MM-dd HH:mm" : "MM/dd/yyyy HH:mm:ss") + "\'"; }));
            _store.Add(typeof(BonType), new BasicConverter("INT", p => toBonType(p), p => (int)(BonType)p));
            _store.Add(typeof(TransactionType), new BasicConverter("INT", p => toTransactionType(p), p => (int)(BonType)p));
            _store.Add(typeof(Job), new BasicConverter("INT", p => toJob(p), p => (int)(Job)p));
            _store.Add(typeof(NatureProduct), new BasicConverter("INT", p => toNatureProduct(p), p => (int)(NatureProduct)p));
            _store.Add(typeof(Unity), new BasicConverter("INT", p => toUnity(p), p => (int)(Unity)p));
            _store.Add(typeof(Abonment), new BasicConverter("INT", p => toAbonment(p), p => (int)(Abonment)p));
            _store.Add(typeof(AgentPermissions), new BasicConverter("INT", p => toPermissions(p), p => (int)(AgentPermissions)p));
            _store.Add(typeof(VersmentType), new BasicConverter("INT", p => toVersmentType(p), p => (int)(VersmentType)p));
            _store.Add(typeof(MessageType), new BasicConverter("INT", p => toMessageType(p), p => (int)(MessageType)p));
            _store.Add(typeof(Rectangle), new BasicConverter("BINARY(32)", (x) => { return null; }, (x) => { return null; }));
        }
        private static Unity toUnity(object p)
        {
            p = toEnum<Unity>(p);
            return p is int ? (Unity)(int)p : (Unity)p;
        }
        private static NatureProduct toNatureProduct(object p)
        {
            p = toEnum<NatureProduct>(p);
            return p is int ? (NatureProduct)(int)p : (NatureProduct)p;
        }
        private static MessageType toMessageType(object p)
        {
            p = toEnum<MessageType>(p);
            return p is int ? (MessageType)(int)p : (MessageType)p;
        }
        private static VersmentType toVersmentType(object p)
        {
            p = toEnum<VersmentType>(p);
            return p is int ? (VersmentType)(int)p : (VersmentType)p;
        }
        private static object toData(object p)
        {
            DateTime dt;
            if (p == null) return DateTime.MinValue;
            var t = p as JString;
            if (t != null)
                if (DateTime.TryParse(t.Value, out dt)) return dt;
                else return DateTime.MinValue;
            var n = p as JNumber;
            if (n != null)
                return new DateTime((long)n.Value);
            if (p is IConvertible)
                return ((IConvertible)p).ToDateTime(null);
            return DateTime.MinValue;
        }
        private static Abonment toAbonment(object p)
        {
            p = toEnum<Abonment>(p);
            return p is int ? (Abonment)(int)p : (Abonment)p;
        }
        private static AgentPermissions toPermissions(object p)
        {
            p = toEnum<AgentPermissions>(p);
            return p is int ? (AgentPermissions)(int)p : (AgentPermissions)p;
        }
        private static Job toJob(object p)
        {
            p = toEnum<Job>(p);
            return p is int ? (Job)(int)p : (Job)p;
        }
        private static BonType toBonType(object p)
        {
            p = toEnum<Abonment>(p);
            return p is int ? (BonType)(int)p : (BonType)p;
        }
        private static TransactionType toTransactionType(object p)
        {
            p = toEnum<TransactionType>(p);
            return p is int ? (TransactionType)(int)p : (TransactionType)p;
        }
        private static object toEnum<T>(object p) where T : struct
        {
            if (p is JNumber) return (int)((JNumber)p).Value;
            if (p is JString)
            {
                T x;
                if (Enum.TryParse(((JString)p).Value, out x)) return x;
                return 0;
            }
            if (p is string)
            {
                T x;
                if (Enum.TryParse((string)p, out x)) return x;
                return 0;
            }
            if (p is IConvertible)
                return ((IConvertible)p).ToInt32(null);
            if (p is T) return p;
            if (p is IConvertible) return ((IConvertible)p).ToInt32(null);
            return 0;
        }
    }
    public delegate object OnChanged(DObject obj, object _old, object _new);
    public class ObjectPrototype : IEnumerable<DProperty>
    {
        public static Dictionary<Type, ObjectPrototype> garbage = new Dictionary<Type, ObjectPrototype>();
        public static Dictionary<Type, Action<ObjectPrototype>> Initalizator = new Dictionary<Type, Action<ObjectPrototype>>();
        Type ObjectType { get; }
        Type BaseType { get; }
        List<DProperty> Properties { get; }
        public ObjectPrototype(Type baseType, Type objectType)
        {
            ObjectType = objectType;
            BaseType = baseType;
        }
        public void Add(Type PropertyType, string propName, string dbType, PropertyAttribute attribute = PropertyAttribute.None, OnChanged OnChanged = null, Constraint OnUpload = null)
        {
            Properties.Add(new DProperty(ObjectType, PropertyType, propName, attribute, this.Properties.Count, OnChanged, OnUpload, dbType));
            var t = new ObjectPrototype(null, null) { { null, null, null } };
        }
        public IEnumerator<DProperty> GetEnumerator() => Properties.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Properties.GetEnumerator();
    }
    public class DObject : JVar
    {
        public new static int __LOAD__(int dp) => JVar.__LOAD__(dp);
        protected virtual void OnPropertyChanged(DProperty dp)
        {
        }
        public static string ConvertCsTypeToTsType(string csType)
        {
            switch (csType)
            {
                case "Int32":
                    return "Number";
                case "Int8":
                    return "Number";
                case "Int16":
                    return "Number";
                case "UInt32":
                    return "Number";
                case "UInt8":
                    return "Number";
                case "UInt16":
                    return "Number";
                case "UInt64":
                    return "Number";
                case "Int64":
                    return "Number";
                case "Double":
                    return "Number";
                case "Single":
                    return "Number";
                default:
                    return csType;
            }
        }
        private static string GetProperty(PropertyInfo prop)
        {
            string get = "", set = "";
            var type = ConvertCsTypeToTsType(prop.PropertyType.Name);
            if (prop.CanRead)
                get = $"\r\n\tpublic get {prop.Name}(){{return this.get<{type}>({prop.DeclaringType.Name}.DP{prop.Name});}}";
            if (prop.CanWrite || prop.CanRead)
                set = $"\r\n\tpublic set {prop.Name}(v:{type}){{this.set({prop.DeclaringType.Name}.DP{prop.Name},v);}}";
            return get + set;
        }
        public static string ToTSObject(Type dtype)
        {
            StringBuilder
                sb = new StringBuilder($"export class {dtype.Name} extends bind.DObject{{\r\n"),
                props = new StringBuilder(""),
                __fields__ = new StringBuilder("\r\n\tstatic __fields__(){return ["),
                ctor = new StringBuilder("\t\r\nstatic ctor(){");
            var flds = new List<string>();
            foreach (var prop in dtype.GetProperties())
            {
                if (prop.DeclaringType != dtype) continue;
                if (!prop.CanWrite && !prop.CanWrite) continue;
                var type = ConvertCsTypeToTsType(prop.PropertyType.Name);
                flds.Add($"this.DP{prop.Name}");
                sb.AppendFormat($"\r\n\tpublic static DP{prop.Name}:bind.DProperty<{type}, {dtype.Name}>; ");
                props.Append(GetProperty(prop));
                ctor.Append($"\t\tthis.DP{prop.Name} = bind.DObject.CreateField<{type}, {dtype.Name}>(\"{prop.Name}\", {type});\r\n");
            }
            __fields__.Append(string.Join(",", flds)).Append("];}");
            var x = sb.Append(props).Append(__fields__.AppendLine(ctor.Append("}").ToString()).AppendLine("}")).ToString();
            return x;
        }
        public static void clearMap()
        {
            foreach (var map in Map.maps)
                if (map.Key == typeof(DataRow))
                    map.Value.properties = new DProperty[map.Value.properties.Length];
            foreach (var dp in Map.dps)
                dp.Value.Reverse();
        }
        private class Map
        {
            public static Dictionary<Type, Map> maps = new Dictionary<Type, Map>();
            public static Dictionary<Type, List<DProperty>> dps = new Dictionary<Type, List<DProperty>>();
            private static List<DProperty> getPropertiesOf(Type type)
            {
                List<DProperty> ps;
                if (!dps.TryGetValue(type, out ps))
                    dps.Add(type, ps = new List<DProperty>());
                return ps;
            }
            public static readonly Map ObjectMap = new Map(typeof(object)) { isFrozen = true };
            private bool _isFrozen;
            private Map _base;
            public Map BaseMap
            {
                get
                {
                    if (Type == typeof(object)) return null;
                    if (Type.BaseType == typeof(object)) return null;
                    return _base == null ? (_base = maps[Type.BaseType]) : _base;
                }
            }
            private int Add(Type propType, string propName, PropertyAttribute attribute, OnChanged OnChanged, Constraint OnUpload, string dbtype)
            {
                if (isFrozen) throw new Exception("");
                var ps = getPropertiesOf(Type);
                var p = new DProperty(Type, propType, propName, attribute, BaseMap.Count + ps.Count, OnChanged, OnUpload, dbtype);
                ps.Add(p);
                return p.Index;
            }
            public static int Register<T, P>(string propName, PropertyAttribute attribute, OnChanged OnChanged, Constraint OnUpload, string dbType)
            {
                var type = typeof(T);
                var typep = typeof(P);
                if (typeof(DataRow).IsAssignableFrom(typep) && OnUpload == null)
                    throw new Exception("");
                if (typeof(DataTable).IsAssignableFrom(typep) && OnUpload == null)
                    throw new Exception("");
                Map map;
                if (!maps.TryGetValue(type, out map))
                {
                    map = new Map(type);
                    var bmap = TryLoadBaseTypesOf(type.BaseType);
                    bmap.isFrozen = true;
                }
                if (map._isFrozen)
                {
                    var btype = type.BaseType;
                    var x = btype == ObjectMap.Type ? "" : btype.FullName + ".__LOAD__();";
                    throw new Exception(" You must include method protected static __LOAD__(){" + x + $"}} \r\n\t\t{type.FullName}");
                }
                return map.Add(typep, propName, attribute, OnChanged, OnUpload, dbType);
            }
            private static Map TryLoadBaseTypesOf(Type type)
            {
                
                if (type == typeof(object)) return ObjectMap;
                if (maps.ContainsKey(type)) return maps[type];
                var e = type.GetMethod("__LOAD__", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic);
                Map map;
                if (e == null)
                {
                    map = new Map(type);
                    maps[type] = map;
                    var bmap = TryLoadBaseTypesOf(type.BaseType);
                    bmap.isFrozen = true;
                }
                else
                {
                    if (!maps.TryGetValue(type, out map))
                    {
                        map = new Map(type);
                        maps[type] = map;
                        var bmap = TryLoadBaseTypesOf(type.BaseType);
                        bmap.isFrozen = true;
                    }
                    e.Invoke(null, new object[1] { 0 });
                    map.isFrozen = true;
                }
                return map;
            }
            public readonly Type Type;
            public DProperty[] properties;
            private bool isFrozen
            {
                get => _isFrozen;
                set
                {
                    if (_isFrozen) return;
                    if (!value) return;
                    var ps = getPropertiesOf(Type);
                    if (BaseMap != null)
                    {
                        var bps = BaseMap.properties;
                        ps.InsertRange(0, bps);
                    }
                    properties = ps.ToArray();
                    _isFrozen = true;
                    dps.Remove(Type);
                }
            }
            public Map(Type type)
            {
                Type = type;
                maps[type] = this;
            }
            public static Map MapOf(Type type)
            {
                if (type == null) return ObjectMap;
                Map m;
                if (!maps.TryGetValue(type, out m))
                    m = TryLoadBaseTypesOf(type);
                m.isFrozen = true;
                return m;
            }
            public int Count => properties.Length;
            public DProperty IndexOf(string propName)
            {
                var map = this;
                foreach (var p in properties)
                    if (p.Name == propName) return p;
                return null;
            }
            public DProperty NameOf(int propIndex)
            {
                if (propIndex < properties.Length && propIndex >= 0) return properties[propIndex];
                return null;
            }
        }
        public void CopyFrom(DObject x)
        {
            if (this == x) return;
            if (x._map == _map)
            {
                Array.Copy(x._values, 1, _values, 1, _values.Length - 1);
                return;
            }
            var e = x.GetProperties();
            var e1 = GetProperties();
            var l = e.Length > e1.Length ? e1.Length : e.Length;
            for (int i = 1; i < l; i++)
            {
                var p = e1[i];
                if (p.Type != e[i].Type || p.Name != e[i].Name) continue;
                _values[p.Index] = x._values[p.Index];
            }
        }
        protected static int Register<OwnerType, PropertyType>(string propName, PropertyAttribute attribute = PropertyAttribute.None, OnChanged OnChanged = null, Constraint OnUpload = null, string dbType = null) where OwnerType : DObject
        {
            return Map.Register<OwnerType, PropertyType>(propName, attribute, OnChanged, OnUpload, dbType);
        }
        protected static int Register<T, P>(string propName, string dbType, PropertyAttribute attribute = PropertyAttribute.None, OnChanged OnChanged = null, Constraint OnUpload = null) where T : DObject
        {
            return Map.Register<T, P>(propName, attribute, OnChanged, OnUpload, dbType);
        }
        protected readonly object[] _values;
        public DProperty IndexOf(string propName)
        {
            var map = Map.MapOf(GetType());
            return map.IndexOf(propName);
        }
        public DProperty NameOf(int propIndex)
        {
            var map = Map.MapOf(GetType());
            return map.NameOf(propIndex);
        }
        public DObject()
        {
            _values = new object[(_map = Map.MapOf(GetType())).Count];
        }
        public static int GetPropertyCount(Type type)
        {
            var m = Map.MapOf(type);
            return m == null ? 0 : m.Count;
        }
        public DObject(Context c, JValue jv)
            : this()
        {
            c.store.Add(jv, this);
            var e = jv as JObject;
            if (e == null) return;
            var x = GetProperties();
            for (int i = 0; i < x.Length; i++)
            {
                var p = x[i];
                if ((p.Attribute & PropertyAttribute.NonModifiableByHost) == PropertyAttribute.NonModifiableByHost) continue;
                var xm = e[p.Name];
                var cc = p.Converter;
                if (cc.IsComplex)
                    ((ComplexConverter)cc).ToCsValue(c, p, this, xm);
                else set(p.Index, ((BasicConverter)cc).ToCsValue(xm));
            }
        }
        public static void FromJson(DObject t, Context c, JValue jv)
        {
            var e = jv as JObject;
            if (e == null) return;
            var x = t.GetProperties();
            for (int i = 0; i < x.Length; i++)
            {
                var p = x[i];
                t.set(p.Index, c.ToCSType(e[p.Name], p.Type));
            }
        }
        protected object get(int i) { return _values[i]; }
        protected internal T get<T>(int i)
        {
            var v = _values[i]; return v == null ? default(T) : (T)v;
        }
        protected internal object Get(int i)
        {
            var v = _values[i]; return v == null ? null : v;
        }
        protected internal void set(int i, object val)
        {
            var m = _map.properties[i];
            if (val != null)
                if (!m.Type.IsAssignableFrom(val.GetType()))
#if !DEBUG
                    return;
#else
                    throw new Exception("Type Error");
#endif
            _values[i] = val;
        }
        protected void set<T>(int i, T val)
        {
            var m = _map.properties[i];
            if (val != null)
                if (!m.Type.IsAssignableFrom(val.GetType()))
#if !DEBUG
                    return;
#else
                    throw new Exception("Type Error");
#endif
            _values[i] = m.OnChanged == null ? val : m.OnChanged(this, _values[i], val);
            OnPropertyChanged(m);
        }
        public DProperty[] GetProperties()
        {
            return _map.properties;
        }
        public static DProperty[] GetProperties<T>() where T : DObject
        {
            return Map.MapOf(typeof(T)).properties;
        }
        public static DProperty[] GetProperties(Type type)
        {
            return Map.MapOf(type).properties;
        }
        protected void StringifyDProperties(Context c, ref bool start)
        {
            var edp = c.GetParameter(GetType(), typeof(DObject)) as DObjectParameter;
            var fs = edp == null ? false : edp.FullyStringify;
            var x = GetProperties();
            var b = c.GetBuilder();
            for (int i = 0, l = x.Length; i < l; i++)
            {
                var e = x[i];
                var natt = edp == null ? null : edp[e.Index];
                var attr = natt == null ? e.Attribute : natt.Value;
                if (!c.IsLocal)
                {
                    if ((attr & PropertyAttribute.NonSerializable) == PropertyAttribute.NonSerializable)
                        continue;
                }
                var val = get(e.Index);
                if (val == null) continue;
                if (start) start = false;
                else b.Append(',');
                
                b.Append('"' + e.Name + "\":");
                if (!fs && !c.IsLocal)
                    if ((attr & PropertyAttribute.SerializeAsId) == PropertyAttribute.SerializeAsId)
                        if (typeof(DataRow).IsAssignableFrom(e.Type))
                        {
                            b.Append(((DataRow)val).Id.ToString());
                            continue;
                        }
                c.Stringify(val);
            }
            if (edp != null && edp.IsFrozen(this) != null)
            {
                if (!start) b.Append(',');
                b.Append("\"IsFrozen\":").Append(edp.IsFrozen(this).Value.ToString().ToLower());
            }
        }
        public virtual void Dispose()
        {
            for (int i = _values.Length - 1; i >= 0; i--)
                _values[i] = null;
        }
        protected void SimulateStringifyDProperties(Context c)
        {
            var x = GetProperties();
            for (int i = 0, l = x.Length; i < l; i++)
                c.SimulateStringify(get(x[i].Index));
        }
        public override void Stringify(Context c)
        {
            var indexer = c.GetIndexer(this);
            if (indexer.IsStringified)
            {
                indexer.Stringify(c);
                return;
            }
            var s = c.GetBuilder();
            s.Append('{');
            var start = true;
            if (indexer.IsReferenced != false)
                if (!(indexer is EIndexer))
                {
                    indexer.Stringify(c);
                    start = false;
                }
            if (!start) s.Append(",");
            s.Append("\"__type__\":\"").Append(GetType().FullName).Append('\"');
            start = false;
            StringifyDProperties(c, ref start);
            s.Append('}');
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
            SimulateStringifyDProperties(c);
        }
        public virtual IStat SaveStat()
        {
            return new Stat((object[])_values.Clone());
        }
        public virtual bool Restore(IStat stat)
        {
            var e = ((Stat)stat).Values;
            for (int i = _values.Length - 1; i >= 0; i--)
                _values[i] = e[i];
            return true;
        }
        private readonly Map _map;
        public new object this[string name]
        {
            get  { var i = _map.IndexOf(name).Index; return i == -1 ? null : get(i); }
            set
            {
                var i = _map.IndexOf(name).Index;
                if (i == -1) return;
                set(i, value);
            }
        }
        public DProperty GetProperty(string s) => _map.IndexOf(s);
    }
    public interface IStat
    {
    }
    public class Stat : IStat
    {
        public object[] Values;
        public Stat(object[] values)
        {
            Values = values;
        }
    }
}