using System;
using System.Data;
using System.Data.SqlClient;
using Server;
using System.Data.Common;
using System.Diagnostics;
using System.IO;

namespace models
{
    
    public sealed class SqlManager : DBManager
    {

        private SqlConnection sql;
        public override DbConnection SQL
        {
            get
            {
                if (sql == null)
                {
                    SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder
                    {
                        DataSource = "(LocalDB)\\MSSQLLocalDB",
                        ConnectTimeout = 3000,
                        AttachDBFilename = Resource.DatabasePath
                    };
                    sql = new SqlConnection(sb.ConnectionString);
                }
                if (sql.State == ConnectionState.Open) return sql;
                if (sql.State == ConnectionState.Closed)
                    sql.Open();
                return sql;
            }
        }

        public override bool Backup(string path,out Exception e)
        {
            throw new NotImplementedException();
        }

       

        public override bool Initialize()
        {
            throw new NotImplementedException();
        }

        public override bool IsDatabaseExist()
        {
            throw new NotImplementedException();
        }

        public override void ResetConnection()
        {
            sql = new SqlConnection($@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={Resource.DatabasePath};Connect Timeout=3000");
        }

        public override bool Restore(string path, out Exception e)
        {
            throw new NotImplementedException();
        }


        public override bool CreateBackup(string backupName, out Exception e)
        {
            var t = new FileInfo(Resource.DatabasePath);
            e = null;
            using (var cmd = SQL.CreateCommand())
            {
                backupName = System.IO.Path.Combine(t.Directory.FullName, (backupName ?? DateTime.Now.ToString("dd_MM_yy_HH_mm")) + ".bak");
                var t1 = new FileInfo(backupName);
                if (t1.Exists) t1.Delete();
                cmd.CommandText = $"BACKUP DATABASE [{t.FullName}] TO DISK ='{backupName}'";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception x)
                {
                    e = x;
                    MyConsole.WriteLine(e);
                    return false;
                }
            }
            return true;
        }

        public void Restore(string DatabaseFullPath, string backUpPath)
        {
            using (var sqlcon = new SqlConnection(SQL.ConnectionString))
            {
                sqlcon.Open();
                string UseMaster = "USE master";
                var UseMasterCommand = new SqlCommand(UseMaster, sqlcon);
                UseMasterCommand.ExecuteNonQuery();
                // The below query will rollback any transaction which is
                //running on that database and brings SQL Server database in a single user mode.
                string Alter1 = @"ALTER DATABASE [" + DatabaseFullPath + "] SET Single_User WITH Rollback Immediate";
                var Alter1Cmd = new SqlCommand(Alter1, sqlcon);
                Alter1Cmd.ExecuteNonQuery();
                // The below query will restore database file from disk where backup was taken ....
                string Restore = @"RESTORE DATABASE [" + DatabaseFullPath + "] FROM DISK = N'" + backUpPath + @"' WITH  FILE = 1,  NOUNLOAD,  STATS = 10";
                var RestoreCmd = new SqlCommand(Restore, sqlcon);
                RestoreCmd.ExecuteNonQuery();
                // the below query change the database back to multiuser
                string Alter2 = @"ALTER DATABASE [" + DatabaseFullPath + "] SET Multi_User";
                var Alter2Cmd = new SqlCommand(Alter2, sqlcon);
                Alter2Cmd.ExecuteNonQuery();
            }
        }

        public void CreateDatabase()
        {
            var t = Resource.DatabasePath;
            var fi = new System.IO.FileInfo(Resource.DatabasePath);
            var lfi = new System.IO.FileInfo(System.IO.Path.ChangeExtension(fi.FullName, "ldt"));
            if (fi.Exists)
                return;
            if (lfi.Exists) lfi.Delete();
            reset:
            MyConsole.WriteLine("Your Databse Being Create");
            SqlConnection.ClearAllPools();
            SqlConnection myConn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Integrated security=SSPI;database=master");
            var str =
                $"CREATE DATABASE [{fi.FullName}] ON PRIMARY  (NAME = System,  FILENAME = '{fi.FullName}',  SIZE = 2MB, MAXSIZE = 200MB, FILEGROWTH = 10%) " +
                $"LOG ON (NAME = SystemLog, FILENAME = '{lfi.FullName}',  SIZE = 1MB,  MAXSIZE = 600MB,  FILEGROWTH = 10%)";

            SqlCommand myCommand = new SqlCommand(str, myConn);
            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
                MyConsole.WriteLine("Your Database Created Successfully");
            }
            catch (SqlException ex)
            {
                MyConsole.WriteLine($"Error : {ex.Message}");
                if (ex.Number == 1801) goto reset;
                SqlConnection.ClearAllPools();
                MyConsole.WriteLine("Fatal Error Occured contact QShop Company 0541082226");
                MyConsole.ReadLine();
                Process.GetCurrentProcess().Close();
                return;
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                    myConn.Close();
            }
            try { SQL?.Close(); } catch(Exception e) { MyConsole.WriteLine(e.Message); } finally { SqlConnection.ClearAllPools(); }
            ResetConnection();
        }

        public override bool CheckTables(DProperty[] dProperty)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            sql.Dispose();
            sql = null;
        }

        public override bool Execute(string v)
        {
            throw new NotImplementedException();
        }
    }
    /*
    public sealed class OledbManager : DBManager
    {

        private System.Data.OleDb.OleDbConnection sql;
        public override DbConnection SQL
        {
            get
            {
                if (sql == null)
                {
                    System.Data.OleDb.OleDbConnectionStringBuilder sb =new System.Data.OleDb.OleDbConnectionStringBuilder()
                    {
                        DataSource = "(LocalDB)\\MSSQLLocalDB",
                    };
                    sb.DataSource = Resource.DatabasePath;
                    sql = new System.Data.OleDb.OleDbConnection(sb.ConnectionString);
                }
                if (sql.State == ConnectionState.Open) return sql;
                if (sql.State == ConnectionState.Closed)
                    sql.Open();
                return sql;
            }
        }

        public override bool CreateBackup()
        {
            throw new NotImplementedException();
        }

        public override void ResetConnection()
        {
        }
    }*/
    
}