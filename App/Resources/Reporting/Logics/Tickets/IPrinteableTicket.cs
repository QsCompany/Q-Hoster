using models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using static System.Math;
using Microsoft.Reporting.WinForms;
namespace QServer.Reporting
{
    public interface IPrinteableTicket
    {
        int Count { get; set; }
        SizeF Draw(Graphics g, PointF pnt, object param);
        SizeF GetSize(Graphics e, object param);
    }
    public interface ITicket
    {
        IPrinteableTicket ToPrinteableTicket();
    }

    public enum CursorStat
    {
        Success = 1,
        NoRoom = 2,
        TooBig = 4,
        WithTooBig = TooBig | 8,
        HeightTooBig = TooBig | 16,
    }

    public interface ITicketParam
    {
        object Get(string s);
        object Get(Type t);
        object Get(object a);
    }

    public class Bound
    {
        public float X, X1;
        public float Y, Y1;
        public float W, H;
        public Bound(System.Drawing.Rectangle rectangle)
        {
            X = rectangle.X;
            Y = rectangle.Y;
            W = rectangle.Width;
            H = rectangle.Height;
            X1 = X + W;
            Y1 = Y + H;
        }

    }

    public class Cursor
    {
        public float X;
        public float Y;
        public float RowHeight;

        public SizeF Margin = new SizeF(6, 6);
        public float MinWidth, MinHeight;
        public Graphics g;
        private Bound _bound;
        public System.Drawing.Rectangle Area
        {
            set
            {
                _bound = new Bound(value);
                X = _bound.X;
                Y = _bound.Y;
            }
        }

        public Cursor()
        {

        }
        public bool IsCursorInStart => X == _bound.X && Y == _bound.Y;
        private void RecalcSize(ref SizeF size)
        {

            if (MinWidth > 0) size.Width = Max(size.Width, MinWidth);
            if (MinHeight > 0) size.Height = Max(size.Height, MinHeight);
            size = size + Margin;
        }
        public CursorStat Draw(IPrinteableTicket ticket, object param)
        {
            var size = ticket.GetSize(g, param);
            var osize = new SizeF(size);
            RecalcSize(ref size);
        deb:
            if (!IsCanFittedInHeight(size)) return RowHeight == 0 ? CursorStat.HeightTooBig : CursorStat.NoRoom;
            if (!IsCanFittedInWidth(size))
            {
                if (X == _bound.X || RowHeight == 0) return CursorStat.WithTooBig;
                Y += RowHeight;
                RowHeight = 0;
                g.DrawLine(Pens.DarkGreen, _bound.X, Y, X, Y);
                X = _bound.X;
                goto deb;
            }

            ticket.Draw(g, new PointF(X + Abs((size.Width - osize.Width) / 2), Y + Abs((size.Height - osize.Height) / 2)), param);
            g.DrawRectangle(Pens.DarkGray, X + 1, Y + 1, size.Width - 2, size.Height - 2);
            X += size.Width;
            RowHeight = Max(RowHeight, size.Height);
            return CursorStat.Success;
        }
        bool IsCanFittedInWidth(SizeF size)
        {
            return X + size.Width <= _bound.X1;
        }
        bool IsCanFittedInHeight(SizeF size)
        {
            return Y + size.Height <= _bound.Y1;
        }
        public Cursor Reset(Graphics grphc, System.Drawing.Rectangle rectangle)
        {
            g = grphc ?? g;
            Area = rectangle;
            RowHeight = 0;
            return this;
        }
    }

    public class TicketDrawer
    {
        public ITicketParam param = new TicketParam() { PA_Font = new Font(new FontFamily("Times New Roman"), 12), PV_Font = new Font(new FontFamily("Times New Roman"), 14) };
        private IPrinteableTicket Current;
        private Cursor cursor = new Cursor();
        private ITicket[] _tickets;
        private int index = -1;
        private bool IsLocked = false;
        private bool NextTicket()
        {
        deb:
            if (_tickets == null) return false;
            if (Current == null || Current.Count <= 0)
                if (++index >= _tickets.Length)
                    return (Current = null) != null;
                else
                {
                    Current = _tickets[index].ToPrinteableTicket();
                    goto deb;
                }

            return true;
        }

