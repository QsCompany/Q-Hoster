//using models;
using models;

using Server;
using System;
//using System;
using System.Collections.Generic;
using DateTime = System.DateTime;
namespace CsvModels
{

    //SemiFinis
    class ARTICLEReader : CsvReader
    {
        public ARTICLEReader() : base("ARTICLE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEARTI => ParseLong(__data__[0]);
        public string CODEARTI => __data__[1];
        public string DESIARTI => __data__[2];
        public string DETAARTI => __data__[3];
        public string CODEBARR => __data__[4];
        public long NUMEFAMI => ParseLong(__data__[5]);
        public string UNITMESU => __data__[6];
        public long NUMEUNIT => ParseLong(__data__[7]);
        public float PRIXACHA => ParseFloat(__data__[8]);
        public float TAUXTVA => ParseFloat(__data__[9]);
        public float FRAIAPPR => ParseFloat(__data__[10]);
        public float  QUANCOMM => ParseFloat(__data__[11]);
        public float  PRIXPMP => ParseFloat(__data__[12]);
        public float  QUANINVE => ParseFloat(__data__[13]);
        public float  QUANMINI => ParseFloat(__data__[14]);
        public float  QUANMAXI => ParseFloat(__data__[15]);
        public float  QUANCART => ParseFloat(__data__[16]);
        public string IMAGARTI => __data__[17];
        public float MARGEDETA => ParseFloat(__data__[18]);
        public float MARGEDEGR => ParseFloat(__data__[19]);
        public float MARGEGROS => ParseFloat(__data__[20]);
        public float PRIXDETAHT => ParseFloat(__data__[21]);
        public float PRIXDETATTC => ParseFloat(__data__[22]);
        public float PRIXDEGRHT => ParseFloat(__data__[23]);
        public float PRIXDEGRTTC => ParseFloat(__data__[24]);
        public float PRIXGROSHT => ParseFloat(__data__[25]);
        public float PRIXGROSTTC =>ParseFloat( __data__[26]);
        public float QUANENTR => ParseFloat(__data__[27]);
        public float QUANSORT => ParseFloat(__data__[28]);
        public string CODELOCA => __data__[29];
        public string NATUARTI => __data__[30];
        public bool  STOCKABL => ParseBoolean( __data__[31]);
        public string PRESSERV => __data__[32];
        public string CODEFOUR => __data__[33];
        public long  NUMEDEPO =>ParseLong( __data__[34]);
        public float  QUANSTOC => ParseFloat(__data__[35]);
        public float  QUANVIRTU =>ParseFloat( __data__[36]);
        public float  PRIXACHATTC =>ParseFloat( __data__[37]);
        public float  PRIXREVI =>ParseFloat( __data__[38]);
        public string IMAGRAPI => __data__[39];
        public float  MARGESPEC => ParseFloat(__data__[40]);
        public float  MARGESUPP => ParseFloat(__data__[41]);
        public float  PRIXSPECHT => ParseFloat(__data__[42]);
        public float  PRIXSPECTTC => ParseFloat(__data__[43]);
        public float  PRIXSUPPHT => ParseFloat(__data__[44]);
        public float  PRIXSUPPTTC => ParseFloat(__data__[45]);
        public string MATIPREM => __data__[46];
        public string POIDUNIT => __data__[47];
        public string MOREBACO => __data__[48];
        public bool  ISPACKAGE => ParseBoolean(__data__[49]);
        public string REFERENCE => __data__[50];
        public DateTime  EXPIRYDATE => ParseDate(__data__[51]);
        public DateTime  VERIFYEXPDATE => ParseDate(__data__[52]);
        public string ALERTDAY => __data__[53];
        public long  NUMEMARQ => ParseLong(__data__[54]);
        public override DataRow ToDataRow()
        {
            var p = __db__.Database.Products[NUMEARTI] ?? new models.Product();
            p.Id = NUMEARTI;
            p.Codebare = CODEBARR;
            p.Ref = CODEARTI;
            p.Name = DESIARTI;
            p.Description = DETAARTI;
            p.PSel = PRIXACHATTC;
            p.DPrice = PRIXDETATTC;
            p.PPrice = PRIXSPECTTC;
            p.HWPrice = PRIXDEGRTTC;
            p.WPrice = PRIXGROSTTC;
            p.Qte = QUANSTOC;
            p.QteMin = QUANMINI;
            p.QteMax = QUANMAXI;
            p.Sockable = STOCKABL;
            p.Unity = GetUnity();
            p.Frais = FRAIAPPR;
            p.Picture = GetPicture();
            p.MD = MARGEDETA;
            p.MG = MARGEDEGR;
            p.MDG = MARGEDEGR;
            p.MPS = MARGESPEC;
            p.MS = MARGEGROS;
            p.TVA = TAUXTVA;
            p.Category = GetFammille();
            p.SerieName = GetMarque();
            p.Nature = GetNature();
            return p;
        }

        private NatureProduct GetNature()
        {
            switch (NATUARTI)
            {
                case "Nom facturable": return NatureProduct.NonFacturable;
                case "En sommeil": return NatureProduct.Bocked;
                default:
                case "Actif": return NatureProduct.Active;
            }
        }

        private string GetMarque()
        {
            return null;
        }

        private Category GetFammille()
        {
            var c = __db__.Database.Categories[NUMEFAMI];
            if (c != null) return c;
            var t = __db__["FAMILLE"].GetByKey(NUMEFAMI.ToString());
            if (t == null) return null;
            return (Category)new FAMILLEReader().Initailize(__db__, t).ToDataRow();
        }

        private string GetPicture()
        {
            return null;
        }

        private Unity GetUnity()
        {
            return Unity.Poid;
        }
    }
    
    class ARTICLEDEPOTReader : CsvReader
    {
        public ARTICLEDEPOTReader() : base("ARTICLEDEPOT", 0)
        { }

        
        public override models.DataRow Parse()
        {
            return null;
        }

