using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using Server;
using System.Text;
using System.Collections.Generic;
using Console = MyConsole;
namespace models
{
    public sealed class MySqlManager : DBManager,IDisposable
    {

        public MySqlConnectionStringBuilder SqlConnectionString
        {
            get
            {
                var sb= new MySqlConnectionStringBuilder()
                {
                    Server = Resource.ServerIP,
                    UserID = Resource.UserID,
                    Password = Resource.Password,
                    Port = Resource.Port,
                    SslMode = MySqlSslMode.None,
                };
                if (false)
                    if (!string.IsNullOrEmpty(Resource.DatabasePath)) sb.Database = Resource.DatabasePath;
                return sb;
            }
        }
        private bool handleException(Exception e, int code)
        {
            if (code == 1042)
                if (!QServer.Core.WindowsService.StartService())
                {
                    System.Windows.Forms.MessageBox.Show("The Service MySQL Cannot be started please goto MySQL and make \r\n\t\tStartup type :(Automatique)\r\n\t\t and press button Apply \r\n\t\tand press button start\r\nrestart program");
                    goto exit;
                }
                else return true;
            else
            {
                if (resetVars(e))
                    return true;
            }

            exit:
            Program.Exit(null, true, true);
            return false;
        }

        private MySqlConnection initializeConnection()
        {
            db:
            MySqlConnection sql = new MySqlConnection(SqlConnectionString.ConnectionString);
            try
            {
                if (sql.State == ConnectionState.Open) return sql;
                if (sql.State == ConnectionState.Closed)
                {
                    sql.Open(); 
                }

            }
            catch (MySqlException e)
            {
                do
                    switch (e.Number)
                    {
                        case 1042:
                        case 1045:
                        case 1049:
                            if (handleException(e, e.Number)) goto db;
                            return null;
                    }
                while ((e = e.InnerException as MySqlException) != null);
                return null;
            }
            return sql;
        }
        private bool resetVars(Exception e)
        {
            System.Windows.Forms.MessageBox.Show(e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            try
            {
                sql?.Dispose();
            }
            catch (Exception)
            {
            }
            if (new QServer.ConnectionManager(this).ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return true;
            MyConsole.WriteLine(e.Message);
            sql.Dispose();
            Program.Exit(null, true, true);
            return false;

        }
        public FileInfo getFile(string fileName = null)
        {
            DateTime Time = DateTime.Now;
            int year = Time.Year;
            int month = Time.Month;
            int day = Time.Day;
            int hour = Time.Hour;
            int minute = Time.Minute;
            int second = Time.Second;
            int millisecond = Time.Millisecond;

            //Save file to C:\ with the current date as a filename
            string path;

            path = "D:\\MySqlBackup\\" + (fileName ?? (year + "-" + month + "-" + day + "-" + hour + "-" + minute + "-" + second + "-" + millisecond)) + ".bin";
            var fi = new FileInfo(path);
            return fi;
        }
        private FileInfo checkDir(string path, string file)
        {

            if (path == null || path == "") return null;
            var dir = new DirectoryInfo(path ?? "");
            if (dir.Exists)
            {
                var f = new FileInfo(System.IO.Path.Combine(path, file));
                if (f.Exists) return f;
            }
            return null;
        }
        private FileInfo checkDir(List< string> paths, string file)
        {
            foreach (var path in paths)
            {
                var f = checkDir(path, file);
                if (f != null) return f;
            }
            return null;
        }

        private FileInfo getMySQLPath(string name)
        {
            var t0 = (@"C:\Program Files\MySQL\MySQL Server 5.2\bin");
            var t = System.IO.Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), @"MySQL\MySQL Server 5.2\bin");
            var t1 = System.IO.Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), @"MySQL\MySQL Server 5.2\bin");
            var paths = (Environment.GetEnvironmentVariable("Path") ?? "").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var lst = new List<string>() {Resource.MySQLPath,
                t0,  t,
                t1
            };