        private CursorStat PrintQuee()
        {
            while (NextTicket())
            {
                var rslt = cursor.Draw(Current, param);
                if (rslt != CursorStat.Success) return rslt;
                Current.Count--;
            }
            return CursorStat.Success;
        }
        private void CPrintPage(out bool morePage)
        {
            switch (PrintQuee())
            {
                case CursorStat.Success:
                    morePage = false;
                    break;
                case CursorStat.NoRoom:
                    morePage = true;
                    break;
                case CursorStat.WithTooBig:
                case CursorStat.TooBig:
                    MessageBox.Show($"The Ticket {Current} its toobig to fit in the page");
                    Current.Count = 0;
                    NextTicket();
                    morePage = true;
                    break;
                case CursorStat.HeightTooBig:
                    if (cursor.IsCursorInStart)
                        goto case CursorStat.TooBig;
                    morePage = true;
                    break;
                default:
                    morePage = false;
                    throw new Exception("Unknown Stat");
            }
        }

        public void Print(Graphics g, System.Drawing.Rectangle area, out bool morePage)
        {
            if (IsLocked) throw new Exception("Its Frozed");
            IsLocked = true;
            cursor.Reset(g, area);
            try
            {
                CPrintPage(out morePage);
            }
            catch
            {
                morePage = false;
            }

            IsLocked = false;
        }

        public IEnumerable<ITicket> Tickets
        {
            get { return _tickets; }
            set { if (IsLocked) throw new Exception("Its Frozed"); _tickets = value?.ToArray(); }
        }

        public void Reset()
        {
            if (IsLocked) throw new Exception("Its Frozed");
            index = -1;
            Current = null;
        }
    }

    public class TicketDocument : PrintDocument
    {
        TicketDrawer drawer = new TicketDrawer();

        public IEnumerable<ITicket> Tickets { get => drawer.Tickets; set => drawer.Tickets = value; }
        public string PrinterName { get; set; }
        public TicketDocument(IEnumerable<Ticket> tickets = null, string PrinterName = null)
        {
            drawer.Tickets = tickets;
            this.PrinterName = PrinterName;

        }
        private string fileName;
        protected override void OnBeginPrint(PrintEventArgs e)
        {
            Success = false;
            PrinterSettings = GetPrinterSettings(PrinterSettings) ?? PrinterSettings;
            drawer.Reset();
            base.OnBeginPrint(e);
        }
        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            drawer.Print(e.Graphics, e.MarginBounds, out var morePage);
            e.HasMorePages = morePage;
            base.OnPrintPage(e);
        }

        protected override void OnQueryPageSettings(QueryPageSettingsEventArgs e)
        {
            e.PageSettings = GetPageSettings(e.PageSettings) ?? e.PageSettings;
            base.OnQueryPageSettings(e);
        }
        public void ShowDialog()
        {
            PrintPreviewDialog dg = new PrintPreviewDialog
            {
                Document = this,
                ShowIcon = true,
                UseAntiAlias = true
            };
            dg.ShowDialog();
        }

        public static string getPdfOrXps(string printerName, out bool isFile)
        {
            printerName = (printerName ?? "").ToLower();
            string xps = null, pdf = null;
            isFile = true;
            foreach (string o in PrinterSettings.InstalledPrinters)
            {
                var s = o.ToLower();
                if (printerName == s) return o;
                if (s.Contains("pdf")) pdf = o;
                if (s.Contains("xps")) xps = o;
            }
            pdf = (pdf ?? xps);
            isFile = pdf != null;
            return pdf ?? PrinterSettings.InstalledPrinters[0];
        }

