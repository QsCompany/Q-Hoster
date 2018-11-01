using System;
using models;
using QServer.Printing;
using System.Linq;
using System.IO;
using Path = System.IO.Path;

namespace QServer.Reporting.BonVent
{
    public static class FactureReportGenerator
    {
        static string GetReportDirectory(string s, int version = 2012)
        {
            var ff = System.IO.Path.Combine($@"App\Resources\Reporting\{version}\{s}");
            MyConsole.WriteLine(ff);
            return ff;
        }

        static int ReportVersion = 0;
        public static bool IsInstalled()
        {
            Microsoft.Win32.RegistryKey registryBase = Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, string.Empty);
            if (registryBase != null)
            {
                var r = registryBase.OpenSubKey("Software\\Microsoft\\ReportViewer\\v2.0.50727");
                return r != null;
            }
            return false;
        }
        public static bool IsInstalledReportViewer()
        {
            try
            {
                Microsoft.Win32.RegistryKey registryBase = Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, string.Empty);
                if (registryBase != null)
                {
                    // check the two possible reportviewer v10 registry keys
                    return registryBase.OpenSubKey(@"Software\Microsoft\ReportViewer\v2.0.50727") != null
                        || registryBase.OpenSubKey(@"Software\Wow6432Node\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\ReportViewer v10") != null
                        || registryBase.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\ReportViewer\v10.0") != null;
                }
            }
            catch (Exception ex)
            {
                
                // put proper exception handling here
            }

