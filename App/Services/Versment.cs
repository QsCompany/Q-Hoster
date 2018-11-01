using Server;
using System.Collections.Generic;
using System.Linq;
using models;
using System;

namespace Api
{
    public class Versment : Service
    {
        public Versment()
            : base("Versment")
        {

        }
        public override bool Create(RequestArgs args)
        {
            args.Send(new models.Versment()
            {
                Id = DataRow.NewGuid(),
                Cassier = args.User.Agent,
                Date = DateTime.Now,
                Type = VersmentType.Espece,
                Ref = $"V{(AppSetting.Default.VersmentCounter++).ToString("D5")}"
            });
            return true;
        }
        public override bool CheckAccess(RequestArgs args)
        {
            if (args.User.IsAgent) return true;
            return args.Method == "GET";
        }

        public override bool Get(RequestArgs args)
        {
            var f = args.Database.Versments[args.Id];
            if (f != null)
                if (args.User.Agent != null || f.Client == args.Client)
                    args.Send(f);
                else
                    return args.SendError("This Versment not belong to you .", false);
            else
                return args.SendError("This Versment isn't exist .", false);
            return true;
        }

        public override bool Post(RequestArgs args)
        {
            var v = args.BodyAsJson as models.Versment;
            if (v == null) return args.SendFail();
            return v.Check(args) && models.Versment.Save(args);
        }

        public override bool Delete(RequestArgs args)
        {
            var c = args.Database.Versments[args.Id];
            if (c == null) return args.SendFail();
            return c.Delete(args);
        }
    }
    public class SVersment : Service
    {
        public SVersment()
            : base("SVersment")
        {
        }
        public override bool Get(RequestArgs args)
        {
            var f = args.Database.SVersments[args.Id];
            if (f != null)
                args.Send(f);
            else
                return args.SendError("This Versment isn't exist .", false);
            return true;
        }
        public override bool Create(RequestArgs args)
        {
            args.Send(new models.SVersment()
            {
                Id = DataRow.NewGuid(),
                Cassier = args.User.Agent,
                Date = DateTime.Now,
                Type = VersmentType.Espece,
                Ref = $"S{(AppSetting.Default.VersmentCounter++).ToString("0000")}"
            });
            return true;
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }

        public override bool Post(RequestArgs args)
        {
            var v = args.BodyAsJson as models.SVersment;
            if (v == null) return args.SendFail();
            return v.Check(args) && models.SVersment.Save(args);
        }

        public override bool Delete(RequestArgs args)
        {
            var v = args.Database.SVersments[args.Id];
            if (v == null) return args.SendFail();
            return v.Delete(args);

        }
    }
}