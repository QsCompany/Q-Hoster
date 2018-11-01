namespace Server
{
    sealed public class EIndexer :Indexer
    {
        public static EIndexer Value=new EIndexer();

        public override int __ref__
        {
            get { return 0; }
            set {  }
        }
        private EIndexer():base(0)
        {
        }
        public override void Stringify(Context c)
        {
        }
        public override void SimulateStringify(Context c)
        {         
        }


        public bool StringifyAsRef(Context c, out bool start)
        {
            var indexer = this;
            var s = c.GetBuilder();
            s.Append('{');
            start = true;
            return false;
        }

        public override void Reset(int @ref)
        {
            __ref__ = @ref;
            IsReferenced = false;
            IsStringified = false;
        }
    }
}