            return false;
        }

        static string GetReport(string ReportPath)
        {
            if (ReportVersion == 0) ReportVersion = 2012;
            return GetReportDirectory(ReportPath, ReportVersion);
        }

        public static string RVVersion = "2012";
        public static Report Format1(this SFacture facture)
        {
            var articles = facture.Articles.ToCostumizedDataset(
                new cc<FakePrice>("ProductName", (ri) => ri.ProductName, typeof(string)),
                 new cc<FakePrice>(nameof(Article.Qte), (ri) => ri.Qte, typeof(float)),
                 new cc<FakePrice>(nameof(Article.Price), (ri) => ri.Price, typeof(float))
                );
            return new Report(GetReport(@"BonAchat\BAchat.rdlc")) { { "Articles", articles } };
        }

        private static Report report1 = new Report($@".\Reporting\{RVVersion}\BonVent\BVent.rdlc");
        public static Report Format1(this Facture facture, Database database)
        {
            int i = 0;
            var versment = facture.getVersmnet(database);
            var report1 = new Report(GetReport(@"BonVent\BVent.rdlc"))
            {
                { "ClientName", facture.Client.FullName },
                { "AgentName", facture.Editeur?.FullName ?? "Admin" },
                { "BonID", facture.Ref ?? "Unknow" },
                { "Date", facture.Date.ToString("dd/MM/yy HH:mm") },
                { "NArticles", facture.Articles.Count.ToString() },
                { "Remarque", "Merci" },
                { "Total", (facture.Total ).ToString("0.00") },
                { "Versment", versment.ToString("0.00") },
                { "Sold", (facture.Total - versment).ToString("0.00") },
                { "SoldGlobal", facture.Client.SoldTotal.ToString("0.00") },

                { "Articles", facture.Articles.ToCostumizedDataset(
                    new cc<Article>(nameof(Article.Order), (ri) => i++, typeof(string)),
                    new cc<Article>(nameof(Article.Label), (ri) => ri.Label, typeof(string)),
                    new cc<Article>(nameof(Article.Qte), (ri) => ri.Qte, typeof(float)),
                    new cc<Article>(nameof(Article.Price), (ri) => ri.Price, typeof(float))
                    )
                }
            };
            //var t = new MyNamespace.Demo().Print(report1);
            return report1;

        }
        public static Report BVENT_A5(this Facture facture, Database database)
        {
            int i = 0;
            var versment = facture.getVersmnet(database);
            var reportPath = GetReport(@".\BonVent\BVent_A5.rdlc");
            var report1 = new Report( reportPath)

            {
                { "CLIENT", facture.Client.FullName },
                { "VENDEUR", facture.Editeur?.FullName ?? "Admin" },
                { "BONID", facture.Ref ?? "Unknow" },
                { "Date", facture.Date.ToString("dd/MM/yy HH:mm") },
                { "Total", (facture.Total ).ToString("0.00") },
                { "Versement", versment.ToString("0.00") },
                { "Sold", (facture.Total - versment).ToString("0.00") },
                { "SoldGlobal", facture.Client.SoldTotal.ToString("0.00") },

                { "Articles", facture.Articles.ToCostumizedDataset(
                    new cc<Article>(nameof(Article.Order), (ri) => i++, typeof(string)),
                    new cc<Article>(nameof(Article.Label), (ri) => ri.Label, typeof(string)),
                    new cc<Article>(nameof(Article.Qte), (ri) => ri.Qte, typeof(float)),
                    new cc<Article>(nameof(Article.Price), (ri) => ri.Price, typeof(float)),
                    new cc<Article>(nameof(Article.Total), (ri) => ri.Total, typeof(float))
                    )
                }
            };
            //var t = new MyNamespace.Demo().Print(report1);
            return report1;

        }

        public static Report Format2(this Facture facture)
        {
            int i = 0;
            Report report = new Report(GetReport(@"BonVent\Format2.rdlc")){
                { "Articles", facture.Articles.ToCostumizedDataset(
                    new cc<Article>(nameof(Article.Order), (ri) => i++, typeof(string)),
                    new cc<Article>(nameof(Article.Qte), (ri) => ri.Qte, typeof(float)),
                    new cc<Article>(nameof(Article.Label), (ri) => ri.Label, typeof(string)))
                },
                { "ClientName", facture.Client.FullName },
                { "AgentName", facture.Editeur?.FullName ?? "" },
                { "BonID", facture.Ref ?? "Unknow" },
                { "Date", facture.Date.ToString("dd/MM/yy HH:mm") },
                { "NArticles", facture.Articles.Count.ToString() },
            };
            
            
            return report;
        }
        public static Report BonCommand(this Facture facture)
        {
            var client = facture.Client;
            var clientName = string.IsNullOrWhiteSpace(client?.Name) ? client?.FullName : client.Name;
            var clientAddress = client?.Address ?? "";

            Report report = new Report(GetReport(@"BonVent\BonCommand.rdlc")){
                { "Articles", facture.Articles.ToCostumizedDataset(
                    new cc<Article>(nameof(Article.Ref), (ri) => ri.Ref, typeof(string)),
                    new cc<Article>(nameof(Article.Label), (ri) => ri.Label, typeof(string)),
                    new cc<Article>(nameof(Article.Qte), (ri) => ri.Qte, typeof(float)),
                    new cc<Article>(nameof(Article.Price), (ri) => ri.Price, typeof(float)),
                    new cc<Article>(nameof(Article.Total), (ri) => ri.Total, typeof(float))
                    ) },
                { "ClientName", facture.Client.FullName },
                { "AgentName", facture.Editeur?.FullName ?? "" },
                { "BonID", facture.Ref ?? "Unknow" },
                { "Date", facture.Date.ToString("dd/MM/yy HH:mm") },
                { "NArticles", facture.Articles.Count.ToString() },
                {"ShopName","QShop " },
                {"ShopAddress","Rue Kaidi Lot 6 BEK Alger" },
                {"ClientName",clientName},
                {"ClientAddress",clientAddress },
                {"Total",facture.Total.ToString("0.00") },
                {"ShopTel","023942332" },
                {"ClientTel",client?.Tel     }
            };
            
            
            return report;
        }
        public static Report Facture(this Facture facture)
        {
            var client = facture.Client;
            var clientName = string.IsNullOrWhiteSpace(client?.Name) ? client?.FullName : client.Name;
            var clientAddress = client?.Address ?? "";

            Report report = new Report(GetReport(@"BonVent\Facture.rdlc")){
                { "Articles", facture.Articles.List },
                { "ClientName", facture.Client.FullName },
                { "BonID", facture.Ref ?? "Unknow" },
                { "Date", facture.Date.ToString("dd/MM/yy HH:mm") },
                { "NArticles", facture.Articles.Count.ToString() },
                {"ShopName","QShop " },
                {"ShopAddress","Rue Kaidi Lot 6 BEK Alger" },
                {"ClientName",clientName},
                {"ClientAddress",clientAddress },
                {"TotalHT",facture.Total.ToString("0.00") },
                {"TotalTTC",facture.Total.ToString("0.00") },
                {"ShopTel","023942332" },
                {"ClientTel",client?.Tel },
                {"ClientCodePostal",client?.CodePostal},
                {"TotalTVA",(facture.Total*19).ToString("0.00") },
                {"RC","SDFDSGDFGDF" },
                {"NIF","DFGDGHFHGFH" },{"TVA","19"}
            };
            
            
            return report;
        }
        private static cc<EtatTransfer>[] _converter = new cc<EtatTransfer>[] {
                new cc<EtatTransfer>(nameof(EtatTransfer.Date), (c) => c.Date.ToShortDateString(), typeof(string)),
                new cc<EtatTransfer>(nameof(EtatTransfer.Ref), (c) => c.Ref, typeof(string)),
                new cc<EtatTransfer>(nameof(EtatTransfer.Transaction), (c) => c.Transaction, typeof(string)),
                new cc<EtatTransfer>(nameof(EtatTransfer.Montant), (c) => c.Montant, typeof(float)),
                new cc<EtatTransfer>(nameof(EtatTransfer.SoldActual), (c) => c.SoldActual, typeof(float))
        };
        internal static Report EtatClient(Database db, Client client)
        {
            var versment = EtatTransfers.GetEtatTransfers(db, client, out var totalFactures, out var totalVersment);
            EtatTransfers.CalcSoldActual(versment);

            var List = DataTable<EtatTransfer>.ToCostumizedDataset(versment, _converter);
            var report1 = new Report(GetReport(@"BonVent\EtatClient.rdlc"))
            {
                { "ClientName", client.Name },
                { "Date", DateTime.Now.ToString("dd/MM/yy HH:mm") },
                { "TotalFactures", totalFactures.ToString("0.00") },
                { "TotalVersments", totalVersment.ToString("0.00") },
                { "List", List},
                { "Sold",(totalFactures-totalVersment).ToString("0.00")}
            };
            return report1;
        }

        public static Report CommandF1(this Command command)
        {
            var articles = command.Articles.ToDataset(new cc<CArticle>("Name", (ri) => ri.Name, typeof(string)),
                new cc<CArticle>(nameof(CArticle.FournisseurName), (ri) => ri.FournisseurName, typeof(string)));

            Report report = new Report(GetReport(@"Command\F1.rdlc")){
                { "Articles", articles }
            };
            
            return report;
        }

        internal static Report ProductsF1(Server.RequestArgs args, Products arr)
        {
            var d = args.Client.GetCookie($"{nameof(ProductsF1)}-IsLocked", false, out var expired);
            if (d != null && !expired)
            {
                args.SendAlert("Wait", "Wait for 15 seconds befor another request", "OK", false);
                return null;
            }
            args.Client.SetCookie($"{nameof(ProductsF1)}-IsLocked", true, DateTime.Now + TimeSpan.FromSeconds(15));
            var articles = arr.ToCostumizedDataset(
                new cc<Product>(nameof(Product.Label), (ri) => ri.Label, typeof(string)),
                new cc<Product>(nameof(Product.CategoryName), (ri) => ri.Category?.Name ?? "", typeof(string))
                );

            Report report = new Report(GetReport(@"Products\F1.rdlc")){
                { "Products", articles }
            };
            return report;
        }

        public static bool ConvertRDLC2016To2012(string fileIN,ref string fileOUT)
        {
            var @in = new System.IO.FileInfo(fileIN);
            fileOUT = fileOUT ?? System.IO.Path.GetTempFileName();
            var @out = new System.IO.FileInfo(fileOUT);
            if (!@in.Exists) return false;
            var txt = File.ReadAllText(@in.FullName);
            //if (@out.Exists) @out.Delete();

            var otxt = txt;
            string deleteTag(string text, string tag)
            {
                //GridLayoutDefinition
                var i0 = text.IndexOf("<" + tag + ">");
                if (i0 == -1) return text;
                var i1 = text.IndexOf("</" + tag + ">", i0);
                if (i1 != -1)
                    return text.Remove(i0, i1 - i0 + 1 + ("</" + tag + ">").Length);
                return text;
            }

            txt = deleteTag(txt, "GridLayoutDefinition")
                .Replace("http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition", "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition")
            .Replace("http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition", "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition");
            if (otxt != txt)
                File.WriteAllText(@out.FullName, txt, System.Text.Encoding.Default);
            return true;
        }
        public static bool ConvertRDLC2016To20121(string fileIN, string fileOUT)
        {
            return true;
        }
    }
}