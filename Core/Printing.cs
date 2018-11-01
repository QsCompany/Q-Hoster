using System;
using System.IO;
using System.Data;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Collections;
using Microsoft.Reporting.WinForms;
using System.Xml;

namespace QServer.Printing
{
    using Rectangle = System.Drawing.Rectangle;
    public class Report : IEnumerable, IDisposable
    {
        public static implicit operator LocalReport(Report report) => report.report;
        public static implicit operator Report(LocalReport report) => new Report(report);

        private LocalReport report;
        public Report(LocalReport report = null) => this.report = report ?? new LocalReport();
        public Report(string reportPath) => report = new LocalReport() { ReportPath = reportPath };
        public void Add(string name, string value, bool visible = true) => report.SetParameters(new ReportParameter(name, value));
        public void Add(string name, string[] value, bool visible = true) => report.SetParameters(new ReportParameter(name, value));
        public void Add(string name, DataTable value) => report.DataSources.Add(new ReportDataSource(name, value));
        public void Add(string name, BindingSource value) => report.DataSources.Add(new ReportDataSource(name, value));
        public void Add(string name, object value) => report.DataSources.Add(new ReportDataSource(name, value));
        public void Add(string name, IEnumerable value) => report.DataSources.Add(new ReportDataSource(name, value));
        public void Add(string name, Type value) => report.DataSources.Add(new ReportDataSource(name, value));
        public IEnumerator GetEnumerator() => null;

        public void Dispose()
        {
            if (report == null) return;
            report.Dispose();
            report = null;
        }

        public static implicit operator ReportToBytes(Report report) => new ReportToBytes();
    }
    public class ReportToBytes:IDisposable
    {
        public IList<MemoryStream> m_streams;
        public DeviceInfo deviceInfo = new DeviceInfo();

        public ReportToBytes()
        {

        }

        private Stream CreateStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
        {
            MemoryStream stream = new MemoryStream();
            m_streams.Add(stream);
            return stream;
        }
        public void Export(LocalReport report)
        {
            m_streams = new List<MemoryStream>();
            report.Render(deviceInfo.Format, deviceInfo.ToString(), CreateStream, out var warnings);

            foreach (Stream stream in m_streams)
                stream.Position = 0;
            SaveTo(@"C:\Users\QCompany\AppData\Local\Temp\fileTest." + deviceInfo.OutputFormat);
        }
        public void Dispose()
        {
            if (m_streams == null) return;
            foreach (var m in m_streams)
                m.Dispose();
            m_streams.Clear();
            m_streams = null;
            deviceInfo = null;
        }
        ~ReportToBytes()
        {
            Dispose();
        }
        public string[] SaveTo(string file)
        {
            var l = new string[m_streams.Count];
            if (m_streams.Count == 1)
            {
                var strm = m_streams[0];
                File.WriteAllBytes(file, m_streams[0].GetBuffer());
                l[0] = file;
                return l;
            }
            var f = new FileInfo(file);
            var p1 = f.Directory.FullName + "\\" + f.Name + "[";
            var p2 = "]" + f.Extension;
            for (int i = 0; i < m_streams.Count; i++)
            {
                var strm = m_streams[i];
                l[i] = p1 + i + p2;
                File.WriteAllBytes(l[i], m_streams[i].GetBuffer());
            }
            return l;
        }
        public string ExportAsTiFF(LocalReport report, string file)
        {
            var er = report.ListRenderingExtensions();
            deviceInfo.OutputFormat = OutputFormat.TIFF;
            byte[] bytes = report.Render("IMAGE", null, out var mimeType, out var encoding, out var extension, out var streamIds, out var warnings);
            return WriteAllBytes(file, extension, bytes);
        }
        private static DirectoryInfo tempPath = new DirectoryInfo(Environment.GetEnvironmentVariable("TEMP"));
        static ReportToBytes()
        {
            try
            {
                if (!tempPath.Exists)
                    tempPath.Create();
            }
            catch
            {
                MyConsole.WriteLine("UnExpected Error Where we accessing to temp PATH");
            }
        }
        public static string getPath(string s) => Path.Combine(tempPath.FullName, s);
        public string ExportAsPdf(LocalReport report, string file)
        {
            try
            {                
                byte[] bytes = report.Render("PDF", null, out var mimeType, out var encoding, out var extension, out var streamIds, out var warnings);
                File.WriteAllBytes(file = getPath(file + "." + extension), bytes);
                return file;
            }
            catch
            {
                return null;
            }
        }
        public static string WriteAllBytes(string fileName,string extension,byte[] bytes)
        {
            var fn = getPath(fileName + "." + extension);
            File.WriteAllBytes(fn, bytes);
            return fn;
        }
        public string ExportAsExcel(LocalReport report, string file)
        {
            byte[] bytes = report.Render("Excel", null, out var mimeType, out var encoding, out var extension, out var streamIds, out var warnings);
            return WriteAllBytes(file , extension, bytes);
        }
        public string ExportAsOpenExcel(LocalReport report, string file)
        {
            byte[] bytes = report.Render("EXCELOPENXML", null, out var mimeType, out var encoding, out var extension, out var streamIds, out var warnings);
            return WriteAllBytes(file, extension, bytes);
        }
        public string ExportAsWord(LocalReport report, string file)
        {
            byte[] bytes = report.Render("WORD", null, out var mimeType, out var encoding, out var extension, out var streamIds, out var warnings);
            return WriteAllBytes(file, extension, bytes);
        }
        public string ExportAsOpenWord(LocalReport report, string file)
        {
            byte[] bytes = report.Render("WORDOPENXML", null, out var mimeType, out var encoding, out var extension, out var streamIds, out var warnings);
            return WriteAllBytes(file, extension, bytes);
        }
    }

