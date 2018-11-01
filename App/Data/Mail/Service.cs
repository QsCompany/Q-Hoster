using models;
using Server;
using System;

namespace Api
{
    public class Mail : Service
    {
        public Mail() : base("Mail")
        {
        }
        public override bool Get(RequestArgs args)
        {
            var cmd = new OperationBuilder("SELECT * FROM `MAILS` WHERE `Id`= " + args.Id);
            if (!args.User.IsAgent)
                cmd.Append(" AND (`From` = {0} OR `To` = {0})", args.Client.Id);
            var _prms = new object[1];
            args.Database.Execute(cmd, (db, rdr, prms) =>
            {
                prms[0] = ((Database)db).Mails.ReadScalar(db.Updator, rdr, true);
            }, _prms);

            var obj = DObject.ToTSObject(typeof(models.Mail));


            if (_prms[0] == null) return args.SendFail();
            args.Send(_prms[0] as models.Mail);
            return true;
        }
        public override bool Post(RequestArgs args)
        {
            var t = args.BodyAsJson as models.Mail;
            if (t == null) args.SendFail();
            t.From = args.Client;
            if (t.To == null) return args.SendError("Sender must me setted");
            t.Date = DateTime.Now;
            if (args.Database.Save(t, false))
                return args.SendSuccess();
            else return args.SendFail();
        }
    }

    public class Mails : Service
    {
        public Mails() : base("Mails")
        {
        }
        private BasicConverter dateTimeConverter = DProperty.GetDbConverter(typeof(DateTime)) as BasicConverter;
        public override bool Get(RequestArgs args)
        {
               var x = args.GetParam("type", out string type);
            models.Mails mails = null;
            if (type == "sended")
                mails = GetSendedMails(args);
            else if (type == "received")
                mails = GetReceivedMails(args);
            args.Send(mails);
            return true;
        }
        public models.Mails GetSendedMails(RequestArgs args)
        {
            args.GetParam("from", out DateTime? from);
            args.GetParam("to", out DateTime? to);
            args.GetParam("unReader", out bool? unReaded);
            var id = args.User.IsAgent && args.Id != -1 ? args.Id : args.Client.Id;

            var cmd = new OperationBuilder("SELECT * FROM `MAILS` Where");
            cmd.Append(args.User.IsAgent && args.Id != -2, " `From`=" + id + " ");
            cmd.Append(from != null, " AND `Date` >= {0} ", from)
                .Append(to != null, " AND `Date` <= {0} ", to)
                .Append(unReaded != null, " AND `Readed` = {0} ", unReaded);
            models.Mails mails = null;

            if (args.Database.Execute(cmd,
                (db, v, prms) => mails = (models.Mails)new models.Mails(args.Database.Clients[id]).Read(args.Database.Updator, v, true)) == true)
                return mails;
            return null;
        }
        public models.Mails GetReceivedMails(RequestArgs args)
        {
            args.GetParam("from", out DateTime? from);
            args.GetParam("to", out DateTime? to);
            args.GetParam("unReader", out bool? unReaded);
            var id = args.User.IsAgent && args.Id != -1 ? args.Id : args.Client.Id;

            var cmd = new OperationBuilder("SELECT * FROM `MAILS` Where `To`=" + id + " ");
            var cmds = args.Database.CreateOperations(cmd);

            cmd.Append(from != null, " AND `Date` >= {0} ", from)
                .Append(to != null, " AND `Date` <= {0} ", to)
                .Append(unReaded != null, " AND `Readed` = {0} ", unReaded == false);

            models.Mails mails = null;

            if (args.Database.Execute(cmds,
                (db, v, prms) => mails = (models.Mails)new models.Mails(args.Database.Clients[id]).Read(args.Database.Updator, v, true)) == true)
                return mails;

            return null;
        }
    }
}
