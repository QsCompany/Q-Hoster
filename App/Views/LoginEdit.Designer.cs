namespace QServer
{
    partial class LoginEdit
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.Pwd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Username = new System.Windows.Forms.TextBox();
            this.btnDiscart = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.Name = new System.Windows.Forms.TextBox();
            this._Name = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(21, 39);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(335, 154);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Authentification Form";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Controls.Add(this._Name);
            this.panel1.Controls.Add(this.Name);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.Pwd);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.Username);
            this.panel1.Location = new System.Drawing.Point(10, 23);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(319, 125);
            this.panel1.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "Mote de passe";
            // 
            // Pwd
            // 
            this.Pwd.Location = new System.Drawing.Point(124, 40);
            this.Pwd.Name = "Pwd";
            this.Pwd.Size = new System.Drawing.Size(192, 24);
            this.Pwd.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Nom d\'utilisateur";
            // 
            // Username
            // 
            this.Username.Location = new System.Drawing.Point(124, 8);
            this.Username.Name = "Username";
            this.Username.Size = new System.Drawing.Size(192, 24);
            this.Username.TabIndex = 1;
            // 
            // btnDiscart
            // 
            this.btnDiscart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDiscart.Location = new System.Drawing.Point(199, 199);
            this.btnDiscart.Name = "btnDiscart";
            this.btnDiscart.Size = new System.Drawing.Size(75, 23);
            this.btnDiscart.TabIndex = 9;
            this.btnDiscart.Text = "Discart";
            this.btnDiscart.UseVisualStyleBackColor = true;
            this.btnDiscart.Click += new System.EventHandler(this.btnDiscart_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(280, 199);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Enregistrer";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Name
            // 
            this.Name.Location = new System.Drawing.Point(124, 74);
            this.Name.Name = "Name";
            this.Name.Size = new System.Drawing.Size(192, 24);
            this.Name.TabIndex = 3;
            // 
            // _Name
            // 
            this._Name.AutoSize = true;
            this._Name.Location = new System.Drawing.Point(3, 77);
            this._Name.Name = "_Name";
            this._Name.Size = new System.Drawing.Size(61, 18);
            this._Name.TabIndex = 2;
            this._Name.Text = "Surnom";
            // 
            // LoginEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 234);
            this.Controls.Add(this.btnDiscart);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox1);
            base.Name = "LoginEdit";
            this.Text = "Edit";
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Pwd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Username;
        private System.Windows.Forms.Button btnDiscart;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label _Name;
        private System.Windows.Forms.TextBox Name;
    }
}