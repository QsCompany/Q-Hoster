using System;
using models;
using QServer.Core;
using Server;
using System.Threading;
using System.Linq;

namespace Api
{
    public class  Facture : Service
    {
        public Facture()
            : base("Facture")
        {
            
        }
        public override bool Create(RequestArgs args)
        {
            object la = args.Client.GetCookie("facturCreation", false, out bool expired);
            if (la != null && !expired) return args.SendError("You must wait for : " + ((DateTime.Now - (DateTime)la).TotalSeconds + " s"));
            args.Client.SetCookie("facturCreation", DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(args.User.IsAgent ? 10 : 20 * 60));

            if (!args.GetParam("CId", out long clientID)) return args.SendFail();
            var client = args.Database.Clients[clientID];

            Abonment abonment;
            TransactionType transaction;
            BonType bonType;

            if (client == null) return args.SendError("Client Not Exist");
            {
                if (!args.GetParam("Type", out int fType)) return args.SendFail();
                bonType = (BonType)fType;

                if (!args.GetParam("TType", out fType)) transaction = TransactionType.Vente;
                else transaction = (TransactionType)fType;


                abonment = Abonment.Detaillant;
                if (args.User.IsAgent)
                {
                    if (!args.GetParam("Abonment", out fType)) abonment = client.Abonment;
                    else abonment = (Abonment)fType;
                }
                else abonment = args.Client.Abonment;

                if (int.TryParse(abonment.ToString(), out var i)) return args.SendError("Abonment Value is UnCorrect");
                if (int.TryParse(bonType.ToString(), out i)) return args.SendError("Facture Type is UnCorrect");
            }

            var c = new models.Facture
            {
                Abonment = abonment,
                IsValidated = ((int)bonType & 4096) == 4096 ? false : true,
                Client = client,
                Editeur = args.Client,
                Id = DataRow.NewGuid(),
                Date = DateTime.Now,
                
                LockedBy = args.Client,
                LockedAt = DateTime.Now.Ticks,
                Vendeur = args.User.Agent,
                LastModified = DateTime.Now,
            };
            c.SetFactureType(bonType, transaction);
            var x = bonType.ToString()[0] + "" + transaction.ToString()[0];
            c.Ref = x + $"{(AppSetting.Default.FactureCounter++).ToString("D5")}";
            if (!args.Database.Save(c, false)) return args.SendError(CodeError.fatal_error, false);
            args.JContext.Add(typeof(models.Facture), new FactureParameter(args.User, typeof(models.Facture)));
            args.Database.Factures.Add(c);
            args.Send(c);
            return true;
        }
        
        public override bool Get(RequestArgs args)
        {
            args.JContext.RequireNew = RequireNew;
            var sjson = args.BodyAsJson as DataRow;
            var id = args.Id;
            if (id == -1)  return args.CodeError(404); 
            var sf = args.Database.Factures[id];
            if (sf == null)  return args.SendError("This Facture Is not Exist", false);
            if (sf.Operation(args)) return true;
            if (!args.User.IsAgent && sf.Client != args.Client) args.SendFail();
            else
            {

                args.JContext.Add(typeof(models.Clients), new DObjectParameter(typeof(models.Clients)) { DIsFrozen = sf.IsValidated });
                args.JContext.Add(typeof(DObject), new DObjectParameter(typeof(DObject)) { DIsFrozen = sf.IsValidated });
                args.JContext.Add(typeof(models.Articles), new DataTableParameter(typeof(models.Articles)) { DIsFrozen = sf.IsValidated });
                args.JContext.Add(typeof(models.Facture), new FactureParameter(args.User, typeof(models.Facture)));
                args.Send(sf);
            }
            return true;
        }
        public void reset(RequestArgs args)
        {
            args.JContext.RequireNew = RequireNew;
            foreach (var c in args.Database.Clients)
            {
                var x = ((System.Collections.Generic.KeyValuePair<long, DataRow>)c).Value as models.Client;
                x.MontantTotal = 0;
                x.VersmentTotal = 0;
                args.Database.Save(x, true);
            }
        }
        public override bool Post(RequestArgs args)
        {
            args.JContext.RequireNew = RequireNew;
            if (!models.Facture.IsAccessibleBy(args, false, out var msg)) return args.SendError(msg);
            if (args.GetParam("Set", out string property)) return set(args, property);
            if (args.GetParam("SetInfo", out string isInfo)) return SetInfo(args);
            return args.SendError("Method Not Implimented +<br>" + CompressedStack.Capture().ToString());
        }
        private bool SetInfo(RequestArgs args)
        {
            var fid = args.Id;
            var f = args.Database.Factures[args.Id];
            if (f == null) return args.SendError("La facture n'exist pas");
            args.JContext.RequireNew = RequireNew;
            var nf = args.BodyAsJson as models.Facture;

            var x = f.SaveStat();
            f.Date = nf.Date;
            f.DateLivraison = nf.DateLivraison;
            f.Editeur = nf.Editeur;
            f.Observation = nf.Observation;
            f.Validator = nf.Validator;
            f.Pour = nf.Pour;
            nf.Dispose();
            nf = null;
            if (args.Database.StrictSave(f, true))
                return args.SendSuccess();
            nf.Dispose();
            f.Restore(x);
            return args.SendFail();
        }

