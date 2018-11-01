using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Server;
using System.Diagnostics;
using System.IO;
using System.Data.Odbc;
using System.Text;
using System.Data.Common;
using System.Collections;

public class Promise
{

}
namespace models
{
    public enum SqlOperation
    {
        Insert, Update, Delete
    }

    public interface IDatabaseOperation
    {
        string BuildSqlCommand(DataBaseStructure db, System.Data.Common.DbCommand c, ref int i);
        string BuildSqlCommand(DataBaseStructure db, System.Data.Common.DbCommand c);
        bool CanBuild { get; }
    }

    public class DatabaseOperation : IDatabaseOperation
    {
        public SqlOperation Operation;
        public DataRow Data;
        public DatabaseOperation(SqlOperation operation, DataRow data)
        {
            Operation = operation;
            Data = data;
        }

        public bool CanBuild => Data != null;

        public string BuildSqlCommand(DataBaseStructure db, System.Data.Common.DbCommand c, ref int i)
        {
            return Operation == SqlOperation.Update ? Data.GetUpdate(c, ref i) : Operation == SqlOperation.Insert ? Data.GetInsert(c, ref i) : Data.GetDelete(c);
        }
        public string BuildSqlCommand(DataBaseStructure db, System.Data.Common.DbCommand c)
        {
            var i = 0;
            return BuildSqlCommand(db, c, ref i);
        }
    }

    public class OperationBuilder : IDatabaseOperation
    {
        public readonly StringBuilder FormatCommand;
        public readonly List<object> Params;
        public OperationBuilder(string formatCommand, params object[] @params)
        {
            this.FormatCommand = new StringBuilder(formatCommand);
            this.Params = new List<object>(@params);
        }

        public bool CanBuild => true;

        public OperationBuilder Append(string s, params object[] values)
        {
            var strs = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                strs[i] = "{" + Params.Count + "}";
                Params.Add(values[i]);
            }
            FormatCommand.AppendFormat(s, strs);
            return this;
        }

