using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Json;
using Server;

namespace QServer.App.Services
{
    public class StatisticDescription:models. DataRow
    {
        public static int DPName = Register<StatisticDescription, string>("Name");
        public string Name { get => get<string>(DPName); set => set(DPName, value); }

        public static int DPLabel = Register<StatisticDescription, string>("Label");
        public string Label { get => get<string>(DPLabel); set => set(DPLabel, value); }

        public static int DPParams = Register<StatisticDescription, JArray>("Params");
        public JArray Params { get => get<JArray>(DPParams); set => set(DPParams, value); }

        public readonly StatisticsHandler Handler;
        public StatisticDescription(string name, string label, StatisticsHandler handler)
        {
            Name = name;
            Label = label;
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Params = new JArray();
        }
    }
    public delegate bool StatisticsHandler(Statistics statistics, RequestArgs args);
    [Core.Service]
    public class Statistics : Server.Service
    {
        private static Dictionary<string, StatisticDescription> _handlers = new Dictionary<string, StatisticDescription>();
        public static StatisticDescription RegisterHandler(string statName, StatisticsHandler handler,string label=null)
        {
            _handlers[statName.ToLowerInvariant()] = new StatisticDescription(statName, label, handler);
            return _handlers[statName.ToLowerInvariant()];
        }
        public static StatisticDescription GetHandler(string statName)
        {
            if (_handlers.TryGetValue(statName.ToLowerInvariant(), out var h)) return h;
            return null;
        }

        public Statistics() : base(nameof(Statistics))
        {

        }
        public override bool Get(RequestArgs args)
        {
            if (args.Path.Length < 2) return this.Options(args);
            return GetHandler(args.Path[1])?.Handler.Invoke(this, args) == true;
        }

        public override bool Options(RequestArgs args)
        {
            if (args.HasParam("methods"))
            {
                var arr = new JArray();
                foreach (var item in _handlers)
                    arr.Push(item.Value);
                args.Send(arr);
            }
            else if (args.HasParam("params") && args.GetParam("method", out string v))
            {
                var x = GetHandler(v);
                args.SendSuccess();
            }
            return true;
        }
        public override void Head(RequestArgs args)
        {
            if (args.HasParam("methods"))
            {
                var arr = new JArray();
                foreach (var item in _handlers)
                    arr.Push((new JString(item.Key)));
                args.Send(arr);
            }
            else if (args.HasParam("params") && args.GetParam("method", out string v))
            {
                var x = GetHandler(v);
                args.SendSuccess();
            }
             
            
        }

        static Statistics()
        {
            RegisterHandler(nameof(NFactureAchatsBydate), NFactureAchatsBydate,"N° des acheter");
            RegisterHandler(nameof(TotalAchatsByClient), TotalAchatsByClient,"Total des achats");
            RegisterHandler(nameof(TotalVersmentByClient), TotalVersmentByClient, "Total des versements");
            RegisterHandler(nameof(ProduitAcheter), ProduitAcheter, "Etat des produits");
        }
        public static bool NFactureAchatsBydate(Statistics statistics, RequestArgs args)
        {
            if (!args.GetParam("from", out DateTime from)) return args.SendFail();
            if (!args.GetParam("to", out DateTime to)) return args.SendFail();
            JObject j = new JObject();
            Dictionary<long, int> _visitors = new Dictionary<long, int>();
            var f = args.Database.Factures.AsList();
            for (int i = f.Length - 1; i >= 0; i--)
            {
                var fc = ((models.Facture)f[i].Value);
                var x = fc.Date;

                if (x >= from && x <= to)
                {
                    var id = fc.Client == null ? "-1" : fc.Client.Id.ToString();

                    var l = ((JNumber)j[id]);
                    if (l == null)
                    {
                        l = new JNumber(1);
                        j[id] = l;
                        continue;
                    }
                    l.Value += 1;
                }
            }
            args.Send(j);
            return true;
        }
        public static bool TotalAchatsByClient(Statistics statistics, RequestArgs args)
        {
            if (!args.GetParam("from", out DateTime from)) return args.SendFail();
            if (!args.GetParam("to", out DateTime to)) return args.SendFail();

            JObject j = new JObject();
            Dictionary<long, int> _visitors = new Dictionary<long, int>();
            var f = args.Database.Factures.AsList();
            for (int i = f.Length - 1; i >= 0; i--)
            {
                var fc = ((models.Facture)f[i].Value);
                var x = fc.Date;
                if (x >= from && x <= to)
                {
                    var id = fc.Client == null ? "unknown" : fc.Client.Id.ToString();
                    var l = ((JNumber)j[id]);
                    if (l == null)
                    {
                        l = new JNumber((decimal)fc.Total);
                        j[id] = l;
                        continue;
                    }
                    l.Value += (decimal)fc.Total;
                }
            }
            args.Send(j);
            return true;
        }
        public static bool TotalVersmentByClient(Statistics statistics, RequestArgs args)
        {
            if (!args.GetParam("from", out DateTime from)) return args.SendFail();
            if (!args.GetParam("to", out DateTime to)) return args.SendFail();
            JObject j = new JObject();
            Dictionary<long, int> _visitors = new Dictionary<long, int>();
            var f = args.Database.Versments.AsList();
            for (int i = f.Length - 1; i >= 0; i--)
            {
                var fc = ((models.Versment)f[i].Value);
                var x = fc.Date;
                if (x >= from && x <= to)
                {
                    var id = fc.Client?.Id.ToString() ?? "unknown client";
                    var l = ((JNumber)j[id]);
                    if (l == null)
                    {
                        l = new JNumber((decimal)fc.Montant);
                        j[id] = l;
                        continue;
                    }
                    l.Value += (decimal)fc.Montant;
                }
            }
            args.Send(j);
            return true;
        }

