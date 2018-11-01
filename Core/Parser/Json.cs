using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

using Server;
using System.Text;

namespace Json
{
    [Serializable]
    sealed public class JRef : JConst<JValue>
    {
        public JRef(JVar value)
            : base(value)
        {
        }


        public override void Stringify(Context c)
        {
            Value.Stringify(c);
        }
    }
    [Serializable]
    abstract public class JValue
    {
        public static int __LOAD__(int dp) => 0;
        public abstract void Stringify(Context c);
        public abstract void SimulateStringify(Context c);

        public abstract void FromJson(Context c, JValue j);
        [DebuggerHidden]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public virtual JValue this[string i]
        {
            get { return null; }
            set { }
        }

        public static unsafe string StringifyString(string s)
        {
            var t = new StringBuilder();
            StringifyString(s, t);
            return t.ToString();
        }
        public static unsafe void StringifyString(string s, StringBuilder b)
        {
            if (s == null)
            {
                b.Append("null");
                return;
            }
            b.Append('"');
            var l = s.Length;
            for (int i = 0; i < l; i++)
            {
                var cc = s[i];
                char.IsControl(cc);
                switch (cc)
                {
                    case '\0':
                        b.Append("\u0000");
                        break;
                    case '"':
                        b.Append("\\\"");
                        continue;
                    case '\\':
                        b.Append("\\\\");
                        continue;

                    case '\b':
                        b.Append("\\b");
                        continue;
                    case '\f':
                        b.Append("\\f");
                        continue;
                    case '\t':
                        b.Append("\\t");
                        continue;
                    case '\n':
                        b.Append("\\n");
                        continue;
                    case '\r':
                        b.Append("\\r");
                        continue;
                    default:
                        if (char.IsControl(cc))
                            b.Append("\\").Append(((UInt16)cc).ToString("X4"));
                        else
                            b.Append(cc);
                        continue;
                }
            }
            b.Append('"');
        }

        
    }
    [Serializable]
    [DebuggerDisplay("{Value}")]
    
    
    abstract public class JConst<T> : JValue
    {
        [DebuggerHidden]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Value { get; set; }
        [DebuggerHidden]
        public JConst(T _value)
        {
            Value = _value;
        }
        [DebuggerHidden]
        public override string ToString()
        {
            return Value == null ? null : Value.ToString();
        }
        [DebuggerHidden]
        public override void SimulateStringify(Context c)
        {

        }

        public override void FromJson(Context c, JValue j)
        {            
        }
    }
    [Serializable]
    [DebuggerDisplay("{Value}")]
    public class JString : JConst<string>
    {
        public JString(string val)
            : base(val)
        {
        }

        public override void Stringify(Context c)
        {
            StringifyString(Value, c.GetBuilder());
        }
        public static implicit operator JString(string r) { return new JString(r); }
        public static implicit operator string(JString r) { return r == null ? null : r.Value; }

        public unsafe static string ReadString(string str)
        {
            var L = str.Length;
            var index = 0;
            fixed (char* s = str)
            {
                index++;
                var i = index;
                while (i < L)
                {
                    var c = s[i];
                    if (c == '\\') i++;
                    else
                        if (s[i] == '"')
                    {
                        var ss = str.Substring(index, i - index);
                        index = i + 1;
                        return ss;
                    }
                    i++;
                }
                return str;
            }
        }
    }
    [Serializable]
    public class  JNumber : JConst<decimal> ,IConvertible
    {
        public JNumber(decimal val)
            : base(val)
        {
        }
        public override void Stringify(Context c)
        {
            c.GetBuilder().Append(Value.ToString());
        }
        public static implicit operator JNumber(decimal r) { return new JNumber(r); }
        public static implicit operator decimal(JNumber r) { return r == null ? decimal.Zero : r.Value; }
        public static implicit operator decimal?(JNumber r) { return r == null ? null : (decimal?)r.Value; }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Decimal;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return Value > 0;
        }

