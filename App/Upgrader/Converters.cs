using Server;
namespace CsvModels
{

    class ARTICLEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public ARTICLEConverter(QFactUpgrade db) : base(db, "ARTICLE")
        {
        }
        public ARTICLEReader CreateWrapper(string[] data) { var t = new ARTICLEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class ARTICLEDEPOTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public ARTICLEDEPOTConverter(QFactUpgrade db) : base(db, "ARTICLEDEPOT")
        {
        }
        public ARTICLEDEPOTReader CreateWrapper(string[] data) { var t = new ARTICLEDEPOTReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class CATEGORIECLIENTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public CATEGORIECLIENTConverter(QFactUpgrade db) : base(db, "CATEGORIECLIENT")
        {
        }
        public CATEGORIECLIENTReader CreateWrapper(string[] data) { var t = new CATEGORIECLIENTReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class CHARGEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public CHARGEConverter(QFactUpgrade db) : base(db, "CHARGE")
        {
        }
        public CHARGEReader CreateWrapper(string[] data) { var t = new CHARGEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class COMPONENTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public COMPONENTConverter(QFactUpgrade db) : base(db, "COMPONENT")
        {
        }
        public COMPONENTReader CreateWrapper(string[] data) { var t = new COMPONENTReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class COMPTEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public COMPTEConverter(QFactUpgrade db) : base(db, "COMPTE")
        {
        }
        public COMPTEReader CreateWrapper(string[] data) { var t = new COMPTEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class DEPOTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public DEPOTConverter(QFactUpgrade db) : base(db, "DEPOT")
        {
        }
        public DEPOTReader CreateWrapper(string[] data) { var t = new DEPOTReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class DEVISEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public DEVISEConverter(QFactUpgrade db) : base(db, "DEVISE")
        {
        }
        public DEVISEReader CreateWrapper(string[] data) { var t = new DEVISEReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class DOCUMENTPIECEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public DOCUMENTPIECEConverter(QFactUpgrade db) : base(db, "DOCUMENTPIECE")
        {
        }
        public DOCUMENTPIECEReader CreateWrapper(string[] data) { var t = new DOCUMENTPIECEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class EVENEMENTSConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public EVENEMENTSConverter(QFactUpgrade db) : base(db, "EVENEMENTS")
        {
        }
        public EVENEMENTSReader CreateWrapper(string[] data) { var t = new EVENEMENTSReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class FAMILLEConverter : Server.CsvConverter
    {
        private models.Categories Categories = new models.Categories(null);

        public FAMILLEConverter(QFactUpgrade db) : base(db, "FAMILLE")
        {
        }
        public FAMILLEReader CreateWrapper(string[] data) { var t = new FAMILLEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
            Categories.Add((models.Category)CreateWrapper(row).Parse());
        }
        public override void LoadDependencies(string[] row)
        {
            var w = CreateWrapper(row);
            var p = Categories[w.NUMEPERE];
            var c = Categories[w.NUMEFAMI];
            if (c != null)
                c.Base = p;
        }

        public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;
    }
    class INVENTORYConverter : CsvConverter
    { 
		
        public INVENTORYConverter(QFactUpgrade db) : base(db, "INVENTORY")
        {
        }
        public INVENTORYReader CreateWrapper(string[] data) { var t = new INVENTORYReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
        public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;
    }
    class LIGNECOMMERCIALEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public LIGNECOMMERCIALEConverter(QFactUpgrade db) : base(db, "LIGNECOMMERCIALE")
        {
        }
        public LIGNECOMMERCIALEReader CreateWrapper(string[] data) { var t = new LIGNECOMMERCIALEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class LIGNEPRODUCTIONConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public LIGNEPRODUCTIONConverter(QFactUpgrade db) : base(db, "LIGNEPRODUCTION")
        {
        }
        public LIGNEPRODUCTIONReader CreateWrapper(string[] data) { var t = new LIGNEPRODUCTIONReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class LINEMOUVEMENTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public LINEMOUVEMENTConverter(QFactUpgrade db) : base(db, "LINEMOUVEMENT")
        {
        }
        public LINEMOUVEMENTReader CreateWrapper(string[] data) { var t = new LINEMOUVEMENTReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_ASConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_ASConverter(QFactUpgrade db) : base(db, "L_AS")
        {
        }
        public L_ASReader CreateWrapper(string[] data) { var t = new L_ASReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_AVConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_AVConverter(QFactUpgrade db) : base(db, "L_AV")
        {
        }
        public L_AVReader CreateWrapper(string[] data) { var t = new L_AVReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_BCConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_BCConverter(QFactUpgrade db) : base(db, "L_BC")
        {
        }
        public L_BCReader CreateWrapper(string[] data) { var t = new L_BCReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_BEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_BEConverter(QFactUpgrade db) : base(db, "L_BE")
        {
        }
        public L_BEReader CreateWrapper(string[] data) { var t = new L_BEReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_BLConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_BLConverter(QFactUpgrade db) : base(db, "L_BL")
        {
        }
        public L_BLReader CreateWrapper(string[] data) { var t = new L_BLReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_BRConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_BRConverter(QFactUpgrade db) : base(db, "L_BR")
        {
        }
        public L_BRReader CreateWrapper(string[] data) { var t = new L_BRReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_BSConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_BSConverter(QFactUpgrade db) : base(db, "L_BS")
        {
        }
        public L_BSReader CreateWrapper(string[] data) { var t = new L_BSReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_CCConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_CCConverter(QFactUpgrade db) : base(db, "L_CC")
        {
        }
        public L_CCReader CreateWrapper(string[] data) { var t = new L_CCReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_CTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_CTConverter(QFactUpgrade db) : base(db, "L_CT")
        {
        }
        public L_CTReader CreateWrapper(string[] data) { var t = new L_CTReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_DSConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_DSConverter(QFactUpgrade db) : base(db, "L_DS")
        {
        }
        public L_DSReader CreateWrapper(string[] data) { var t = new L_DSReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_FCConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_FCConverter(QFactUpgrade db) : base(db, "L_FC")
        {
        }
        public L_FCReader CreateWrapper(string[] data) { var t = new L_FCReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_FFConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_FFConverter(QFactUpgrade db) : base(db, "L_FF")
        {
        }
        public L_FFReader CreateWrapper(string[] data) { var t = new L_FFReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_FPConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_FPConverter(QFactUpgrade db) : base(db, "L_FP")
        {
        }
        public L_FPReader CreateWrapper(string[] data) { var t = new L_FPReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_RTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_RTConverter(QFactUpgrade db) : base(db, "L_RT")
        {
        }
        public L_RTReader CreateWrapper(string[] data) { var t = new L_RTReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class L_TRConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public L_TRConverter(QFactUpgrade db) : base(db, "L_TR")
        {
        }
        public L_TRReader CreateWrapper(string[] data) { var t = new L_TRReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class MARQUEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public MARQUEConverter(QFactUpgrade db) : base(db, "MARQUE")
        {
        }
        public MARQUEReader CreateWrapper(string[] data) { var t = new MARQUEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class MESUREConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public MESUREConverter(QFactUpgrade db) : base(db, "MESURE")
        {
        }
        public MESUREReader CreateWrapper(string[] data) { var t = new MESUREReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 1;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class MODEREGLEMENTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public MODEREGLEMENTConverter(QFactUpgrade db) : base(db, "MODEREGLEMENT")
        {
        }
        public MODEREGLEMENTReader CreateWrapper(string[] data) { var t = new MODEREGLEMENTReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class MONTANTCHARGEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public MONTANTCHARGEConverter(QFactUpgrade db) : base(db, "MONTANTCHARGE")
        {
        }
        public MONTANTCHARGEReader CreateWrapper(string[] data) { var t = new MONTANTCHARGEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class MONTANTCHARGEPRODUCTIONConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public MONTANTCHARGEPRODUCTIONConverter(QFactUpgrade db) : base(db, "MONTANTCHARGEPRODUCTION")
        {
        }
        public MONTANTCHARGEPRODUCTIONReader CreateWrapper(string[] data) { var t = new MONTANTCHARGEPRODUCTIONReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class MOTIFREGLEMENTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public MOTIFREGLEMENTConverter(QFactUpgrade db) : base(db, "MOTIFREGLEMENT")
        {
        }
        public MOTIFREGLEMENTReader CreateWrapper(string[] data) { var t = new MOTIFREGLEMENTReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class PARAMSConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public PARAMSConverter(QFactUpgrade db) : base(db, "PARAMS")
        {
        }
        public PARAMSReader CreateWrapper(string[] data) { var t = new PARAMSReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class PIECECOMMERCIALEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public PIECECOMMERCIALEConverter(QFactUpgrade db) : base(db, "PIECECOMMERCIALE")
        {
        }
        public PIECECOMMERCIALEReader CreateWrapper(string[] data) { var t = new PIECECOMMERCIALEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class PIECEINJOINConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public PIECEINJOINConverter(QFactUpgrade db) : base(db, "PIECEINJOIN")
        {
        }
        public PIECEINJOINReader CreateWrapper(string[] data) { var t = new PIECEINJOINReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class PRIXARTICLETIERSConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public PRIXARTICLETIERSConverter(QFactUpgrade db) : base(db, "PRIXARTICLETIERS")
        {
        }
        public PRIXARTICLETIERSReader CreateWrapper(string[] data) { var t = new PRIXARTICLETIERSReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class PRIXARTICLE_CLConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public PRIXARTICLE_CLConverter(QFactUpgrade db) : base(db, "PRIXARTICLE_CL")
        {
        }
        public PRIXARTICLE_CLReader CreateWrapper(string[] data) { var t = new PRIXARTICLE_CLReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class PRIXARTICLE_FNConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public PRIXARTICLE_FNConverter(QFactUpgrade db) : base(db, "PRIXARTICLE_FN")
        {
        }
        public PRIXARTICLE_FNReader CreateWrapper(string[] data) { var t = new PRIXARTICLE_FNReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class PRODUCTIONConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public PRODUCTIONConverter(QFactUpgrade db) : base(db, "PRODUCTION")
        {
        }
        public PRODUCTIONReader CreateWrapper(string[] data) { var t = new PRODUCTIONReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_ASConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_ASConverter(QFactUpgrade db) : base(db, "P_AS")
        {
        }
        public P_ASReader CreateWrapper(string[] data) { var t = new P_ASReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_AVConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_AVConverter(QFactUpgrade db) : base(db, "P_AV")
        {
        }
        public P_AVReader CreateWrapper(string[] data) { var t = new P_AVReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_BCConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_BCConverter(QFactUpgrade db) : base(db, "P_BC")
        {
        }
        public P_BCReader CreateWrapper(string[] data) { var t = new P_BCReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_BEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_BEConverter(QFactUpgrade db) : base(db, "P_BE")
        {
        }
        public P_BEReader CreateWrapper(string[] data) { var t = new P_BEReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_BLConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_BLConverter(QFactUpgrade db) : base(db, "P_BL")
        {
        }
        public P_BLReader CreateWrapper(string[] data) { var t = new P_BLReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_BRConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_BRConverter(QFactUpgrade db) : base(db, "P_BR")
        {
        }
        public P_BRReader CreateWrapper(string[] data) { var t = new P_BRReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_BSConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_BSConverter(QFactUpgrade db) : base(db, "P_BS")
        {
        }
        public P_BSReader CreateWrapper(string[] data) { var t = new P_BSReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_CCConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_CCConverter(QFactUpgrade db) : base(db, "P_CC")
        {
        }
        public P_CCReader CreateWrapper(string[] data) { var t = new P_CCReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_CTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_CTConverter(QFactUpgrade db) : base(db, "P_CT")
        {
        }
        public P_CTReader CreateWrapper(string[] data) { var t = new P_CTReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_DSConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_DSConverter(QFactUpgrade db) : base(db, "P_DS")
        {
        }
        public P_DSReader CreateWrapper(string[] data) { var t = new P_DSReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_FCConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_FCConverter(QFactUpgrade db) : base(db, "P_FC")
        {
        }
        public P_FCReader CreateWrapper(string[] data) { var t = new P_FCReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_FFConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_FFConverter(QFactUpgrade db) : base(db, "P_FF")
        {
        }
        public P_FFReader CreateWrapper(string[] data) { var t = new P_FFReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_FPConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_FPConverter(QFactUpgrade db) : base(db, "P_FP")
        {
        }
        public P_FPReader CreateWrapper(string[] data) { var t = new P_FPReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_RTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_RTConverter(QFactUpgrade db) : base(db, "P_RT")
        {
        }
        public P_RTReader CreateWrapper(string[] data) { var t = new P_RTReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class P_TRConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public P_TRConverter(QFactUpgrade db) : base(db, "P_TR")
        {
        }
        public P_TRReader CreateWrapper(string[] data) { var t = new P_TRReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class QTEALLDEPOTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public QTEALLDEPOTConverter(QFactUpgrade db) : base(db, "QTEALLDEPOT")
        {
        }
        public QTEALLDEPOTReader CreateWrapper(string[] data) { var t = new QTEALLDEPOTReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class QTEDEPOTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public QTEDEPOTConverter(QFactUpgrade db) : base(db, "QTEDEPOT")
        {
        }
        public QTEDEPOTReader CreateWrapper(string[] data) { var t = new QTEDEPOTReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class REGLEMENTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public REGLEMENTConverter(QFactUpgrade db) : base(db, "REGLEMENT")
        {
        }
        public REGLEMENTReader CreateWrapper(string[] data) { var t = new REGLEMENTReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class ROWSINVEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public ROWSINVEConverter(QFactUpgrade db) : base(db, "ROWSINVE")
        {
        }
        public ROWSINVEReader CreateWrapper(string[] data) { var t = new ROWSINVEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class STOCKMOUVEMENTConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public STOCKMOUVEMENTConverter(QFactUpgrade db) : base(db, "STOCKMOUVEMENT")
        {
        }
        public STOCKMOUVEMENTReader CreateWrapper(string[] data) { var t = new STOCKMOUVEMENTReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class S_ARConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public S_ARConverter(QFactUpgrade db) : base(db, "S_AR")
        {
        }
        public S_ARReader CreateWrapper(string[] data) { var t = new S_ARReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class S_CLConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public S_CLConverter(QFactUpgrade db) : base(db, "S_CL")
        {
        }
        public S_CLReader CreateWrapper(string[] data) { var t = new S_CLReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class S_FNConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public S_FNConverter(QFactUpgrade db) : base(db, "S_FN")
        {
        }
        public S_FNReader CreateWrapper(string[] data) { var t = new S_FNReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class TACHE_PRIVILEGEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public TACHE_PRIVILEGEConverter(QFactUpgrade db) : base(db, "TACHE_PRIVILEGE")
        {
        }
        public TACHE_PRIVILEGEReader CreateWrapper(string[] data) { var t = new TACHE_PRIVILEGEReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class TIERSConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public TIERSConverter(QFactUpgrade db) : base(db, "TIERS")
        {
        }
        public TIERSReader CreateWrapper(string[] data) { var t = new TIERSReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
            CreateWrapper(row);
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class TURNOVERCEILLINGPERIODConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public TURNOVERCEILLINGPERIODConverter(QFactUpgrade db) : base(db, "TURNOVERCEILLINGPERIOD")
        {
        }
        public TURNOVERCEILLINGPERIODReader CreateWrapper(string[] data) { var t = new TURNOVERCEILLINGPERIODReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class TYPEFORMATConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public TYPEFORMATConverter(QFactUpgrade db) : base(db, "TYPEFORMAT")
        {
        }
        public TYPEFORMATReader CreateWrapper(string[] data) { var t = new TYPEFORMATReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class TYPEPIECEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public TYPEPIECEConverter(QFactUpgrade db) : base(db, "TYPEPIECE")
        {
        }
        public TYPEPIECEReader CreateWrapper(string[] data) { var t = new TYPEPIECEReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }

    class VALUEFORMATConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public VALUEFORMATConverter(QFactUpgrade db) : base(db, "VALUEFORMAT")
        {
        }
        public VALUEFORMATReader CreateWrapper(string[] data) { var t = new VALUEFORMATReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class VALUEMESUREConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public VALUEMESUREConverter(QFactUpgrade db) : base(db, "VALUEMESURE")
        {
        }
        public VALUEMESUREReader CreateWrapper(string[] data) { var t = new VALUEMESUREReader(); t.Initailize(Db, data); return t; }
        public static int __KEY__ = 0;
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class VERIFY_COMPTOIRConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public VERIFY_COMPTOIRConverter(QFactUpgrade db) : base(db, "VERIFY_COMPTOIR")
        {
        }
        public VERIFY_COMPTOIRReader CreateWrapper(string[] data) { var t = new VERIFY_COMPTOIRReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class VERIFY_COMPTOIR2Converter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public VERIFY_COMPTOIR2Converter(QFactUpgrade db) : base(db, "VERIFY_COMPTOIR2")
        {
        }
        public VERIFY_COMPTOIR2Reader CreateWrapper(string[] data) { var t = new VERIFY_COMPTOIR2Reader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class V_MOUVEMENT_PRODUCTIONConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public V_MOUVEMENT_PRODUCTIONConverter(QFactUpgrade db) : base(db, "V_MOUVEMENT_PRODUCTION")
        {
        }
        public V_MOUVEMENT_PRODUCTIONReader CreateWrapper(string[] data) { var t = new V_MOUVEMENT_PRODUCTIONReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class V_PIECEINJOINConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public V_PIECEINJOINConverter(QFactUpgrade db) : base(db, "V_PIECEINJOIN")
        {
        }
        public V_PIECEINJOINReader CreateWrapper(string[] data) { var t = new V_PIECEINJOINReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
    class V_PIECE_WITH_SOLDEConverter : CsvConverter
    { 
		public override T CreateWrapper<T>(string[] data) => this.CreateWrapper(data) as T;

        public V_PIECE_WITH_SOLDEConverter(QFactUpgrade db) : base(db, "V_PIECE_WITH_SOLDE")
        {
        }
        public V_PIECE_WITH_SOLDEReader CreateWrapper(string[] data) { var t = new V_PIECE_WITH_SOLDEReader(); t.Initailize(Db, data); return t; }
        public override void LoadBasics(string[] row)
        {
        }
        public override void LoadDependencies(string[] row)
        {
        }
    }
}
