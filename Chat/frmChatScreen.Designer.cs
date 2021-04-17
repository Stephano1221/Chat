namespace Chat
{
    partial class FrmChatScreen
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
            this.xlblConnectedUsers = new System.Windows.Forms.Label();
            this.xlsvConnectedUsers = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.xtbxSendMessage = new System.Windows.Forms.TextBox();
            this.xlbxChat = new System.Windows.Forms.ListBox();
            this.xtlpInterface = new System.Windows.Forms.TableLayoutPanel();
            this.xbtnDisconnect = new System.Windows.Forms.Button();
            this.xtlpInterface.SuspendLayout();
            this.SuspendLayout();
            // 
            // xlblConnectedUsers
            // 
            this.xlblConnectedUsers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlblConnectedUsers.AutoSize = true;
            this.xlblConnectedUsers.Location = new System.Drawing.Point(3, 0);
            this.xlblConnectedUsers.Name = "xlblConnectedUsers";
            this.xlblConnectedUsers.Size = new System.Drawing.Size(244, 13);
            this.xlblConnectedUsers.TabIndex = 0;
            this.xlblConnectedUsers.Text = "Connected Users";
            this.xlblConnectedUsers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // xlsvConnectedUsers
            // 
            this.xlsvConnectedUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlsvConnectedUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.xlsvConnectedUsers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.xlsvConnectedUsers.HideSelection = false;
            this.xlsvConnectedUsers.Location = new System.Drawing.Point(3, 23);
            this.xlsvConnectedUsers.Name = "xlsvConnectedUsers";
            this.xtlpInterface.SetRowSpan(this.xlsvConnectedUsers, 2);
            this.xlsvConnectedUsers.Size = new System.Drawing.Size(244, 524);
            this.xlsvConnectedUsers.TabIndex = 1;
            this.xlsvConnectedUsers.UseCompatibleStateImageBehavior = false;
            this.xlsvConnectedUsers.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 25;
            // 
            // xtbxSendMessage
            // 
            this.xtbxSendMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtbxSendMessage.ForeColor = System.Drawing.Color.Gray;
            this.xtbxSendMessage.Location = new System.Drawing.Point(253, 503);
            this.xtbxSendMessage.MaxLength = 2000;
            this.xtbxSendMessage.Multiline = true;
            this.xtbxSendMessage.Name = "xtbxSendMessage";
            this.xtbxSendMessage.Size = new System.Drawing.Size(619, 44);
            this.xtbxSendMessage.TabIndex = 0;
            this.xtbxSendMessage.Text = "Enter a message...";
            this.xtbxSendMessage.Enter += new System.EventHandler(this.xtxtbxSendMessage_Enter);
            this.xtbxSendMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.xtxtbxSendMessage_KeyDown);
            this.xtbxSendMessage.Leave += new System.EventHandler(this.xtxtbxSendMessage_Leave);
            // 
            // xlbxChat
            // 
            this.xlbxChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xlbxChat.FormattingEnabled = true;
            this.xlbxChat.HorizontalScrollbar = true;
            this.xlbxChat.Location = new System.Drawing.Point(253, 23);
            this.xlbxChat.Name = "xlbxChat";
            this.xlbxChat.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.xlbxChat.Size = new System.Drawing.Size(619, 472);
            this.xlbxChat.TabIndex = 1;
            // 
            // xtlpInterface
            // 
            this.xtlpInterface.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtlpInterface.ColumnCount = 2;
            this.xtlpInterface.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.xtlpInterface.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.xtlpInterface.Controls.Add(this.xlsvConnectedUsers, 0, 1);
            this.xtlpInterface.Controls.Add(this.xtbxSendMessage, 1, 2);
            this.xtlpInterface.Controls.Add(this.xlblConnectedUsers, 0, 0);
            this.xtlpInterface.Controls.Add(this.xlbxChat, 1, 1);
            this.xtlpInterface.Location = new System.Drawing.Point(13, 38);
            this.xtlpInterface.Name = "xtlpInterface";
            this.xtlpInterface.RowCount = 3;
            this.xtlpInterface.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.xtlpInterface.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.xtlpInterface.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.xtlpInterface.Size = new System.Drawing.Size(875, 550);
            this.xtlpInterface.TabIndex = 2;
            // 
            // xbtnDisconnect
            // 
            this.xbtnDisconnect.Location = new System.Drawing.Point(13, 13);
            this.xbtnDisconnect.Name = "xbtnDisconnect";
            this.xbtnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.xbtnDisconnect.TabIndex = 3;
            this.xbtnDisconnect.Text = "Disconnect";
            this.xbtnDisconnect.UseVisualStyleBackColor = true;
            this.xbtnDisconnect.Click += new System.EventHandler(this.xbtnDisconnect_Click);
            // 
            // FrmChatScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.xbtnDisconnect);
            this.Controls.Add(this.xtlpInterface);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmChatScreen";
            this.Text = "ChatScreen";
            this.xtlpInterface.ResumeLayout(false);
            this.xtlpInterface.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label xlblConnectedUsers;
        private System.Windows.Forms.TextBox xtbxSendMessage;
        private System.Windows.Forms.ListBox xlbxChat;
        private System.Windows.Forms.ListView xlsvConnectedUsers;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.TableLayoutPanel xtlpInterface;
        private System.Windows.Forms.Button xbtnDisconnect;
    }
}