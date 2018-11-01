using System;
using System.Threading;
using Server;

namespace QServer.Services
{
    [Core.HosteableObject(typeof(SessionIdService))]
    public class SessionIdService : Service
    {
        private static Guid _gsessionId = Guid.NewGuid();
        private static string _sessionID = "\"" + _gsessionId.ToString("N") + "\"";
        public static string SessionId
        {
            get
            {
                return _sessionID;
            }
        }
        public static Guid Guid { get { return _gsessionId; } }
        public SessionIdService() : base("SessionId")
        {
        }
        static object _syncObject = new object();
        

        public override bool Get(RequestArgs args)
        {
            args.Send(SessionId);
            return true;
        }
    }
    [Core.HosteableObject(typeof(GuidService ),typeof(GuidSerializer))]
    public class GuidService:Service
    {
        public GuidService():base("Guid")
        {

        }
        static object _syncObject = new object();
        
        public static long GetGuid() {
            lock (_syncObject)
            {
                var dt = DateTime.Now;
                var Y = dt.Year;
                var M = dt.Month;
                var D = dt.Day;
                var H = dt.Hour;
                var MN = dt.Minute;
                var S = dt.Second;
                var MS = dt.Millisecond;
                var id = (((long)((((Y - 2000) * 12 + M) * 31 + D) * 24 + H) * 60 + MN) * 60 + S) * 1000 + MS;
                Thread.Sleep(2);
                return id;
            }
        }

        public override bool Get(RequestArgs args)
        {
            var s = GetGuid();
            args.Send("[" + s + "," + s + 1000 + "]");
            return true;
        }
    }
}
