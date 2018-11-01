using System;
using System.Collections.Generic;
using System.Linq;
using Json;
using Server;

namespace models
{
    public class UserSetting : DataRow, IHistory
    {
        public new static int __LOAD__(int dp) => DataRow.__LOAD__(DPopened_sfactures);
        public static int DPTopNavBarVisibility = Register<UserSetting, bool>("TopNavBarVisibility");
        public bool TopNavBarVisibility { get => get<bool>(DPTopNavBarVisibility); set => set(DPTopNavBarVisibility, value); }

        public static int DPOfflineMode = Register<UserSetting, bool>("OfflineMode");
        public bool OfflineMode { get => get<bool>(DPOfflineMode); set => set(DPOfflineMode, value); }

        public static int DPAppTitleHidden = Register<UserSetting, bool>("AppTitleHidden");
        public bool AppTitleHidden { get => get<bool>(DPAppTitleHidden); set => set(DPAppTitleHidden, value); }

        public static int DPShowSPTooltips = Register<UserSetting, bool>("ShowSPTooltips");
        public bool ShowSPTooltips { get => get<bool>(DPShowSPTooltips); set => set(DPShowSPTooltips, value); }

        public static int DPopened_facture = Register<UserSetting, long>("opened_facture");
        public long opened_facture { get => get<long>(DPopened_facture); set => set(DPopened_facture, value); }

        public static int DPopened_sfacture = Register<UserSetting, long>("opened_sfacture");
        public long opened_sfacture { get => get<long>(DPopened_sfacture); set => set(DPopened_sfacture, value); }

        public static int DPselectedPage = Register<UserSetting, string>("selectedPage");
        public string selectedPage { get => get<string>(DPselectedPage); set => set(DPselectedPage, value); }

        public static int DPopened_factures = Register<UserSetting, string>("opened_factures", Server.PropertyAttribute.None, null, null, "NVARCHAR(250)");
        public string opened_factures { get => get<string>(DPopened_factures); set => set(DPopened_factures, value); }

        public static int DPopened_sfactures = Register<UserSetting, string>("opened_sfactures", Server.PropertyAttribute.None, null, null, "NVARCHAR(250)");
        public string opened_sfactures { get => get<string>(DPopened_sfactures); set => set(DPopened_sfactures, value); }
        public UserSetting()
        {
            
        }
        public override string TableName => nameof(Database.UsersSetting);
        public UserSetting(Context c, JValue jv) : base(c, jv)
        {
        }
    }
    public class UsersSetting : DataTable<UserSetting>
    {
        
        public UsersSetting(DataRow owner) : base(owner)
        {
        }

        public UsersSetting(Context c, JValue jv) : base(c, jv)
        {
        }

        protected override void GetOwner(DataBaseStructure d, Path c)
        {
            c.Owner.set(c.Property.Index, d);
        }
    }
    [QServer.Core.HosteableObject(typeof(UserSettingService),typeof(UserSettingSerializer))]
    public class UserSettingService : Server.Service
    {
        public UserSettingService() : base(nameof(UserSetting))
        {
        }
        private UserSetting GetUserSetting(RequestArgs args)
        {
            var db = args.Database;
            var app = db.UsersSetting[args.Client.Id];
            if (app == null)
            {
                db.UsersSetting.Add(app = new UserSetting() { Id = args.Client.Id });
                db.Save(app, false);
            }
            return app;
        }
        public override bool Get(RequestArgs args)
        {
            args.Send(GetUserSetting(args));
            return true;
        }
        public override bool Post(RequestArgs args)
        {
            args.JContext.RequireNew = RequireAllNew;
            if (!(args.BodyAsJson is UserSetting us)) return args.SendFail();
            var ous = GetUserSetting(args);
            us.Id = ous.Id;
            ous.CopyFrom(us);
            if (args.Database.Save(ous, true)) return args.SendSuccess();
            else return args.SendFail();
        }
    }

    public class UserSettingSerializer : DataRowTypeSerializer
    {
        public UserSettingSerializer() : base("models.UserSetting")
        {
        }

        public override DataTable Table => Database.__Default.UsersSetting;

        public override bool CanBecreated => true;

        protected override JValue CreateItem(Context c, JValue jv) => new UserSetting(c, jv);
    }
}