        public virtual PageSettings GetPageSettings(PageSettings settings)
        {
            return new PageSettings
            {
                Margins = new Margins(15, 15, 15, 15),
                Color = false
            };
        }
        protected virtual PrinterSettings GetPrinterSettings(PrinterSettings settings)
        {
            var ps = new PrinterSettings
            {
                PrinterName = getPdfOrXps(PrinterName, out var isFile),
                PrintToFile = isFile,
            };
            var t = FileName;
            if (!string.IsNullOrWhiteSpace(t))
                ps.PrintFileName = t;
            return ps;

        }
        protected virtual string FileName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(fileName) || !AutoFileGenerator) return fileName ?? "";
                if (AutoFileGenerator) return ReginerateNewFile();
                return null;
            }
        }

        public bool Success { get; protected set; }

        public bool AutoFileGenerator;
        public string ReginerateNewFile()
        {
            string file = DateTime.Now.Ticks.ToString();
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return fileName = System.IO.Path.Combine(directory, file + ".pdf");
        }
    }
}
namespace MyNamespace
{
    using System;
    using System.IO;
    using System.Text;
    using System.Drawing.Imaging;
    using System.Drawing.Printing;
    using System.Collections.Generic;

    using Rectangle = System.Drawing.Rectangle;

    public class Demo : IDisposable
    {
        private int m_currentPageIndex;
        private IList<MemoryStream> m_streams;
        
        private Stream CreateStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
        {
            MemoryStream stream = new MemoryStream();
            m_streams.Add(stream);
            return stream;
        }
        private void Export(LocalReport report)
        {
            m_streams = new List<MemoryStream>();
            report.Render("Image", deviceInfo.ToString(), CreateStream, out var warnings);
            foreach (Stream stream in m_streams)
                stream.Position = 0;
        }
        private void PrintPage(object sender, PrintPageEventArgs ev)
        { 
            if (deviceInfo.OutputFormat != OutputFormat.EMF) throw null;

            Metafile pageImage = new Metafile(m_streams[m_currentPageIndex]);

            // Adjust rectangular area with printer margins.
            Rectangle adjustedRect = new Rectangle(
                ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
                ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
                ev.PageBounds.Width,
                ev.PageBounds.Height);

            // Draw a white background for the report
            ev.Graphics.FillRectangle(Brushes.White, adjustedRect);

            // Draw the report content
            ev.Graphics.DrawImage(pageImage, adjustedRect);
            
            m_currentPageIndex++;
            ev.HasMorePages = (m_currentPageIndex < m_streams.Count);
        }

        private void Print()
        {
            if (m_streams == null || m_streams.Count == 0)
                throw new Exception("Error: no stream to print.");

            switch (deviceInfo.OutputFormat)
            {
                case OutputFormat.EMF:
                    break;
                case OutputFormat.JPEG:
                case OutputFormat.PNG:
                    for (; m_currentPageIndex < m_streams.Count; m_currentPageIndex++)
                        File.WriteAllBytes(@"C:\Users\QCompany\AppData\Local\Temp\test(" + m_currentPageIndex + ")." + deviceInfo.OutputFormat, (m_streams[m_currentPageIndex] as MemoryStream).GetBuffer());
                    return;
                default:
                    return;
            }
            var preferedPaperSize = new PaperSize("CostumeSize", 425, 1100);

            PrintDocument printDoc = new PrintDocument
            {
                PrinterSettings = new PrinterSettings()
            };
            printDoc.BeginPrint += OnBeginPrint;
            printDoc.EndPrint += OnEndPrintPage;
            printDoc.PrintPage += OnPrintPage;
            printDoc.QueryPageSettings += QuerySettings;
            printDoc.DefaultPageSettings = new PageSettings() { PrinterSettings = new PrinterSettings() { }, PaperSize = preferedPaperSize };
            if (!printDoc.PrinterSettings.IsValid)
            {
                throw new Exception("Error: cannot find the default printer.");
            }
            else
            {
                printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
                m_currentPageIndex = 0;
                printDoc.Print();
            }
        }

