using System;
using System.Data.Common;
using Server;

namespace models
{
    public abstract class DBManager:IDisposable
    {
        public abstract DbConnection SQL { get; }

        public abstract bool Backup(string path, out Exception e);
        public abstract bool Restore(string path, out Exception e);


        public abstract bool CreateBackup(string fileName ,out Exception e);

        public abstract void ResetConnection();

        public abstract bool IsDatabaseExist();

        public abstract bool Initialize();

        public abstract bool CheckTables(DProperty[] dProperty);

        public abstract void Dispose();

        public abstract bool Execute(string v);
    }

}