    public class ReportPrinter:IDisposable
    {
        private ReportToBytes report;
        private int m_currentPageIndex;
        public ReportPrinter(ReportToBytes report)
        {
            this.report = report;
        }
        public void Print()
        {
            if (report.deviceInfo.Format.ToLower() != "image") throw new Exception("The File cannot be printed");
            if (report.m_streams == null || report.m_streams.Count == 0) throw new Exception("Error: no stream to print.");
            report.deviceInfo.ReformatPaper = true;
            Print("File.pdf");
        }
        private void Print(string file)
        {
            PrintDocument printDoc = new PrintDocument { PrinterSettings = new PrinterSettings(), DefaultPageSettings = new PageSettings() { PrinterSettings = new PrinterSettings() { } } };
            printDoc.PrinterSettings.PrinterName = report.deviceInfo.PrinterName;
            printDoc.PrinterSettings.PrintFileName = file;
            printDoc.PrinterSettings.PrintToFile = file != null;

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
        delegate void PrintFoemt(MemoryStream memoryStream, PrintPageEventArgs ev);
        private PrintFoemt ToMethod(OutputFormat format)
        {
            switch (format)
            {
                case OutputFormat.EMF:
                    return PrintEMF;
                case OutputFormat.PNG:
                    return PrintPNG;
                case OutputFormat.JPEG:
                    return PrintJPEG;
                case OutputFormat.MEMORYBMP:
                    return PrintMemoryBMP;
                case OutputFormat.BMP:
                    return PrintBMP;
                case OutputFormat.WMF:
                    return PrintWMF;
                case OutputFormat.GIF:
                    return PrintGIF;
                case OutputFormat.TIFF:
                    return PrintTIFF;
                case OutputFormat.EXIF:
                    return PrintEXIF;
                case OutputFormat.ICON:
                    return PrintICON;
                default:
                    throw new NotImplementedException();
            }
        }
        private void PrintPage(object sender, PrintPageEventArgs ev)
        {
            if (report.deviceInfo.OutputFormat != OutputFormat.EMF) throw null;
            var m = ToMethod(report.deviceInfo.OutputFormat);
            var mem = report.m_streams[m_currentPageIndex];
            mem.Position = 0;
            m(mem, ev);
            m_currentPageIndex++;
            ev.HasMorePages = (m_currentPageIndex < report.m_streams.Count);
        }

        private void PrintICON(MemoryStream memoryStream, PrintPageEventArgs ev)
        {
            throw new NotImplementedException();
        }

        private void PrintEXIF(MemoryStream memoryStream, PrintPageEventArgs ev)
        {
            throw new NotImplementedException();
        }

        private void PrintTIFF(MemoryStream memoryStream, PrintPageEventArgs ev)
        {
            throw new NotImplementedException();
        }

        private void PrintGIF(MemoryStream memoryStream, PrintPageEventArgs ev)
        {
            throw new NotImplementedException();
        }

        private void PrintWMF(MemoryStream memoryStream, PrintPageEventArgs ev)
        {
            throw new NotImplementedException();
        }

        private void PrintBMP(MemoryStream memoryStream, PrintPageEventArgs ev)
        {
            throw new NotImplementedException();
        }

        private void PrintMemoryBMP(MemoryStream memoryStream, PrintPageEventArgs ev)
        {
            throw new NotImplementedException();
        }

        private void PrintJPEG(MemoryStream memoryStream, PrintPageEventArgs ev)
        {
            throw new NotImplementedException();
        }

        private void PrintPNG(MemoryStream memoryStream, PrintPageEventArgs ev)
        {
            throw new NotImplementedException();
        }

        private void PrintEMF(MemoryStream stream, PrintPageEventArgs ev)
        {
            var g = ev.Graphics;
            Metafile pageImage = new Metafile(report.m_streams[m_currentPageIndex]);

            // Adjust rectangular area with printer margins.
            Rectangle adjustedRect = new Rectangle(ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
                ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
                ev.PageBounds.Width,
                ev.PageBounds.Height);

            // Draw a white background for the report
            g.FillRectangle(System.Drawing.Brushes.White, adjustedRect);

            // Draw the report content
            g.DrawImage(pageImage, adjustedRect);
        }

        public void Dispose()
        {
            if (report == null) return;
            report.Dispose();
            m_currentPageIndex = -1;
        }
        ~ReportPrinter()
        {
            Dispose();
        }
    }
    public enum OutputFormat
    {
        EMF,
        PNG,
        JPEG,
        MEMORYBMP,
        BMP,
        WMF,
        GIF,
        TIFF,
        EXIF,
        ICON,
    }
    public class DeviceInfo
    {
        public OutputFormat OutputFormat = OutputFormat.EMF;
        public bool ReformatPaper;
        public string Format = "IMAGE";
        public double PageWidth = 8.5 / 2, PageHeight = 11, MarginTop = 0.2, MarginLeft = 0.2, MarginRight = 0.2, MarginBottom = 0.2;
        public override string ToString()
        {
            return $@"<DeviceInfo>
                        <OutputFormat>{OutputFormat.ToString()}</OutputFormat>" + (
                        ReformatPaper ?
                        $@"<PageWidth>{PageWidth}in</PageWidth>
                        <PageHeight>{PageHeight}in</PageHeight>
                        <MarginTop>{MarginTop}in</MarginTop>
                        <MarginLeft>{MarginLeft}in</MarginLeft>
                        <MarginRight>{MarginRight}in</MarginRight>
                        <MarginBottom>{MarginBottom}in</MarginBottom>" : "") +
                    @"</DeviceInfo>";
        }
        public string PrinterName;
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
    }
}