            foreach (var item in Environment.GetLogicalDrives())
            {
                lst.Add(System.IO.Path.Combine(item, @"Program Files\MySQL\MySQL Server 5.2\bin\"));
                lst.Add(System.IO.Path.Combine(item, @"Program Files (x86)\MySQL\MySQL Server 5.2\bin\"));
            }

            lst.AddRange(paths);

            var c = checkDir(lst, name);
            if (c != null) Resource.MySQLPath = c.Directory.FullName;
            return c;
        }

        public bool InternalBackup(FileInfo fileName, out Exception e)
        {
            try
            {
                MyConsole.WriteSeparator('*', "Backup Starting", '*');
                var fi = fileName;
                if (!fi.Directory.Exists) fi.Directory.Create();
                if (fi.Exists) fi.Delete();

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = getMySQLPath("mysqldump.exe")?.FullName ?? "mysqldump",
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Console.WriteLine("\tProcess of Backup is :" + psi.FileName);
                var sql = SqlConnectionString;

                psi.Arguments = $@"-u{sql.UserID} -p{sql.Password}  -h{sql.Server} --databases {Resource.DatabasePath}";
                psi.UseShellExecute = false;
                Console.WriteLine("\tArguments of Process of Backup is :" + psi.Arguments);
                byte[] bytes;
                int ec;
                string str;

                using (Process process = Process.Start(psi))
                {
                    Console.WriteLine("\tProcess of Backup is started");
                    bytes = RequestArgs.EncodeGZip(Encoding.Unicode.GetBytes(str = process.StandardOutput.ReadToEnd()));
                    process.WaitForExit();
                    Console.WriteLine("\tProcess of Backup is exited");
                    ec = process.ExitCode;
                    Console.WriteLine("The Process Exited with code " + ec);
                    process.Close();
                }
                if (ec == 0)
                    using (FileStream file = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write))
                    {
                        file.Write(bytes, 0, bytes.Length);
                        file.Close();
                        Console.WriteLine("\tBackup Successfull Saved In :"+file.Name);
                        e = null;
                        Resource.LastTimeBackup = DateTime.Now;
                        return true;
                    }
                else
                {
                    e = new Exception("CodeError : {ec} \r\n Internal Error\r\n" + str) {  };
                    Console.WriteLine("\t\tBackup Error :" + "CodeError : {ec} \r\n Internal Error\r\n" + str);
                    goto err;
                }
            }
            catch (IOException ex)
            {
                e = ex;
                goto err;
            }
            err:
            MyConsole.WriteLine("Error , unable to backup!");
            MyConsole.WriteSeparator();
            MyConsole.WriteLine(e?.Message);
            MyConsole.WriteSeparator('*', "Backup error", '*');
            return false;
        }
        public override bool Backup(string fileName, out Exception e)
        {
            var fi = getFile(fileName);
            if (fi.Exists) { e = new Exception("Essayer avec un autre nom !!"); return false; }
            return InternalBackup(fi, out e);
        }
        public QServer.AesCBC aes = new QServer.AesCBC(new byte[32] { 234, 23, 196, 234, 69, 238, 92, 244, 50, 110, 70, 181, 109, 139, 252, 209, 146, 174, 40, 140, 129, 41, 58, 89, 102, 193, 99, 194, 178, 192, 239, 152 });
        public override bool Restore(string fileName, out Exception e)
        {
            var fi = getFile(fileName);
            return InternalRestore(fi, out e);
        }
        public bool InternalRestore(FileInfo fi,out Exception e)
        {
            try
            {
                string input;
                if (!fi.Exists)
                {
                    e = new Exception("Le Fichier " + fi.FullName + " N'Exist pas");
                    goto err;
                }
                MyConsole.WriteSeparator('*', "Backup Starting", '*');
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = getMySQLPath("mysql.exe")?.FullName ?? "mysql.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };
                var sql = SqlConnectionString;
                psi.Arguments = $@"-u{sql.UserID} -p{sql.Password}  -h{sql.Server} --database={Database}";

                psi.UseShellExecute = false;
                int ec = 0;
                using (Process process = Process.Start(psi))
                {
                    using (FileStream file = fi.OpenRead())
                        input = Encoding.Unicode.GetString(RequestArgs.DecodeGZip(file));
                    process.StandardInput.WriteLine(input);
                    process.StandardInput.Close();
                    process.WaitForExit();
                    ec = process.ExitCode;
                    process.Close();
                }
                e = null;
                if (ec == 0)
                    return true;
                else
                {
                    e = new Exception("CodeError : {ec} \r\n Internal Error\r\n") { };
                    goto err;
                }
            }
            catch (IOException ex)
            {
                e = ex;
                goto err;
            }

            err:
            MyConsole.WriteLine("Error , unable to backup!");
            MyConsole.WriteSeparator();
            MyConsole.WriteLine(e?.Message);
            MyConsole.WriteSeparator('*', "Backup error", '*');
            return false;
        }
        private MySqlConnection sql;

        public MySqlManager()
        {
        }

        public override DbConnection SQL
        {
            get
            {
                if (sql != null) {
                    if (sql.Ping())
                        return sql;
                    else
                    {
                        if (sql.State == ConnectionState.Closed)
                        {
                            sql.Dispose();
                            sql = initializeConnection();
                        }
                        else if (sql.State == ConnectionState.Broken)
                        {
                            sql.Close();
                            sql.Dispose();
                            sql = initializeConnection();
                        }
                        return sql;
                    }
                }
                return sql = initializeConnection();
            }
        }

        public override bool IsDatabaseExist()
        {
            using (var cmd = SQL.CreateCommand())
            {
                cmd.CommandText = $"SHOW DATABASES LIKE '{Server.Resource.DatabasePath}'";
                try
                {
                    using (var cc = cmd.ExecuteReader())
                        return cc.HasRows;
                }
                catch (Exception e)
                {
                    MyConsole.WriteLine(e.Message);
                    return false;
                }
            }
        }
        public override bool CreateBackup(string fileName,out Exception e)
        {
            return Backup(fileName, out e);
        }

        public override void ResetConnection()
        {
            using (SQL)
            {
            }
            sql = null;
        }
        public string Database;
        

        public override bool Initialize() => (sql = initializeConnection()) == null ? false : true;

        private SQL.SQLDatabase _schema;
        public SQL.SQLDatabase DatabaseSchema => _schema ?? (_schema = new SQL.SQLDatabase(SQL));
        public static List<string> GetTables(DbConnection SQL)
        {
            var df = SQL.GetSchema("tables");
            List<string> tables = new List<string>();
            foreach (System.Data.DataRow r in df.Rows)
            {
                var v = r["database_name"] as string;
                switch (v)
                {
                    case "mysql":
                    case "information_schema":
                    case "":
                    case null:
                        continue;
                    default:
                        tables.Add(v.ToLower());
                        break;
                }
            }
            return tables;
        }
        public static List<string> GetDossiers(DbConnection SQL)
        {
            var df = SQL.GetSchema("databases");
            List<string> tables = new List<string>();
            foreach (System.Data.DataRow r in df.Rows)
            {
                var v = r["database_name"] as string;
                switch (v)
                {
                    case "mysql":
                    case "information_schema":
                    case "":
                    case null:
                        continue;
                    default:
                        tables.Add(v.ToLower());
                        break;
                }
            }
            return tables;
        }

        public bool CheckDatabase(MySqlConnection SQL)
        {
            var t = DateTime.Now.ToString("%y-%m-%d %H:%M:%s");
            var df = SQL.GetSchema("databases");
            List<string> tables = new List<string>();
            foreach (System.Data.DataRow r in df.Rows)
            {
                var v = r["database_name"] as string;
                switch (v)
                {
                    case "mysql":
                    case "information_schema":
                    case "":
                    case null:
                        continue;
                    default:
                        tables.Add(v.ToLower());
                        break;
                }
            }

            if (tables.Contains(Resource.DatabasePath ?? ""))
                return true;
            if(tables.Contains("qshopdatabase")) 
            {
                Resource.DatabasePath = Database = "qshopdatabase";
                return true;
            }

            //db:
            //var f = new QServer.SelectDatabase(this, tables);
            //string db=null;
            //bool toSelect = true;
            //if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    toSelect = !f.Create;
            //    db = f.SelectedDatabase;
            //    goto crc;
            //}
            //else
            //    Program.Exit(null, true, true);
            //return false;
            //crc:
            return false;

        }
        public override bool Execute(string cmdString)
        {
            using (var cmd = SQL.CreateCommand())
            {
                db:
                cmd.CommandText = cmdString;
                try
                {
                    var i = cmd.ExecuteNonQuery();
                    return true;
                }
                catch (MySqlException e) when (e.Number == 1046)
                {
                    cmdString = $"use {Resource.DatabasePath}; {cmdString};";
                    goto db;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }


        public override bool CheckTables(DProperty[] dProperties)
        {
            var rslt = new List<SQLCheckResult>();
            var SQL = this.SQL;
            SQL.SQLSchema schema = null;
            SQL.SQLDatabase database;
            try
            {
                db:
                _schema = null;
                database = DatabaseSchema;
                schema = database[Resource.DatabasePath];
                if (schema == null)
                {
                    if (!Execute($"CREATE SCHEMA {Resource.DatabasePath}"))
                    {
                        Resource.DatabasePath = "qshopdatabase";
                        goto db;
                    }
                    goto db;
                }
                if (!Execute($"use `" + Resource.DatabasePath + "`")) throw new Exception("Enable to select database");
            }
            catch (Exception e) { return false; }
            foreach (var dp in dProperties)
                if (typeof(DataTable).IsAssignableFrom(dp.Type))
                {
                    var table = schema[dp.Name];
                    var prs = Server.DObject.GetProperties(dp.Type.BaseType.GetGenericArguments()[0]);
                    if (table == null)
                    {
                        if (!Execute(DataTable.CreateTable(dp.Name, prs))) throw new Exception("The table " + dp.Name + " cannot be created");
                        continue;
                    }

                    foreach (var pr in prs)
                    {
                        if ((pr.Attribute & PropertyAttribute.Optional) == PropertyAttribute.Optional) continue;
                        var col = table[pr.Name];

                        if (col == null)
                        {
                            MyConsole.WriteLine($"Column of {table.Name} has no column {pr.Name}");
                            continue;
                        }

                        if (Server.DBType.GetDBType(col.DBType) == DBType.GetDBType(pr.DBType)) continue;
                        Console.WriteLine($"the type of column {pr.Name} {pr.DBType} is not same as Database Column Type {col.DBType}");
                    }
                }
            
            return true;
        }
        public class SQLCheckResult
        {
            public SQL.ISQLUnit Target { get; set; }
            public string Name { get; set; }
            public string Operation { get; set; }
            public SQL.ISQLUnit Parent { get; set; }
        }
        public override void Dispose()
        {
            sql.Dispose();
            aes = null;
            sql = null;
        }
    }
}