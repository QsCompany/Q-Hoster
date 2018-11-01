using System;
using System.IO;
using System.Runtime.InteropServices;
using Json;
using models;

namespace Server
{
    [Serializable]
    public class AppSetting : DataRow
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
        public new static int __LOAD__(int dp) => DPCheckPrices;
        public readonly static int DPAllowDuplicateArticles = Register<AppSetting, bool>("AllowDuplicateArticles", PropertyAttribute.None, null);
        public bool AllowDuplicateArticles { get { return get<bool>(DPAllowDuplicateArticles); } set { set(DPAllowDuplicateArticles, value); } }

        public readonly static int DPIsQteLimited = Register<AppSetting, bool>("IsQteLimited", PropertyAttribute.None, null);
        public bool IsQteLimited { get { return get<bool>(DPIsQteLimited); } set { set(DPIsQteLimited, value); } }

        public readonly static int DPName = Register<AppSetting, string>("Name", PropertyAttribute.None, null);
        public string Name { get { return get<string>(DPName); } set { set(DPName, value); } }

        public readonly static int DPPath = Register<AppSetting, string>("Path", PropertyAttribute.None, null);
        public string Path { get { return get<string>(DPPath); } set { set(DPPath, value); } }

        public readonly static int DPDatabasePath = Register<AppSetting, string>("DatabasePath", PropertyAttribute.None, null);
        public string DatabasePath { get { return get<string>(DPDatabasePath); } set { set(DPDatabasePath, value); } }

        public readonly static int DPAddresses = Register<AppSetting, string>("Addresses", PropertyAttribute.None, null);
        public string Addresses { get { return get<string>(DPAddresses); } set { set(DPAddresses, value); } }

        public readonly static int DPFactureCounter = Register<AppSetting, int>("FactureCounter", PropertyAttribute.None, null);
        public int FactureCounter { get { return get<int>(DPFactureCounter); } set { set(DPFactureCounter, value); } }
        
        public readonly static int DPIdentificater = Register<AppSetting, int>("Identificater", PropertyAttribute.None, null);
        public int Identificater { get { return get<int>(DPIdentificater); } set { set(DPIdentificater, value); } }

        public readonly static int DPSFactureCounter = Register<AppSetting, int>("SFactureCounter", PropertyAttribute.None, null);
        public int SFactureCounter { get { return get<int>(DPSFactureCounter); } set { set(DPSFactureCounter, value); } }

        public readonly static int DPVersmentCounter = Register<AppSetting, int>("VersmentCounter", PropertyAttribute.None, null);
        public int VersmentCounter { get { return get<int>(DPVersmentCounter); } set { set(DPVersmentCounter, value); } }

        public readonly static int DPSVersmentCounter = Register<AppSetting, int>("SVersmentCounter", PropertyAttribute.None, null);
        public int SVersmentCounter { get { return get<int>(DPSVersmentCounter); } set { set(DPSVersmentCounter, value); } }

        public readonly static int DPCheckPrices = Register<AppSetting, bool>("CheckPrices", PropertyAttribute.None, null);
        public bool CheckPrices { get { return get<bool>(DPCheckPrices); } set { set(DPCheckPrices, value); } }

        public AppSetting() => Default = this;

        public AppSetting(Database database)  :this()
        {
            Database = database;
        }

        protected AppSetting(Context c, JValue jv) : base(c, jv)
        {
        }

        private Database _database;
        public static AppSetting Default;

        public Database Database
        {
            get { return _database; }
            set
            {
                _database = value;
            }
        }

        protected override void OnPropertyChanged(DProperty dp)
        {
            Database?.Save(this, true);
            base.OnPropertyChanged(dp);                    
        }
        public static FileInfo File => new FileInfo(System.IO.Path.Combine(Resource.ResourcePath, "AppSettings.json"));
    }

    public class AppSettings : DataTable<AppSetting>
    {
        public new static int __LOAD__(int dp) => DPOwner;
        public AppSettings(Database database) : base(database)
        {
        }

        protected AppSettings(Context c, JValue jv) : base(c, jv)
        {
        }

        protected override void GetOwner(DataBaseStructure d, models.Path c)
        {
            
        }
    }
}        