namespace Chat
{
    partial class frmManageRanks
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
            this.xlblPermissions = new System.Windows.Forms.Label();
            this.xlblRanks = new System.Windows.Forms.Label();
            this.xbtnRemoveRank = new System.Windows.Forms.Button();
            this.xbtnAddRank = new System.Windows.Forms.Button();
            this.xbtnRankPromote = new System.Windows.Forms.Button();
            this.xbtnRankDemote = new System.Windows.Forms.Button();
            this.xlsvRanks = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.xtbxName = new System.Windows.Forms.TextBox();
            this.xlsvPermissions = new System.Windows.Forms.ListView();
            this.name = new System.Windows.Forms.ColumnHeader();
            this.description = new System.Windows.Forms.ColumnHeader();
            this.xbtnBack = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.xlblPermissions, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.xlblRanks, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.xbtnRemoveRank, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.xbtnAddRank, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.xbtnRankPromote, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.xbtnRankDemote, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.xlsvRanks, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.xtbxName, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.xlsvPermissions, 4, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 38);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.99999F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(876, 550);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // xlblPermissions
            // 
            this.xlblPermissions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlblPermissions.AutoSize = true;
            this.xlblPermissions.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.xlblPermissions.Location = new System.Drawing.Point(353, 0);
            this.xlblPermissions.Name = "xlblPermissions";
            this.xlblPermissions.Size = new System.Drawing.Size(520, 30);
            this.xlblPermissions.TabIndex = 1;
            this.xlblPermissions.Text = "Permissions";
            this.xlblPermissions.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // xlblRanks
            // 
            this.xlblRanks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlblRanks.AutoSize = true;
            this.xlblRanks.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.xlblRanks.Location = new System.Drawing.Point(153, 0);
            this.xlblRanks.Name = "xlblRanks";
            this.xlblRanks.Size = new System.Drawing.Size(94, 30);
            this.xlblRanks.TabIndex = 3;
            this.xlblRanks.Text = "Ranks";
            this.xlblRanks.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // xbtnRemoveRank
            // 
            this.xbtnRemoveRank.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.xbtnRemoveRank.Location = new System.Drawing.Point(53, 4);
            this.xbtnRemoveRank.Name = "xbtnRemoveRank";
            this.xbtnRemoveRank.Size = new System.Drawing.Size(75, 23);
            this.xbtnRemoveRank.TabIndex = 4;
            this.xbtnRemoveRank.Text = "−";
            this.xbtnRemoveRank.UseVisualStyleBackColor = true;
            this.xbtnRemoveRank.Click += new System.EventHandler(this.xbtnRemoveRank_Click);
            // 
            // xbtnAddRank
            // 
            this.xbtnAddRank.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.xbtnAddRank.Location = new System.Drawing.Point(272, 4);
            this.xbtnAddRank.Name = "xbtnAddRank";
            this.xbtnAddRank.Size = new System.Drawing.Size(75, 23);
            this.xbtnAddRank.TabIndex = 5;
            this.xbtnAddRank.Text = "+";
            this.xbtnAddRank.UseVisualStyleBackColor = true;
            this.xbtnAddRank.Click += new System.EventHandler(this.xbtnAddRank_Click);
            // 
            // xbtnRankPromote
            // 
            this.xbtnRankPromote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.xbtnRankPromote.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.xbtnRankPromote.Location = new System.Drawing.Point(3, 278);
            this.xbtnRankPromote.Name = "xbtnRankPromote";
            this.xbtnRankPromote.Size = new System.Drawing.Size(44, 23);
            this.xbtnRankPromote.TabIndex = 7;
            this.xbtnRankPromote.Text = "^";
            this.xbtnRankPromote.UseVisualStyleBackColor = true;
            this.xbtnRankPromote.Click += new System.EventHandler(this.xbtnRankPromote_Click);
            // 
            // xbtnRankDemote
            // 
            this.xbtnRankDemote.Location = new System.Drawing.Point(3, 307);
            this.xbtnRankDemote.Name = "xbtnRankDemote";
            this.xbtnRankDemote.Size = new System.Drawing.Size(44, 23);
            this.xbtnRankDemote.TabIndex = 8;
            this.xbtnRankDemote.Text = "v";
            this.xbtnRankDemote.UseVisualStyleBackColor = true;
            this.xbtnRankDemote.Click += new System.EventHandler(this.xbtnRankDemote_Click);
            // 
            // xlsvRanks
            // 
            this.xlsvRanks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlsvRanks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.tableLayoutPanel1.SetColumnSpan(this.xlsvRanks, 3);
            this.xlsvRanks.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.xlsvRanks.Location = new System.Drawing.Point(53, 33);
            this.xlsvRanks.MultiSelect = false;
            this.xlsvRanks.Name = "xlsvRanks";
            this.tableLayoutPanel1.SetRowSpan(this.xlsvRanks, 3);
            this.xlsvRanks.Size = new System.Drawing.Size(294, 514);
            this.xlsvRanks.TabIndex = 6;
            this.xlsvRanks.UseCompatibleStateImageBehavior = false;
            this.xlsvRanks.View = System.Windows.Forms.View.List;
            this.xlsvRanks.SelectedIndexChanged += new System.EventHandler(this.xlsvRanks_SelectedIndexChanged);
            // 
            // xtbxName
            // 
            this.xtbxName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtbxName.Location = new System.Drawing.Point(353, 33);
            this.xtbxName.MaxLength = 100;
            this.xtbxName.Name = "xtbxName";
            this.xtbxName.Size = new System.Drawing.Size(520, 23);
            this.xtbxName.TabIndex = 9;
            this.xtbxName.TextChanged += new System.EventHandler(this.xtbxName_TextChanged);
            this.xtbxName.Leave += new System.EventHandler(this.xtbxName_Leave);
            // 
            // xlsvPermissions
            // 
            this.xlsvPermissions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlsvPermissions.CheckBoxes = true;
            this.xlsvPermissions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.name,
            this.description});
            this.xlsvPermissions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.xlsvPermissions.Location = new System.Drawing.Point(353, 63);
            this.xlsvPermissions.MultiSelect = false;
            this.xlsvPermissions.Name = "xlsvPermissions";
            this.tableLayoutPanel1.SetRowSpan(this.xlsvPermissions, 2);
            this.xlsvPermissions.Size = new System.Drawing.Size(520, 484);
            this.xlsvPermissions.TabIndex = 10;
            this.xlsvPermissions.UseCompatibleStateImageBehavior = false;
            this.xlsvPermissions.View = System.Windows.Forms.View.Details;
            this.xlsvPermissions.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.xlsvPermissions_ItemCheck);
            // 
            // name
            // 
            this.name.Text = "Name";
            // 
            // description
            // 
            this.description.Text = "Description";
            // 
            // xbtnBack
            // 
            this.xbtnBack.Location = new System.Drawing.Point(15, 9);
            this.xbtnBack.Name = "xbtnBack";
            this.xbtnBack.Size = new System.Drawing.Size(75, 23);
            this.xbtnBack.TabIndex = 1;
            this.xbtnBack.Text = "Back";
            this.xbtnBack.UseVisualStyleBackColor = true;
            this.xbtnBack.Click += new System.EventHandler(this.xbtnBack_Click);
            // 
            // frmManageRanks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.xbtnBack);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmManageRanks";
            this.Text = "ManageRanks";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label xlblPermissions;
        private Label xlblRanks;
        private Button xbtnBack;
        private Button xbtnRemoveRank;
        private Button xbtnAddRank;
        private ListView xlsvRanks;
        private ColumnHeader columnHeader1;
        private Button xbtnRankPromote;
        private Button xbtnRankDemote;
        private TextBox xtbxName;
        private ListView xlsvPermissions;
        private ColumnHeader name;
        private ColumnHeader description;
    }
}