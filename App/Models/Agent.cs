using System;
using System.Collections.Generic;
using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Agent), typeof(AgentSerializer))]
    public class Agent : Login
    {
        public new static int __LOAD__(int dp) => Login.__LOAD__(DPName);
        public static int DpIsDisponible = Register<Agent, bool>("IsDisponible");
        public static int DPPermission = Register<Agent, AgentPermissions>("Permission");

        public static int DPName = Register<Agent, string>("Name");
        public string Name { get => get<string>(DPName); set => set(DPName, value); }

        public bool IsDisponible { get => get<bool>(DpIsDisponible);
            set => set(DpIsDisponible, value);
        }
        public override AgentPermissions Permission { get => get<AgentPermissions>(DPPermission); set => set(DPPermission, value); }
        public Agent()
        {
        }
        public Agent(Context c,JValue jv):base(c,jv)
        {
        }
        internal bool? GetSVersments(ref SVersments source)
        {
            var t = new models.SVersments(null);
            var id = Id;
            foreach (System.Collections.Generic.KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.SVersment)x.Value;
                if (sv.Cassier?.Id == id) t.Add(sv);
            }
            source = t;
            return true;
        }
        internal bool? GetVersments(ref Versments source)
        {
            var t = new models.Versments(null);
            var id = Id;
            foreach (System.Collections.Generic.KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.Versment)x.Value;
                if (sv.Cassier?.Id == id) t.Add(sv);
            }
            source = t;
            return true;
        }

        public override bool Check()
        {
            return base.Check() && GlobalRegExp.name.IsMatch(Name ?? "");
        }
        public override bool Check(RequestArgs args)
        {
            var cc = @"^[a-z|A-Z\d\s]*$".toRegexp();
            if (!GlobalRegExp.name.IsMatch(Name ?? "")) return args.SendError("Agent Name " + Name + " IsInvalid");
            return base.Check(args);
        }
        public override bool Check(RequestArgs args, ref List<Client.Message> k)
        {
            var t = base.Check(args, ref k);
            if (!GlobalRegExp.name.IsMatch(Name ?? "")) { k.Add(new Client.Message("Argument Null", "Agent Name " + Name + " IsInvalid")); return false; }
            return t;
        }
    }
}