        public long  NUMEARTI => ParseLong(__data__[0]);
        public long  NUMEDEPO => ParseLong(__data__[1]);
        public float  QUANENTR => ParseFloat(__data__[2]);
        public float  QUANSORT => ParseFloat(__data__[3]);
        public float  PRIXPMP => ParseFloat(__data__[4]);
        public float  QUANDEPO => ParseFloat(__data__[5]);
    }
    class CATEGORIECLIENTReader : CsvReader
    {
        public CATEGORIECLIENTReader() : base("CATEGORIECLIENT", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }
        public long  NUMECATE => ParseLong(__data__[0]);
        public string LIBECATE => __data__[1];
        public string MARGCATE => __data__[2];
    }
    class CHARGEReader : CsvReader
    {
        public CHARGEReader() : base("CHARGE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMECHAR => ParseLong(__data__[0]);
        public string CODECHAR => __data__[1];
        public string LIBECHAR => __data__[2];
    }
    class COMPONENTReader : CsvReader
    {
        public COMPONENTReader() : base("COMPONENT", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMCOMPO => ParseLong(__data__[0]);
        public long NUMEARTI => ParseLong(__data__[1]);
        public string ARTICOMP => __data__[2];
        public string CODEARTI => __data__[3];
        public string DESIARTI => __data__[4];
        public float QUANBASE => ParseFloat(__data__[5]);
        public float PRIXREVI => ParseFloat(__data__[6]);
        public string TAUXPERT => __data__[7];
        public float MONTPERT => ParseFloat(__data__[8]);
        public float MONTTOTA => ParseFloat(__data__[9]);
        public string UNITMESU => __data__[10];
        public float QUANPERT => ParseFloat(__data__[11]);
        public float QUANCONS => ParseFloat(__data__[12]);
    }
    class COMPTEReader : CsvReader
    {
        public COMPTEReader() : base("COMPTE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMECOMP => ParseLong(__data__[0]);
        public string NOMCOMPT => __data__[1];
        public string RIBCOMPT => __data__[2];
        public string AGENCOMP => __data__[3];
        public string SOLDINIT => __data__[4];
        public float TOTAENTR => ParseFloat(__data__[5]);
        public float TOTASORT => ParseFloat(__data__[6]);
        public string SOLDACTU => __data__[7];
    }
    class DEPOTReader : CsvReader
    {
        public DEPOTReader() : base("DEPOT", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEDEPO => ParseLong(__data__[0]);
        public string CODEDEPO => __data__[1];
        public string DESIDEOP => __data__[2];
        public string ADREDEPO => __data__[3];
    }
    class DEVISEReader : CsvReader
    {
        public DEVISEReader() : base("DEVISE", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string IDENDEVI => __data__[0];
        public string LIBEDEVI => __data__[1];
        public string TAUXCHAN => __data__[2];
    }
    class DOCUMENTPIECEReader : CsvReader
    {
        public DOCUMENTPIECEReader() : base("DOCUMENTPIECE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEDOCU => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string DCONTENT => __data__[2];
        public string FILENAME => __data__[3];
        public string EXTEFILE => __data__[4];
        public string NOTEDOCU => __data__[5];
    }
    class EVENEMENTSReader : CsvReader
    {
        public EVENEMENTSReader() : base("EVENEMENTS", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEEVEN => ParseLong(__data__[0]);
        public string USERNAME => __data__[1];
        public string CODEEVEN => __data__[2];
        public string DESIEVEN => __data__[3];
        public DateTime DATEEVEN => ParseDate(__data__[4]);
        public string CURRTRAN => __data__[5];
        public string USERADDR => __data__[6];
    }

    class FAMILLEReader : CsvReader
    {
        public FAMILLEReader() : base("FAMILLE", 0)
        { }
        public override models.DataRow Parse() => new models.Category()
        {
            Id = NUMEFAMI,
            Name = LIBEFAMI
        };

        public long NUMEFAMI => ParseLong(__data__[0]);
        public string LIBEFAMI => ParseString(__data__[1]);
        public long NUMEPERE => ParseLong(__data__[2]);
        public string IMAGFAMI => __data__[3];
        public override DataRow ToDataRow()
        {
            var c = __db__.Database.Categories[NUMEFAMI] ?? new Category();
            c.Id = NUMEFAMI;
            c.Name = LIBEFAMI;
            var csvBase = CsvTable.GetByKey(NUMEPERE.ToString());
            if (csvBase != null)
                c.Base = new FAMILLEReader().Initailize(__db__, csvBase).ToDataRow() as Category;
            return c;
        }
    }
    class INVENTORYReader : CsvReader
    {
        public INVENTORYReader() : base("INVENTORY", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEINVE => ParseLong(__data__[0]);
        public string CODEINVE => __data__[1];
        public DateTime DATEINVE => ParseDate(__data__[2]);
        public string VALIDATE => __data__[3];
        public string NOTEINVE => __data__[4];
        public long NUMEDEPO => ParseLong(__data__[5]);
        public DateTime DATEVALI => ParseDate(__data__[6]);
    }
    class LIGNECOMMERCIALEReader : CsvReader
    {
        public LIGNECOMMERCIALEReader() : base("LIGNECOMMERCIALE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public float QUANCART => ParseFloat(__data__[6]);
        public float NOMBCART => ParseFloat(__data__[7]);
        public float QUANCOMM => ParseFloat(__data__[8]);
        public float PRIXPMP => ParseFloat(__data__[9]);
        public float PRIXHT => ParseFloat(__data__[10]);
        public float REMISE => ParseFloat(__data__[11]);
        public long NUMEORIG => ParseLong(__data__[12]);
        public float TAUXTVA => ParseFloat(__data__[13]);
        public string EXPIRYDATE => __data__[14];
    }
    class LIGNEPRODUCTIONReader : CsvReader
    {
        public LIGNEPRODUCTIONReader() : base("LIGNEPRODUCTION", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMLPROD => ParseLong(__data__[0]);
        public long NUMEPROD => ParseLong(__data__[1]);
        public string TYPEPROD => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public float QUANBASE => ParseFloat(__data__[6]);
        public float QUANDEMA => ParseFloat(__data__[7]);
        public float PRIXREVI => ParseFloat(__data__[8]);
        public string TAUXPERT => __data__[9];
        public long NUMEDEPO => ParseLong(__data__[10]);
        public float QUANCOMM => ParseFloat(__data__[11]);
        public float MONTPERT => ParseFloat(__data__[12]);
        public float MONTTOTA => ParseFloat(__data__[13]);
        public string UNITMESU => __data__[14];
        public float QUANPERT => ParseFloat(__data__[15]);
        public float QUANCONS => ParseFloat(__data__[16]);
    }
    class LINEMOUVEMENTReader : CsvReader
    {
        public LINEMOUVEMENTReader() : base("LINEMOUVEMENT", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELINE => ParseLong(__data__[0]);
        public long NUMEMOUV => ParseLong(__data__[1]);
        public string CODETYPE => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string DESIARTI => __data__[4];
        public float QUANMOUV => ParseFloat(__data__[5]);
        public float PRIXREVI => ParseFloat(__data__[6]);
        public string CODEARTI => __data__[7];
        public float QUANCART => ParseFloat(__data__[8]);
        public float NOMBCART => ParseFloat(__data__[9]);
        public float PRIXPUMP => ParseFloat(__data__[10]);
    }


