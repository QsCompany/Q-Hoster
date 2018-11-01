using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using models;
using Server;

namespace QServer
{
    public partial class ClientEdit : Form
    {
        public Client Client { get; private set; }
        public IStat OldStat { get; private set; }
        private bool Objg;
        private Dictionary<string, TextBox> txts = new Dictionary<string, TextBox>();
        private ClientEdit()
        {
            
            InitializeComponent();
            
            
        }
        private DialogResult Showv(models.Client client, bool oblg)
        {
            this.Client = client;
            this.OldStat = client.SaveStat();
            this.btnDiscart.Visible = !(this.Objg = oblg);
            collectTextBoxes();
            return base.ShowDialog();
        }

        public static DialogResult Show(Client client, bool oblg)
        {
            using (var t = new ClientEdit())
                return t.Showv(client, oblg);
        }
        private void collectTextBoxes()
        {
            var t = new[] { panel1, panel3 };
            foreach (var pnl in t)
                foreach (var cnt in pnl.Controls)
                {
                    var txt = cnt as TextBox;
                    if (txt == null) continue;
                    var vl = Client.GetProperty(txt.Name);
                    if (vl == null) continue;
                    
                    txts.Add(txt.Name, txt);
                    txt.Text = Client.get<string>(vl.Index);
                    txt.TextChanged += Txt_TextChanged;
                }
        }

        private void Txt_TextChanged(object sender, EventArgs e)
        {
            var txt = sender as TextBox;
            this.Client[txt.Name] = txt.Text;
        }

        private void ClientEdit_Load(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!close)
            {
                if(!this._check())
                e.Cancel = true;
            }

            base.OnClosing(e);
        }
        private bool close;
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this._check())
            {
                this.close = true;
                this.Close();
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            this._check();
        }

        private void btnDiscart_Click(object sender, EventArgs e)
        {
            if (this.Objg) return;
            this.Client.Restore(this.OldStat);
            this.close = true;
            this.Close();
        }
        ToolTip tt = new ToolTip();
        bool _check()
        {

            if (!Client.Check(out List<models.Client.Message> errors))
            {
                foreach (var err in errors)
                {
                    var txt = txts[err.Title];
                    tt?.Dispose();
                    tt = new ToolTip
                    {
                        IsBalloon = true,
                        InitialDelay = 0,
                        ShowAlways = true,
                        BackColor = Color.Red,
                        ToolTipIcon = ToolTipIcon.Error,
                        ToolTipTitle = err.Title,
                        UseAnimation = true
                    };
                    tt.SetToolTip(txt, err.Body);
                    tt.Show(err.Body, txt);
                    break;

                }
                return false;
            }
            else
            {
                tt?.RemoveAll(); tt?.Dispose(); tt = null;
            }
            return true;
        }
    }
}
