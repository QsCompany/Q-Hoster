using System;
using System.Collections.Generic;
using System.Text;
using Json;
using models;
using QServer.Serialization;
using Serializers;

namespace Server
{

    public class  JSON:JObject
    {
        public string Type;
        public JRef Ref;
    }

    public delegate  bool Resolve(string type, JObject t);

    [Serializable]
    unsafe public class Context
    {
        public Dictionary<string, Typeserializer> Serializers => server.Serializers;
        private Dictionary<JVar, Indexer> wrefs = new Dictionary<JVar, Indexer>();
        private readonly JArray refs = new JArray();
        private Boolean Optimized;
        private Boolean Convert;
        private Boolean iss;
        private Int32 index;
        private String s;
        public bool IsLocal { get; private set; }
        public char Current => s[index];

        public void Register(Typeserializer x) {
            Serializers.Add(x.Name, x);
        }

        public static bool isAssembly;
        public static Dictionary<string, Typeserializer> __ = new Dictionary<string, Typeserializer>();
        public static Dictionary<string, Typeserializer> __notFound = new Dictionary<string, Typeserializer>();


        private readonly QServer.Core.Server server;

        public Context(bool isLocal,Database database,QServer.Core.Server server)
        {
            this.server = server;
            IsLocal = isLocal;
            s = "";
            Database = database;
        }
        public Database Database;
        private void test()
        {
            Read("{}", false);
        }

        private string readString()
        {
            var l = this.s.Length;
            fixed (char* s = this.s)
            {
                index++;
                var i = index;
                while (i < l)
                {
                    if (s[i] == '"')
                    {
                        var ss = this.s.Substring(index, i - index);
                        index = i + 1;
                        return ss;
                    }
                    i++;
                }
                throw null;
            }
        }
        

        private JString ReadString()
        {
            var l = this.s.Length;
            fixed (char* s = this.s)
            {
                index++;
                var i = index;
                while (i < l)
                {
                    var c = s[i];
                    if (c == '\\') i++;
                    else
                        if (s[i] == '"')
                        {
                            var ss = this.s.Substring(index, i - index);
                            index = i + 1;
                            return new JString(ss);
                        }
                    i++;
                }
                throw null;
            }
        }

        public static string ReadString1(string str,int index,out int cursor)
        {
            var l = str.Length;
            fixed (char* s = str)
            {
                index++;
                var i = index;
                while (i < l)
                {
                    var c = s[i];
                    if (c == '\\') i++;
                    else if (s[i] == '"')
                    {
                        var ss = str.Substring(index, i - index);
                        cursor = i + 1;
                        return ss;
                    }
                    i++;
                }
                throw null;
            }
        }
        private float readNumber()
        {
            var l = this.s.Length;
            fixed (char* s = this.s)
            {
                var i = index;
                while (i < l)
                {
                    var c = s[i];
                    var b = (int)c;
                    if (c == ',' || c == ']' || c == '}')
                    {
                        var nms = this.s.Substring(index, i - index);
                        index = i;
                        float f = 0;
                        if (!float.TryParse(nms, out f)) throw null;
                        return f;
                    }
                    i++;
                }
                return float.NaN;
            }
        }

        private JNumber ReadNumber()
        {
            var l = this.s.Length;
            fixed (char* s = this.s)
            {
                var i = index;
                while (i < l)
                {
                    var c = s[i];
                    var b = (int)c;
                    if (c == ',' || c == ']' || c == '}')
                    {
                        var nms = this.s.Substring(index, i - index);
                        if (FloatVirguleChar != '.')
                            nms = nms.Replace('.', FloatVirguleChar);
                        index = i;
                        decimal f = 0;
                        if (!decimal.TryParse(nms, out f)) return null;
                        return new JNumber(f);
                    }
                    i++;
                }
                return null;
            }
        }
        public static readonly char FloatVirguleChar = (3.5f).ToString()[1];
        private JBool ReadBoolean()
        {
            if (s[index] == 't')
            {
                index += 4;
                return JBool.True;
            }
            index += 5;
            return JBool.False;
        }
        private JArray ReadArray()
        {
            var t = new JArray();
            var ibn = index;
            index++;
        strt:
            switch (s[index])
            {
                case ',':
                    index++;
                    t.Push(_read());
                    goto strt;
                case ']':
                    index++;
                    return t;
                default:
                    t.Push(_read());
                    goto strt;
            }
        }
        //int i = 0;

