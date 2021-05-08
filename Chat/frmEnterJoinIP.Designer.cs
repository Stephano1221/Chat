namespace Chat
{
    partial class FrmEnterJoinIp
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
            this.xlblIpAddress = new System.Windows.Forms.Label();
            this.xbtnJoin = new System.Windows.Forms.Button();
            this.xtbxIp = new System.Windows.Forms.TextBox();
            this.xlblError = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.xlblIpAddress, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.xbtnJoin, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.xtbxIp, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.xlblError, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 15);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(256, 134);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // xlblIpAddress
            // 
            this.xlblIpAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlblIpAddress.AutoSize = true;
            this.xlblIpAddress.Location = new System.Drawing.Point(4, 0);
            this.xlblIpAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.xlblIpAddress.Name = "xlblIpAddress";
            this.xlblIpAddress.Size = new System.Drawing.Size(248, 35);
            this.xlblIpAddress.TabIndex = 0;
            this.xlblIpAddress.Text = "IP Address";
            this.xlblIpAddress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // xbtnJoin
            // 
            this.xbtnJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xbtnJoin.Location = new System.Drawing.Point(4, 96);
            this.xbtnJoin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.xbtnJoin.Name = "xbtnJoin";
            this.xbtnJoin.Size = new System.Drawing.Size(248, 35);
            this.xbtnJoin.TabIndex = 2;
            this.xbtnJoin.Text = "Join";
            this.xbtnJoin.UseVisualStyleBackColor = true;
            this.xbtnJoin.Click += new System.EventHandler(this.xbtnJoin_Click);
            // 
            // xtbxIp
            // 
            this.xtbxIp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtbxIp.Location = new System.Drawing.Point(4, 38);
            this.xtbxIp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.xtbxIp.Name = "xtbxIp";
            this.xtbxIp.Size = new System.Drawing.Size(248, 23);
            this.xtbxIp.TabIndex = 1;
            this.xtbxIp.TextChanged += new System.EventHandler(this.xtbxIp_TextChanged);
            // 
            // xlblError
            // 
            this.xlblError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.xlblError.AutoSize = true;
            this.xlblError.ForeColor = System.Drawing.Color.Red;
            this.xlblError.Location = new System.Drawing.Point(4, 58);
            this.xlblError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.xlblError.Name = "xlblError";
            this.xlblError.Size = new System.Drawing.Size(32, 35);
            this.xlblError.TabIndex = 3;
            this.xlblError.Text = "Error";
            this.xlblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrmEnterJoinIp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 161);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "FrmEnterJoinIp";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "IP Address";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label xlblIpAddress;
        private System.Windows.Forms.TextBox xtbxIp;
        private System.Windows.Forms.Button xbtnJoin;
        private System.Windows.Forms.Label xlblError;
    }
}