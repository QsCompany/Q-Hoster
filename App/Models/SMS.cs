using Json;
using models;
using Server;
using System;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.SMSsService), typeof(Serializers.SMSsSerializer))]
    public class SMSs : DataTable<SMS>
    {
        public SMSs(DataRow owner) : base(owner) { }
        public SMSs() : base(null) { }
        public SMSs(Context c, JValue v) : base(c, v) { }
        protected override void GetOwner(DataBaseStructure d, Path c) => c.Owner.set(c.Property.Index, d);
        public override string Name => nameof(SMSs);
    }
    [QServer.Core.HosteableObject(typeof(Api.SMSService), typeof(Serializers.SMSSerializer))]
    public class SMS : DataRow
    {
        public new static int __LOAD__(int dp) => DataRow.__LOAD__(DPTitle);

        public override string TableName => nameof(SMSs);

        public SMS()
        {
        }
        
        public SMS(Context c, JValue v) : base(c, v) { }
        public static int DPFrom = Register<SMS, Client>("From",PropertyAttribute.AsId, OnUpload: (d, b) => ((Database)d).GetClient(b));
        public Client From { get => get<Client>(DPFrom); set => set(DPFrom, value); }


        public static int DPTo = Register<SMS, Client>("To", PropertyAttribute.AsId, OnUpload: (d, b) => ((Database)d).GetClient(b));
        public Client To { get => get<Client>(DPTo); set => set(DPTo, value); }


        public static int DPIsReaded = Register<SMS, bool>("IsReaded");
        public bool IsReaded { get => get<bool>(DPIsReaded); set => set(DPIsReaded, value); }


        public static int DPTitle = Register<SMS, string>("Title");
        public string Title { get => get<string>(DPTitle); set => set(DPTitle, value); }

        public static int DPMessage = Register<SMS, string>("Message");
        public string Message { get => get<string>(DPMessage); set => set(DPMessage, value); }
        
        public static int DPDate = Register<SMS, DateTime>("Date");
        public DateTime Date { get => get<DateTime>(DPDate); set => set(DPDate, value); }

    }


}
namespace Serializers
{
    class SMSSerializer : DataRowTypeSerializer
    {
        public SMSSerializer() : base("models.SMS")
        {
        }

        public override DataTable Table => data.SMSs;

        public override bool CanBecreated => true;

        protected override JValue CreateItem(Context c, JValue jv)
        {
            return jv == null ? new SMS() : new SMS(c, jv);
        }
    }
    class SMSsSerializer : Typeserializer
    {

        public SMSsSerializer()
            : base("models.SMSs")
        {
        }
        public override JValue ToJson(Context c, object ov) => null;
        public override JValue Swap(Context c, JValue jv, bool requireNew) => new models.SMSs(c, jv);
        public override Object FromJson(Context c, JValue jv) => new models.SMSs(c, jv);

        public override void Stringify(Context c, object p) => throw new NotImplementedException();

        public override void SimulateStringify(Context c, object p) => throw new NotImplementedException();
    }
}

namespace Api
{

    class SMSService : Service
    {
        public SMSService() : base("sms")
        {
        }