        private JValue ReadObject()
        {
            var t = new JSON();
            string propName = null;
            JValue val = null, @ref;
            string type = null;
            bool isRef = false;
            bool isArray = false;
            JRef tref = null;
            index++;
            do
            {
                switch (s[index])
                {
                    case ',':
                        index++;
                        goto default;
                    default:
                        propName = readString();
                        if (s[index] == ':')
                        {
                            index++;                        
                            switch (propName)
                            {
                                case "@ref":
                                    var v = ReadRef();
                                    refs[v] = tref = new JRef(t);
                                    t.Ref = tref;
                                    continue;
                                case "__type__":
                                    var tx = _read();
                                    type = tx is JNumber n ? n.Value.ToString() : tx?.ToString();
                                    t.Type = type;
                                    continue;
                                case "__ref__":
                                    var l = readNumber();
                                    @ref = refs[(int)l];
                                    index++;
                                    return @ref;
                                case "__list__":
                                    isArray = true;
                                    break;
                            }
                            val = _read();
                        }
                        else throw null;
                        if (!isRef)
                            t[propName] = val;
                        continue;
                    case '}':
                        index++;
                        if (type == "5")
                        {
                            isArray = true;
                            var xt = t["__value__"];
                            tref.Value = xt;
                            return xt;
                        }
                        if (Convert && type != null)
                        {
                            
                            Typeserializer p;
                            if (Serializers.TryGetValue(type, out p))
                            {
                                var r = p.Swap(this, t, RequireNew != null && RequireNew(type, t));
                                if (tref != null)
                                    tref.Value = r;
                                return r;
                            }
                        }
                        if (isArray)
                        {
                            if (tref != null)
                                tref.Value = t["__list__"];
                            return t["__list__"];
                        }
                        return new JObject(t);
                }
            } while (index < s.Length);
            return val;
        }

        public Resolve RequireNew;
        private int ReadRef()
        {
            if (s[index] == '{') index += 11;            
            var v = ReadNumber();
            if (s[index] == '}') index++;
            return v == null ? 0 : (int)v.Value;
        }
        private Null ReadNull()
        {
            index += 4;
            return null;
        }
        private Undefinned ReadUndefinned()
        {
            index += 9;
            return null;
        }
        private JValue _read()
        {
            readWhiteSpace();
            JValue v;
            if (index >= s.Length) return null;
            switch (s[index])
            {
                case '[':
                    v = ReadArray();
                    break;
                case '{':
                    v = ReadObject(); break;
                case '"':
                    v = ReadString(); break;
                case 't':
                case 'f':
                    v = ReadBoolean(); break;
                case 'u':
                    v = ReadUndefinned(); break;
                case 'n':
                    v = ReadNull(); break;
                default:
                    v = ReadNumber(); break;
            }
            readWhiteSpace();
            return v;
        }

        private void readWhiteSpace()
        {
            next:
            if (index >= s.Length) return;
            switch (s[index])
            {
                case '\t':
                case '\r':
                case '\n':
                case ' ':
                    index++;
                    goto next;
            }
        }

        public JValue Read(string s, bool serialize)
        {
            if (s.Length == 0) return Null.Value;
            index = 0;
            Convert = serialize;
            this.s = s;
            var e = (double)decimal.MaxValue;
            return _read();

        }

        public StringBuilder Stringify(JValue s)
        {
            if (iss) throw null;
            if (s == null) { return new StringBuilder(); }
            iss = true;
            sb.Clear();
            wrefs.Clear();
            Optimized = false;
            s.SimulateStringify(this);
            if (sb.Length != 0) throw null;
            var t = new List<KeyValuePair<JVar, Indexer>>();
            foreach (var l in wrefs)
                if (l.Value.NRefs > 0)
                {
                    l.Value.__ref__ = t.Count;
                    t.Add(l);
                    l.Value.Reset(t.Count);
                }

            wrefs.Clear();
            foreach (var l in t)
                wrefs.Add(l.Key, l.Value);
            Optimized = true;
            s.Stringify(this);
            iss = false;
            return sb;
        }

        private void _write(JValue v)
        {

        }
        public void WriteString(JString v)
        {

        }
        public void WriteNumber()
        {

        }
        public void WriteBoolean()
        {

        }
        public void WriteArray() { }
        public void WriteObject() { }
        public void WriteNull() { }
        public void WriteUndefinned() { }
        private StringBuilder sb = new StringBuilder();


        public Indexer GetIndexer(JVar key)
        {
            Indexer r;
            if (!wrefs.TryGetValue(key, out r))
                if (!Optimized)
                    wrefs.Add(key, r = new Indexer(wrefs.Count));
                else return EIndexer.Value;
            return r;
        }


        public StringBuilder GetBuilder()
        {
            return sb;
        }

        public static Type TString = typeof(string);
        public static Type TFloat = typeof(float);
        public static Type TDateTime = typeof(DateTime);
        public static Type TGuid = typeof(Guid);
        public static Type TBool = typeof(bool);
        public static Type TInt32 = typeof(int);
        public static Type TDouble = typeof(double);
        public static Type TLong = typeof(long);
        public bool CreateNew;


