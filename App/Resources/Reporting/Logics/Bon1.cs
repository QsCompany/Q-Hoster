using System;
using System.Windows.Forms;
using models;
using Microsoft.Reporting.WinForms;
using System.Threading;

namespace Server.Reporting
{

    public partial class Bon1 : Form
    {
        private Facture facture { get; }
        public Database Database { get; }
        public static Bon1 _default;
        public static Bon1 Default { get => _default ?? (_default = new Bon1()); }

        private Bon1()
        {
            InitializeComponent();
            this.reportViewer1.ShowPrintButton = true;
            
        }
        private Bon1(Database database, Facture facture) : this()
        {
            this.facture = facture;
            Database = database;
        }
        public static bool letClose;
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Print();
        }

        private Articles GetData()
        {
            return facture.Articles;
        }

        public static void Print(Database d, Facture f)
        {
            var t = new Thread(() => { new Bon1(d, f).ShowDialog(); });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
        private void Print()
        {
            if (facture == null) return;
            ReportDataSource datasource = new ReportDataSource("Articles", facture.Articles.List);
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(datasource);

            var versment = facture.getVersmnet(Database);
            reportViewer1.LocalReport.SetParameters(
                new ReportParameterCollection
                {
                    new ReportParameter("ClientName",facture.Client.FullName),
                    new ReportParameter("AgentName",facture.Editeur?.FullName??""),
                    new ReportParameter("BonID",facture.Ref??"Unknow"),
                    new ReportParameter("Date",facture.Date.ToString("dd/MM/yy HH:mm")),
                    new ReportParameter("NArticles",facture.Articles.Count.ToString()),
                    new ReportParameter("Remarque", "Merci"),
                    new ReportParameter("Total",facture.Total.ToString("0.00")),
                    new ReportParameter("Versment",versment.ToString("0.00")),
                    new ReportParameter("Sold", (facture.Total-versment).ToString("0.00")),
                    new ReportParameter("SoldGlobal", facture.Client.SoldTotal.ToString("0.00"))
                });
            
            reportViewer1.RefreshReport();
            
        }
    }
}
