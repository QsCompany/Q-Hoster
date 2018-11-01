using Json;
using Server;
using System;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Mail), typeof(Serializers.MailSerializer))]
    public class Mail : DataRow
    {
        public new static int __LOAD__(int dp) => DataRow.__LOAD__(DPDate);


        public static int DPFrom = Register<Mail, Client>("From", PropertyAttribute.AsId | PropertyAttribute.NonModifiableByHost, null, (d, p) => ((Database)d).GetClient(p));
        public Client From { get => get<Client>(DPFrom); set => set(DPFrom, value); }

        public static int DPTo = Register<Mail, Client>("To", PropertyAttribute.AsId, null, (d, p) => ((Database)d).GetClient(p));
        public Client To { get => get<Client>(DPTo); set => set(DPTo, value); }

        public static int DPSubject = Register<Mail, string>("Subject", PropertyAttribute.None, null, null, "nvarchar(45)");
        public string Subject { get => get<string>(DPSubject); set => set(DPSubject, value); }

        public static int DPBody = Register<Mail, string>("Body", PropertyAttribute.None, null, null, "nvarchar(255)");
        public string Body { get => get<string>(DPBody); set => set(DPBody, value); }
        
        public static int DPReaded = Register<Mail, bool>("Readed", PropertyAttribute.NonModifiableByHost);
        public bool Readed { get => get<bool>(DPReaded); set => set(DPReaded, value); }

        public static int DPDate = Register<Mail, DateTime>("Date", PropertyAttribute.NonModifiableByHost);
        public DateTime Date { get => get<DateTime>(DPDate); set => set(DPDate, value); }

        public Mail(Context c, JValue jv) : base(c, jv)
        {
        }

        public Mail()
        {
        }
    }
    [QServer.Core.HosteableObject(typeof(Api.Mails), typeof(Serializers.MailsSerializer))]
    public class Mails : DataTable<Mail>
    {
        public new static int __LOAD__(int dp) => DataTable<Mail>.__LOAD__(12);
        public Mails(Client owner) : base(owner)
        {
        }
        public Mails(Context c, JValue jv) : base(c, jv)
        {
        }

        protected override void GetOwner(DataBaseStructure d, Path c) => ((Database)d).GetClient(c);
    }
}