        private string stringifyString(string s)
        {
            return JValue.StringifyString(s);

        }
        private static System.Globalization.CultureInfo EnUSCulter = new System.Globalization.CultureInfo("en-US");
        public StringBuilder Stringify(object p)
        {
            
            if (p == null) {  sb.Append("null"); return sb; }
            var t = p.GetType();
            if (t == TString)
                sb.Append(stringifyString(p.ToString()));
            else if (t == TDateTime) {
                var d = ((DateTime)p).ToUniversalTime();
                sb.Append($"\"{d.Year:d4}-{d.Month:d2}-{d.Day:d2}T{d.Hour:d2}:{d.Minute:d2}:{d.Second:d2}.{d.Millisecond:d3}Z\"");
            }
            else if (t == TBool)
                sb.Append(p.ToString().ToLower());
            else if (t == TFloat || t == TInt32 || t == TDouble || t == TLong)
                sb.Append(((IConvertible)p).ToString(System.Globalization.CultureInfo.InvariantCulture));
            else if (p is JValue)
                ((JValue)p).Stringify(this);
            else
            {
                Typeserializer x;
                if (!Serializers.TryGetValue(t.FullName, out x))
                {
                    if (t.IsEnum)
                        sb.Append((int)Enum.Parse(p.GetType(), ((Enum)p).ToString()));

                    else throw null;
                }
                else
                    x.Stringify(this, p);
            }
            return sb;
        }
        public object ToCSType(JValue p, Type tp)
        {
            if (p == null)
            {
                if (tp.IsClass) return null;
                if (tp == TFloat) return 0f;
                if (tp == TDouble) return 0.0;
                if (tp == TInt32) return 0;
                if (tp == TString || tp == TGuid) return null;
                if (tp == TDateTime) return new DateTime(0);
                if (tp == TBool) return false;
                if (tp == TLong) return 0L;
            }
            else
            {
                var pt = p.GetType();
                if (tp == pt) return p;
                if (p.GetType().IsSubclassOf(tp)) return p;
                if (tp.IsSubclassOf(p.GetType())) return p;
                if (p is JValue)
                    if (p is JString)
                    {
                        if (tp == TString) return ((JString)p).Value;
                        if (tp == TDateTime) return DateTime.Parse(((JString)p));
                        if (tp == TGuid) return Guid.Parse(((JString)p));
                    }
                    else if (p is JNumber)
                        if (tp == TFloat)
                            return (float)((JNumber)p).Value;
                        else if (tp == TLong)
                            return (long)((JNumber)p).Value;
                        else ;
                    else if (p is JBool && tp == TBool)
                        return ((JBool)p).Value;
                    else if (p is Null && tp.IsClass)
                        return null;
                    else if (p is Undefinned && tp.IsClass)
                        return null;
            }
            Typeserializer s;
            if (Serializers.TryGetValue(tp.FullName, out s)) return s.FromJson(this, p);
            if (tp.IsEnum) return tp.GetEnumValues().GetValue(0);
            return null;
        }


        public void SimulateStringify(object p)
        {
            if (p == null)
            {
                return;
            }
            var t = p.GetType();
            if (t == TString || t == TDateTime || t == TGuid)
                return;
            if (t == TFloat || t == TBool || t == TInt32 || t == TDouble || t == TLong)
                return;
            if (p is JValue)
                ((JValue)p).SimulateStringify(this);
            else
            {
                Typeserializer x;
                if (!Serializers.TryGetValue(t.FullName, out x)) return;
                x.SimulateStringify(this, p);
            }
        }

        public Context Reset()
        {
            wrefs.Clear();
            refs.Clear();
            sb.Clear();
            index = 0;
            RequireNew = null;
            store.Clear();
            return this;
            
        }
        private readonly Dictionary<Type, ISerializeParametre> serializeParameter = new Dictionary<Type, ISerializeParametre>();
        internal Dictionary<JValue, JValue> store = new System.Collections.Generic.Dictionary<JValue, JValue>(233);

        public void ResetParameterSerialiers()
        {
            serializeParameter.Clear();
        }
        public void Add(Type type, ISerializeParametre serializeParameter)
        {
            if (serializeParameter.FromType.IsAssignableFrom(type))
                if (this.serializeParameter.ContainsKey(type)) this.serializeParameter[type] = serializeParameter;
                else
                    this.serializeParameter.Add(type, serializeParameter);
            else throw null;
        }
        public ISerializeParametre GetParameter(Type ThisType, Type ScopType)
        {
            var x = (ISerializeParametre)null;
        next:
            if (ThisType == null) return null;
            serializeParameter.TryGetValue(ThisType, out x);
            if (x == null)
            {
                if (ThisType == ScopType) return x;
                ThisType = ThisType.BaseType;
                goto next;
            }
            return x;
        }

        internal void WriteProperty(string p, string __service__)
        {
            throw new NotImplementedException();
        }
    }

}
