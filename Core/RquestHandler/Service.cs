using System;
using Json;

namespace Server
{
    abstract public class Service
    {
        public static bool RequireNew(string s, JObject k) { return true; }
        public readonly string Name;
        public Service(string name)
        {
            Name = name;
        }
        public virtual bool CheckAccess(RequestArgs args)
        {
            return true;
        }
        public virtual bool Get(RequestArgs args) {
            return false;
        }
        public bool AllowCrossOrigin = true;
        public virtual bool CanbeDelayed(RequestArgs args) => false;
        public virtual bool Options(RequestArgs args) {

            if (AllowCrossOrigin)
            {
                var response = args.context.Response;
                response.AddHeader("Access-Control-Allow-Headers", "*");
                response.AddHeader("Access-Control-Allow-Methods", "*");
                response.AddHeader("Access-Control-Max-Age", "1728000");
                response.AppendHeader("Access-Control-Allow-Origin", "*");
                return true;
            }
            return true;
        }
        public virtual bool Create(RequestArgs args) => false;
        public virtual bool Update(RequestArgs args) => false;
        public virtual bool Delete(RequestArgs args) => false;

        public virtual void Put(RequestArgs args) { }
        public virtual void Head(RequestArgs args) { }
        public virtual bool Post(RequestArgs args) { return false; }
        public virtual void Else(RequestArgs args)
        {
            if (args.Method == "CREATE")
                this.Create(args);
        }

        public virtual bool SUpdate(RequestArgs args)
        {
            return false;
        }

        public virtual void Exec(RequestArgs args)
        {
            //if (AllowCrossOrigin) args.context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            if (!CheckAccess(args)) return;
            switch (args.Method)
            {
                case "GET":
                    Get(args);
                    break;
                case "POST":
                    Post(args);
                    break;

                case "PUT":
                    Put(args);
                    break;

                case "DELETE":
                    Delete(args);
                    break;
                case "CREATE":
                    Create(args);
                    return;
                case "UPDATE":
                    Update(args);
                    break;
                case "SUPDATE":
                    SUpdate(args);
                    break;
                case "OPTIONS":
                    Options(args);
                    return;
                case "PRINT":
                    Print(args);
                    return;
                case "SET":
                    Set(args);
                    return;
                case "HEAD":
                    Head(args);
                    return;
                case "OPEN":
                    Open(args);
                    return;
                default:
                    Else(args);
                    return;
            }
        }

        protected virtual bool Open(RequestArgs args)
        {
            return true;
        }
        protected virtual bool Close(RequestArgs args)
        {
            return true;
        }

        protected virtual bool Set(RequestArgs args) => true;

        public virtual bool Print(RequestArgs args) => true;

        protected static bool RequireAllNew(string type, JObject t) { return true; }
    }
}