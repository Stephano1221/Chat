namespace Chat
{
    partial class frmEnterJoinIP
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.xlblIPAddress = new System.Windows.Forms.Label();
            this.xbtnJoin = new System.Windows.Forms.Button();
            this.xtxtbxIP = new System.Windows.Forms.TextBox();
            this.xlblError = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.xlblIPAddress, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.xbtnJoin, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.xtxtbxIP, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.xlblError, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 13);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(259, 136);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // xlblIPAddress
            // 
            this.xlblIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlblIPAddress.AutoSize = true;
            this.xlblIPAddress.Location = new System.Drawing.Point(3, 0);
            this.xlblIPAddress.Name = "xlblIPAddress";
            this.xlblIPAddress.Size = new System.Drawing.Size(253, 30);
            this.xlblIPAddress.TabIndex = 0;
            this.xlblIPAddress.Text = "IP Address";
            this.xlblIPAddress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // xbtnJoin
            // 
            this.xbtnJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xbtnJoin.Location = new System.Drawing.Point(3, 83);
            this.xbtnJoin.Name = "xbtnJoin";
            this.xbtnJoin.Size = new System.Drawing.Size(253, 50);
            this.xbtnJoin.TabIndex = 2;
            this.xbtnJoin.Text = "Join";
            this.xbtnJoin.UseVisualStyleBackColor = true;
            this.xbtnJoin.Click += new System.EventHandler(this.XbtnJoin_Click);
            // 
            // xtxtbxIP
            // 
            this.xtxtbxIP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtxtbxIP.Location = new System.Drawing.Point(3, 33);
            this.xtxtbxIP.Name = "xtxtbxIP";
            this.xtxtbxIP.Size = new System.Drawing.Size(253, 20);
            this.xtxtbxIP.TabIndex = 1;
            this.xtxtbxIP.TextChanged += new System.EventHandler(this.xtxtbxIP_TextChanged);
            // 
            // xlblError
            // 
            this.xlblError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.xlblError.AutoSize = true;
            this.xlblError.ForeColor = System.Drawing.Color.Red;
            this.xlblError.Location = new System.Drawing.Point(3, 50);
            this.xlblError.Name = "xlblError";
            this.xlblError.Size = new System.Drawing.Size(29, 30);
            this.xlblError.TabIndex = 3;
            this.xlblError.Text = "Error";
            this.xlblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EnterJoinIP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 161);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "EnterJoinIP";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter IP";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label xlblIPAddress;
        private System.Windows.Forms.TextBox xtxtbxIP;
        private System.Windows.Forms.Button xbtnJoin;
        private System.Windows.Forms.Label xlblError;
    }
}