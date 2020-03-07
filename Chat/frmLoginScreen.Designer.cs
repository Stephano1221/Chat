namespace Chat
{
    partial class FrmLoginScreen
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
            this.xtlpUsername = new System.Windows.Forms.TableLayoutPanel();
            this.xbtnJoin = new System.Windows.Forms.Button();
            this.xtxtbxUsername = new System.Windows.Forms.TextBox();
            this.xbtnHost = new System.Windows.Forms.Button();
            this.xlblUsername = new System.Windows.Forms.Label();
            this.xlblUsernameError = new System.Windows.Forms.Label();
            this.xtlpUsername.SuspendLayout();
            this.SuspendLayout();
            // 
            // xtlpUsername
            // 
            this.xtlpUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtlpUsername.ColumnCount = 2;
            this.xtlpUsername.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.xtlpUsername.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.xtlpUsername.Controls.Add(this.xbtnJoin, 1, 4);
            this.xtlpUsername.Controls.Add(this.xtxtbxUsername, 0, 2);
            this.xtlpUsername.Controls.Add(this.xbtnHost, 0, 4);
            this.xtlpUsername.Controls.Add(this.xlblUsername, 0, 1);
            this.xtlpUsername.Controls.Add(this.xlblUsernameError, 0, 3);
            this.xtlpUsername.Location = new System.Drawing.Point(94, 227);
            this.xtlpUsername.Name = "xtlpUsername";
            this.xtlpUsername.RowCount = 7;
            this.xtlpUsername.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.xtlpUsername.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.xtlpUsername.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.xtlpUsername.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.xtlpUsername.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.xtlpUsername.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.xtlpUsername.Size = new System.Drawing.Size(716, 194);
            this.xtlpUsername.TabIndex = 0;
            // 
            // xbtnJoin
            // 
            this.xbtnJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xbtnJoin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xbtnJoin.Location = new System.Drawing.Point(361, 73);
            this.xbtnJoin.Name = "xbtnJoin";
            this.xbtnJoin.Size = new System.Drawing.Size(352, 118);
            this.xbtnJoin.TabIndex = 1;
            this.xbtnJoin.Text = "Join";
            this.xbtnJoin.UseVisualStyleBackColor = true;
            this.xbtnJoin.Click += new System.EventHandler(this.xbtnJoin_Click);
            // 
            // xtxtbxUsername
            // 
            this.xtxtbxUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtlpUsername.SetColumnSpan(this.xtxtbxUsername, 2);
            this.xtxtbxUsername.Location = new System.Drawing.Point(3, 23);
            this.xtxtbxUsername.Name = "xtxtbxUsername";
            this.xtxtbxUsername.Size = new System.Drawing.Size(710, 20);
            this.xtxtbxUsername.TabIndex = 1;
            this.xtxtbxUsername.TextChanged += new System.EventHandler(this.xtxtbxUsername_TextChanged);
            // 
            // xbtnHost
            // 
            this.xbtnHost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xbtnHost.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xbtnHost.Location = new System.Drawing.Point(3, 73);
            this.xbtnHost.Name = "xbtnHost";
            this.xbtnHost.Size = new System.Drawing.Size(352, 118);
            this.xbtnHost.TabIndex = 0;
            this.xbtnHost.Text = "Host";
            this.xbtnHost.UseVisualStyleBackColor = true;
            this.xbtnHost.Click += new System.EventHandler(this.xbtnHost_Click);
            // 
            // xlblUsername
            // 
            this.xlblUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlblUsername.AutoSize = true;
            this.xtlpUsername.SetColumnSpan(this.xlblUsername, 2);
            this.xlblUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xlblUsername.Location = new System.Drawing.Point(3, 0);
            this.xlblUsername.Name = "xlblUsername";
            this.xlblUsername.Size = new System.Drawing.Size(710, 20);
            this.xlblUsername.TabIndex = 0;
            this.xlblUsername.Text = "Username";
            this.xlblUsername.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // xlblUsernameError
            // 
            this.xlblUsernameError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlblUsernameError.AutoSize = true;
            this.xtlpUsername.SetColumnSpan(this.xlblUsernameError, 2);
            this.xlblUsernameError.ForeColor = System.Drawing.Color.Red;
            this.xlblUsernameError.Location = new System.Drawing.Point(3, 45);
            this.xlblUsernameError.Name = "xlblUsernameError";
            this.xlblUsernameError.Size = new System.Drawing.Size(710, 25);
            this.xlblUsernameError.TabIndex = 2;
            this.xlblUsernameError.Text = "Error";
            // 
            // frmLoginScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.xtlpUsername);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmLoginScreen";
            this.Text = "LoginScreen";
            this.xtlpUsername.ResumeLayout(false);
            this.xtlpUsername.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel xtlpUsername;
        private System.Windows.Forms.TextBox xtxtbxUsername;
        private System.Windows.Forms.Label xlblUsername;
        private System.Windows.Forms.Button xbtnJoin;
        private System.Windows.Forms.Button xbtnHost;
        private System.Windows.Forms.Label xlblUsernameError;
    }
}