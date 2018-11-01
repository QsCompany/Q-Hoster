using Json;

namespace Server
{
    public class  Indexer : JObject
    {
        public bool IsStringified;
        public bool? IsReferenced;
        public int NRefs;
        public virtual int __ref__
        {
            get => (int)((JNumber)base["__ref__"]).Value;
            set => ((JNumber)base["__ref__"]).Value = value;
        }
        public Indexer(int number)
        {
            base["__ref__"] = new JNumber(number);
        }
        public override void Stringify(Context c)
        {
            var s = c.GetBuilder();
            if (!IsStringified)
            {
                s.Append("\"@ref\":");
                IsStringified = true;
            }
            //else this.NRefs++;
            s.Append("{\"__ref__\":").Append(__ref__.ToString()).Append("}");
        }


        public bool StringifyAsRef(Context c,out bool start)
        {
            var indexer = this;
            var s = c.GetBuilder();
            start = false;
            if (indexer.IsStringified)
            {
                s.Append("{\"__ref__\":").Append(__ref__.ToString()).Append("}");
                return true;
            }
            s.Append("{\"@ref\":{\"__ref__\":").Append(__ref__.ToString()).Append("}");
            IsStringified = true;
            return false;
        }

        public override void SimulateStringify(Context c)
        {
            if (IsStringified)
                NRefs++;
            else
                IsStringified = true;
        }
        public virtual void Reset(int @ref)
        {
            __ref__ = @ref;
            IsReferenced = NRefs > 0;
            IsStringified = false;
        }

    }
}