using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvModels;
#if DEBUG
using QHostTrans;
#endif


namespace Server
{
    public class QFactUpgrade
    {
        public Dictionary<string, Csv> tables = new Dictionary<string, Csv>();
        public Dictionary<string, CsvConverter> Converters = new Dictionary<string, CsvConverter>();
        public models.Database Database { get; }

        public CsvConverter GetConverter(string name) => Converters.TryGetValue(name.ToUpperInvariant(), out var v) ? v : null;
        public QFactUpgrade(string path)
        {
            Path = new System.IO.DirectoryInfo(path);
            Initialize();
            var table = this["L_BL"];
            var convr = this.GetConverter("L_BL");
            foreach (var lgn in Iterate<LigneReader>("L_BL"))
            {
                var p = lgn.GetPiece();
                var lignes = p.GetLignes(this);
            }
        }

        public IEnumerable<T> Iterate<T>(string table) where T : CsvReader
        {
            var tbl = this[table];
            var cnv = GetConverter(table);
            foreach (var r in tbl.GetRows())
            {
                var x = cnv.CreateWrapper<T>(r); ;
                if (x != null)
                    yield return x;
            }
        }

        public IEnumerable<CsvReader> Iterate(string table)
        {
            var tbl = this[table];
            var cnv = GetConverter(table);
            foreach (var r in tbl.GetRows())
            {
                var x = cnv.CreateWrapper<CsvReader>(r);
                if (x != null)
                    yield return x;
            }
        }

        private void BeginConvert()
        {
            foreach (var cnv in Converters)
            {
                foreach (var r in cnv.Value.Table.GetRows())
                {
                    cnv.Value.LoadBasics(r);
                }
            }

            foreach (var cnv in Converters)
            {
                foreach (var r in cnv.Value.Table.GetRows())
                {
                    cnv.Value.LoadDependencies(r);
                }
            }
        }

        public DirectoryInfo Path { get; }

        public void Initialize()
        {
            FileInfo[] array = Path.GetFiles();
            for (int i = 0; i < array.Length; i++)
            {
                var f = array[i];
                string tname = null;
                if (f.Name == "__keys__.csv")
                    __keys__ = new Csv(f.FullName, "__keys__");

                else if (f.Extension.EndsWith(".csv"))
                    this[tname = f.Name.Replace(f.Extension, "").ToUpper()] = new Csv(f.FullName, tname);
            }
            if (__keys__ != null)
                for (int i = 0; i < __keys__.Count; i++)
                {
                    var r = __keys__[i];
                    var t = this[r[0]];
                    if (t == null) continue;
                    if (string.IsNullOrWhiteSpace(r[1])) continue;
                    var k = r[1]?.Trim().Split(':');
                    t.KeyName = k[0];
                    t.IndexWith();
                }

            LoadConverters(this);
        }

        public string BuildEntity(out string registers)
        {

            StringBuilder sb = new StringBuilder().AppendLine();
            StringBuilder reg = new StringBuilder().AppendLine();
            StringBuilder redare = new StringBuilder().AppendLine();

            reg.AppendLine("void  LoadConverters(QFactUpgrade db){");
            foreach (var kv in tables)
            {
                var n = kv.Key;
                var t = kv.Value;
                BuildClass(t, sb, reg, redare);
            }
            reg.AppendLine("}");
            registers = reg.ToString();
            sb.Append(redare);
            return sb.ToString();
        }

        private void BuildClass(Csv csv, StringBuilder sb, StringBuilder reg, StringBuilder _reader)
        {
            int keyIndex = csv.KeyName == null ? -1 : csv.ColumnIndex(csv.KeyName);
            var pp = _reader.AppendLine($"class {csv.TableName}Reader:CsvReader {{");

            pp.AppendLine($"public {csv.TableName}Reader():base(\"{csv.TableName.ToUpperInvariant()}\",{keyIndex})").Append("{}");
            pp.AppendLine("public override models.DataRow Parse(){\r\nreturn null;\r\n}");
            pp.AppendLine("");
            sb.AppendLine($"class {csv.TableName}Converter:CsvConverter {{");
            sb.AppendLine($"public {csv.TableName}Converter(QFactUpgrade db):base(db,\"{csv.TableName.ToUpperInvariant()}\"){{").Append('}');
            sb.AppendLine($"public {csv.TableName}Reader CreateWrapper(string[] data){{var t=new {csv.TableName}Reader();t.Initailize(Db,data); return t; }}");
            if (csv.KeyName != null)
                sb.AppendLine($"public static int __KEY__ = {keyIndex};");
            for (int i = 0; i < csv.ColumnCount; i++)
            {
                var c = csv.ColumnName(i);
                //sb.AppendLine($"public static int {c} = {i};");
                pp.AppendLine($"public string {c} => __data__[{i}];");
            }

            sb.AppendLine("public override void LoadBasics(string[] row){\r\n}");
            sb.AppendLine("public override void LoadDependencies(string[] row){\r\n}");
            sb.AppendLine("}");
            pp.AppendLine("}");
            //sb.Append(pp);
            reg.AppendLine($"Converters[\"{csv.TableName.ToUpperInvariant()}\"]=new {csv.TableName}Converter(db);");
        }

        Csv __keys__;

        public Csv this[string name]
        {
            get => tables.TryGetValue(name.ToUpperInvariant(), out var tbl) ? tbl : null;
            set => tables[name.ToUpperInvariant()] = value;
        }