    abstract class LigneReader : CsvReader
    {
        public string __pieceName__;
        public LigneReader(string tableName, int keyIndex) : base(tableName, keyIndex)
        {
            __pieceName__ = tableName.Replace("L_", "P_");
        }
        public virtual PieceReader GetPiece()
        {
            var t = __db__[__pieceName__];
            var c = __db__.GetConverter(__pieceName__);
            var this_key = __db__[__tableName__].ColumnIndex("NUMEPIEC");
            if (this_key == -1) return null;
            string key = __data__[this_key];
            if (key == null) return null;
            var ci = t.ColumnIndex("NUMEPIEC");
            if (ci == -1) return null;
            foreach (var pc in t.GetRows())
            {
                if (pc[ci] == key)
                    return c.CreateWrapper<PieceReader>(pc);
            }
            return null;
        }
    }
    class L_ASReader :  LigneReader
    {
        public L_ASReader() : base("L_AS", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMLPROD => ParseLong(__data__[0]);
        public long NUMEPROD => ParseLong(__data__[1]);
        public string TYPEPROD => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public float QUANBASE => ParseFloat(__data__[6]);
        public float QUANDEMA => ParseFloat(__data__[7]);
        public float PRIXREVI => ParseFloat(__data__[8]);
        public string TAUXPERT => __data__[9];
        public long NUMEDEPO => ParseLong(__data__[10]);
        public float QUANCOMM => ParseFloat(__data__[11]);
        public float MONTPERT => ParseFloat(__data__[12]);
        public float MONTTOTA => ParseFloat(__data__[13]);
        public string UNITMESU => __data__[14];
        public float QUANPERT => ParseFloat(__data__[15]);
        public float QUANCONS => ParseFloat(__data__[16]);
    }
    class L_AVReader :  LigneReader
    {
        public L_AVReader() : base("L_AV", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
    }
    class L_BCReader :  LigneReader
    {
        public L_BCReader() : base("L_BC", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
    }
    class L_BEReader :  LigneReader
    {
        public L_BEReader() : base("L_BE", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELINE => ParseLong(__data__[0]);
        public long NUMEMOUV => ParseLong(__data__[1]);
        public string CODETYPE => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public float QUANCART => ParseFloat(__data__[6]);
        public float NOMBCART => ParseFloat(__data__[7]);
        public float QUANMOUV => ParseFloat(__data__[8]);
        public float PRIXPUMP => ParseFloat(__data__[9]);
        public float PRIXREVI => ParseFloat(__data__[10]);
        public override DataRow ToDataRow()
        {
            var art = __db__.Database.FakePrices[NUMEARTI];
            art = art ?? new models.FakePrice();
            {
                art.Id = NUMELINE;
                art.Facture = __db__.Database.SFactures[NUMEMOUV];
                //art.ProductName = DESIARTI;
                art.PSel = PRIXREVI;
                //art. = this.PRIXPUMP;
                art.Qte = QUANMOUV;
                art.Product = __db__.Database.Products[NUMEARTI];
            }
            __db__.Database.FakePrices[art.Id] = art;
            return art;
        }

    }
    class L_BLReader : LigneReader
    {
        public L_BLReader() : base("L_BL", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public int ORDRLIGN =>ParseInt32(__data__[2]);
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
        public string EXPIRYDATE => __data__[15];
        public override DataRow ToDataRow()
        {
            var art = __db__.Database.Articles[NUMEARTI];
            art = art ?? new Article();
            {
                art.Id = NUMELIGN;
                art.Facture = __db__.Database.Factures[NUMEPIEC];
                art.ProductName = DESIARTI;
                art.PSel = PRIXPMP;
                art.Price = this.PRIXHT;
                art.Qte = QUANCOMM;
                art.Product = __db__.Database.Products[NUMEARTI];
            }
            __db__.Database.Articles[art.Id] = art;
            return art;
        }
    }
    class L_BRReader :  LigneReader
    {
        public L_BRReader() : base("L_BR", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
    }
    class L_BSReader :  LigneReader
    {
        public L_BSReader() : base("L_BS", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELINE => ParseLong(__data__[0]);
        public long NUMEMOUV => ParseLong(__data__[1]);
        public string CODETYPE => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public float QUANCART => ParseFloat(__data__[6]);
        public float NOMBCART => ParseFloat(__data__[7]);
        public float QUANMOUV => ParseFloat(__data__[8]);
        public float PRIXPUMP => ParseFloat(__data__[9]);
        public float PRIXREVI => ParseFloat(__data__[10]);
    }
    class L_CCReader :  LigneReader
    {
        public L_CCReader() : base("L_CC", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
    }
    class L_CTReader :  LigneReader
    {
        public L_CTReader() : base("L_CT", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
    }
    class L_DSReader :  LigneReader
    {
        public L_DSReader() : base("L_DS", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMLPROD => ParseLong(__data__[0]);
        public long NUMEPROD => ParseLong(__data__[1]);
        public string TYPEPROD => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public float QUANBASE => ParseFloat(__data__[6]);
        public float QUANDEMA => ParseFloat(__data__[7]);
        public float PRIXREVI => ParseFloat(__data__[8]);
        public string TAUXPERT => __data__[9];
        public long NUMEDEPO => ParseLong(__data__[10]);
        public float QUANCOMM => ParseFloat(__data__[11]);
        public float MONTPERT => ParseFloat(__data__[12]);
        public float MONTTOTA => ParseFloat(__data__[13]);
        public string UNITMESU => __data__[14];
        public float QUANPERT => ParseFloat(__data__[15]);
        public float QUANCONS => ParseFloat(__data__[16]);
    }
    class L_FCReader :  LigneReader
    {
        public L_FCReader() : base("L_FC", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
    }
    class L_FFReader :  LigneReader
    {
        public L_FFReader() : base("L_FF", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
    }
    class L_FPReader :  LigneReader
    {
        public L_FPReader() : base("L_FP", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
    }
    class L_RTReader :  LigneReader
    {
        public L_RTReader() : base("L_RT", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIGN => ParseLong(__data__[0]);
        public long NUMEPIEC => ParseLong(__data__[1]);
        public string ORDRLIGN => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public string DETAARTI => __data__[6];
        public float QUANCART => ParseFloat(__data__[7]);
        public float NOMBCART => ParseFloat(__data__[8]);
        public float QUANCOMM => ParseFloat(__data__[9]);
        public float PRIXPMP => ParseFloat(__data__[10]);
        public float PRIXHT => ParseFloat(__data__[11]);
        public float REMISE => ParseFloat(__data__[12]);
        public long NUMEORIG => ParseLong(__data__[13]);
        public float TAUXTVA => ParseFloat(__data__[14]);
    }
    class L_TRReader :  LigneReader
    {
        public L_TRReader() : base("L_TR", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELINE => ParseLong(__data__[0]);
        public long NUMEMOUV => ParseLong(__data__[1]);
        public string CODETYPE => __data__[2];
        public long NUMEARTI => ParseLong(__data__[3]);
        public string CODEARTI => __data__[4];
        public string DESIARTI => __data__[5];
        public float QUANCART => ParseFloat(__data__[6]);
        public float NOMBCART => ParseFloat(__data__[7]);
        public float QUANMOUV => ParseFloat(__data__[8]);
        public float PRIXPUMP => ParseFloat(__data__[9]);
        public float PRIXREVI => ParseFloat(__data__[10]);
    }
    class MARQUEReader : CsvReader
    {
        public MARQUEReader() : base("MARQUE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEMARQ => ParseLong(__data__[0]);
        public string DESIMARQ => __data__[1];
        public string IMAGMARQ => __data__[2];
    }
    class MESUREReader : CsvReader
    {
        public MESUREReader() : base("MESURE", 1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string LIBEMESU => __data__[0];
        public string UNITMESU => __data__[1];
    }
    class MODEREGLEMENTReader : CsvReader
    {
        public MODEREGLEMENTReader() : base("MODEREGLEMENT", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMELIBE => ParseLong(__data__[0]);
        public string LIBEREGL => __data__[1];
        public string DESCREGL => __data__[2];
        public string AJOUTIMB => __data__[3];
    }
    class MONTANTCHARGEReader : CsvReader
    {
        public MONTANTCHARGEReader() : base("MONTANTCHARGE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEARTI => ParseLong(__data__[0]);
        public long NUMECHAR => ParseLong(__data__[1]);
        public float MONTCHAR => ParseFloat(__data__[2]);
    }
    class MONTANTCHARGEPRODUCTIONReader : CsvReader
    {
        public MONTANTCHARGEPRODUCTIONReader() : base("MONTANTCHARGEPRODUCTION", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPROD => ParseLong(__data__[0]);
        public long NUMECHAR => ParseLong(__data__[1]);
        public float MONTCHAR => ParseFloat(__data__[2]);
        public float QUANPROD => ParseFloat(__data__[3]);
        public float MONTTOTA => ParseFloat(__data__[4]);
    }
    class MOTIFREGLEMENTReader : CsvReader
    {
        public MOTIFREGLEMENTReader() : base("MOTIFREGLEMENT", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEMOTI => ParseLong(__data__[0]);
        public string LIBEMOTI => __data__[1];
        public string TYPEMOTI => __data__[2];
        public string SPECFIEL => __data__[3];
        public string ISCHARGE => __data__[4];
    }
    class PARAMSReader : CsvReader
    {
        public PARAMSReader() : base("PARAMS", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPARA => ParseLong(__data__[0]);
        public string NAMEPARA => __data__[1];
        public string NOTEPARA => __data__[2];
        public string ACTIPARA => __data__[3];
        public string DATAVALUE => __data__[4];
        public string VALUEUNIVERSAL => __data__[5];
    }
    class PIECECOMMERCIALEReader : CsvReader
    {
        public PIECECOMMERCIALEReader() : base("PIECECOMMERCIALE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long  NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long  NUMETIER => ParseLong(__data__[4]);
        public DateTime  DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public float  TOTAHT => ParseFloat(__data__[7]);
        public float  TOTATVA => ParseFloat(__data__[8]);
        public float  REMISE => ParseFloat(__data__[9]);
        public float  AUTRFRAI => ParseFloat(__data__[10]);
        public float  VERSPIEC => ParseFloat(__data__[11]);
        public long NUMESEND => ParseLong(__data__[12]);
        public long NUMERECE => ParseLong(__data__[13]);
        public float  MONTTIMB => ParseFloat(__data__[14]);
        public string CODESEND => __data__[15];
        public DateTime DATESEND => ParseDate(__data__[16]);
        public string TYPESEND => __data__[17];
        public string DETAPIEC => __data__[18];
        public long  NUMEDEPO => ParseLong(__data__[19]);
        public string CATEPRIX => __data__[20];
        public string NOMRAPID => __data__[21];
        public string USERPIEC => __data__[22];
        public float  NETHT => ParseFloat(__data__[23]);
        public float  NETTVA => ParseFloat(__data__[24]);
        public float  TOTAPIEC => ParseFloat(__data__[25]);
        public float  RESTPIEC => ParseFloat(__data__[26]);
        public string NOMCOMTA => __data__[27];
        public string USERCREA => __data__[28];
        public DateTime  DATECREA => ParseDate(__data__[29]);
        public string USERALTE => __data__[30];
        public DateTime  DATEALTE => ParseDate(__data__[31]);
        public string FORCETYPE => __data__[32];
        public float  VERSTRAN => ParseFloat(__data__[33]);
        public bool DELIVERED => ParseBoolean(__data__[34]);
    }
    class PIECEINJOINReader : CsvReader
    {
        public PIECEINJOINReader() : base("PIECEINJOIN", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public long NUMESEND => ParseLong(__data__[1]);
        public DateTime DATESEND => ParseDate(__data__[2]);
    }
    class PRIXARTICLETIERSReader : CsvReader
    {
        public PRIXARTICLETIERSReader() : base("PRIXARTICLETIERS", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPRIXTIER => ParseLong(__data__[0]);
        public long NUMEARTI => ParseLong(__data__[1]);
        public long NUMETIER => ParseLong(__data__[2]);
        public float PRIXTIER => ParseFloat(__data__[3]);
    }
    class PRIXARTICLE_CLReader : CsvReader
    {
        public PRIXARTICLE_CLReader() : base("PRIXARTICLE_CL", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPRIXTIER => ParseLong(__data__[0]);
        public long NUMEARTI => ParseLong(__data__[1]);
        public long NUMETIER => ParseLong(__data__[2]);
        public string CODETIER => __data__[3];
        public string RAISSOCI => __data__[4];
        public string NOMCONT => __data__[5];
        public float PRIXTIER => ParseFloat(__data__[6]);
    }
    class PRIXARTICLE_FNReader : CsvReader
    {
        public PRIXARTICLE_FNReader() : base("PRIXARTICLE_FN", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPRIXTIER => ParseLong(__data__[0]);
        public long NUMEARTI => ParseLong(__data__[1]);
        public long NUMETIER => ParseLong(__data__[2]);
        public string CODETIER => __data__[3];
        public string RAISSOCI => __data__[4];
        public string NOMCONT => __data__[5];
        public float PRIXTIER => ParseFloat(__data__[6]);
    }
    class PRODUCTIONReader : CsvReader
    {
        public PRODUCTIONReader() : base("PRODUCTION", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPROD => ParseLong(__data__[0]);
        public string CODEPROD => __data__[1];
        public string LIBEPROD => __data__[2];
        public string TYPEPROD => __data__[3];
        public long NUMEARTI => ParseLong(__data__[4]);
        public string CODEARTI => __data__[5];
        public string DESIARTI => __data__[6];
        public float QUANPROD => ParseFloat(__data__[7]);
        public DateTime DATEPROD => ParseDate(__data__[8]);
        public long NUMEDEPO => ParseLong(__data__[9]);
        public float PRIXREVI => ParseFloat(__data__[10]);
        public string VALIDATE => __data__[11];
        public float MONTTOTA => ParseFloat(__data__[12]);
        public float TOTACHAR => ParseFloat(__data__[13]);
        public float TOTAREVI => ParseFloat(__data__[14]);
    }
    abstract class PieceReader : CsvReader
    {
        public string __ligneName__;
        public PieceReader(string name, int keyIndex) : base(name, keyIndex)
        {
            __ligneName__ = name.Replace("P_", "L_");

        }
        public override CsvReader Initailize(QFactUpgrade db, string[] data)
        {
            base.Initailize(db, data);

            return this;
        }
        public virtual List<LigneReader> GetLignes(QFactUpgrade db)
        {
            var t = db[__ligneName__];
            var c = db.GetConverter(__ligneName__);

            var this_key = db[__tableName__].ColumnIndex("NUMEPIEC");
            if (this_key == -1) return null;

            var lign_key = t.ColumnIndex("NUMEPIEC");
            if (lign_key == -1) return null;

            string key = __data__[this_key];
            if (key == null) return null;

            var r = new List<LigneReader>();
            foreach (var pc in t.GetRows())
            {
                if (pc[lign_key] == key)
                    r.Add(c.CreateWrapper<LigneReader>(pc));
            }
            return r;
        }

