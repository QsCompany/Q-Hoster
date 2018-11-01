using Json;
using models;
using Server;
using System;
using System.Collections.Generic;

namespace Api
{
    [QServer.Core.Service]
    public class CommandsSercie : Service
    {
        public CommandsSercie() : base("Commands")
        {
        }
        public override bool Get(RequestArgs args)
        {
            if (args.GetParam("from", out DateTime from) && args.GetParam("to", out DateTime to))
            {
                var x = new models.Commands(null);
                foreach (var kv in args.Database.Commands.AsList())
                {
                    var cmd = ((models.Command)kv.Value);
                    var dt = cmd.Date;
                    if (dt >= from && dt < to)
                        x.Add(cmd);
                }
                args.Send(x);
                return true;
            }
            return args.SendError("Incorrect Params");
        }
    }
}
namespace Api
{
    [QServer.Core.Service]
    public class CommandService : Service
    {
        public CommandService() : base("Command")
        {

        }
        public override bool CheckAccess(RequestArgs args) => args.User?.IsAgent == true;
        private static Dictionary<long, Command> commands = new Dictionary<long, Command>();
        public static Command Command(RequestArgs args)
        {
            if (!commands.TryGetValue(args.User.Agent.Id, out var command))
            {
                var lt = new DateTime(0);
                foreach (var kv in args.Database.Commands.AsList())
                {
                    var Icmd = kv.Value as Command;
                    if (Icmd.Date >= lt) command = Icmd;
                }

                if (command == null)
                {
                    command = command ?? new Command()
                    {
                        Id = DataRow.NewGuid(),
                        Date = DateTime.Now,
                        IsOpen = true,
                        LockedBy = args.User.Agent
                    };
                    if (!args.Database.Save(command, false)) return null;
                }

            }
            return command;
        }
        public override bool Get(RequestArgs args)
        {
            if (!args.HasParam("Id"))
            {
                var command = Command(args);
                if (command == null) return args.SendError("UnExpected Error While Fetching Articles of commnad");
                args.Send(command);
                return true;
            }
            var cmd = args.Database.Commands[args.Id];
            if (cmd == null) return args.SendError("La facture command peut etre supprimer");
            args.GetParam("operation", out string operation);
            if (operation == null)
                args.Send(cmd);
            else if (operation == "OPEN")
                return Open(args, cmd);
            else if (operation == "CLOSE")
                return Close(args, cmd);
            else if (operation == "GETARTICLES")
                args.Send(cmd.Articles);
            else
                return args.SendFail();
            return true;
        }

        public override bool Create(RequestArgs args)
        {
            DateTime date = args.GetParam("date", out date) ? date : DateTime.Now;
            var cmd = new Command()
            {
                Date = date,
                IsOpen = true,
                Id = DataRow.NewGuid(),
                LockedBy = args.User.Agent
            };
            if (args.Database.StrictSave(cmd, false))
            {
                args.Database.Commands.Add(cmd);
                args.Send(cmd); return true;
            }
            return args.SendError(QServer.Core.CodeError.DatabaseError, false);
        }

        private bool Close(RequestArgs args, Command cmd)
        {
            bool force = args.GetParam("force", out force) ? force : false;
            if (cmd.IsAccessibleBy(args.User, force, out var msg))
            {
                cmd.LockedBy = null;
                cmd.IsOpen = false;
                args.Database.StrictSave(cmd, true);
                return true;
            }
            else return args.SendError(msg, false);
        }

        private bool Open(RequestArgs args, Command cmd)
        {
            bool force = args.GetParam("force", out force) ? force : false;
            if (cmd.IsAccessibleBy(args.User, force, out var msg))
            {
                cmd.LockedBy = args.User.Agent;
                cmd.IsOpen = true;
                args.Database.Save(cmd, true);
                return true;
            }
            else return args.SendError(msg, false);
        }
        