        public OperationBuilder Append(bool condition, string s, params object[] values)
        {
            if (!condition) return this;
            var strs = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                strs[i] = "{" + Params.Count + "}";
                Params.Add(values[i]);
            }
            FormatCommand.AppendFormat(s, strs);
            return this;
        }
        public string BuildSqlCommand(DataBaseStructure db, DbCommand c, ref int counter)
        {
            var t = new object[Params.Count];
            for (int i = 0; i < t.Length; i++)
            {
                var val = Params[i];
                var p = c.CreateParameter();
                p.ParameterName = (++counter).ToString();
                p.Value = val;
                t[i] = "@" + p.ParameterName;
                c.Parameters.Add(p);
            }
            return string.Format(FormatCommand.ToString(), t);
        }

        public string BuildSqlCommand(DataBaseStructure db, DbCommand c)
        {
            var i = 0;
            return BuildSqlCommand(db, c, ref i);
        }
    }

    public class DatabaseOperationGroup : IDatabaseOperation, IEnumerable<IDatabaseOperation>
    {
        private List<IDatabaseOperation> operations = new List<IDatabaseOperation>();
        public int Length => operations.Count;

        public bool CanBuild => operations.Count > 0;

        public IDatabaseOperation this[int index]
        {
            get => operations[index];
        }
        public DatabaseOperationGroup Add(SqlOperation operation, DataRow data)
        {
            operations.Add(new DatabaseOperation(operation, data));
            return this;
        }
        public DatabaseOperationGroup Add(IDatabaseOperation operation)
        {
            operations.Add(operation);
            return this;
        }

        public DatabaseOperationGroup Add(string formatCommand, params object[] @params)
        {
            operations.Add(new OperationBuilder(formatCommand, @params));
            return this;
        }
        public DatabaseOperationGroup()
        {

        }
        public DatabaseOperationGroup(SqlOperation operation, DataRow data) => operations.Add(new DatabaseOperation(operation, data));
        public DatabaseOperationGroup(string formatCommand, params object[] @params) => operations.Add(new OperationBuilder(formatCommand, @params));

        public DatabaseOperationGroup(IDatabaseOperation operation) => this.operations.Add(operation);

        public string BuildSqlCommand(DataBaseStructure db, DbCommand c, ref int i)
        {
            var s = new StringBuilder();
            for (int j = 0, l = operations.Count; j < l; j++)
                s.Append(operations[j].BuildSqlCommand(db, c, ref i)).AppendLine(";");
            return s.ToString();
        }

        public string BuildSqlCommand(DataBaseStructure db, DbCommand c)
        {
            var i = 0;
            return BuildSqlCommand(db, c, ref i);
        }

        public IEnumerator<IDatabaseOperation> GetEnumerator() => operations.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => operations.GetEnumerator();
    }

    public class DataBaseStructure : DataRow
    {
        static int @const;
        protected DatabaseUpdator _updater;
        public DBManager DB = new MySqlManager();
        protected Dictionary<Type, DProperty> tables = new Dictionary<Type, DProperty>();
        public  List<Path> ItemConstraints = new List<Path>();
        public  List<Path> ListConstraints = new List<Path>();
        public DatabaseUpdator Updator => (_updater ?? (_updater = new DatabaseUpdator(this)));
        public DataTable this[Type type] => tables.TryGetValue(type, out var tbl) ? get<DataTable>(tbl.Index) : null;

        protected static void Empty(DataBaseStructure d, Path c) { }
        public new static int __LOAD__(int dp) => DataRow.__LOAD__(@const);
        public static string GetPlur(string s) => s.EndsWith("y") ? s.Substring(0, s.Length - 1) + "ies" : s + "s";
        
        public DatabaseOperationGroup CreateOperations(SqlOperation operation, DataRow data) => new DatabaseOperationGroup(operation, data);
        public DatabaseOperationGroup CreateOperations(string formatCommand, params object[] @params) => new DatabaseOperationGroup(formatCommand, @params);
        public DatabaseOperationGroup CreateOperations() => new DatabaseOperationGroup();
        public DatabaseOperationGroup CreateOperations(IDatabaseOperation operation) => new DatabaseOperationGroup(operation);

        public object GetValue(DProperty dp) => get(dp.Index);

        public bool? Execute(IDatabaseOperation x, Action<DataBaseStructure, DbDataReader, object[]> callback, params object[] @params)
        {

            using (var sqlTran = DB.SQL.BeginTransaction())
            using (var c = DB.SQL.CreateCommand())
            {
                try
                {
                    c.CommandText = x.BuildSqlCommand(this, c);
                    var aff = c.ExecuteReader();
                    try
                    {
                        callback?.Invoke(this, aff, @params);
                    }
                    catch (Exception e)
                    {
                    }
                    if (!aff.IsClosed)
                        aff.Close();
                    sqlTran.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    try
                    {
                        sqlTran.Rollback();
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    return false;
                }
            }
        }


        public void SaveAllTables()
        {
            foreach (var property in GetProperties())
                if (typeof(DataTable).IsAssignableFrom(property.Type))
                {
                    var t = get<DataTable>(property.Index);
                    t.Save(DB.SQL);
                }
        }
        public override void Dispose()
        {
            Console.WriteLine("Database Is Disposing");
            this.DB.Dispose();

            for (int i = 0; i < _values.Length; i++)
            {
                if (_values[i] is IDisposable)
                    (_values[i] as IDisposable).Dispose();
            }
            base.Dispose();
            Console.WriteLine("Database Is Disposed");
        }

        public bool Delete(DataRow art)
        {
            if (DB.SQL.State == ConnectionState.Closed)
                DB.SQL.Open();
            using (var c = DB.SQL.CreateCommand())
            {
                try
                {
                    c.CommandText = art.GetDelete(c);
                    var ii = c.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MyConsole.WriteLine(e.Message);
                    return false;
                }
            }
            return true;
        }
        public bool DropAllTables()
        {
            var t = true;
            foreach (var dp in models.Database.GetProperties(typeof(models.Database)))
            {
                if (this.Get(dp.Index) is models.DataTable)
                    if (!this.Exec("DROP TABLE " + dp.Name))
                    {
                        t = false;
                    }
            }
            return true;
        }
        public bool Save(DataRow art, bool update)
        {
            DateTime lx = default;
            if (art is IHistory x) { lx = x.LastModified; x.LastModified = DateTime.Now; }
            else x = null;
            db:
            int i = update ? 0 : 5;
            using (var c = DB.SQL.CreateCommand())
            {
                deb:

                c.Parameters.Clear();
                int j = 0;
                switch (i)
                {
                    case 0:
                    case 6:
                        c.CommandText = art.GetUpdate(c, ref j);
                        break;
                    case 1:
                    case 5:
                        c.CommandText = art.GetInsert(c, ref j);
                        break;
                    default:
                        if (x != null)
                            x.LastModified = lx;
                        return false;
                }
                // fatal error:-2147467259
                // etablision fail:-2147467259
                //MySqlExecption
                //Socket Exception
                //System.InvalidOperationException
                try
                {
                    var aff = c.ExecuteNonQuery();
                    if (i == 0 && aff == 0) { i++; goto deb; }
                    else if (i == 6 && aff == 0) { i++; goto deb; }
                    return true;
                }
                catch (MySql.Data.MySqlClient.MySqlException e)
                {
                    if (e.ErrorCode == 1046)
                    {
                        c.CommandText = "use " + Resource.DatabasePath + ";" + c.CommandText + ";";
                        goto deb;
                    }
                    i++;
                    goto deb;
                }
                catch (Exception e)
                {
                    i++;
                    goto deb;
                }
            }
        }

        public bool StrictSave(DataRow art, bool update)
        {
            DateTime lx = default;
            if (art is IHistory x) { lx = x.LastModified; x.LastModified = DateTime.Now; }
            else x = null;
            using (var c = DB.SQL.CreateCommand())
            {
                c.Parameters.Clear();
                int j = 0;
                switch (update)
                {
                    case true:
                        c.CommandText = art.GetUpdate(c, ref j);
                        break;
                    default:
                        c.CommandText = art.GetInsert(c, ref j);
                        break;
                }
                deb:
                try
                {
                    var aff = c.ExecuteNonQuery();
                    return true;
                }

                catch (MySql.Data.MySqlClient.MySqlException e)
                {
                    if (e.ErrorCode == 1046)
                    {
                        c.CommandText = "use " + Resource.DatabasePath + ";" + c.CommandText + ";";
                        goto deb;
                    }
                    goto deb;
                }
                catch
                {
                    if (x != null)
                        x.LastModified = lx;
                    return false;
                }
            }
        }

        public bool? Save(IDatabaseOperation x)
        {
            if (!x.CanBuild) return true;
            using (var sqlTran = DB.SQL.BeginTransaction())
            using (var c = DB.SQL.CreateCommand())
            {
                deb:
                try
                {
                    c.CommandText = x.BuildSqlCommand(this, c);
                    var aff = c.ExecuteNonQuery();
                    sqlTran.Commit();
                    return true;
                }

                catch (MySql.Data.MySqlClient.MySqlException e) when (e.ErrorCode == 1046)
                {

                    c.CommandText = "use " + Resource.DatabasePath + ";" + c.CommandText + ";";
                    goto deb;
                }
                catch (Exception e)
                {
                    try
                    {
                        sqlTran.Rollback();
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    return false;
                }

            }
        }
        public bool Exec(string cmd)
        {
            if (DB.SQL.State == ConnectionState.Closed)
                DB.SQL.Open();
            using (var c = DB.SQL.CreateCommand())
            {
                try
                {
                    c.CommandText = cmd;
                    c.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e) { MyConsole.WriteLine(e.Message); return false; }
            }
        }


        public void CreateTableIfNotExist(string tableName, Type datarowType)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("message", nameof(tableName));
            if (!typeof(DataRow).IsAssignableFrom(datarowType)) throw new ArgumentNullException(nameof(datarowType));

            var x = DataTable.CreateTable(tableName, GetProperties(datarowType));
            x = x.Replace("CREATE TABLE", "CREATE TABLE IF NOT EXISTS ");
            using (var c = DB.SQL.CreateCommand())
                try
                {
                    c.CommandText = x;
                    c.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MyConsole.WriteLine(e.Message);
                }
        }

        public void CreateTable(DataRow t)
        {
            var _tbls = new List<string>();

            {
                _tbls.Add("DROP TABLE [dbo].[" + t.TableName + "]");
                _tbls.Add(DataTable.CreateTable(t.TableName, GetProperties(t.GetType())));
            }
            foreach (var x in _tbls)
                using (var c = DB.SQL.CreateCommand())
                    try
                    {
                        c.CommandText = x;
                        c.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        MyConsole.WriteLine(e.Message);
                    }
        }
        private void ResetTable(int index)
        {
            var property = GetProperties()[index];
            var _tbls = new List<string>(2);
            if (typeof(DataTable).IsAssignableFrom(property.Type))
            {
                _tbls.Add("DROP TABLE [dbo].[" + property.Name + "]");
                _tbls.Add(DataTable.CreateTable(property.Name, GetProperties(property.Type.BaseType.GetGenericArguments()[0])));
            }
            if (DB.SQL.State == ConnectionState.Closed)
                DB.SQL.Open();
            foreach (var x in _tbls)
                using (var c = DB.SQL.CreateCommand())
                    try
                    {
                        c.CommandText = x;
                        c.ExecuteNonQuery();
                    }
                    catch (Exception ee)
                    {
                        MyConsole.WriteLine(ee.Message);
                    }
        }
        public void CreateEntity()
        {
            var _tbls = new List<string>();
            foreach (var property in GetProperties())
                if (typeof(DataTable).IsAssignableFrom(property.Type))
                {
                    _tbls.Add("DROP TABLE " + property.Name + "");
                    try
                    {
                        _tbls.Add(DataTable.CreateTable(property.Name, GetProperties(property.Type.BaseType.GetGenericArguments()[0])));
                    }
                    catch (Exception e)
                    {
                        MyConsole.WriteLine(e.Message);
                    }

                }

            foreach (var x in _tbls)
                using (var c = DB.SQL.CreateCommand())
                    try
                    {
                        c.CommandText = x;
                        c.ExecuteNonQuery();
                    }
                    catch (Exception ee)
                    {
                        MyConsole.WriteLine(ee.Message);
                    }
        }

        public static bool IsUploading;
        public virtual void Load() => new DatabaseUpdator(this).Update();
        
        protected virtual void InitTables()
        {
            foreach (var p in GetProperties(typeof(Database)))
                if (typeof(DataTable).IsAssignableFrom(p.Type))
                {

                    tables[p.Type] = p;
                    var t = p.Type;
                    do
                    {
                        if (t.GetGenericArguments().Length != 0)
                            tables[t.GetGenericArguments()[0]] = p;
                        t = t.BaseType;
                    } while (t != typeof(DataTable));
                }
        }
        public DataBaseStructure() => InitTables();
    }

    public class Database : DataBaseStructure
    {
        public new static int __LOAD__(int dp) => DataBaseStructure.__LOAD__(DPFournisseurs);
        public static int DPSFactures = Register<Database, SFactures>("SFactures", PropertyAttribute.NonSerializable, null, Empty);
        public static int DPAgents = Register<Database, Agents>("Agents", PropertyAttribute.NonSerializable, null, Empty);

        public static int DPArticles = Register<Database, Articles>("Articles", PropertyAttribute.NonSerializable, null, Empty);
        public static int DPCategories = Register<Database, Categories>("Categories", PropertyAttribute.NonSerializable, null, Empty);
        public static int DPClients = Register<Database, Clients>("Clients", PropertyAttribute.NonSerializable, null, Empty);
        public static int DPFournisseurs = Register<Database, Fournisseurs>("Fournisseurs", PropertyAttribute.NonSerializable, null, Empty);

        public static int DPFactures = Register<Database, Factures>("Factures", PropertyAttribute.NonSerializable, null, Empty);

        public static int DPProducts = Register<Database, Products>("Products", PropertyAttribute.NonSerializable, null, Empty);

        public static int DPVersments = Register<Database, Versments>("Versments", PropertyAttribute.NonSerializable, null, Empty);
        public static int DPSVersments = Register<Database, SVersments>("SVersments", PropertyAttribute.NonSerializable, null, Empty);
        public static int DPLogins = Register<Database, Logins>("Logins", PropertyAttribute.NonSerializable, null, Empty);

        public static int DPFakePrices = Register<Database, FakePrices>("FakePrices", PropertyAttribute.NonSerializable, null, Empty);
        public static int DPPictures = Register<Database, Pictures>("Pictures", PropertyAttribute.NonSerializable, null, Empty);

        public readonly static int DPAppSettings = Register<Database, AppSettings>("AppSettings", PropertyAttribute.None, null, Empty);
        public AppSettings AppSettings { get { return get<AppSettings>(DPAppSettings); } set { set(DPAppSettings, value); } }



        public static int DPMails = Register<Database, Mails>("Mails", PropertyAttribute.Private, null, Empty);
        public Mails Mails { get => get<Mails>(DPMails); set => set(DPMails, value); }

        public static int DPProjets = Register<Database, Projets>("Projets", PropertyAttribute.None, null, Empty);
        public Projets Projets { get => get<Projets>(DPProjets); set => set(DPProjets, value); }


        public static int DPCArticles = Register<Database, CArticles>("CArticles", PropertyAttribute.Private, null, Empty);
        public CArticles CArticles { get => get<CArticles>(DPCArticles); set => set(DPCArticles, value); }

        public static int DPCommands = Register<Database, Commands>("Commands", PropertyAttribute.Private, null, Empty);
        public Commands Commands { get => get<Commands>(DPCommands); set => set(DPCommands, value); }

        public static int DPUsersSetting = Register<Database, UsersSetting>(nameof(UsersSetting), PropertyAttribute.Private, null, Empty);
        public UsersSetting UsersSetting { get => get<models.UsersSetting>(DPUsersSetting); set => set(DPUsersSetting, value); }
        
        public static int DPSMSs = Register<Database, SMSs>("SMSs", OnUpload: Empty);
        public SMSs SMSs { get => get<SMSs>(DPSMSs); set => set(DPSMSs, value); }

        private static Database database;
        public static Database __Default => database ?? (database = new Database());

        public Agents Agents
        {
            get => (Agents)get(DPAgents);
            set => set(DPAgents, value);
        }
        public Articles Articles
        {
            get => (Articles)get(DPArticles);
            set => set(DPArticles, value);
        }
        public Categories Categories
        {
            get => (Categories)get(DPCategories);
            set => set(DPCategories, value);
        }
        public Clients Clients
        {
            get => (Clients)get(DPClients);
            set => set(DPClients, value);
        }
        public Fournisseurs Fournisseurs
        {
            get => (Fournisseurs)get(DPFournisseurs);
            set => set(DPFournisseurs, value);
        }
        public Logins Logins
        {
            get => (Logins)get(DPLogins);
            set => set(DPLogins, value);
        }

        public Factures Factures
        {
            get => (Factures)get(DPFactures);
            set => set(DPFactures, value);
        }
        
        public Products Products
        {
            get => (Products)get(DPProducts);
            set => set(DPProducts, value);
        }
        public SFactures SFactures
        {
            get => (SFactures)get(DPSFactures);
            set => set(DPSFactures, value);
        }
        public Versments Versments
        {
            get => (Versments)get(DPVersments);
            set => set(DPVersments, value);
        }
        public SVersments SVersments
        {
            get => (SVersments)get(DPSVersments);
            set => set(DPSVersments, value);
        }
        public FakePrices FakePrices
        {
            get => (FakePrices)get(DPFakePrices);
            set => set(DPFakePrices, value);
        }
        
        public int Save(out int count)
        {
            count = 0;
            var errors = 0;
            var dps = GetProperties();
            if (DB.SQL.State == ConnectionState.Closed)
                DB.SQL.Open();
            using (var cmd = DB.SQL.CreateCommand())
            {
                cmd.CommandText = "ClearDatabase";
                var e = cmd.ExecuteScalar();
                foreach (var dp in dps)
                {
                    var v = GetValue(dp) as DataTable;
                    if (v != null)
                        foreach (var pair in v.AsList())
                            try
                            {
                                cmd.Parameters.Clear();
                                int i = 0;
                                count++;
                                cmd.CommandText = pair.Value.GetInsert(cmd, ref i);
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception)
                            {
                                errors++;
                            }
                }
            }
            return errors;
        }
        
        private void InitializeProps()
        {
            Agents = new Agents(this) { IsHistory = true };
            Articles = new Articles(this) { IsHistory = true };
            Categories = new Categories(this) { IsHistory = true };
            Clients = new Clients(this) { IsHistory = true };
            Factures = new Factures(this) { IsHistory = true };
            //Pictures = new Pictures(this);
            Products = new Products(this) { IsHistory = true };
            Versments = new Versments(this);
            SVersments = new SVersments(this);
            Logins = new Logins(this) { IsHistory = true };
            FakePrices = new FakePrices(this) { IsHistory = true };
            SFactures = new SFactures(this) { IsHistory = true };
            Fournisseurs = new Fournisseurs(this) { IsHistory = true };
            AppSettings = new AppSettings(this);
            Mails = new Mails(null);
            Projets = new Projets(null);
            Commands = new Commands(this);
            CArticles = new CArticles(this);
            UsersSetting = new UsersSetting(this);
            SMSs = new SMSs(this);
            new models.Agent();
            new models.Article();
            new models.Category();
            new models.Client();
            new models.Facture();
            new models.Picture();
            new models.Product();
            new models.Versment();
            new models.SVersment();
            new models.Login();
            new models.FakePrice();
            new models.SFacture();

            new Fournisseur();
            new AppSetting();
            new Projet();
            new UserSetting();

            new EtatTransfer();
            new EtatTransfers(this);
            new SMS();
            new Ticket();
            new models1.File();
            new models1.Folder();
            Tickets.__LOAD__(09);
        }

        

        public Database()
        {
            InitializeProps();
            try
            {
                if (!DB.Initialize()) throw new Exception("Error");

            }
            catch (Exception e)
            {
                MyConsole.WriteLine(e.Message);
                return;
            }

            var error = !DB.CheckTables(GetProperties(GetType()));
            if (!DB.Execute($"use `" + Resource.DatabasePath + "`")) throw new Exception("Enable to select database");
            if (error)
                this.CreateEntity();

            Program.OnExit += () => Dispose();
        }
        
        internal void GetDealer(Path c)
        {

            if (c.Id != 0)
                c.Owner.set(c.Property.Index, Clients[c.Id]);
        }

        internal void GetCategory(Path c)
        {

            if (c.Id != 0)
                c.Owner.set(c.Property.Index, Categories[c.Id]);
        }

        internal void GetVersment(Path c)
        {
        }

        internal void GetClient(Path c)
        {
            if (c.Id != 0)
                c.Owner.set(c.Property.Index, Clients[c.Id]);
        }

        internal void GetProjet(Path c)
        {
            if (c.Id != 0)
                c.Owner.set(c.Property.Index, Projets[c.Id]);
        }

        internal void GetFournisseur(Path c)
        {
            if (c.Id != 0)
                c.Owner.set(c.Property.Index, Fournisseurs[c.Id]);
        }

        internal void GetCommand(Path c)
        {
            if (c.Id != 0)
                c.Owner.set(c.Property.Index, Commands[c.Id]);
        }

        internal void GetProduct(Path c)
        {

            if (c.Id != 0)
                c.Owner.set(c.Property.Index, Products[c.Id]);
        }

        internal void GetFacture(Path c)
        {
            if (c.Id != 0)
                c.Owner.set(c.Property.Index, Factures[c.Id]);
        }

        internal void GetSFacture(Path c)
        {
            if (c.Id != 0)
                c.Owner.set(c.Property.Index, SFactures[c.Id]);
        }

        internal void GetAgent(Path c)
        {
            if (c.Id != 0)
                c.Owner.set(c.Property.Index, Agents[c.Id]);
        }

        internal void GetArticles(Path c)
        {
            var o = c.Owner as Facture;
            foreach (var p in Articles.AsList())
            {
                var v = p.Value as Article;
                var w = v.Facture;
                if (w != null && w == o)
                    o.Articles.Add(v);
            }
        }


        internal void GetPrice(Path c)
        {
            if (c.Id != 0)
                c.Owner.set(c.Property.Index, FakePrices[c.Id]);
        }

        internal void GetFakePrice(Path c)
        {
            if (c.Id != 0)
            {
                var e = FakePrices[c.Id];
                c.Owner.set(c.Property.Index, e);
            }
        }

        internal void GetLogin(Path c)
        {
            c.Owner.set(c.Property.Index, Logins[c.Id]);
        }

        internal void GetSVersment(Path c)
        {
            if (c.Id != 0)
                c.Owner.set(c.Property.Index, SVersments[c.Id]);
        }
        
        public override void Load()
        {
            base.Load();
            if (AppSettings.Count == 0)
            {
                var x = new AppSetting(this);
                AppSettings.Add(x);
                Save(x, false);
            }
            else
            {
                (AppSettings.AsList()[0].Value as AppSetting).Database = this;
            }
        }
    }
}