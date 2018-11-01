using System;
using Json;
using QServer.Core;
using Server;

namespace models
{

    public enum NatureProduct
    {
        Active,
        Bocked,
        NonFacturable
    }
    public enum Unity
    {
        Piece,
        Poid,
        Longeur,
        Surface,
        Volume,
    }
    public interface IHistory
    {
        DateTime LastModified { get; set; }
    }
    [QServer.Core.HosteableObject(typeof(Api.Product), typeof(ProductSerializer))]
    public class  Product : Price
    {
        public override string ToString() => Name;
        public new static int __LOAD__(int dp) => DpSerieName;

        public static int DPRef = Register<Product, string>("Ref", PropertyAttribute.None, null, null, "nvarchar(15)");
        public string Ref { get => get<string>(DPRef); set => set(DPRef, value); }

        public static int DpCategory = Register<Product, Category>("Category", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetCategory(c));
        public Category Category { get => get<Category>(DpCategory);
            set => set(DpCategory, value);
        }
        
        public static int DpName = Register<Product, string>("Name",PropertyAttribute.None,null,null, "VARCHAR(50)"); public string Name { get => get<string>(DpName);
            set => set(DpName, value);
        }
        public static int DpDescription = Register<Product, string>("Description", PropertyAttribute.None, null, null, "VARCHAR(50)"); public string Description { get => get<string>(DpDescription);
            set => set(DpDescription, value);
        }
        
        public string Picture
        {
            get => get<string>(DpPicture);
            set => set(DpPicture, value) ;
        }
        public static int DpDimention = Register<Product, string>("Dimention"); public string Dimention { get => get<string>(DpDimention);
            set => set(DpDimention, value);
        }

        public static int DpQuality = Register<Product, float>("Quality"); public float Quality { get => get<float>(DpQuality);
            set => set(DpQuality, value);
        }
        public static int DpSerieName = Register<Product, string>("SerieName"); public string SerieName { get => get<string>(DpSerieName);
            set => set(DpSerieName, value);
        }


        public static int DPFrais = Register<Product, float>("Frais");
        public float Frais { get => get<float>(DPFrais); set => set(DPFrais, value); }


        public static int DPTVA = Register<Product, float>("TVA");
        public float TVA { get => get<float>(DPTVA); set => set(DPTVA, value); }



        public static int DPMD = Register<Product, float>("MD");
        public float MD { get => get<float>(DPMD); set => set(DPMD, value); }

        public static int DPMDG = Register<Product, float>("MDG");
        public float MDG { get => get<float>(DPMDG); set => set(DPMDG, value); }

        public static int DPMG = Register<Product, float>("MG");
        public float MG { get => get<float>(DPMG); set => set(DPMG, value); }
        
        public static int DPMS = Register<Product, float>("MS");
        public float MS { get => get<float>(DPMS); set => set(DPMS, value); }
        
        public static int DPMPS = Register<Product, float>("MPS");
        public float MPS { get => get<float>(DPMPS); set => set(DPMPS, value); }
        
        public static int DPSockable = Register<Product, bool>("Sockable");
        public bool Sockable { get => get<bool>(DPSockable); set => set(DPSockable, value); }
        
        public static int DPNature = Register<Product, NatureProduct>("Nature");
        public NatureProduct Nature { get => get<NatureProduct>(DPNature); set => set(DPNature, value); }


        public static int DPUnity = Register<Product, Unity>("Unity");
        public Unity Unity { get => get<Unity>(DPUnity); set => set(DPUnity, value); }


        public static int DPQteMax = Register<Product, float>("QteMax");
        public float QteMax { get => get<float>(DPQteMax); set => set(DPQteMax, value); }

        /// <summary>
        /// Qte minimale à vender
        /// </summary>
        public static int DPQteMin = Register<Product, float>("QteMin");
        public float QteMin { get => get<float>(DPQteMin); set => set(DPQteMin, value); }
        
        public static int DPCodebare = Register<Product, string>("Codebare");
        public string Codebare { get => get<string>(DPCodebare); set => set(DPCodebare, value); }


        public string Label => $"{(Name ?? "").Trim()} {(Dimention ?? "").Trim()} {(string.IsNullOrWhiteSpace(SerieName) ? "" : " - " + SerieName.Trim())} ";
        public string CategoryName => Category?.Name ?? "";