        public override bool Print(RequestArgs args)
        {
            var command = Command(args);
            if (command == null) return args.SendError("May be the facture been deleted");
            var report = QServer.Reporting.BonVent.FactureReportGenerator.CommandF1(command);

            var rp = new QServer.Printing.ReportToBytes();
            var file = rp.ExportAsPdf(report, command.Id.ToString());
            var g = Guid.NewGuid();
            if (file != null)
                Downloader.Set(g, file, args.Client.Id);
            args.Send(new JObject() { ["Response"] = new JObject() { ["FileName"] = new JString(g.ToString()), ["Success"] = new JBool(file != null) } });
            return true;
        }

        //public static Printing.Report CommandF1(this Command command)
        //{
        //    var articles = command.Articles.ToDataset(new cc<FakePrice>("ProductName", (ri) => ri.ProductName, typeof(string)));
        //    Printing.Report report = new Printing.Report($@".\Reporting\{RVVersion}\Command\CommandF1.rdlc"){
        //        { "Articles", articles }
        //    };
        //    Printing.ReportToBytes r = new ReportToBytes();
        //    r.ExportAsPdf(report, "Test");
        //    return report;
        //}

    }
}
namespace Api
{
    [QServer.Core.Service]
    public class CArticleService : Service
    {
        public CArticleService() : base("CArticle")
        {
        }

        public override bool Post(RequestArgs args)
        {
            args.JContext.RequireNew = (a, b) => true;
            if (!(args.BodyAsJson is CArticle args_art))
                return args.SendFail();
            var db = args.Database;
            Command command = args_art.Command;
            if (command == null) return args.SendError("Cette Facture est supprimer");
            if (!command.IsOpen) return args.SendError("La facture doit ouvrir avant n'import quel modification");
            //if (args_art.Product == null) return args.SendError("Le Produit est nest pas definir");
            //if (command.IsAccessibleBy(args.User, false, out var msg)) return args.SendError(msg, false);

            var cart = command.Articles[args_art.Id];
            var isnull = cart == null;
            if (isnull)
            {
                cart = db.CArticles[args_art.Id];
                if (cart == null)
                    cart = new CArticle() { Id = DataRow.NewGuid() };
            }
            else
                if (cart.Command != command) return args.SendError(QServer.Core.CodeError.propably_hacking);

            var stat = cart.SaveStat();
            cart.CopyFrom(args_art);

            //if ((cart.Product == null)) return args.SendAlert("Data Error", "L'article doit etre saisir", "OK", false);
            if (db.Save(cart, !isnull))
            {
                if (isnull)
                {
                    command.Articles.Add(cart);
                    db.CArticles.Add(cart);
                }
                args.Send(cart);
            }
            else
            {
                cart.Restore(stat);
                return args.SendError(QServer.Core.CodeError.DatabaseError);
            }
            return true;
        }

        public override bool Get(RequestArgs args)
        {
            var art = args.Database.CArticles[args.Id];
            if (art == null) return args.SendFail();
            args.Send(art); return true;
        }

        public override bool Delete(RequestArgs args)
        {
            var art = args.Database.CArticles[args.Id];
            if (art == null) return args.SendSuccess();
            if (args.Database.Delete(art))
            {
                args.Database.CArticles.Remove(art);
                art?.Command.Articles.Remove(art);
                return args.SendSuccess();
            }
            return args.SendFail();
        }
    }


    [QServer.Core.Service]
    public class CArticlesService : Service
    {

        public CArticlesService() : base("CArticles") { }

        public override bool Get(RequestArgs args)
        {
            if (!args.HasParam("Id"))
            {
                var command = CommandService.Command(args);
                if (command == null) return args.SendError("Un Expected Error ");
                args.JContext.Add(typeof(models.CArticles), new DataTableParameter(typeof(models.CArticles)){ SerializeItemsAsId = false });
                args.Send(command.Articles);
                return true;
            }
            var art = args.Database.Commands[args.Id];
            if (art == null) return args.SendFail();
            
            args.Send(art); return true;
        }
    }
}