        private bool set(RequestArgs args, string property)
        {
            if (!args.GetParam("Id", out long fid)) return args.SendFail();
            if (!args.GetParam("value", out long cid)) return args.SendFail();
            var f = args.Database.Factures[fid];
            if (f == null) return args.SendError("ParamNullExeption");
            switch (property)
            {
                case nameof(models.Facture.Client):
                    var c = args.Database.Clients[cid];
                    if (c == null) return args.SendError("ParamNullExeption");
                    return QServer.Complex.FactureManager.ChangeClient(args, f, c);
                case nameof(models.Facture.Observation):

                    break;
                default:
                    break;
            }
            return args.SendFail();
        }
        
        public override bool Delete(RequestArgs args)
        {
            var id = args.Id;
            if (id == -1) return args.SendError("The facture Is not exist");
            var orgFacture = args.Database.Factures[id];
            if (orgFacture == null) return args.SendError("The facture Is not exist");
            return QServer.Complex.FactureManager.DeleteFacture(args, orgFacture);
        }

        public override void Else(RequestArgs args)
        {
            if (args.Method == "PRINT")
            {
                //Print(args);
            }
            else if (args.Method == "CLOSE")
                Close(args);
            else if (args.Method == "OPEN")
                Open(args);
            else
                base.Else(args);
        }
        protected override bool Open(RequestArgs args)
        {
            if (!models.Facture.IsAccessibleBy(args, true, out var msg)) return args.SendError(msg);
            var curFacture = args.Database.Factures[args.Id];


            curFacture.LockedBy = args.Client;
            curFacture.LockedAt = DateTime.Now.Ticks;

            return args.SendStatus(args.Database.Save(curFacture, true));
        }
        
        protected override bool Close(RequestArgs args)
        {
            if (!models.Facture.IsAccessibleBy(args,false, out var msg)) return args.SendError(msg);
            var curFacture = args.Database.Factures[args.Id];
            if (curFacture.LockedBy != null)
            {
                curFacture.LockedBy = null;
            }
            args.Database.Save(curFacture, true);
            args.SendSuccess();
            return true;
        }
        public override bool CheckAccess(RequestArgs args)
        {
            args.JContext.RequireNew = RequireNew;
            return base.CheckAccess(args);
        }
        //private void Print(RequestArgs args)
        //{
        //    if (!args.User.IsAgent) { args.SendAlert("Printer", "Vous ne pouvez pas d'imprimer les facture <br> Contacter L'administrateur ", "OK", false); return; }
        //    var id = args.Id;
        //    var fact = args.Database.Factures[args.Id];
        //    if (fact == null) { args.SendAlert("Error", "Facture N'esist pas", "OK", false); return; }
        //    Thread t = new Thread(new ParameterizedThreadStart(Print));
        //    t.Start(new PrintInfo(args.Database, fact));
        //}
        //[STAThread]
        //public static void Print(object arg)
        //{
        //    //var args = (PrintInfo)arg;
        //    //Server.Reporting.Bon1.Print(args.Database, args.Facture);
        //}

        public bool ChangeClient(RequestArgs args)
        {
            if (!args.User.IsAgent) return args.SendFail();
            var d = args.Database;
            var facture = d.Factures[args.Id];
            var oldClient = facture.Client;
            if (!args.GetParam("ToClient", out long id)) goto cne;
            var newClient = d.Clients[id];
            if (newClient == null) goto cne;
            var c = args.Client.GetCookie("CFC" + facture.Id, true, out var expired) as Message;
            if (expired || c == null) return args.SendError("Time expired ?? ");

            var m = args.SendConfirm("Confirmation", "Do you want to transfer versment of this facture to ", "Transfer", "Cancel", true);



            return true;
            cne:
            return args.SendError("Le Client n'exist pas, Possible est supprimé");

        }
    }
    public struct PrintInfo
    {
        public PrintInfo(Database database,models. Facture facture)
        {
            Database = database;
            this.Facture = facture;
        }

        public Database Database { get; }
        public models. Facture Facture { get; }
    }