        public virtual models.DataRow ToDataRow() { return null; }

    }
    class P_ASReader :  PieceReader
    {
        public P_ASReader() : base("P_AS", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPROD => ParseLong(__data__[0]);
        public string CODEPROD => __data__[1];
        public string LIBEPROD => __data__[2];
        public string TYPEPROD => __data__[3];
        public long NUMEARTI => ParseLong(__data__[4]);
        public string CODEARTI => __data__[5];
        public string DESIARTI => __data__[6];
        public float QUANPROD => ParseFloat(__data__[7]);
        public DateTime DATEPROD => ParseDate(__data__[8]);
        public long NUMEDEPO => ParseLong(__data__[9]);
        public float PRIXREVI => ParseFloat(__data__[10]);
        public float MONTTOTA => ParseFloat(__data__[11]);
        public string VALIDATE => __data__[12];
        public float TOTACHAR => ParseFloat(__data__[13]);
        public float TOTAREVI => ParseFloat(__data__[14]);
    }
    class P_AVReader :  PieceReader
    {
        public P_AVReader() : base("P_AV", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public float MONTTIMB => ParseFloat(__data__[17]);
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public DateTime DATESEND => ParseDate(__data__[21]);
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string NOMCOMTA => __data__[24];
    }
    class P_BCReader :  PieceReader
    {
        public P_BCReader() : base("P_BC", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public string DATEPIEC => __data__[5];
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public string MONTTIMB => __data__[17];
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public string DATESEND => __data__[21];
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string NOMCOMTA => __data__[24];
    }
    class P_BEReader :  PieceReader
    {
        public P_BEReader() : base("P_BE", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEMOUV => ParseLong(__data__[0]);
        public string CODEMOUV => __data__[1];
        public string CODETYPE => __data__[2];
        public DateTime DATEMOUV => ParseDate(__data__[3]);
        public float TOTAMOUV => ParseFloat(__data__[4]);
        public string NOTEMOUV => __data__[5];
        public string DEPOSOUR => __data__[6];
    }
    class P_BLReader :  PieceReader
    {
        public P_BLReader() : base("P_BL", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public float MONTTIMB => ParseFloat(__data__[17]);
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public DateTime DATESEND => ParseDate(__data__[21]);
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string CATEPRIX => __data__[24];
        public string NOMCOMTA => __data__[25];
        public float VERSTRAN => ParseFloat(__data__[26]);
        public string DELIVERED => __data__[27];
    }
    class P_BRReader :  PieceReader
    {
        public P_BRReader() : base("P_BR", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public float MONTTIMB => ParseFloat(__data__[17]);
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public DateTime DATESEND => ParseDate(__data__[21]);
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string NOMCOMTA => __data__[24];
    }
    class P_BSReader :  PieceReader
    {
        public P_BSReader() : base("P_BS", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEMOUV => ParseLong(__data__[0]);
        public string CODEMOUV => __data__[1];
        public string CODETYPE => __data__[2];
        public DateTime DATEMOUV => ParseDate(__data__[3]);
        public float TOTAMOUV => ParseFloat(__data__[4]);
        public string NOTEMOUV => __data__[5];
        public string DEPOSOUR => __data__[6];
    }
    class P_CCReader :  PieceReader
    {
        public P_CCReader() : base("P_CC", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public float MONTTIMB => ParseFloat(__data__[17]);
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public DateTime DATESEND => ParseDate(__data__[21]);
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string CATEPRIX => __data__[24];
        public string NOMCOMTA => __data__[25];
    }
    class P_CTReader :  PieceReader
    {
        public P_CTReader() : base("P_CT", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public float MONTTIMB => ParseFloat(__data__[17]);
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public DateTime DATESEND => ParseDate(__data__[21]);
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string CATEPRIX => __data__[24];
        public string NOMRAPID => __data__[25];
        public string NOMCOMTA => __data__[26];
        public string FORCETYPE => __data__[27];
    }
    class P_DSReader :  PieceReader
    {
        public P_DSReader() : base("P_DS", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPROD => ParseLong(__data__[0]);
        public string CODEPROD => __data__[1];
        public string LIBEPROD => __data__[2];
        public string TYPEPROD => __data__[3];
        public long NUMEARTI => ParseLong(__data__[4]);
        public string CODEARTI => __data__[5];
        public string DESIARTI => __data__[6];
        public float QUANPROD => ParseFloat(__data__[7]);
        public DateTime DATEPROD => ParseDate(__data__[8]);
        public long NUMEDEPO => ParseLong(__data__[9]);
        public string PRIXREVI => __data__[10];
        public float MONTTOTA => ParseFloat(__data__[11]);
        public string VALIDATE => __data__[12];
        public float TOTACHAR => ParseFloat(__data__[13]);
        public float TOTAREVI => ParseFloat(__data__[14]);
    }
    class P_FCReader :  PieceReader
    {
        public P_FCReader() : base("P_FC", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public float MONTTIMB => ParseFloat(__data__[17]);
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public DateTime DATESEND => ParseDate(__data__[21]);
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string CATEPRIX => __data__[24];
        public string NOMCOMTA => __data__[25];
        public float VERSTRAN => ParseFloat(__data__[26]);
    }
    class P_FFReader :  PieceReader
    {
        public P_FFReader() : base("P_FF", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public float MONTTIMB => ParseFloat(__data__[17]);
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public DateTime DATESEND => ParseDate(__data__[21]);
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string NOMCOMTA => __data__[24];
    }
    class P_FPReader :  PieceReader
    {
        public P_FPReader() : base("P_FP", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public float MONTTIMB => ParseFloat(__data__[17]);
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public DateTime DATESEND => ParseDate(__data__[21]);
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string CATEPRIX => __data__[24];
        public string NOMCOMTA => __data__[25];
    }
    class P_RTReader :  PieceReader
    {
        public P_RTReader() : base("P_RT", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public string CODEPIEC => __data__[1];
        public string TYPEPIEC => __data__[2];
        public string CODETYPE => __data__[3];
        public long NUMETIER => ParseLong(__data__[4]);
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public long NUMELIBE => ParseLong(__data__[6]);
        public string DETAPIEC => __data__[7];
        public float TOTAHT => ParseFloat(__data__[8]);
        public float TOTATVA => ParseFloat(__data__[9]);
        public float REMISE => ParseFloat(__data__[10]);
        public string AUTRFRAI => __data__[11];
        public float NETHT => ParseFloat(__data__[12]);
        public float VERSPIEC => ParseFloat(__data__[13]);
        public long NUMESEND => ParseLong(__data__[14]);
        public long NUMERECE => ParseLong(__data__[15]);
        public float NETTVA => ParseFloat(__data__[16]);
        public float MONTTIMB => ParseFloat(__data__[17]);
        public float TOTAPIEC => ParseFloat(__data__[18]);
        public float RESTPIEC => ParseFloat(__data__[19]);
        public string CODESEND => __data__[20];
        public DateTime DATESEND => ParseDate(__data__[21]);
        public string TYPESEND => __data__[22];
        public long NUMEDEPO => ParseLong(__data__[23]);
        public string CATEPRIX => __data__[24];
        public string NOMCOMTA => __data__[25];
    }
    class P_TRReader :  PieceReader
    {
        public P_TRReader() : base("P_TR", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEMOUV => ParseLong(__data__[0]);
        public string CODEMOUV => __data__[1];
        public string CODETYPE => __data__[2];
        public DateTime DATEMOUV => ParseDate(__data__[3]);
        public float TOTAMOUV => ParseFloat(__data__[4]);
        public string NOTEMOUV => __data__[5];
        public string DEPOSOUR => __data__[6];
        public string DEPODEST => __data__[7];
    }
    class QTEALLDEPOTReader : CsvReader
    {
        public QTEALLDEPOTReader() : base("QTEALLDEPOT", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEARTI => ParseLong(__data__[0]);
        public float QUANENTR => ParseFloat(__data__[1]);
        public float QUANSORT => ParseFloat(__data__[2]);
        public float QUANDEPO => ParseFloat(__data__[3]);
    }
    class QTEDEPOTReader : CsvReader
    {
        public QTEDEPOTReader() : base("QTEDEPOT", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEARTI => ParseLong(__data__[0]);
        public long NUMEDEPO => ParseLong(__data__[1]);
        public string CODEDEPO => __data__[2];
        public string DESIDEOP => __data__[3];
        public float QUANENTR => ParseFloat(__data__[4]);
        public float QUANSORT => ParseFloat(__data__[5]);
        public float QUANDEPO => ParseFloat(__data__[6]);
        public float PRIXPMP => ParseFloat(__data__[7]);
        public string CODEARTI => __data__[8];
        public string DESIARTI => __data__[9];
        public long NUMEFAMI => ParseLong(__data__[10]);
        public string LIBEFAMI => __data__[11];
        public string NATUARTI => __data__[12];
    }
    class REGLEMENTReader : CsvReader
    {
        public REGLEMENTReader() : base("REGLEMENT", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEREGL => ParseLong(__data__[0]);
        public string REFEREGL => __data__[1];
        public long NUMETIER => ParseLong(__data__[2]);
        public long NUMEPIEC => ParseLong(__data__[3]);
        public long NUMELIBE => ParseLong(__data__[4]);
        public float MONTREGL => ParseFloat(__data__[5]);
        public DateTime DATEREGL => ParseDate(__data__[6]);
        public string TYPEREGL => __data__[7];
        public string NOTEREGL => __data__[8];
        public long NUMECOMP => ParseLong(__data__[9]);
        public long NUMEMOTI => ParseLong(__data__[10]);
        public string USERCREA => __data__[11];
        public DateTime DATECREA => ParseDate(__data__[12]);
        public string USERALTE => __data__[13];
        public DateTime DATEALTE => ParseDate(__data__[14]);
    }
    class ROWSINVEReader : CsvReader
    {
        public ROWSINVEReader() : base("ROWSINVE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEROWS => ParseLong(__data__[0]);
        public long NUMEINVE => ParseLong(__data__[1]);
        public long NUMEARTI => ParseLong(__data__[2]);
        public string CODEARTI => __data__[3];
        public string DESIARTI => __data__[4];
        public float QUANTHEO => ParseFloat(__data__[5]);
        public float QUANREAL => ParseFloat(__data__[6]);
        public float PRIXPUMP => ParseFloat(__data__[7]);
        public float QUANECAR => ParseFloat(__data__[8]);
    }
    class STOCKMOUVEMENTReader : CsvReader
    {
        public STOCKMOUVEMENTReader() : base("STOCKMOUVEMENT", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEMOUV => ParseLong(__data__[0]);
        public string CODEMOUV => __data__[1];
        public string CODETYPE => __data__[2];
        public DateTime DATEMOUV => ParseDate(__data__[3]);
        public float TOTAMOUV => ParseFloat(__data__[4]);
        public string NOTEMOUV => __data__[5];
        public string DEPOSOUR => __data__[6];
        public string DEPODEST => __data__[7];
        public long NUMEINVE => ParseLong(__data__[8]);
        public string USERCREA => __data__[9];
        public DateTime DATECREA => ParseDate(__data__[10]);
        public string USERALTE => __data__[11];
        public DateTime DATEALTE => ParseDate(__data__[12]);
        public long NUMEPROD => ParseLong(__data__[13]);
    }
    class S_ARReader : CsvReader
    {
        public S_ARReader() : base("S_AR", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEARTI => ParseLong(__data__[0]);
        public string CODEARTI => __data__[1];
        public string DESIARTI => __data__[2];
        public long NUMEFAMI => ParseLong(__data__[3]);
        public string NATUARTI => __data__[4];
        public float PRIXDETATTC => ParseFloat(__data__[5]);
        public float PRIXDEGRTTC => ParseFloat(__data__[6]);
        public float PRIXGROSTTC => ParseFloat(__data__[7]);
        public float QUANSTOC => ParseFloat(__data__[8]);
    }
    class S_CLReader : CsvReader
    {
        public S_CLReader() : base("S_CL", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMETIER => ParseLong(__data__[0]);
        public string TYPETIER => __data__[1];
        public string CODETIER => __data__[2];
        public string CIVITIER => __data__[3];
        public string RAISSOCI => __data__[4];
        public string NOMCONT => __data__[5];
        public long NUMELIBE => ParseLong(__data__[6]);
        public string ADRETIER => __data__[7];
        public string VILLTIER => __data__[8];
        public string CODEPOST => __data__[9];
        public string PAYSTIER => __data__[10];
        public string EMAITIER => __data__[11];
        public string SITEWEB => __data__[12];
        public string TELFIXE1 => __data__[13];
        public string TELFIXE2 => __data__[14];
        public string TELMOBI1 => __data__[15];
        public string TELMOBI2 => __data__[16];
        public long NUMEFAX => ParseLong(__data__[17]);
        public string ACTITIER => __data__[18];
        public string SECTACTI => __data__[19];
        public string BANQAGEN => __data__[20];
        public string COMPBANQ => __data__[21];
        public string REGICOMM => __data__[22];
        public string MATRFISC => __data__[23];
        public string ARTIIMPO => __data__[24];
        public float RESTINIT => ParseFloat(__data__[25]);
        public float TOTAPIEC => ParseFloat(__data__[26]);
        public float TOTAREGL => ParseFloat(__data__[27]);
        public float RESTACTU => ParseFloat(__data__[28]);
        public long NUMECATE => ParseLong(__data__[29]);
        public long NUMECOMP => ParseLong(__data__[30]);
        public float TOTADEBI => ParseFloat(__data__[31]);
        public float TOTACRED => ParseFloat(__data__[32]);
        public string STATTIER => __data__[33];
        public string APPLYTURNOVERCEILLING => __data__[34];
        public string APPLYBALANCECEILLING => __data__[35];
        public string BALANCECEILLING => __data__[36];
        public string BARCODE => __data__[37];
        public string TURNOVERTHRESHOLD => __data__[38];
        public string LOYALTYREMISE => __data__[39];
        public string CUMULATIVETURNOVER => __data__[40];
        public string WONAMOUNT => __data__[41];
    }
    class S_FNReader : CsvReader
    {
        public S_FNReader() : base("S_FN", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMETIER => ParseLong(__data__[0]);
        public string TYPETIER => __data__[1];
        public string CODETIER => __data__[2];
        public string CIVITIER => __data__[3];
        public string RAISSOCI => __data__[4];
        public string NOMCONT => __data__[5];
        public long NUMELIBE => ParseLong(__data__[6]);
        public string ADRETIER => __data__[7];
        public string VILLTIER => __data__[8];
        public string CODEPOST => __data__[9];
        public string PAYSTIER => __data__[10];
        public string EMAITIER => __data__[11];
        public string SITEWEB => __data__[12];
        public string TELFIXE1 => __data__[13];
        public string TELFIXE2 => __data__[14];
        public string TELMOBI1 => __data__[15];
        public string TELMOBI2 => __data__[16];
        public long NUMEFAX => ParseLong(__data__[17]);
        public string ACTITIER => __data__[18];
        public string SECTACTI => __data__[19];
        public string BANQAGEN => __data__[20];
        public string COMPBANQ => __data__[21];
        public string REGICOMM => __data__[22];
        public string MATRFISC => __data__[23];
        public string ARTIIMPO => __data__[24];
        public float RESTINIT => ParseFloat(__data__[25]);
        public float TOTAPIEC => ParseFloat(__data__[26]);
        public float TOTAREGL => ParseFloat(__data__[27]);
        public float RESTACTU => ParseFloat(__data__[28]);
        public long NUMECATE => ParseLong(__data__[29]);
        public long NUMECOMP => ParseLong(__data__[30]);
        public float TOTADEBI => ParseFloat(__data__[31]);
        public float TOTACRED => ParseFloat(__data__[32]);
        public string STATTIER => __data__[33];
    }
    class TACHE_PRIVILEGEReader : CsvReader
    {
        public TACHE_PRIVILEGEReader() : base("TACHE_PRIVILEGE", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string PIECE => __data__[0];
        public string OBJECT => __data__[1];
        public string FIELD => __data__[2];
        public string TACHE => __data__[3];
        public string S => __data__[4];
        public string I => __data__[5];
        public string U => __data__[6];
        public string D => __data__[7];
        public string X => __data__[8];
        public string R => __data__[9];
    }
    class TIERSReader : CsvReader
    {
        public TIERSReader() : base("TIERS", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMETIER => ParseLong(__data__[0]);
        public string TYPETIER => __data__[1];
        public string CODETIER => __data__[2];
        public string CIVITIER => __data__[3];
        public string RAISSOCI => __data__[4];
        public string NOMCONT => __data__[5];
        public long NUMELIBE => ParseLong(__data__[6]);
        public string ADRETIER => __data__[7];
        public string VILLTIER => __data__[8];
        public string CODEPOST => __data__[9];
        public string PAYSTIER => __data__[10];
        public string EMAITIER => __data__[11];
        public string SITEWEB => __data__[12];
        public string TELFIXE1 => __data__[13];
        public string TELFIXE2 => __data__[14];
        public string TELMOBI1 => __data__[15];
        public string TELMOBI2 => __data__[16];
        public long NUMEFAX => ParseLong(__data__[17]);
        public string ACTITIER => __data__[18];
        public string SECTACTI => __data__[19];
        public string BANQAGEN => __data__[20];
        public string COMPBANQ => __data__[21];
        public string REGICOMM => __data__[22];
        public string MATRFISC => __data__[23];
        public string ARTIIMPO => __data__[24];
        public float RESTINIT => ParseFloat(__data__[25]);
        public float TOTAPIEC => ParseFloat(__data__[26]);
        public float TOTAREGL => ParseFloat(__data__[27]);
        public long NUMECATE => ParseLong(__data__[28]);
        public float RESTACTU => ParseFloat(__data__[29]);
        public long NUMECOMP => ParseLong(__data__[30]);
        public float TOTACRED => ParseFloat(__data__[31]);
        public float TOTADEBI => ParseFloat(__data__[32]);
        public string STATTIER => __data__[33];
        public float APPLYTURNOVERCEILLING => ParseFloat(__data__[34]);
        public float APPLYBALANCECEILLING => ParseFloat(__data__[35]);
        public float BALANCECEILLING => ParseFloat(__data__[36]);
        public string BARCODE => __data__[37];
        public string TURNOVERTHRESHOLD => __data__[38];
        public string LOYALTYREMISE => __data__[39];
        public string CUMULATIVETURNOVER => __data__[40];
        public string WONAMOUNT => __data__[41];
        public override DataRow ToDataRow()
        {
            if (TYPETIER == "CL")
                return GetClient();
            return GetFournisseur();
        }

        private DataRow GetClient()
        {
            Client c;
            c = __db__.Database.Clients[NUMETIER];
            if (c == null) __db__.Database.Clients.Add(c = new models.Client());
            {
                c.Id = NUMETIER;
                c.Name = NOMCONT;
                c.Ville = VILLTIER;
                c.CodePostal = CODEPOST;
                c.Email = EMAITIER;
                c.Address = ADRETIER;
                c.CapitalSocial = RAISSOCI;
                c.Mobile = TELMOBI1;
                c.SiteWeb = SITEWEB;
                c.Tel = TELMOBI1;
                c.VersmentTotal = TOTAREGL;
                c.MontantTotal = TOTAPIEC;
                c.Job = GetActivity();
                c.NCompte = COMPBANQ;
                c.NIF = MATRFISC;
                c.NRC = REGICOMM;
                c.NAI = ARTIIMPO;
            }
            return c;
        }

        private Job GetActivity()
        {
            return Job.Detaillant;
        }

        private DataRow GetFournisseur()
        {
            Fournisseur c;
            c = __db__.Database.Fournisseurs[NUMETIER];
            if (c == null) __db__.Database.Fournisseurs.Add(c = new models.Fournisseur());
            {
                c.Id = NUMETIER;
                c.Name = NOMCONT;
                c.Ville = VILLTIER;
                c.CodePostal = CODEPOST;
                c.Email = EMAITIER;
                c.Address = ADRETIER;
                c.CapitalSocial = RAISSOCI;
                c.Mobile = TELMOBI1;
                c.SiteWeb = SITEWEB;
                c.Tel = TELMOBI1;
                c.VersmentTotal = TOTAREGL;
                c.MontantTotal = TOTAPIEC;
                c.NCompte = COMPBANQ;
                c.NIF = MATRFISC;
                c.NRC = REGICOMM;
                c.NAI = ARTIIMPO;
            }
            return c;
        }
    }
    class TURNOVERCEILLINGPERIODReader : CsvReader
    {
        public TURNOVERCEILLINGPERIODReader() : base("TURNOVERCEILLINGPERIOD", 0)
        { }
        public override models.DataRow Parse()
        {
            
            return null;
        }

