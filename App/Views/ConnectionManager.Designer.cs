namespace QServer
{
    partial class ConnectionManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionManager));
            this.btnDiscart = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ServerPort = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.DatabaseName = new System.Windows.Forms.TextBox();
            this._Name = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.UserID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ServerIP = new System.Windows.Forms.TextBox();
            this.btnReload = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnCreateStock = new System.Windows.Forms.Button();
            this.checkConnection = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ServerPort)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDiscart
            // 
            resources.ApplyResources(this.btnDiscart, "btnDiscart");
            this.btnDiscart.Name = "btnDiscart";
            this.btnDiscart.UseVisualStyleBackColor = true;
            this.btnDiscart.Click += new System.EventHandler(this.btnDiscart_Click);
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.btnCreateStock);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.btnDiscart);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.DatabaseName);
            this.panel1.Controls.Add(this.ServerPort);
            this.panel1.Controls.Add(this.btnReload);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this._Name);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.Password);
            this.panel1.Controls.Add(this.UserID);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.ServerIP);
            this.panel1.Name = "panel1";
            // 
            // ServerPort
            // 
            resources.ApplyResources(this.ServerPort, "ServerPort");
            this.ServerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.ServerPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ServerPort.Name = "ServerPort";
            this.ServerPort.Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // DatabaseName
            // 
            resources.ApplyResources(this.DatabaseName, "DatabaseName");
            this.DatabaseName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.DatabaseName.Name = "DatabaseName";
            this.DatabaseName.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // _Name
            // 
            resources.ApplyResources(this._Name, "_Name");
            this._Name.Name = "_Name";
            // 
            // Password
            // 
            resources.ApplyResources(this.Password, "Password");
            this.Password.Name = "Password";
            // 
            // UserID
            // 
            resources.ApplyResources(this.UserID, "UserID");
            this.UserID.Name = "UserID";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // ServerIP
            // 
            resources.ApplyResources(this.ServerIP, "ServerIP");
            this.ServerIP.Name = "ServerIP";
            this.ServerIP.TextChanged += new System.EventHandler(this.ServerIP_TextChanged);
            // 
            // btnReload
            // 
            resources.ApplyResources(this.btnReload, "btnReload");
            this.btnReload.Name = "btnReload";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnCreateStock
            // 
            resources.ApplyResources(this.btnCreateStock, "btnCreateStock");
            this.btnCreateStock.Name = "btnCreateStock";
            this.btnCreateStock.UseVisualStyleBackColor = true;
            this.btnCreateStock.Click += new System.EventHandler(this.btnCreateStock_Click);
            // 
            // checkConnection
            // 
            resources.ApplyResources(this.checkConnection, "checkConnection");
            this.checkConnection.Name = "checkConnection";
            this.checkConnection.UseVisualStyleBackColor = true;
            this.checkConnection.Click += new System.EventHandler(this.checkConnection_Click);
            // 
            // ConnectionManager
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.checkConnection);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ConnectionManager";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ServerPort)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDiscart;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label _Name;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.TextBox UserID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ServerIP;
        private System.Windows.Forms.NumericUpDown ServerPort;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox DatabaseName;
        private System.Windows.Forms.Button btnCreateStock;
        private System.Windows.Forms.Button checkConnection;
    }
}