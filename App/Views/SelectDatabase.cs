using models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QServer
{
    public partial class SelectDatabase : Form
    {
        private readonly System.Data.Common.DbConnection db;

        public SelectDatabase(System.Data.Common.DbConnection db) : this(db, MySqlManager.GetDossiers(db))
        {
        }
        public SelectDatabase(System.Data.Common.DbConnection db, List<string> list)
        {
            this.db = db;
            InitializeComponent();
            if (list != null)
                foreach (var x in list)
                {
                    checkedListBox1.Items.Add(x);
                    this.List.Add(x.ToLower().Trim());
                }

            this.checkedListBox1.SelectedIndex = this.checkedListBox1.Items.IndexOf(this.GetSelectedDatabase() ?? "");
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var v = checkedListBox1.SelectedItem as string;
            if (v == null) return;
            SetSelectedDatabase(v);
            Close(true);
        }
        private bool exit;
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!exit) e.Cancel = true;
            base.OnClosing(e);
        }
        public bool Create { get; private set; }

        private string selectedDatabase;

        public string GetSelectedDatabase()
        {
            return selectedDatabase;
        }

        public void SetSelectedDatabase(string value)
        {
            selectedDatabase = value;
            this.checkedListBox1.SelectedItem = value ?? "";
        }

        public List<string> List { get; } = new List<string>();

        public new void Close(bool b = false) { DialogResult = b ? DialogResult.OK : DialogResult.Cancel; exit = true; base.Close(); }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            var v = textBox1.Text.ToLower().Trim();
            if (chech(v))
            {
                SetSelectedDatabase(v);
                Create = true;
                Close(true);
            }
        }
        private bool chech(string s)
        {
            if (s == null) return false;
            if (s == "mysql") return false;
            if (s == "information_schema") return false;
            if (List.Contains(s)) return false;
            if (GlobalRegExp.alphapitic.IsMatch(s))
            {
                return true;
            }
            return false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (chech(textBox1.Text.ToLower().Trim()))
                textBox1.BackColor = Color.White;
            else textBox1.BackColor = Color.Red;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close(false);
        }
    }
}