    [HosteableObject(typeof(UpdateFacture))]
    public class  UpdateFacture : Service
    {
        public UpdateFacture()
            : base("UpdateFacture")
        {
        }
        public override bool Get(RequestArgs args)
        {
            var id = args.Id;
            if (id == -1) goto fail;

            var f = args.Database.Factures[id];
            if (f == null) goto fail;
            if (f.IsValidated) goto ph;
            if (!args.User.IsAgent && args.Client != f.Client) goto ph;
            args.Send(f.GetUpdates());

            return true;
            fail: args.SendFail();
            return false;
            ph:
            if (args.User.IsAgent || args.Client == f.Client)
            {
                args.Send(f);
                return true;
            }
            return false;

        }
    }

}

namespace Api
{
    public class Factures : Service
    {
        public Factures() : base("Factures")
        {
        }
        private bool isSameDay(DateTime d, DateTime x)
        {
            return d.DayOfYear == x.DayOfYear && d.Year == x.Year;
        }
        private bool isSameWeek(DateTime d, DateTime x)
        {
            return d.Year == x.Year && Math.Abs(x.DayOfYear - d.DayOfYear) <= 7;
        }
        private bool isSameMonth(DateTime d, DateTime x)
        {
            return d.Month == x.Month && d.Year == x.Year;
        }
        private bool isSameYear(DateTime d, DateTime x)
        {
            return d.Year == x.Year;
        }

        public override bool Get(RequestArgs args)
        {
            if (args.GetParam("csv", out bool csv) && csv) return GetCSV(args);
            var e = args.Database.Factures;
            args.GetParam("Freezed", out bool frozen);
            if (!args.User.IsAgent)
            {
                var c = args.Client;
                var le = new models.Factures(c);
                foreach (var f in e.AsList())
                {
                    var t = (models.Facture)f.Value;
                    if (t.Freezed != frozen) continue;
                    if (t.Client == c) le.Add(t);
                }
                e = le;
            }

            args.JContext.Add(typeof(models.Factures), new DataTableParameter(typeof(models.Factures)) { SerializeItemsAsId = false, DIsFrozen = !true });
            args.JContext.Add(typeof(models.Articles), new ArticlesParameter(typeof(models.Articles)) { SerializeItemsAsId = true, DIsFrozen = true, FullyStringify = true });
            args.JContext.Add(typeof(models.Facture), new FactureParameter(args.User, typeof(models.Facture), false, false, true));
            args.Client.SetCookie(Name.ToLower() + "_lasttimeupdated", System.DateTime.Now, args.Server.ExpiredTime);
            args.JContext.GetBuilder().Clear();
            e.Stringify(args.JContext, frozen ? _1frozen : (Func<DataRow, bool>)_1nonfrozen);
            
            return true;
        }
        static bool _frozen(models.Facture p) => p.Freezed;
        static bool _nonfrozen(models.Facture p) => !p.Freezed;

        static bool _1frozen(models.DataRow p) => ((models.Facture)p).Freezed;
        static bool _1nonfrozen(models.DataRow p) => !((models.Facture)p).Freezed;

        private bool GetCSV(RequestArgs args)
        {
            var exlude = new[] { nameof(models.Facture.LockedBy), nameof(models.Facture.LockedAt), nameof(models.Facture.Articles) };
            var csv = new CSV<models.Facture>(p => {
                return exlude.Contains(p.Name) ? PropertyAttribute.NonSerializable : p.Attribute;
            });
            args.GetParam("Freezed", out bool frozen);
            var cc = csv.Stringify(args.JContext, args.Database.Factures, frozen ? (Func<models.Facture, bool>)_frozen : _nonfrozen).ToString();
            args.context.Response.AddHeader("format", "csv");
            args.GZipSend(cc);
            return true;
        }

        public override bool SUpdate(RequestArgs args)
        {
            if (!args.User.IsAgent) return args.SendFail();
            args.JContext.Add(typeof(models.Factures), new DataTableParameter(typeof(models.Factures)) { SerializeItemsAsId = false, DIsFrozen = !true });
            args.JContext.Add(typeof(models.Articles), new ArticlesParameter(typeof(models.Articles)) { SerializeItemsAsId = true, DIsFrozen = true, FullyStringify = true });
            args.JContext.Add(typeof(models.Facture), new FactureParameter(args.User, typeof(models.Facture)));
            return args.Database.Factures.SendUpdates(args);
        }

        protected override bool Set(RequestArgs args)
        {
            if (args.GetParam("Freezed", out bool freezed))
                return SetFreezed(args, freezed);
            return args.SendStatus(false);
        }
        private static bool SetFreezed(RequestArgs args, bool freezed)
        {
            var factures = args.BodyAsJson as Json.JArray;
            var fc = args.Database.Factures;
            var ops = args.Database.CreateOperations();
            foreach (var item in factures)
            {
                if (!(item is Json.JNumber n)) continue;
                var f = fc[(long)n.Value];
                if (f == null || f.Freezed == freezed) continue;
                f.Freezed = freezed;
                ops.Add(SqlOperation.Update, f);
            }
            return args.SendStatus(args.Database.Save(ops) == true);
        }
    }
}