        void LoadConverters(QFactUpgrade db)
        {
            Converters["ARTICLE"] = new ARTICLEConverter(db);
            Converters["ARTICLEDEPOT"] = new ARTICLEDEPOTConverter(db);
            Converters["CATEGORIECLIENT"] = new CATEGORIECLIENTConverter(db);
            Converters["CHARGE"] = new CHARGEConverter(db);
            Converters["COMPONENT"] = new COMPONENTConverter(db);
            Converters["COMPTE"] = new COMPTEConverter(db);
            Converters["DEPOT"] = new DEPOTConverter(db);
            Converters["DEVISE"] = new DEVISEConverter(db);
            Converters["DOCUMENTPIECE"] = new DOCUMENTPIECEConverter(db);
            Converters["EVENEMENTS"] = new EVENEMENTSConverter(db);
            Converters["FAMILLE"] = new FAMILLEConverter(db);
            Converters["INVENTORY"] = new INVENTORYConverter(db);
            Converters["LIGNECOMMERCIALE"] = new LIGNECOMMERCIALEConverter(db);
            Converters["LIGNEPRODUCTION"] = new LIGNEPRODUCTIONConverter(db);
            Converters["LINEMOUVEMENT"] = new LINEMOUVEMENTConverter(db);
            Converters["L_AS"] = new L_ASConverter(db);
            Converters["L_AV"] = new L_AVConverter(db);
            Converters["L_BC"] = new L_BCConverter(db);
            Converters["L_BE"] = new L_BEConverter(db);
            Converters["L_BL"] = new L_BLConverter(db);
            Converters["L_BR"] = new L_BRConverter(db);
            Converters["L_BS"] = new L_BSConverter(db);
            Converters["L_CC"] = new L_CCConverter(db);
            Converters["L_CT"] = new L_CTConverter(db);
            Converters["L_DS"] = new L_DSConverter(db);
            Converters["L_FC"] = new L_FCConverter(db);
            Converters["L_FF"] = new L_FFConverter(db);
            Converters["L_FP"] = new L_FPConverter(db);
            Converters["L_RT"] = new L_RTConverter(db);
            Converters["L_TR"] = new L_TRConverter(db);
            Converters["MARQUE"] = new MARQUEConverter(db);
            Converters["MESURE"] = new MESUREConverter(db);
            Converters["MODEREGLEMENT"] = new MODEREGLEMENTConverter(db);
            Converters["MONTANTCHARGE"] = new MONTANTCHARGEConverter(db);
            Converters["MONTANTCHARGEPRODUCTION"] = new MONTANTCHARGEPRODUCTIONConverter(db);
            Converters["MOTIFREGLEMENT"] = new MOTIFREGLEMENTConverter(db);
            Converters["PARAMS"] = new PARAMSConverter(db);
            Converters["PIECECOMMERCIALE"] = new PIECECOMMERCIALEConverter(db);
            Converters["PIECEINJOIN"] = new PIECEINJOINConverter(db);
            Converters["PRIXARTICLETIERS"] = new PRIXARTICLETIERSConverter(db);
            Converters["PRIXARTICLE_CL"] = new PRIXARTICLE_CLConverter(db);
            Converters["PRIXARTICLE_FN"] = new PRIXARTICLE_FNConverter(db);
            Converters["PRODUCTION"] = new PRODUCTIONConverter(db);
            Converters["P_AS"] = new P_ASConverter(db);
            Converters["P_AV"] = new P_AVConverter(db);
            Converters["P_BC"] = new P_BCConverter(db);
            Converters["P_BE"] = new P_BEConverter(db);
            Converters["P_BL"] = new P_BLConverter(db);
            Converters["P_BR"] = new P_BRConverter(db);
            Converters["P_BS"] = new P_BSConverter(db);
            Converters["P_CC"] = new P_CCConverter(db);
            Converters["P_CT"] = new P_CTConverter(db);
            Converters["P_DS"] = new P_DSConverter(db);
            Converters["P_FC"] = new P_FCConverter(db);
            Converters["P_FF"] = new P_FFConverter(db);
            Converters["P_FP"] = new P_FPConverter(db);
            Converters["P_RT"] = new P_RTConverter(db);
            Converters["P_TR"] = new P_TRConverter(db);
            Converters["QTEALLDEPOT"] = new QTEALLDEPOTConverter(db);
            Converters["QTEDEPOT"] = new QTEDEPOTConverter(db);
            Converters["REGLEMENT"] = new REGLEMENTConverter(db);
            Converters["ROWSINVE"] = new ROWSINVEConverter(db);
            Converters["STOCKMOUVEMENT"] = new STOCKMOUVEMENTConverter(db);
            Converters["S_AR"] = new S_ARConverter(db);
            Converters["S_CL"] = new S_CLConverter(db);
            Converters["S_FN"] = new S_FNConverter(db);
            Converters["TACHE_PRIVILEGE"] = new TACHE_PRIVILEGEConverter(db);
            Converters["TIERS"] = new TIERSConverter(db);
            Converters["TURNOVERCEILLINGPERIOD"] = new TURNOVERCEILLINGPERIODConverter(db);
            Converters["TYPEFORMAT"] = new TYPEFORMATConverter(db);
            Converters["TYPEPIECE"] = new TYPEPIECEConverter(db);

            Converters["VALUEFORMAT"] = new VALUEFORMATConverter(db);
            Converters["VALUEMESURE"] = new VALUEMESUREConverter(db);
            Converters["VERIFY_COMPTOIR"] = new VERIFY_COMPTOIRConverter(db);
            Converters["VERIFY_COMPTOIR2"] = new VERIFY_COMPTOIR2Converter(db);
            Converters["V_MOUVEMENT_PRODUCTION"] = new V_MOUVEMENT_PRODUCTIONConverter(db);
            Converters["V_PIECEINJOIN"] = new V_PIECEINJOINConverter(db);
            Converters["V_PIECE_WITH_SOLDE"] = new V_PIECE_WITH_SOLDEConverter(db);
        }
    }
}