using System;
using QServer.Core;
using Server;
using models;
using Json;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Api
{
    public abstract class TableService : Service
    {
        public TableService(string name) : base(name)
        {
        }

        public override bool SUpdate(RequestArgs args)
        {
            return (args.Database[Name] as DataTable)?.SendUpdates(args) ?? false;
        }
    }
    public delegate PropertyAttribute AttributeOf(DProperty dp);
    public class CSV<T> where T : DataRow, new()
    {

        private DProperty[] IdProps;
        private DProperty[] FlProps;
        private Context context;
        private StringBuilder s;
        public CSV(AttributeOf selector=null)
        {
            DProperty[] Props;
            Props = DObject.GetProperties<T>();
            var t = new List<DProperty>();
            var x = new List<DProperty>();
            foreach (var p in Props)
            {
                var attr = selector == null ? p.Attribute : selector(p);
                if ((attr & PropertyAttribute.NonSerializable) == PropertyAttribute.NonSerializable)
                    continue;
                if ((attr & PropertyAttribute.SerializeAsId) == PropertyAttribute.SerializeAsId)
                    x.Add(p);
                else
                    t.Add(p);
            }
            FlProps = t.ToArray();
            IdProps = x.ToArray();
            s = new StringBuilder();
        }
        private void BuildHeader(StringBuilder s)
        {
            s.Clear();
            for (int i = 0; i < FlProps.Length; i++)
            {
                if (i != 0) s.Append(';');
                s.Append(FlProps[i].Name);
            }
            var x = FlProps.Length > 0;
            for (int i = 0; i < IdProps.Length; i++)
            {
                if (x) s.Append(';');
                else x = true;
                s.Append(IdProps[i].Name);
            }
            s.AppendLine();
        }
        public StringBuilder Stringify(Context context, DataTable<T> table, Func<T,bool> selctor = null)
        {
            this.s = context.GetBuilder();
            this.context = context;
            var t = table.AsList();
            s.Length = t.Length * 100;
            this.BuildHeader(s);
            for (int i = 0; i < t.Length; i++)
            {
                var v = (T)t[i].Value;
                if (selctor?.Invoke(v) == false)
                    continue;
                BuildRow(v);
            }
            return s;
        }
        public StringBuilder Stringify<P>(Context context,IEnumerable<P> source, Func<P,T>  getter , Func<T, bool> selctor = null)
        {
            s = context.GetBuilder();
            s.Clear();
            this.context = context;
            this.BuildHeader(s);
            foreach (var item in source)
            {
                var v = getter(item);
                if (v == null) continue;
                if (selctor?.Invoke(v) == false)
                    continue;
                BuildRow(v);
            }
            return s;
        }

        private void BuildRow( T row)
        {
            var x = false;
            var s = context.GetBuilder();
            for (int i = 0; i < FlProps.Length; i++)
            {
                var p = FlProps[i];
                if (x) s.Append(';');
                else x = true;
                var v = row.Get(p.Index);
                context.Stringify(v);
            }
            for (int i = 0; i < IdProps.Length; i++)
            {
                var p = IdProps[i];
                if (x) s.Append(';');
                else x = true;
                if (!(row.Get(p.Index) is DataRow v)) context.GetBuilder().Append("null");
                else context.GetBuilder().Append(v.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            s.AppendLine();
        }
    }

    public class  Products :TableService
    {
        private static ISerializeParametre GetParams1 = new DObjectParameter(typeof(models.Product), true) { DIsFrozen = false };
        private static PriceParameter GetParams2 = new PriceParameter() /*{ Job = args.User.Client.Abonment, IsAdmin = args.User.IsAgent, FullyStringify = args.User.IsAgent }*/;
        private static DataTableParameter GetParams3 = new DataTableParameter(typeof(models.Products)) { SerializeItemsAsId = false };


        public Products ():base("Products")
        {
        }
        public override bool Get(RequestArgs args)
        {
            if (args.HasParam("csv")) return this.GetCSV(args);
            GetParams2.Job = args.User.Client.Abonment;
            GetParams2.IsAdmin = args.User.IsAgent;
            GetParams2.FullyStringify = args.User.IsAgent;

            args.JContext.Add(typeof(models.Product), GetParams1);
            args.JContext.Add(typeof(models.FakePrice), GetParams2);
            args.JContext.Add(typeof(models.Products), GetParams3);
            args.Client.SetCookie("last_product_update", DateTime.Now.ToString(), DateTime.Now + TimeSpan.FromDays(2));
            args.Client.SetCookie(Name.ToLower() + "_lasttimeupdated", System.DateTime.Now, args.Server.ExpiredTime);
            args.Send(args.Database.Products);
            return true;
        }

        private bool GetCSV(RequestArgs args)
        {
            var exlude = new[] { "Frais", "TVA", "MD", "MDG", "MG", "MS", "MPS", "Sockable", "Nature", "Unity", "QteMax", "QteMin", "Revage" };
            var csv = new CSV<models.Product>(p => {
                return exlude.Contains(p.Name) ? PropertyAttribute.NonSerializable : p.Attribute;
            });
            var cc = csv.Stringify(args.JContext, args.Database.Products).ToString();
            args.context.Response.AddHeader("format", "csv");
            args.GZipSend(cc);
            return true;
        }

        public override bool SUpdate(RequestArgs args)
        {
            args.Database.Products.SendUpdates(args);
            return true;
        }
        public override bool Print(RequestArgs args)
        {
            var str = ((args.BodyAsJson as JString)?.Value)?.Split('|');
            if (str == null) return args.SendError("Empty Body");

            var arr = new models.Products(null);
            foreach (var s in str)
            {
                if(long.TryParse(s,out var id))
                {
                    var p = args.Database.Products[long.Parse(s)];
                    if (p != null) arr.Add(p);
                }
                
            }
            var report = QServer.Reporting.BonVent.FactureReportGenerator.ProductsF1(args,arr);
            if (report == null) return false;
            var rp = new QServer.Printing.ReportToBytes();
            var file = rp.ExportAsPdf(report, arr.Name.ToString());
            var g = Guid.NewGuid();
            if (file != null)
                Downloader.Set(g, file, args.Client.Id);
            args.Send(new JObject() { ["Response"] = new JObject() { ["FileName"] = new JString(g.ToString()), ["Success"] = new JBool(file != null) } });
            return true;
            
        }

    }
    [HosteableObject(typeof(UpdateProducts))]
    public class  UpdateProducts : Service
    {
        public UpdateProducts()
            : base("UpdateProducts")
        {
        }

        public override bool Get(RequestArgs args)
        {
            var from = args.context.Request.QueryString.Get("From") ?? args.Client.GetCookie("last_product_update", true) as string;
            if (from == null) return args.SendError(CodeError.propably_hacking);
            else
            {
                args.Client.SetCookie("last_product_update", DateTime.Now.ToString(), DateTime.Now + TimeSpan.FromDays(2));
                args.Database.Products.GetUpdates(args.JContext, DateTime.Parse(from));
                args.Send(args.JContext.GetBuilder().ToString());
            }
            return true;
        }
    }
}