        public long NUMETIER => ParseLong(__data__[0]);
        public DateTime DATEFROM => ParseDate(__data__[1]);
        public DateTime DATETO => ParseDate(__data__[2]);
        public string TURNOVERCEILLING => __data__[3];
    }
    class TYPEFORMATReader : CsvReader
    {
        public TYPEFORMATReader() : base("TYPEFORMAT", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string TYPEFORM => __data__[0];
        public string TAILCOMP => __data__[1];
        public string SEPAFORM => __data__[2];
        public float MONTFORM => ParseFloat(__data__[3]);
        public string YEARFORM => __data__[4];
        public string ACTIFORM => __data__[5];
    }
    class TYPEPIECEReader : CsvReader
    {
        public TYPEPIECEReader() : base("TYPEPIECE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string CODETYPE => __data__[0];
        public string PREFIXE => __data__[1];
        public string TYPEFORM => __data__[2];
        public string TYPEPIEC => __data__[3];
        public string ACTIVATION => __data__[4];
        public string EFFESTOC => __data__[5];
        public string EFFETIER => __data__[6];
        public string SENSTYPE => __data__[7];
        public string ACCETRAN => __data__[8];
    }
    
    class VALUEFORMATReader : CsvReader
    {
        public VALUEFORMATReader() : base("VALUEFORMAT", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string TYPEFORM => __data__[0];
        public float MONTVALU => ParseFloat(__data__[1]);
        public string YEARVALU => __data__[2];
        public string CODETYPE => __data__[3];
        public string MAXVALUE => __data__[4];
    }
    class VALUEMESUREReader : CsvReader
    {
        public VALUEMESUREReader() : base("VALUEMESURE", 0)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string UNITFROM => __data__[0];
        public string VALURESU => __data__[1];
        public string UNITMETO => __data__[2];
    }
    class VERIFY_COMPTOIRReader : CsvReader
    {
        public VERIFY_COMPTOIRReader() : base("VERIFY_COMPTOIR", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string CODEPIEC => __data__[0];
        public long NUMEREGL => ParseLong(__data__[1]);
        public long NUMEPIEC => ParseLong(__data__[2]);
        public long NUMETIER => ParseLong(__data__[3]);
        public long NUMEPIEC_RCV => ParseLong(__data__[4]);
        public long NUMETIER_RCV => ParseLong(__data__[5]);
    }
    class VERIFY_COMPTOIR2Reader : CsvReader
    {
        public VERIFY_COMPTOIR2Reader() : base("VERIFY_COMPTOIR2", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string CODEPIEC => __data__[0];
        public long NUMEREGL => ParseLong(__data__[1]);
        public long NUMEPIEC => ParseLong(__data__[2]);
        public long NUMETIER => ParseLong(__data__[3]);
        public long NUMEPIEC_RCV => ParseLong(__data__[4]);
        public long NUMETIER_RCV => ParseLong(__data__[5]);
    }
    class V_MOUVEMENT_PRODUCTIONReader : CsvReader
    {
        public V_MOUVEMENT_PRODUCTIONReader() : base("V_MOUVEMENT_PRODUCTION", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEMOUV => ParseLong(__data__[0]);
        public string CODEMOUV => __data__[1];
        public string CODETYPE => __data__[2];
        public string TYPEPIEC => __data__[3];
        public DateTime DATEMOUV => ParseDate(__data__[4]);
        public float TOTAMOUV => ParseFloat(__data__[5]);
        public string NOTEMOUV => __data__[6];
        public string DEPOSOUR => __data__[7];
        public string DEPODEST => __data__[8];
        public long NUMEINVE => ParseLong(__data__[9]);
        public long NUMEPROD => ParseLong(__data__[10]);
    }
    class V_PIECEINJOINReader : CsvReader
    {
        public V_PIECEINJOINReader() : base("V_PIECEINJOIN", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public long NUMEPIEC => ParseLong(__data__[0]);
        public long NUMESEND => ParseLong(__data__[1]);
        public string CODETYPE => __data__[2];
        public string CODEPIEC => __data__[3];
        public string TYPEPIEC => __data__[4];
        public DateTime DATEPIEC => ParseDate(__data__[5]);
        public float NETHT => ParseFloat(__data__[6]);
        public float TOTAPIEC => ParseFloat(__data__[7]);
    }
    class V_PIECE_WITH_SOLDEReader : CsvReader
    {
        public V_PIECE_WITH_SOLDEReader() : base("V_PIECE_WITH_SOLDE", -1)
        { }
        public override models.DataRow Parse()
        {
            return null;
        }

        public string TYPEPIEC => __data__[0];
        public long NUMEPIEC => ParseLong(__data__[1]);
        public long NUMETIER => ParseLong(__data__[2]);
        public DateTime DATEPIEC => ParseDate(__data__[3]);
        public string CODEPIEC => __data__[4];
        public float TOTAPIEC => ParseFloat(__data__[5]);
        public float VERSPIEC => ParseFloat(__data__[6]);
        public float RESTPIEC => ParseFloat(__data__[7]);
        public string SENSTYPE => __data__[8];
        public string EFFETIER => __data__[9];
    }

}