        public override bool Create(RequestArgs args)
        {
            var sms = new models.SMS()
            {
                From = args.Client,
                Date = DateTime.UtcNow,
                Id = DataRow.NewGuid()
            };
            args.Send(sms);
            return true;
        }
        public override bool Delete(RequestArgs args)
        {
            var sms = args.Database.SMSs[args.Id];
            if (sms == null) return true;
            if (sms.From?.Id != args.Client?.Id) return args.SendFail();
            if (args.Database.Delete(sms))
            {
                args.Database.SMSs.Remove(sms);
                sms.To?.DeleteUnReadedSMS(sms);
                return true;
            }
            return args.SendError(QServer.Core.CodeError.DatabaseError);
        }
        public override bool Get(RequestArgs args)
        {
            if (args == null) return false;
            var sms = args.Database.SMSs[args.Id];
            if (sms == null) return args.SendFail();
            args.Send(sms);
            return true;
        }
        public override bool Post(RequestArgs args)
        {
            args.JContext.RequireNew = RequireNew;
            if (args.HasParam("MakeReaded")) return MakeReaded(args);
            if (!(args.BodyAsJson is SMS sms)) return args.SendFail();

            var osms = args.Database.SMSs[sms.Id];
            if (osms != null) return args.SendError("Les Message ne peut pas changer", false);
            models.Client from = sms.From;
            models.Client to = sms.To;
            sms.IsReaded = false;
            if (args.User.IsAdmin && from == null)
                from = args.Client;
            else from = args.Client;
            
            if (to == null) return args.SendError("Le destinataire doit etre definir");
            if (string.IsNullOrWhiteSpace(sms.Title) && string.IsNullOrWhiteSpace(sms.Message)) return args.SendError("Le Titre de message ou le message doit etre saisir",false);
            if (to == from) return args.SendError("Tu ne peut pas envoyer le message a toi meme");
            
            if (args.Database.Save(sms, false))
            {
                args.Database.SMSs.Add(sms);
                to.GetSMSs(args.Database).Add(sms);
                return true;
            }
            else return args.SendError(QServer.Core.CodeError.DatabaseError);
        }

        private bool MakeReaded(RequestArgs args)
        {
            var sms = args.Database.SMSs[args.Id];
            if (sms.To != args.Client) return args.SendFail();
            if (!sms.IsReaded) {
                sms.IsReaded = true;
                if (args.Database.Save(sms, true))
                {
                    sms.To.MakeSMSReaded(sms);
                    return true;
                }
                return args.SendError(QServer.Core.CodeError.DatabaseError);
            }
            return true;
        }

        protected override bool Set(RequestArgs args)
        {
            var osms = args.Database.SMSs[args.Id];
            if (osms == null) return args.SendError("The SMS IsNot Exist");
            var d = args.GetParam("Value");
            if (args.BodyAsJson is SMS sms)
                return args.SendFail();
            if (!(args.BodyAsJson is JObject obj))
                return args.SendFail();
            sms = new SMS(args.JContext, obj);
            if (!args.User.IsAgent)
            {
                obj.Remove(nameof(sms.From));
                obj.Remove(nameof(sms.To));
            }
            foreach (var kv in obj)
                osms[kv.Key] = sms[kv.Key];
            return args.Database.Save(osms, true);
        }

    }
    class SMSsService : Service
    {
        public SMSsService() : base("smss")
        {
        }
        public override bool Get(RequestArgs args)
        {
            var sms = args.Database.SMSs.AsList();
            var mid = args.Client.Id;
            if (args.HasParam("NonReaded"))
                args.Send(new CSV<SMS>().Stringify(args.JContext, args.Database.SMSs, (s) => !s.IsReaded && s.To?.Id == mid).ToString());
            else if (args.HasParam("Readed"))
                args.Send(new CSV<SMS>().Stringify(args.JContext, args.Database.SMSs, (s) => s.IsReaded && s.To?.Id == mid).ToString());
            else if (args.HasParam("Sended"))
                args.Send(new CSV<SMS>().Stringify(args.JContext, args.Database.SMSs, (s) => s.From?.Id == mid).ToString());
            else if (args.HasParam("SendedNonReaded"))
                args.Send(new CSV<SMS>().Stringify(args.JContext, args.Database.SMSs, (s) => !s.IsReaded && s.From?.Id == mid).ToString());
            else if (args.HasParam("all"))
            {
                if (args.User.IsAdmin)
                    args.Send(new CSV<SMS>().Stringify(args.JContext, args.Database.SMSs, null).ToString());
                else
                    args.Send(new CSV<SMS>().Stringify(args.JContext, args.Database.SMSs, (s) => s.From?.Id == mid || s.To?.Id == mid).ToString());
            }
            else
            {
                args.Send(new CSV<SMS>().Stringify(args.JContext, args.Database.SMSs, (s) => !s.IsReaded && s.To?.Id == mid).ToString());

            }
            return true;
        }
    }
}