        public static bool ProduitAcheter(Statistics statistics, RequestArgs args)
        {
            if (!args.GetParam("from", out DateTime from)) return args.SendFail();
            if (!args.GetParam("to", out DateTime to)) return args.SendFail();
            var hasClient = args.GetParam("cid", out long cid) && cid > 0;
            JObject j = new JObject();
            Dictionary<long, int> _visitors = new Dictionary<long, int>();
            var f = args.Database.Articles.AsList();
            for (int i = f.Length - 1; i >= 0; i--)
            {
                var art = ((models.Article)f[i].Value);
                var fc = art.Facture;
                if (fc != null)
                {
                    var x = fc?.Date ?? art.LastModified;
                    var client = fc.Client;
                    if (!(x >= from && x <= to && (!hasClient || cid == (client == null ? -1 : client.Id))))
                        continue;
                }
                else if (hasClient) continue;
                var id = art.Product?.Id.ToString() ?? "unknown";
                var l = ((ProduitStat)j[id]);
                if (l == null)
                    j[id] = l = new ProduitStat(art.Product?.Label);
                l.Add(art.Qte, art.Total, art.GetTotalBenifice());
            }
            args.Send(j);
            return true;
        }
    }
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class MyAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string positionalString;

        // This is a positional argument
        public MyAttribute(string positionalString)
        {
            this.positionalString = positionalString;

            // TODO: Implement code here

            throw new NotImplementedException();
        }

        public string PositionalString
        {
            get { return positionalString; }
        }

        // This is a named argument
        public int NamedInt { get; set; }
    }
    public class ProduitStat : JValue
    {
        public float Qte;
        public float Total;
        public float Benifice;
        public string Label;
        public ProduitStat(string label)
        {
            Label = label;
        }
        public ProduitStat(float qte, float total, float benifice)
        {
            Qte = qte;
            Total = total;
            Benifice = benifice;
        }
        public void Add(float qte, float total, float benifice)
        {
            Qte += qte;
            Total += total;
            Benifice += benifice;
        }
        public override void FromJson(Context c, JValue j)
        {

        }

        public override void SimulateStringify(Context c)
        {

        }

        public override void Stringify(Context c)
        {
            var s = c.GetBuilder();
            s.Append('[');
            if (FactureStat.strinngifyLabel)
                s.Append(JString.StringifyString(Label)).Append(',');
            s.Append(Qte).Append(',')
                .Append(Total).Append(',')
                .Append(Benifice).Append(']');
        }
    }
    public class FactureStat : JValue
    {
        public float Total;
        public float Versement;
        public string Label;
        public FactureStat(string label)
        {
            Label = label;
        }
        public FactureStat(float total, float versement)
        {
            Total = total;
            Versement = versement;
        }
        public void Add(float total, float versement)
        {
            Total += total;
            Versement += versement;
        }
        public override void FromJson(Context c, JValue j)
        {

        }

        public override void SimulateStringify(Context c)
        {

        }

        public override void Stringify(Context c)
        {
            var s = c.GetBuilder();
            s.Append('[');
            if (strinngifyLabel)
                s.Append(JString.StringifyString(Label)).Append(',');
            s.Append(Total).Append(',')
            .Append(Versement).Append(']');
        }
        public static bool strinngifyLabel = false;
    }

    
}
