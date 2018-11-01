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

namespace QServer
{
    public partial class LoginEdit : Form
    {
        private Dictionary<string, TextBox> txts = new Dictionary<string, TextBox>();
        models.Login Data;
        Server.IStat oldStat;
        bool Oblg;
        private LoginEdit()
        {
            InitializeComponent();
            
        }
        
        private void _Edit(models.Login data,bool oblg)
        {
            Data = data;
            Oblg = oblg;
            btnDiscart.Visible = !oblg;
            oldStat = data.SaveStat();
            collectTextBoxes();
            ShowDialog();
        }
        private void collectTextBoxes()
        {
            var t = new[] { panel1 };
            foreach (var pnl in t)
                foreach (var cnt in pnl.Controls)
                {
                    var txt = cnt as TextBox;
                    if (txt == null) continue;
                    var vl = Data.GetProperty(txt.Name);
                    if (vl == null) continue;
                    txts.Add(txt.Name, txt);
                    txt.Text = Data.get<string>(vl.Index);
                    txt.TextChanged += Txt_TextChanged;
                }
        }

        private void Txt_TextChanged(object sender, EventArgs e)
        {
            var txt = sender as TextBox;
            this.Data[txt.Name] = txt.Text;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!close)
                if (!Check()) { e.Cancel = true; return; }
            base.OnClosing(e);
        }
        bool close;
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (Check())
            {
                close = true;
                this.Close();
            }
        }

        private void btnDiscart_Click(object sender, EventArgs e)
        {
            this.Data.Restore(this.oldStat);
            if (!this.Oblg)
                this.close = true;
            this.Close();
        }
        ToolTip tt;
        bool Check()
        {
            if (!Data.Check(out var errors))
            {
                bool hasError = false;
                foreach (var err in errors)
                {
                    if (!txts.TryGetValue(err.Title, out var txt)) continue;
                    tt?.Dispose();
                    hasError = true;
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
                    return false;

                }
                return !hasError;
            }
            tt?.Dispose(); tt = null;
            return true;
        }

        internal static void Edit(Login agent, bool v)
        {
            new LoginEdit()._Edit(agent, v);
        }
    }
}
