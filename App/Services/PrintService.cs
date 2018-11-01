using System;
using Json;
using Server;
using System.Collections.Generic;
using QServer.Core;

namespace QServer.Services
{
    

    [Core. HosteableObject(typeof(PrintDataService),typeof(PrintDataSerializer))]
    public class PrintData : DObject
    {
        public static int DPHandlerId = Register<PrintData, string>("HandlerId");
        public string HandlerId { get => get<string>(DPHandlerId); set => set(DPHandlerId, value); }

        public static int DPModel = Register<PrintData, string>("Model", PropertyAttribute.NonSerializable);
        public string Model { get => get<string>(DPModel); set => set(DPModel, value); }


        public static int DPData = Register<PrintData, JValue>("Data", PropertyAttribute.NonSerializable);
        public JValue Data { get => get<JValue>(DPData); set => set(DPData, value); }


        public static int DPDataId = Register<PrintData, long>("DataId", PropertyAttribute.NonSerializable);
        public long DataId { get => get<long>(DPDataId); set => set(DPDataId, value); }


        public static int DPResponse = Register<PrintData, JValue>("Response", PropertyAttribute.NonModifiableByHost);
        public JValue Response { get => get<JValue>(DPResponse); set => set(DPResponse, value); }
        public PrintData(Context c, JValue jv):base(c,jv)
        {

        }
        public PrintData()
        {

        }
        //static void Main()
        //{
        //    var tt = DObject.ToTSObject(typeof(PrintData));
        //}
    }
    public class PrintDataSerializer : Typeserializer
    {
        public PrintDataSerializer() : base("Printing.PrintData")
        {
        }
        public override object FromJson(Context c, JValue jv) => jv is PrintData ? jv : new PrintData(c, jv);

        public override void SimulateStringify(Context c, object p) => c.SimulateStringify(p);

        public override void Stringify(Context c, object p) => c.Stringify(p as JValue);

        public override JValue Swap(Context c, JValue jv, bool requireNew) => !requireNew && jv is PrintData ? jv : new PrintData(c, jv);
    }

    public class PrintDataService : Service
    {
        public PrintDataService() : base("Print")
        {
            Register(new FacturePrinter());
            Register(new SFacturePrinter());

        }
        public override bool CanbeDelayed(RequestArgs args) => true;


        public override void Exec(RequestArgs args)
        {
            var m = args.Method.ToLower();
            
            if (m == "print")
            {
                if (!(args.BodyAsJson is PrintData data)) goto fail;
                if (!handlers.TryGetValue(data.HandlerId, out var handlerId)) goto fail;
                try
                {
                    handlerId.Print(args);
                }
                catch(Exception e) {
                    MyConsole.WriteLine("Un expected error when print a pdf \r\n\t" + e.InnerException?.Message + "\r\n\t" + e.Message);
                    args.SendAlert(e.Source, e.Message, "OK", false);
                }
            }
            else if (m == "get")
            {
                if (!handlers.TryGetValue(args.GetParam("HandlerId") ?? "", out var handlerId)) goto fail;
                handlerId.GetModels(args);
            }
            else goto fail;
            return;
        fail:
            args.SendFail();
        }


        private Dictionary<string, PrintHandler> handlers = new Dictionary<string, PrintHandler>();
        public void Register(PrintHandler handler)
        {
            handlers.Add(handler.Name, handler);
        }
    }

    public abstract class PrintHandler
    {
        public abstract string Name { get; }
        public abstract bool Print(RequestArgs args);
        public abstract bool GetModels(RequestArgs args);
    }
    public class FacturePrinter : PrintHandler
    {
        public override string Name => "FacturePrinter";

        public override bool GetModels(RequestArgs args)
        {
            args.Send(new JArray() { (JString)"Format A5", (JString)"Format1", (JString)"Format2", (JString)"BonCommand", (JString)"Facture", (JString)"Etat Client" });
            return true;
        }
        public override bool Print(RequestArgs args)
        {
            var dt = args.BodyAsJson as PrintData;
            var f = args.Database.Factures[dt.DataId];
            if (f == null) return args.SendError("May be the facture been deleted");
            Printing.Report report = null;
            switch(dt.Model)
            {
                case null:

                case "Format A5":
                    report = Reporting.BonVent.FactureReportGenerator.BVENT_A5(f, args.Database);
                    break;
                case "Format1":
                    report = Reporting.BonVent.FactureReportGenerator.Format1(f, args.Database);
                    break;
                case "Format2":
                    report = Reporting.BonVent.FactureReportGenerator.Format2(f);
                    break;
                case "Etat Client":
                    report = Reporting.BonVent.FactureReportGenerator.EtatClient(args.Database, f.Client);
                    break;
                case "BonCommand":
                    report = Reporting.BonVent.FactureReportGenerator.BonCommand(f);
                    break;
                case "Facture":
                    report = Reporting.BonVent.FactureReportGenerator.Facture(f);
                    break;
                default:
                    return args.SendError("Le model {dt.Model} n'exist pas");
            }
            var rp = new Printing.ReportToBytes();
            var file = rp.ExportAsPdf(report, f.Id + "[" + ++counter + "]");            
            var g = Guid.NewGuid();
            if (file != null)
                global::Api.Downloader.Set(g, file, args.Client.Id);
            args.Send(new JObject() { ["Response"] = new JObject() { ["FileName"] = new JString(g.ToString()), ["Success"] = new JBool(file != null) } });
            return true;
        }

        private  static volatile int counter = 0;
        private Microsoft.Reporting.WinForms.ReportViewer _reportViewerSales;
        private Microsoft.Reporting.WinForms.ReportViewer reportViewerSales => _reportViewerSales ?? (_reportViewerSales = new Microsoft.Reporting.WinForms.ReportViewer());
        
        public void Show(RequestArgs args)
        {

        }
    }

    public class SFacturePrinter : PrintHandler
    {
        public override string Name => "SFacturePrinter";

        public override bool GetModels(RequestArgs args)
        {
            args.Send(new JArray() { (JString)"Format1", (JString)"Tickets"});
            return true;
        }

        public override bool Print(RequestArgs args)
        {
            string file;
            switch ((args.BodyAsJson as PrintData).Model)
            {
                case "Format1":
                    var f = args.Database.SFactures[(args.BodyAsJson as PrintData).DataId];
                    var report = Reporting.BonVent.FactureReportGenerator.Format1(f);
                    var rp = new Printing.ReportToBytes();
                    file = rp.ExportAsPdf(report, f.Id + "[" + ++counter + "]");
                    break;
                case "Tickets":
                    var e = (args.BodyAsJson as PrintData).Data as models.Tickets;
                    file = e.Print(args);
                    break;
                default:
                    return args.SendError("Le model {dt.Model} n'exist pas");
            }
            Guid g = Guid.NewGuid();
            if (file != null)
                global::Api.Downloader.Set(g, file, args.Client.Id);
            args.Send(new JObject() { ["Response"] = new JObject() { ["FileName"] = new JString(g.ToString()), ["Success"] = new JBool(file != null) } });
            return true;
        }

        private static volatile int counter = 0;
    }
}
