namespace Chat
{
    partial class frmConnecting
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
            this.xlblConnectingTo = new System.Windows.Forms.Label();
            this.xlblServerName = new System.Windows.Forms.Label();
            this.xtlpConnectingTo = new System.Windows.Forms.TableLayoutPanel();
            this.xtlpConnectingTo.SuspendLayout();
            this.SuspendLayout();
            // 
            // xlblConnectingTo
            // 
            this.xlblConnectingTo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlblConnectingTo.AutoSize = true;
            this.xlblConnectingTo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.xlblConnectingTo.Location = new System.Drawing.Point(3, 0);
            this.xlblConnectingTo.Name = "xlblConnectingTo";
            this.xlblConnectingTo.Size = new System.Drawing.Size(794, 250);
            this.xlblConnectingTo.TabIndex = 0;
            this.xlblConnectingTo.Text = "Connecting to";
            this.xlblConnectingTo.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // xlblServerName
            // 
            this.xlblServerName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlblServerName.AutoSize = true;
            this.xlblServerName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.xlblServerName.Location = new System.Drawing.Point(3, 250);
            this.xlblServerName.Name = "xlblServerName";
            this.xlblServerName.Size = new System.Drawing.Size(794, 250);
            this.xlblServerName.TabIndex = 1;
            this.xlblServerName.Text = "Server Name";
            this.xlblServerName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // xtlpConnectingTo
            // 
            this.xtlpConnectingTo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtlpConnectingTo.ColumnCount = 1;
            this.xtlpConnectingTo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.xtlpConnectingTo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.xtlpConnectingTo.Controls.Add(this.xlblServerName, 0, 1);
            this.xtlpConnectingTo.Controls.Add(this.xlblConnectingTo, 0, 0);
            this.xtlpConnectingTo.Location = new System.Drawing.Point(50, 50);
            this.xtlpConnectingTo.Name = "xtlpConnectingTo";
            this.xtlpConnectingTo.RowCount = 2;
            this.xtlpConnectingTo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.xtlpConnectingTo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.xtlpConnectingTo.Size = new System.Drawing.Size(800, 500);
            this.xtlpConnectingTo.TabIndex = 2;
            // 
            // Connecting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.xtlpConnectingTo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Connecting";
            this.Text = "Connecting";
            this.xtlpConnectingTo.ResumeLayout(false);
            this.xtlpConnectingTo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Label xlblConnectingTo;
        private Label xlblServerName;
        private TableLayoutPanel xtlpConnectingTo;
    }
}