        public float PrixRevient
        {
            get { return PSel * (1 + TVA / 100) + Frais; }
        }



        #region Lot
        public static readonly int DPRevage = Register<Product, FakePrice>("Revage", PropertyAttribute.None, null, (d, c) => ((Database)d).GetFakePrice(c));
        public FakePrice Revage { get => get<FakePrice>(DPRevage);
            set => set(DPRevage, value);
        }

        internal bool CheckLot(RequestArgs args, out bool IsNew)
        {
            var s = PSel;
            var a = DPrice;
            var b = PPrice;
            var c = HWPrice;
            var d = WPrice;

            var prd = this;
            if (prd == null)
            {
                IsNew = false;
                return args.SendError(CodeError.product_not_exist);
            }
            Product org;
            IsNew = (org = args.Database.Products[Id]) == null;
            if (!IsNew)
            {
                if (org.Revage != Revage) return args.SendError("The Revage of lot cannot be modified by this request", false);
            }
            float lv = (float)(_values[DPPSel] ?? 0f);

            for (int i = DPValue; i < DPWValue + 1; i++)
            {
                float v = (float)(_values[i] ?? 0f);
                if (v <= lv)
                {
                    if (i == DPValue)
                        _values[i] = v = (float)Math.Ceiling(lv * 1.33);
                    else _values[i] = v = lv;
                }
                lv = v;
            }
            return true;
        }
        #endregion


        public int Index { get; internal set; }

        public static int DpPicture = Register<Product, string>("Picture", PropertyAttribute.None, null);
        public Product(Context c, JValue jv)
            : base(c, jv)
        {
        }
        public Product()
        {
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }
        
        internal bool Check(RequestArgs args, out bool isNew)
        {
            var d = args.Database;
            isNew = d.Products[Id] == null;
            //if (!CheckPrices(args)) return false;

            if (!Validator.IsProductName(Name))
                return args.SendError("The Name Of Products Is Invalid", false);

            if (!Validator.IsSerieName(SerieName))
                return args.SendError("The Serie Name Of Products Is Invalid", false);

            if (!Validator.IsDimention(Dimention))
                return args.SendError("The Dimention Of Products Is Invalid", false);

            if (Quality < 0)
                return args.SendError("The Name Of Products Is Invalid", false);

            if (Category != null && d.Categories[Category.Id] == null)
                return args.SendError($"The Category {Category.Name} Is Not Exist", false);

            if (!Validator.IsDescription(Description))
                return args.SendError("The Description Of Products Is Invalid", false);

            //if (Picture != null && d.Pictures[Picture] == null)
            //    return args.SendError($"Please Save The Picture Befor Use it", false);

            return true;
        }
        public bool Plus(RequestArgs args, float qte)
        {
            return UpdateQte(args, qte + Qte);
        }
        public bool Minus(RequestArgs args, float qte)
        {
            return UpdateQte(args, Qte - qte);
        }
        public bool UpdateQte(RequestArgs args, float value)
        {
            var price = this;
            var qte = Qte;
            LastModified = DateTime.Now;

            var e = "Update [dbo].[FakePrices] SET [Qte]=" + value + " WHERE [Id]=" + Id;
            var t = args.Database.Exec(e);
            set(DPQte, value);

            return t;
        }

        public FakePrice getPrevious(FakePrice oart)
        {
            var prev = (FakePrice)null;
            var cur = Revage;
            while (cur != oart && cur != null)
            {
                prev = cur;
                cur = cur.Next;
            }
            return prev;
        }
        public bool getPrevious(FakePrice oart,out FakePrice prevArt)
        {
            var prev = (FakePrice)null;
            var cur = Revage;
            while (cur != oart && cur != null)
            {
                prev = cur;
                cur = cur.Next;
            }
            prevArt = prev;
            return cur == oart;
        }
    }


    public class Composant : DataRow
    {
        public static new int __LOAD__(int dp) => DataRow.__LOAD__(DPPerte);


        public static int DPProduct = Register<Composant, Product>("Product");
        public Product Product { get => get<Product>(DPProduct); set => set(DPProduct, value); }
        
        public static int DPQte = Register<Composant, float>("Qte");
        public float Qte { get => get<float>(DPQte); set => set(DPQte, value); }


        public static int DPPerte = Register<Composant, float>("Perte");
        public float Perte { get => get<float>(DPPerte); set => set(DPPerte, value); }

    }

}