        public byte ToByte(IFormatProvider provider)
        {
            return (byte)Value;
        }

        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToChar(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return new DateTime(((IConvertible)Value).ToInt64(provider));
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return Value;
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDouble(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt64(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToSByte(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToSingle(provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return Value.ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return null;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt16(provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt32(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt64(provider);
        }
    }
    [Serializable]
    public class  JBool : JConst<bool>
    {
        public static JBool True = new JBool(true);
        public static JBool False = new JBool(false);

        public JBool(bool val)
            : base(val)
        {
        }
        public override void Stringify(Context c)
        {
            c.GetBuilder().Append(Value ? "true" : "false");
        }
        public static implicit operator JBool(bool r) { return new JBool(r); }
        public static implicit operator bool?(JBool r) { return r == null ? null : (bool?)r.Value; }
        public static implicit operator bool(JBool r) { return r == null ? false : r.Value; }
    }
    [Serializable]
    abstract public class JVar : JValue
    {
        public static new int __LOAD__(int dp) => JValue.__LOAD__(0) + 1;
        public override void FromJson(Context c, JValue j)
        {
            
        }
    }

    [DebuggerDisplay("Length = {Count}")]
     public  partial class JObject : IDictionary<string, JValue>
    {

        public static new int __LOAD__(int dp) => JValue.__LOAD__(0) + 1;
        public void Add(KeyValuePair<string, JValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(KeyValuePair<string, JValue> item)
        {
            var e = this[item.Key];
            return e == item.Value;
        }

        public void CopyTo(KeyValuePair<string, JValue>[] array, int arrayIndex)
        {
            var e= _values.ToArray();
            for (int i = arrayIndex,j=0; i < array.Length; i++,j++)
                array[i] = e[j];            
        }

        public bool Remove(KeyValuePair<string, JValue> item)
        {
            JValue d;
            return _values.TryRemove(item.Key, out d);
        }
        [DebuggerHidden]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Count { get { return _values.Count; } private set { } }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DebuggerHidden]
        public bool IsReadOnly { get { return false; } private set { } }

        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public void Add(string key, JValue value)
        {
            this[key] = value;
        }

        public bool Remove(string key)
        {
            JValue u;
            return _values.TryRemove(key, out u);
        }

        public bool TryGetValue(string key, out JValue value)
        {
            return _values.TryGetValue(key, out value);
        }
        [DebuggerHidden]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public ICollection<string> Keys { get { return _values.Keys; } private set { } }
        [DebuggerHidden]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public ICollection<JValue> Values { get { return _values.Values; } private set { } }
    }

    [Serializable]
    [DebuggerTypeProxy(typeof(MscorlibDictionaryDebugView))]
     public  partial class JObject : JVar,IEnumerable<KeyValuePair<string, JValue>>
    {

        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static volatile int i;

        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static List<JObject> disp = new List<JObject>();
        ~JObject()
        {
            //disp.Clear();//.Add(this);
            i--;
        }

        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Type JRefType = typeof(JRef);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ConcurrentDictionary<string, JValue> _values;

        [DebuggerHidden]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override JValue this[string i]
        {
            get
            {
                JValue v;
                if (_values.TryGetValue(i, out v)) return v == null ? null : v.GetType() == JRefType ? ((JRef)v).Value : v; return null;
            }
            set
            {
                _values.AddOrUpdate(i, value, (ii, o) => value);
            }
        }

        public JObject()
        {
            _values = new ConcurrentDictionary<string, JValue>();
        }

        public JObject(JSON t)
        {
            _values = t._values;
        }
        public override void Stringify(Context c)
        {
            var isJobj = GetType() == typeof(JObject) || GetType() == typeof(JSON);
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
            {
                var b = s.Length;
                indexer.Stringify(c);
                start = b == s.Length;
            }
            if (!isJobj)
            {
                if (!start) s.Append(',');
                s.Append("\"__type__\"").Append(":\"").Append(GetType().FullName).Append('\"');
                start = false;
            }
            foreach (var kv in _values)
            {
                if (!start) s.Append(",");
                else start = false;
                s.Append('"' + kv.Key + "\":");
                if (kv.Value == null)
                    s.Append("null");
                else kv.Value.Stringify(c);
            }
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
            indexer.SimulateStringify(c);
            foreach (var kv in _values)
                if (kv.Value != null)
                    kv.Value.SimulateStringify(c);
        }


        [DebuggerHidden]        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        [DebuggerHidden]        
        IEnumerator<KeyValuePair<string, JValue>> IEnumerable<KeyValuePair<string, JValue>>.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public override void FromJson(Context c, JValue j)
        {            
        }
    }

    sealed public class MscorlibDictionaryDebugView
    {
        private IDictionary<string, JValue> dict;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<string, JValue>[] Items
        {
            get
            {
                KeyValuePair<string, JValue>[] array = new KeyValuePair<string, JValue>[dict.Count];
                dict.CopyTo(array, 0);
                return array;
            }
        }

        public MscorlibDictionaryDebugView(IDictionary<string, JValue> dictionary)
        {
            dict = dictionary;
        }
    }
    sealed public class MscorlibCollectionDebugView
    {
        private ICollection<JValue> collection;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public JValue[] Items
        {
            get
            {
                var array = new JValue[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }
        }

        public MscorlibCollectionDebugView(ICollection<JValue> collection)
        {
            if (collection == null)
                return;
            this.collection = collection;
        }
    }
    
     public  partial class JArray:ICollection<JValue>
    {

        public void Add(JValue item)
        {
            _values.Add(item);
        }

        void ICollection<JValue>.Clear()
        {
            _values.Clear();
        }

        public bool Contains(JValue item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(JValue[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public int Count => _values.Count;

        public bool IsReadOnly => false;

        public bool Remove(JValue item)
        {
            return _values.Remove(item);
        }
    }

    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(MscorlibCollectionDebugView))]
     public  partial class JArray : JVar,IEnumerable<JValue>
    {

     
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static readonly Type JRefType = typeof(JRef);

     
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly List<JValue> _values = new List<JValue>();

     
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public JValue this[int i]
        {
            get
            {
                if (i < 0) return null;
                if (i < _values.Count)
                {
                    var v = _values[i];
                    return v == null ? null : (v.GetType().Equals(JRefType) ? ((JRef)v).Value : v);
                }
                return null;
            }
            set
            {
                if (i < 0)
                    return;
                if (i >= _values.Count)
                    _values.AddRange(new JValue[i - _values.Count + 1]);
                _values[i] = value;
            }
        }

     
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Length => _values.Count;

        public override void Stringify(Context c)
        {
            var indexer = c.GetIndexer(this);
            if (indexer.IsStringified)
            {
                indexer.Stringify(c);
                return;
            }
            var sb = c.GetBuilder();
            sb.Append('{');
            
            var type = GetType();
            string typeName;
            if (type == typeof(JArray))
                typeName = "sys.List";
            else typeName = type.FullName;
            if (indexer.IsReferenced != false)
                if (!(indexer is EIndexer))
                {
                    indexer.Stringify(c);
                    sb.Append(',');
                }
            
            sb.Append("\"__type__\":\"").Append(typeName).Append('"');
            sb.Append(",\"__list__\":[");
            for (int i = 0,l=_values.Count; i < l; i++)
            {
                var v = _values[i];
                if (i != 0) sb.Append(',');
                if (v == null) sb.Append("null");
                else v.Stringify(c);
            }
            sb.Append("]");
            StringifyProperties(c);
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
                
            
            for (int i = 0, l = _values.Count; i < l; i++)
            {
                var v = _values[i];
                if (v != null)
                    v.SimulateStringify(c);
            }
            SimulateStringifyProperties(c);            
        }

        protected virtual void StringifyProperties(Context c)
        {
        }
        protected virtual void SimulateStringifyProperties(Context c)
        {
        }
        public void Push(JValue jValue)
        {
            _values.Add(jValue);
        }
        public void Push(JArray jValue)
        {
            _values.AddRange(jValue._values);
        }
        public void Push(JArray[] jValue)
        {
            _values.AddRange(jValue);
        }
        public void Clear()
        {
            _values.Clear();
        }
        public JValue Pop()
        {
            var i = _values.Count - 1;
            if (i < 0) return null;
            var v = _values[i];
            _values.RemoveAt(i);
            return v;
        }

        [DebuggerHidden]
        
        public IEnumerator<JValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        [DebuggerHidden]
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }
    }

    public enum JsonToken
    {
        String,
        Boolean,
        Number,
        Object,
        Float,
        Null,
        Undefined,
        Array
    }
    [Serializable]
    public class  Undefinned : JValue
    {
        public static Undefinned Value = new Undefinned();
        private Undefinned()
        {

        }
        
        public override void Stringify(Context c)
        {
            c.GetBuilder().Append("undefinned");
        }

        public override void SimulateStringify(Context c)
        {

        }

        public override void FromJson(Context c, JValue j)
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    public class  Null : JValue
    {
        public static Null Value = new Null();
        private Null()
        {
        }
        public override void Stringify(Context c)
        {
            c.GetBuilder().Append("null");
        }

        public override void SimulateStringify(Context c)
        {
        }

        public override void FromJson(Context c, JValue j)
        {
            throw new NotImplementedException();
        }
    }
    public class  __Service__:DObject
    {
        public new static int __LOAD__(int dp) => DPISS;
		public static int DPService = Register<__Service__, string>("__service__");
        public static int DPdropRequest = Register<__Service__, bool>("dropRequest");
        public static int DPserviceData = Register<__Service__, JValue>("serviceData");
        public static int DPrequestData = Register<__Service__, object>("requestData");
		public static int DPISS = Register<__Service__, bool>("iss");
		public string __service__ {
            set => set(DPService, value);
		}
    
        public bool dropRequest {
            set => set(DPdropRequest, value);
        }
        public JValue serviceData {
            set => set(DPService, value);
        }
        public object requestData {
            set => set<object>(DPService, value);
        }

		public bool IsSuccess
		{
			get => (bool)get(DPISS);
		    set => set(DPISS, value);
		}

        public __Service__(string service)
        {
            set(DPService, service);
        }
    }
}
