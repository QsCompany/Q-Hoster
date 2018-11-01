using models;
using MySql.Data.MySqlClient;
using Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QServer
{
    public partial class ConnectionManager : Form
    {
        public ConnectionManager(MySqlManager m)
        {
            InitializeComponent();
            this.sql = m;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            btnReload_Click(null, null);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!IsIpValid()) MessageBox.Show("Unvalid Vaule : Server IP");
            else
            {
                saveData();
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void saveData()
        {

            Resource.ServerIP = ServerIP.Text;
            Resource.Port = (uint)ServerPort.Value;
            Resource.UserID = UserID.Text;
            Resource.Password = Password.Text;
            Resource.DatabasePath = DatabaseName.Text.ToLower();
        }

        private bool exit;
        private MySqlManager sql;

        private void btnReload_Click(object sender, EventArgs e)
        {
            ServerIP.Text = Resource.ServerIP;
            ServerPort.Value = Resource.Port;
            UserID.Text = Resource.UserID;
            Password.Text = Resource.Password;
            DatabaseName.Text = Resource.DatabasePath;
        }

        private void btnDiscart_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!exit)
                e.Cancel = true;
        }
        private void ServerIP_TextChanged(object sender, EventArgs e)
        {
            IsIpValid();
        }
        private bool IsIpValid()
        {
            var c = ServerIP.Text;
            var vv = c.Split('.').Length == 4 && IPAddress.TryParse(ServerIP.Text, out var ip);

            if (vv)
                ServerIP.BackColor = Color.White;
            else ServerIP.BackColor = Color.Red;

            var vv1 = GlobalRegExp.alphapitic.IsMatch(DatabaseName.Text) && DatabaseName.Text.Length > 2;
            if (vv1)
                DatabaseName.BackColor = Color.White;
            else DatabaseName.BackColor = Color.Red;
            return vv && vv1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        new void Close()
        {
            exit = true;
            base.Close();
        }
        
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            IsIpValid();
        }

        private void btnCreateStock_Click(object sender, EventArgs e)
        {
            if (!IsIpValid()) { MessageBox.Show("Invalid input"); return; }
            var dbname = Resource.DatabasePath;
            Exception exp = null;
            saveData();
            Resource.DatabasePath = "";
            using (MySqlConnection x = new MySqlConnection(sql.SqlConnectionString.ConnectionString))
                try
                {
                    x.Open();
                    using (var cmd = x.CreateCommand())
                    {
                        cmd.CommandText = $"CREATE DATABASE `{DatabaseName.Text}`";
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            Resource.DatabasePath = DatabaseName.Text;
                            MessageBox.Show("La creation de stock reussir");
                            DialogResult = DialogResult.OK;
                            Close();
                            return;
                        }
                        else
                        {
                            goto fail;
                        }
                    }
                }
                catch (Exception c) { Resource.DatabasePath = dbname; exp = c; }
            fail:
            MessageBox.Show(exp?.Message ?? "", "Creation failed");
        }

        private void checkConnection_Click(object sender, EventArgs e)
        {
            if (!IsIpValid()) { MessageBox.Show("Invalid input"); return; }
            if (CheckConnection(out var c)) MessageBox.Show("Connection Created  Successfully");
            else MessageBox.Show(c?.Message, "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool CheckConnection(out Exception e)
        {
            saveData();
            var dbname = Resource.DatabasePath;
            Resource.DatabasePath = "";
            using (MySqlConnection x = new MySqlConnection(sql.SqlConnectionString.ConnectionString))
                try
                {
                    x.Open();
                    x.Ping();
                    e = null;
                    Resource.DatabasePath = dbname;
                    return true;
                }
                catch (Exception c) { e = c; Resource.DatabasePath = dbname; return false; }
        }
    }
}
