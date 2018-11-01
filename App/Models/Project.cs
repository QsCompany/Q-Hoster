using models;
using System;
using System.Collections.Generic;
using Json;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Projet), typeof(Serializers.ProjetSerializer))]
    public class Projet : SiegeSocial
    {
        public new static int __LOAD__(int dp) => SiegeSocial.__LOAD__(DPName);
        
        public static int DPName = Register<Projet, string>("Name");
        public string Name { get => get<string>(DPName); set => set(DPName, value); }        
        
        public Projet()
        {
        }
        public Projet(Context c, JValue jv) : base(c, jv)
        {
        }

        public bool? GetFactures(ref Factures source)
        {
            return null;
        }
        public bool? GetVersments(ref Versments source)
        {
            return null;
        }
        
        public bool Check()
        {
            return GlobalRegExp.name.IsMatch(Name ?? "");
        }
        public bool Check(RequestArgs args)
        {
            return true;
        }
        public bool Check(RequestArgs args, ref List<Client.Message> k)
        {
            return true;
        }
        static Projet()
        {
            var xx = DObject.ToTSObject(typeof(Projets));
        }
    }

    [QServer.Core.HosteableObject(typeof(Api.Projets), typeof(Serializers. ProjetsSerializer))]
    public class Projets : DataTable<Projet>
    {
        public Projets(DataRow owner) : base(owner)
        {

        }
        protected override void GetOwner(DataBaseStructure d, Path c) => ((Database)d).GetProjet(c);

        public Projets(Context c, JValue jv) : base(c, jv)
        {
        }
        public override JValue Parse(JValue json)
        {
            return json;
        }
    }


}
namespace Api
{
    public class Projet : Service
    {

        public Projet()
            : base("Projet")
        {

        }
        public override bool Create(RequestArgs args)
        {
            var t = new models.Projet()
            {
                Id = models.DataRow.NewGuid(),
                Name = args.GetParam("Name")
            };
            args.Send(t);
            return true;
        }
        public override bool Get(RequestArgs args)
        {
            var t = args.Database.Projets;
            args.Send(t[args.Id]);
            return true;
        }

        public override bool Post(RequestArgs args)
        {
            args.JContext.RequireNew = (cx, l) => cx == "models.Projet";
            var c = args.BodyAsJson as models.Agent;
            if (c == null) return args.SendFail();
            var oagent = args.Database.Agents[c.Id];
            return args.Server.SignupAgent(args, c) ? args.SendInfo("The Agent Successfully Changed", true) : false;
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
        public override bool Delete(RequestArgs args)
        {
            return args.SendFail();
        }
    }
    public class Projets : TableService
    {
        public Projets() : base("Projets")
        {
        }
        public override bool Get(RequestArgs args)
        {
            if (!args.User.IsAgent) return args.SendError(QServer.Core.CodeError.access_restricted);
            args.JContext.Add(typeof(models.Projets), new DataTableParameter(typeof(models.Projets)) { SerializeItemsAsId = false });
            args.Client.SetCookie(Name.ToLower() + "_lasttimeupdated", System.DateTime.Now, args.Server.ExpiredTime);
            args.Send(args.Database.Projets);
            return true;
        }
        public override bool CheckAccess(RequestArgs args) => args.User.IsAgent;
    }
}

namespace Serializers
{
    public class ProjetSerializer : DataRowTypeSerializer
    {
        public ProjetSerializer()
            : base("models.Projet")
        {

        }

        public override bool CanBecreated => false;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return new Agent(c, jv);
        }
        public override DataTable Table => data.Projets;

        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }
    }
    public class ProjetsSerializer : Typeserializer
    {
        public ProjetsSerializer()
            : base("models.Projets")
        {

        }
        public override JValue ToJson(Context c, object ov)
        {
            return null;
        }
        public override JValue Swap(Context c, JValue jv, bool requireNew) => new Projets(c, jv);
        public override Object FromJson(Context c, JValue jv) => new Projets(c, jv);

        public override void Stringify(Context c, object p)
        {
            throw new NotImplementedException();
        }

        public override void SimulateStringify(Context c, object p)
        {
            throw new NotImplementedException();
        }
    }
}