        private void QuerySettings(object sender, QueryPageSettingsEventArgs e)
        {
            e.PageSettings.PaperSize= new PaperSize("CostumeSize", 425, 1100);
            e.PageSettings.PaperSource = new PaperSource() { RawKind = (int)PaperSourceKind.Custom, SourceName = "CostumeSize" };
        }

        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            e.PageSettings.PaperSize = new PaperSize("CostumeSize", 425, 1100);
        }

        private void OnEndPrintPage(object sender, PrintEventArgs e)
        {   
        }

        private void OnBeginPrint(object sender, PrintEventArgs e)
        {   
        }

        // Create a local report for Report.rdlc, load the data,
        //    export the report to an .emf file, and print it.
        private void Run(Database database)
        {
            
            var lreport = new LocalReport();            
            var facture = (database.Factures.AsList()[1].Value as models.Facture);
            var versment = facture.getVersmnet(database);
            QServer.Printing.Report report = new QServer.Printing.Report(@".\Reporting\BonVent\BVent.rdlc")
            {
                { "Articles", facture.Articles.List },
                { "ClientName", facture.Client.FullName },
                { "AgentName", facture.Editeur?.FullName ?? "" },
                { "BonID", facture.Ref ?? "Unknow" },
                { "Date", facture.Date.ToString("dd/MM/yy HH:mm") },
                { "NArticles", facture.Articles.Count.ToString() },
                { "Remarque", "Merci" },
                { "Total", facture.Total.ToString("0.00") },
                { "Versment", versment.ToString("0.00") },
                { "Sold", (facture.Total - versment).ToString("0.00") },
                { "SoldGlobal", facture.Client.SoldTotal.ToString("0.00") }
            };

            var reportStream = new QServer.Printing.ReportToBytes();
            reportStream.Export(report);
            var t = new QServer.Printing.ReportPrinter(reportStream);
            t.Print();

            reportStream.ExportAsPdf(report, "File.pdf");
            Export(report);
            Print();
        }
        public  bool Print(QServer.Printing.Report report) 
        {
            var reportStream = new QServer.Printing.ReportToBytes();
            reportStream.Export(report);
            var t = new QServer.Printing.ReportPrinter(reportStream);
            t.Print();

            //reportStream.ExportAsPdf(report, "File.pdf");
            Export(report);
            Print();
            return true;
        }
        public void Dispose()
        {
            if (m_streams != null)
            {
                foreach (Stream stream in m_streams)
                    stream.Close();
                m_streams = null;
            }
        }
        public DeviceInfo deviceInfo = new DeviceInfo();
    }
    public class DeviceInfo
    {
        public OutputFormat OutputFormat=OutputFormat.PNG;
        public double PageWidth = 8.5/2, PageHeight = 11, MarginTop = 0.2, MarginLeft = 0.2, MarginRight = 0.2, MarginBottom = 0.2;

        public override string ToString()
        {
            if (OutputFormat == OutputFormat.EMF)
                return $@"<DeviceInfo>
                        <OutputFormat>EMF</OutputFormat>
                        <PageWidth>{PageWidth}in</PageWidth>
                        <PageHeight>{PageHeight}in</PageHeight>
                        <MarginTop>{MarginTop}in</MarginTop>
                        <MarginLeft>{MarginLeft}in</MarginLeft>
                        <MarginRight>{MarginRight}in</MarginRight>
                        <MarginBottom>{MarginBottom}in</MarginBottom>
                    </DeviceInfo>";
            else
                return $"<DeviceInfo><OutputFormat>{OutputFormat.ToString()}</OutputFormat></DeviceInfo>";
        }

    }
    public enum OutputFormat
    {
        EMF,
        PNG,
        JPEG
    }
}