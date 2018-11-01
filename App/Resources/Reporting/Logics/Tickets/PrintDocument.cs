using Json;
using Server;
using System.Collections.Generic;
using System.Drawing.Printing;

namespace Printing
{
    public delegate bool ProcessPrintDocument(RequestArgs args, PrintDocument document);
    public class PrintDocument : DObject
    {
        public static new int __LOAD__(int dp) => DObject.__LOAD__(DPPageSettings);

        public static int DPHandler = Register<PrintDocument, string>("Handler");
        public string Handler { get => get<string>(DPHandler); set => set(DPHandler, value); }

        public static int DPOwnerId = Register<PrintDocument, long>("OwnerId", PropertyAttribute.NonSerializable);
        public long OwnerId { get => get<long>(DPOwnerId); set => set(DPOwnerId, value); }

        public static int DPOwner = Register<PrintDocument, string>("Owner", PropertyAttribute.NonSerializable);
        public string Owner { get => get<string>(DPOwner); set => set(DPOwner, value); }

        public static int DPParams = Register<PrintDocument, JArray>("Params", PropertyAttribute.NonSerializable);
        public JArray Params { get => get<JArray>(DPParams); set => set(DPParams, value); }

        public static int DPPrinterSettings = Register<PrintDocument, PrinterSettings>("PrinterSettings", PropertyAttribute.NonSerializable);
        public PrinterSettings PrinterSettings { get => get<PrinterSettings>(DPPrinterSettings); set => set(DPPrinterSettings, value); }

        public static int DPPageSettings = Register<PrintDocument, PageSettings>("PageSettings", PropertyAttribute.NonSerializable);
        public PageSettings PageSettings { get => get<PageSettings>(DPPageSettings); set => set(DPPageSettings, value); }



        public static int DPResponse = Register<PrintDocument, object>("Response");
        public object Response { get => get<object>(DPResponse); set => set(DPResponse, value); }

        private Dictionary<string, ProcessPrintDocument> dictionary = new Dictionary<string, ProcessPrintDocument>();
        public void Register(string documentHandler, ProcessPrintDocument document)
        {
            if (document != null)
                dictionary[documentHandler] = document;
            else if (dictionary.ContainsKey(documentHandler)) dictionary.Remove(documentHandler);
        }
        public bool Process(RequestArgs args) => dictionary.TryGetValue(Handler, out var handler) ? handler(args, this) : args.SendError("Service is Unavaible");
    }
}