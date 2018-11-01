using Json;
using QServer.Core;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Category), typeof(CategorySerializer))]
    public class  Category : DataRow
    {
        public new static int __LOAD__(int dp) => DpName;
        public static int DpBase = Register<Category, Category>("Base", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetCategory(c)); public Category Base { get => get<Category>(DpBase);
            set => set(DpBase, value);
        }
        public static int DpName = Register<Category, string>("Name"); public string Name { get => get<string>(DpName);
            set => set(DpName, value);
        }


        public Category()
        {
            
        }
        

        public Category(Context c,JValue jv):base(c,jv)
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }
        

        internal bool Check(RequestArgs args, out bool isNew)
        {
            var d = args.Database;
            isNew = d.Categories[Id] == null;
            if (!Validator.IsCategoryName(Name))
                args.SendError("The Category Name Is Invalid", false);
            var t = Base;
            if (t != null)
                if (d.Categories[Base.Id] == null)
                    args.SendError(CodeError.propably_hacking);
                else while (t != null)
                    {
                        if (t.Id == Id) return args.SendError("Redundounce Of Three", false);
                        t = t.Base;
                    }
            
            return true;
        }

        public override int Repaire(Database db)
        {
            throw new System.NotImplementedException();
        }
    }
}