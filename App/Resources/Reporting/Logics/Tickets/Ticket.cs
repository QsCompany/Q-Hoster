using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Threading;
using System.Windows.Forms;
using static System.Math;
using PointF = System.Drawing.PointF;
using Pen = System.Drawing.Pen;
using SizeF = System.Drawing.SizeF;
using Graphics = System.Drawing.Graphics;
using Font = System.Drawing.Font;
using Color = System.Drawing.Color;
using Brushes = System.Drawing.Brushes;
using models;
using Server;
using QServer.Reporting;
using Json;
using QServer.Serialization;

namespace QServer.Reporting
{
    public partial class TForm : Form
    {
        private static Stack<Ticket> Tickets = new Stack<Ticket>();
        static TForm() { Initialize(); }
        public TForm()
        {
            document.Tickets = Tickets;
            InitializeComponent();
        }
        private static void Initialize()
        {
            Tickets.Clear();
            Tickets.Push(new Ticket() { PrixAchat = 230, PrixVent = 400, Count = 13 });
            Tickets.Push(new Ticket() { PrixAchat = 12340, PrixVent = 23400, Count = 5 });
            Tickets.Push(new Ticket() { PrixAchat = 0, PrixVent = 20, Count = 10 });
            Tickets.Push(new Ticket() { PrixAchat = 10, PrixVent = 23400, Count = 3 });
        }
        System.Threading.Timer timer;
        protected override void OnLoad(EventArgs e)
        {
            PictureBox p = null;
            if (p != null)
                p.SizeChanged += (s, ex) =>
                {
                    if (timer != null) { timer.Change(500, Timeout.Infinite); return; }
                    timer = new System.Threading.Timer((a) =>
                    {
                        timer.Dispose();
                        timer = null;
                        /////
                        var g = p.CreateGraphics();
                        g.Clear(Color.White);
                        var drawer = new TicketDrawer { Tickets = Tickets };
                        drawer.Reset();
                        drawer.Print(g, new System.Drawing.Rectangle(20, 20, p.Width - 40, p.Height - 40), out var morePage);

                        ////
                    }, this, 500, Timeout.Infinite);
                };
            base.OnLoad(e);
        }
        [STAThread]
        static void iMain()
        {
            Application.VisualStyleState = System.Windows.Forms.VisualStyles.VisualStyleState.ClientAndNonClientAreasEnabled;
            Application.SetCompatibleTextRenderingDefault(true);
            Application.Run(new TForm());
        }

        private void InitializeComponent()
        {
            printPreview = new System.Windows.Forms.PrintPreviewControl();
            btnPrint = new System.Windows.Forms.Button();
            btnSetup = new System.Windows.Forms.Button();
            SuspendLayout();

            // 
            // printPreview
            // 
            printPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            printPreview.Location = new System.Drawing.Point(104, 2);
            printPreview.Name = "printPreview";
            printPreview.Size = new System.Drawing.Size(725, 442);
            printPreview.TabIndex = 0;
            printPreview.Document = document;
            // 
            // btnPrint
            // 
            btnPrint.Location = new System.Drawing.Point(12, 12);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new System.Drawing.Size(86, 23);
            btnPrint.TabIndex = 1;
            btnPrint.Text = "Imprimer";
            btnPrint.UseVisualStyleBackColor = true;
            btnPrint.Click += new System.EventHandler(BtnPrint_Click);
            // 
            // btnSetup
            // 
            btnSetup.Location = new System.Drawing.Point(12, 41);
            btnSetup.Name = "btnSetup";
            btnSetup.Size = new System.Drawing.Size(86, 23);
            btnSetup.TabIndex = 2;
            btnSetup.Text = "Configuration";
            btnSetup.UseVisualStyleBackColor = true;
            btnSetup.Click += new System.EventHandler(BtnSetup_Click);
            // 
            // TForm
            // 
            ClientSize = new System.Drawing.Size(832, 444);
            Controls.Add(btnSetup);
            Controls.Add(btnPrint);
            Controls.Add(printPreview);
            Name = "TForm";
            printPreview.Document.DefaultPageSettings = pageSettings;
            ResumeLayout(false);

        }

        private PrintPreviewControl printPreview;
        private Button btnPrint;
        private Button btnSetup;
        private PageSettings pageSettings = new PageSettings();
        private void BtnPrint_Click(object sender, EventArgs se)
        {
            document = new TicketDocument(Tickets);
            TicketDocument t = new TicketDocument(Tickets);
            document.Print();
        }

        private void BtnSetup_Click(object sender, EventArgs e)
        {
        }
        private TicketDocument document = new TicketDocument();

    }


    public class TicketParam : ITicketParam
    {
        public Font PA_Font;
        public Font PV_Font;

