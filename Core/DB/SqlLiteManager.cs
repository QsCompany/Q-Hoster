//using System;
//using System.Data;
//using MySql.Data.MySqlClient;
//using System.Data.SQLite;
//using System.Data.Common;

//namespace models
//{
//    public sealed class SqlLiteManager : DBManager
//    {
//        public SqlLiteManager()
//        {
//        //    var a = SQL;
//        //    var cmd = a.CreateCommand();
//        //    cmd.CommandText = "create table test(id integer)";
//        //    cmd.ExecuteNonQuery();

//        }
//        private SQLiteConnection sql;
//        public override DbConnection SQL
//        {
//            get
//            {

//                if (sql == null)
//                {
//                    System.Data.SQLite.SQLiteConnectionStringBuilder sb = new System.Data.SQLite.SQLiteConnectionStringBuilder()
//                    {
                       
//                    };
//                    sb.DataSource = "Test";
//                    sql = new SQLiteConnection(sb.ConnectionString);
//                }
//                if (sql.State == ConnectionState.Open) return sql;
//                if (sql.State == ConnectionState.Closed)
//                    sql.Open();
//                return sql;
//            }
//        }
//        public override bool CreateBackup()
//        {
//            throw new NotImplementedException();
//        }

//        public override void ResetConnection()
//        {
//            MySqlConnection.ClearAllPools();
//        }
//    }

//}