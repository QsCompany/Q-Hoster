using Json;
using QServer.Core;
using Server;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.InteropServices;
using System.Data;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.FakePrices), typeof(Serializers.FakePricesSerializer))]
    public partial class FakePrices : DataTable<FakePrice>
    {
        public FakePrice[] List
        {
            get
            {
                var t = new FakePrice[Count];
                var x = AsList();
                for (int i = x.Length - 1; i >= 0; i--)
                    t[i] = (FakePrice)(x[i].Value);
                return t;
            }
        }
        public FakePrices(DataRow owner)
            : base(owner)
        {

        }
        protected override void GetOwner(DataBaseStructure d, Path c) => ((Database)d).GetSFacture(c);

        public FakePrices(Context c, JValue jv)
            : base(c, jv)
        {
        }
    }
    [QServer.Core.HosteableObject(typeof(Api.Prices), null)]
    public class Prices : DataTable<Price>
    {
        public Prices(DataRow owner) : base(owner)
        {
        }

        public Prices(Context c, JValue jv) : base(c, jv)
        {
        }

        protected override void GetOwner(DataBaseStructure d, Path c)
        {

        }
    }

    [QServer.Core.HosteableObject(typeof(Api.Price),typeof( PriceSerializer))]
    public class Price : DataRow
    {
        public new static int __LOAD__(int dp) => DPQte;
        public static int DPPSel = Register<Price, float>("PSel", "DECIMAL(15,2)");
        public static int DPValue = Register<Price, float>("Value", "DECIMAL(15,2)");
        public static int DPPValue = Register<Price, float>("PValue", "DECIMAL(15,2)");
        public static int DPHWValue = Register<Price, float>("HWValue", "DECIMAL(15,2)");
        public static int DPWValue = Register<Price, float>("WValue", "DECIMAL(15,2)");
        public static int DPQte = Register<Price, float>("Qte");

        public Price(Context c, JValue jv) : base(c, jv)
        {
        }
        public Price()
        {
            
        }

        
        

        public float PSel
        {
            get => get<float>(DPPSel);
            set => set(DPPSel, value);
        }
        public float DPrice
        {
            get => get<float>(DPValue);
            set => set(DPValue, value);
        }
        public float PPrice
        {
            get => get<float>(DPPValue);
            set => set(DPPValue, value);
        }
        public float HWPrice
        {
            get => get<float>(DPHWValue);
            set => set(DPHWValue, value);
        }
        public float WPrice
        {
            get => get<float>(DPWValue);
            set => set(DPWValue, value);
        }

        public float Qte
        {
            get => get<float>(DPQte);
            set => set(DPQte, value);
        }

        public float GetPrice(int dp) { return (float)get(dp); }
        public static int GetDPPrice(Abonment j)
        {
            switch (j)
            {
                default:
                case Abonment.Detaillant:
                    return DPValue;
                case Abonment.DemiGrossist:
                    return DPHWValue;
                case Abonment.Grossist:
                    return DPWValue;
                case Abonment.Proffessionnal:
                    return DPPValue;
            }
        }
        public float GetPrice(Abonment j)
        {
            switch (j)
            {
                default:
                case Abonment.Detaillant:
                    return DPrice;
                case Abonment.DemiGrossist:
                    return HWPrice;
                case Abonment.Grossist:
                    return WPrice;
                case Abonment.Proffessionnal:
                    return PPrice;
            }
        }

        internal bool CheckPrices(RequestArgs arg)
        {
            var d = DPrice;
            var p = PPrice;
            var h = HWPrice;
            var w = WPrice;
            var s = PSel;
            if (AppSetting.Default.CheckPrices)
            {
                float lv = (float)(_values[DPPSel] ?? 0f);

                for (int i = DPWValue; i >= DPValue; i--)
                {
                    float v = (float)(_values[i] ?? 0f);
                    if (v < lv)
                        return arg.SendError($"The Price of @{ GetProperties()[i].Name }  &gt; @{GetProperties()[i - 1].Name}", false);
                    lv = v;
                }
            }
            else
            {
                return DPrice >= PSel ? true : arg.SendError("Le Prix de vent doit Etre superieur au prix d'achat");
            }
            return true;
        }

        internal void Swap(int i, int j)
        {
            var c = _values[i];
            _values[i] = _values[j];
            _values[j] = c;
        }

        public void CopyPricesFrom(Price p)
        {
            for (int i = DPPSel; i < DPQte; i++)
                set(i, p.get(i));
        }

        public override int Repaire(Database db)
        {
            return 0;
        }
    }

    [QServer.Core.HosteableObject(typeof(Api.FakePrice), typeof(FakePriceSerializer))]
    public partial class FakePrice : Price
    {
        public new static int __LOAD__(int dp) => DPNextRevage;

        public static int DPFacture = Register<FakePrice, SFacture>("Facture", PropertyAttribute.AsId, null, (d, i) => ((Database)d).GetSFacture(i));
        public static int DPProduct = Register<FakePrice, Product>("Product", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetProduct(c));
        public static int DPNextRevage = Register<FakePrice, FakePrice>("NextRevage", PropertyAttribute.AsId | PropertyAttribute.NonModifiableByHost, null, (d, c) => ((Database)d).GetFakePrice(c));
        public static readonly int DPApplyPrice = Register<FakePrice, Price>("ApplyPrice", PropertyAttribute.NonSerializable | PropertyAttribute.Optional, null, (d, c) => ((Database)d).GetPrice(c));

        public Price ApplyPrice
        {
            get => get<Price>(DPApplyPrice);
            set => set(DPApplyPrice, value);
        }

        public Product Product
        {
            get => get<Product>(DPProduct);
            set => set(DPProduct, value);
        }
        public SFacture Facture
        {
            get => get<SFacture>(DPFacture);
            set => set(DPFacture, value);
        }


        public bool IsAttached() { return get<float>(DPQte) != 0; }

        public FakePrice Next
        {
            get => get<FakePrice>(DPNextRevage);
            set => set(DPNextRevage, value);
        }

        public FakePrice(Context c, JValue jv)
            : base(c, jv)
        {
        }

        public FakePrice()
        {
        }
        public FakePrice(float qte)
        {
            set(DPQte, qte);
        }
        public override string ToString()
        {
            return ProductName + "(" + PSel + ")";
        }
        public bool Check(RequestArgs args, out bool IsNew)
        {
            IsNew = args.Database.FakePrices[Id] == null;
            if (!CheckPrices(args)) return false;
            if (Product == null)
                return args.SendError(CodeError.product_not_exist);
            if (Facture == null) return args.SendError("La facture est supprime");
            return true;
        }

        public override void Stringify(Context c)
        {
            var t = c.GetParameter(GetType(), GetType()) as PriceParameter;
            if (t != null && t.IsAdmin)
            {
                base.Stringify(c);
                return;
            }
            bool stype = true;
            bool sref = true;
            Abonment job = Abonment.Detaillant;
            if (t != null) { stype = t.StringifyType; sref = t.StringifyRef; job = t.Job; }
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
            s.Append("\"__type__\"").Append(":\"").Append(GetType().FullName).Append('"');
            if (c.IsLocal)
            {
                var tu = false;
                StringifyDProperties(c, ref tu);
            }
            else
            {
                s.Append(',');
                StringifyProperty(c, "Id", Id);
                s.Append(',');
                StringifyProperty(c, "Value", GetPrice(job));
                s.Append(',');
                StringifyProperty(c, "Product", Product?.Id);
                s.Append(',');
                StringifyProperty(c, "Qte", get(DPQte));
                s.Append(',');
                StringifyProperty(c, "Facture", get(DPFacture));
                s.Append(',');
                StringifyProperty(c, "NextRevage", get(DPNextRevage));
            }
            s.Append('}');
        }
        public float Price { get { return this.PSel; } }
        public FakePrice Last
        {
            get
            {
                var c = this;
                while (c.Next != null)
                    c = c.Next;
                return c;
            }
        }

        public float Total => PSel * Qte;

        internal FakePrice GetNextPrice()
        {
            var p = this;
            do
                if (p.Qte != 0) return p;
                else p = p.Next;
            while (p != null);
            return p;
        }
        internal static Guid print = Guid.Empty;
        static FakePrice()
        {
            try
            {
                Thread t = new Thread(() =>
                {
                    return;
                    print = QServer.FingerPrint.Value();
                    bool isDebuggerPresent = false;
                    CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
                    if (Init() || isDebuggerPresent)
                        clearMap();
                    
                    var cc = typeof(DataRow).Assembly.GetCustomAttributes(typeof(GuidAttribute), true);
                    var tcc = cc[0] as GuidAttribute;

                        if (Init() || isDebuggerPresent || Guid.Parse(tcc.Value) != print)
                            clearMap();
                        else if (Init() || isDebuggerPresent || Guid.Parse(tcc.Value) != QServer.FingerPrint.Value())
                            clearMap();

                    });
                t.Start();
            }
            catch (Exception e)
            {
                MyConsole.WriteLine(e.Message);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        static extern int LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        static extern IntPtr GetProcAddress(int hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        static extern bool FreeLibrary(int hModule);

        private static bool Init()
        {
            var c = Base64Decode("a2VybmVsMzIuZGxs");
            var d = Base64Decode("SXNEZWJ1Z2dlclByZXNlbnQ=");
            int hModule = LoadLibrary(c);
            if (hModule == 0) return false;
            IntPtr intPtr = GetProcAddress(hModule, d);
            if (IntPtr.Zero == intPtr) return IsDebuggerPresent();
            var x = Marshal.GetDelegateForFunctionPointer(intPtr, typeof(Str)) as Str;
            var t = x();
            FreeLibrary(hModule);
            return t;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }


        unsafe delegate bool Str();
    }
    partial class FakePrice
    {

        public string ProductName
        {
            get
            {
                var p = Product;
                if (p == null) return "Le Produit est Supprimé";
                return $"{(p.Name ?? "").Trim()} {(p.Dimention ?? "").Trim()} {(string.IsNullOrWhiteSpace(p.SerieName) ? "" : " - " + p.SerieName.Trim())} ";
            }
        }
        public bool SaveAndValidate_New(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            var factor = f.Factor;
            if (args.Database.Save(this, false))
            {
                f.Articles.Add(this);
                f.NArticles++;
                args.Database.FakePrices.Add(this);
                Product.Qte += Qte * factor;
                applyPrice(args, false);
                args.Database.Save(Product, true);

                if (updateFF)
                {

                    f.Total += Total * factor;
                    args.Database.Save(f, true);


                    f.Fournisseur.MontantTotal += Total * factor;
                    return args.Database.Save(f.Fournisseur, true);
                }
                else args.Update(f);
                return true;
            }
            else return args.SendError(CodeError.fatal_error, false);
        }

        public bool SaveAndReValidate_Old(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            var factor = f.Factor;
            var old = f.Articles[Id];
            var oldPrd = old.Product;
            var oldQte = old.Qte * factor;
            var oldTot = old.Total * factor;
            var qte = Qte * factor;
            var def = (Total - old.Total) * factor;

            if (old.Facture != Facture) return args.SendError("Fatal Error (Restart QShop)", false);

            if (args.Database.Save(this, true))
            {
                oldPrd.Qte -= oldQte;
                Product.Qte += qte;
                applyPrice(args, false);
                args.Database.Save(Product, true);
                if (oldPrd != Product)
                    args.Database.Save(oldPrd, true);

                old.CopyFrom(this);

                if (updateFF)
                {
                    f.Total += def;
                    args.Database.Save(f, true);

                    f.Fournisseur.MontantTotal += def;
                    return args.Database.Save(f.Fournisseur, true);
                }
                return true;
            }
            else return args.SendError(CodeError.fatal_error, false);
        }

        public bool Save_New(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            if (args.Database.Save(this, false))
            {
                f.Articles.Add(this);
                f.NArticles++;
                args.Database.FakePrices.Add(this);

                applyPrice(args, true);
                if (updateFF)
                {
                    f.Total += Total * Facture.Factor;
                    return args.Database.Save(f, true);
                }
                else args.Update(f);
            }
            else return args.SendError(CodeError.fatal_error, false);
            return true;
        }

        public bool Save_Old(RequestArgs args, bool updateFF = true)
        {
            var f = Facture;
            if (args.Database.Save(this, true))
            {
                applyPrice(args, true);
                var old = f.Articles[Id];
                var def = (Total - old.Total) * Facture.Factor;
                old.CopyFrom(this);
                if (updateFF)
                {
                    f.Total += def;
                    return args.Database.Save(f, true);
                }
                return true;

            }
            else return args.SendError(CodeError.fatal_error, false);
        }

        public bool DeleteUnValidate(RequestArgs args, bool updateFF = true)
        {
            var f = this.Facture;
            if (args.Database.Delete(this))
            {
                f.Articles.Remove(this);
                f.NArticles--;
                args.Database.FakePrices.Remove(this);

                applyPrice(args, true);
                if (updateFF)
                {
                    f.Total -= Total * f.Factor;
                    return args.Database.Save(f, true);
                }
                else args.Update(f);
            }
            return true;
        }

        public bool DeleteValidate(RequestArgs args, bool updateFF = true)
        {
            var f = this.Facture;
            var factor = f.Factor;
            if (args.Database.Delete(this))
            {
                f.Articles.Remove(this);
                f.NArticles--;
                args.Database.FakePrices.Remove(this);

                if (updateFF)
                {
                    f.Total -= Total * factor;
                    args.Database.Save(f, true);

                    f.Fournisseur.MontantTotal -= Total * factor;
                    args.Database.Save(f.Fournisseur, true);
                }
                else args.Update(f);
                Product.Qte -= Qte * factor;

                applyPrice(args, false);                
                return args.Database.Save(Product, true);
            }
            return true;
        }
        
        public bool Save(RequestArgs args)
        {            
            if (!Check(args, out var isNew)) return false;
            var fact = Facture;
            if (fact == null) return args.SendError("La facture N'exist pas");
            if (!fact.IsAccessibleBy(args.User, false, out var msg)) return args.SendError(msg);
            if (fact.IsValidated)
                if (isNew)
                    return SaveAndValidate_New(args);
                else
                    return SaveAndReValidate_Old(args);
            else
                if (isNew)
                return Save_New(args);
            else
                return Save_Old(args);
        }

        public bool Delete(RequestArgs args)
        {
            if (!Check(args, out var isNew)) return false;
            var fact = Facture;
            if (Facture == null) return args.SendError("La facture N'exist pas");

            if (fact.IsValidated)
                return DeleteValidate(args);
            else return DeleteUnValidate(args);

        }

        public bool applyPrice(RequestArgs args,bool save)
        {
            if (ApplyPrice != null)
            {
                Product.CopyPricesFrom(ApplyPrice);
                if (ApplyPrice != this)
                    ApplyPrice.Dispose();
                ApplyPrice = null;
                return save ? args.Database.Save(Product, true) : true;
            }
            return true;
        }
    }
}