        public object Get(string s)
        {
            throw new NotImplementedException();
        }

        public object Get(Type t)
        {
            throw new NotImplementedException();
        }

        public object Get(object a)
        {
            throw new NotImplementedException();
        }
    }

    public class PrinteableTicket : IPrinteableTicket
    {
        public string PrixAchat { get; set; }
        public string PrixVent { get; set; }
        public int Count { get; set; }
        private static Pen pen = new Pen(Brushes.Black, 2);
        public SizeF Draw(Graphics g, PointF pnt, object param)
        {
            var p = param as TicketParam;
            var size = GetSize(g, param, out SizeF mpa, out SizeF mpv);
            var ls = size.Width / 2;
            var x = pnt.X + 7;
            var y = pnt.Y + 7;
            g.DrawString(PrixAchat, p.PA_Font, Brushes.DimGray, (size.Width - mpa.Width) / 2 + x, 0 + y);
            g.DrawLine(pen, (size.Width - ls) / 2 + x, mpa.Height + 2 + y, (size.Width + ls) / 2 + x, mpa.Height + 2 + y);
            g.DrawString(PrixVent, p.PV_Font, Brushes.Black, (size.Width - mpv.Width) / 2 + x, mpa.Height + 5 + y, System.Drawing.StringFormat.GenericDefault);
            return size;
        }
        public SizeF GetSize(Graphics e, object param, out SizeF mpa, out SizeF mpv)
        {
            var p = param as TicketParam;
            mpa = e.MeasureString(PrixAchat, p.PA_Font);
            mpv = e.MeasureString(PrixVent, p.PV_Font);
            return new SizeF(Max(mpa.Width, mpv.Width) + 15, mpa.Height + 5 + mpv.Height + 15);
        }
        public SizeF GetSize(Graphics e, object param) => GetSize(e, param, out var a, out var b);
    }
}

namespace models
{
    [QServer.Core.HosteableObject(null, typeof(TicketSerializer))]
    public class Ticket : DObject, ITicket
    {
        public new static int __LOAD__(int dp) => DObject.__LOAD__(DPCount);
        
        public static int DPPrixAchat = Register<Ticket, float>("PrixAchat");

        public static int DPPrixVent = Register<Ticket, float>("PrixVent");
        public static int DPCount = Register<Ticket, int>("Count");

        public float PrixAchat { get => get<float>(DPPrixAchat); set => set(DPPrixAchat, value); }
        public float PrixVent { get => get<float>(DPPrixVent); set => set(DPPrixVent, value); }
        public int Count { get => get<int>(DPCount); set => set(DPCount, value); }

        private static char[] NumberTransform = new[] { 'Z', 'K', 'I', 'F', 'A', 'N', 'B', 'O', 'R', 'D' };

        public Ticket(Context c, JValue jv) : base(c, jv)
        {
        }

        public Ticket()
        {
        }

        private static string Number2Code(float n)
        {
            var s = n.ToString("n2");
            string r = "";
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '.' || c == ',') continue;
                else r += NumberTransform[c - '0'];
            }
            return r;
        }

        public IPrinteableTicket ToPrinteableTicket() => new PrinteableTicket() { PrixAchat = Number2Code(PrixAchat), PrixVent = PrixVent.ToString("n2"), Count = Count };
    }

    [QServer.Core.HosteableObject(typeof(Api.Tickets), typeof(TicketsSerializers))]
    public class Tickets : DObject
    {
        public new static int __LOAD__(int mp) => mp + Server.DObject.__LOAD__(DPPrinterName);
        public static int DPPrinterName = Register<Tickets, string>("PrinterName");
        public string PrinterName { get => get<string>(DPPrinterName); set => set(DPPrinterName, value); }

        public static int DPResponse = Register<Tickets, JObject>("Response", PropertyAttribute.NonModifiableByHost);
        public JObject Response { get => get<JObject>(DPResponse); set => set(DPResponse, value); }
        public static int DPValues = Register<Tickets, JArray>("Values");

        public Tickets(Context c, JValue jv) : base(c, jv)
        {
            
        }

        public JArray Values { get => get<JArray>(DPValues); set => set(DPValues, value); }

        public void Print()
        {
            TicketDocument r = new TicketDocument(System.Linq.Enumerable.Cast<Ticket>(Values), PrinterName);
            var a = r.ReginerateNewFile();
            r.Print();
        }

        internal string Print(RequestArgs args)
        {
            TicketDocument r = new TicketDocument(System.Linq.Enumerable.Cast<Ticket>(Values), PrinterName);
            var a = r.ReginerateNewFile();
            r.Print();
            return a;            
        }

    }
}