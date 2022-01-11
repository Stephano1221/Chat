//#define messageReceivedUpdates

namespace Chat
{
    public partial class FrmChatScreen : Form
    {
        private bool askToClose = true;

        private delegate void HeartbeatDelegate(Client client);
        private delegate void ClearClientClistDelegate();
        private delegate void AddClientToClientListDelegate(string username);
        private delegate void PrintChatMessageDelegate(string text);
        private delegate DialogResult ShowMessageBoxDelegate(string message, string caption, MessageBoxButtons messageBoxButtons, MessageBoxIcon messageBoxIcon);

        private delegate void OpenMainMenuDelegate();

        public FrmChatScreen()
        {
            InitializeComponent();
            SetProcessingEventHandlers();
            SetFormEventHandlers();
            xlsvConnectedUsers.Columns[0].Width = xlsvConnectedUsers.Width - 5;
        }

        private void SetFormEventHandlers()
        {
            this.Load += new EventHandler(FrmChatScreen_Load);
            this.FormClosing += new FormClosingEventHandler(OnClosing);
        }

        private void SetProcessingEventHandlers()
        {
            FrmHolder.processing.HeartbeatTimeoutEvent += OnHeartbeatTimeoutFailure;
            FrmHolder.processing.PrintChatMessageEvent += OnPrintChatMessage;
            FrmHolder.processing.ClearClientListEvent += OnClearClientList;
            FrmHolder.processing.AddClientToClientListEvent += OnAddClientToClientList;
            FrmHolder.processing.ShowMessageBoxEvent += OnShowMessagBox;
            FrmHolder.processing.OpenMainMenuEvent += OnOpenMainMenu;
        }

        private void ClearClientList()
        {
            xlsvConnectedUsers.Items.Clear();
        }

        private void AddClientToClientList(string username)
        {
            xlsvConnectedUsers.Items.Add(username);
        }

        private void FrmChatScreen_Load(object sender, EventArgs e)
        {
            SetParentFormWindowText(true);
        }

        private void SetParentFormWindowText(bool showConnectionInformation)
        {
            if (showConnectionInformation)
            {
                if (FrmHolder.hosting)
                {
                    this.ParentForm.Text = $"{FrmHolder.applicationWindowText} - {FrmHolder.username} hosting on {FrmHolder.processing.publicIp}";
                }
                else
                {
                    this.ParentForm.Text = $"{FrmHolder.applicationWindowText} - {FrmHolder.username} on {FrmHolder.joinIP}";
                }
            }
            else
            {
                this.ParentForm.Text = $"{FrmHolder.applicationWindowText}";
            }
        }

        private void PrintChatMessage(string chatMessage)
        {
            xlbxChat.Items.Add(chatMessage);
        }


        private DialogResult ShowMessageBox(string message, string caption, MessageBoxButtons messageBoxButtons, MessageBoxIcon messageBoxIcon)
        {
            message = message == null ? "" : message;
            caption = caption == null ? "" : caption;
            return DialogResult = MessageBox.Show(this, message, caption, messageBoxButtons, messageBoxIcon);
        }

        private void OpenMainMenu()
        {
            askToClose = false;
            FrmLoginScreen frmLoginScreen = new FrmLoginScreen
            {
                MdiParent = this.ParentForm,
                Dock = DockStyle.Fill
            };
            SetParentFormWindowText(false);
            frmLoginScreen.Show();
            this.Close();
        }

        #region Event handlers
        private void OnHeartbeatTimeoutFailure(object sender, Client client)
        {
            if (xlbxChat.InvokeRequired)
            {
                xlbxChat.BeginInvoke(new HeartbeatDelegate(FrmHolder.processing.HeartbeatTimeoutFailure), client);
            }
        }

        private void OnClearClientList(object sender, EventArgs e)
        {
            if (xlsvConnectedUsers.InvokeRequired)
            {
                xlsvConnectedUsers.BeginInvoke(new ClearClientClistDelegate(ClearClientList));
            }
            else
            {
                xlsvConnectedUsers.Items.Clear();
            }
        }

        private void OnAddClientToClientList(object sender, string username)
        {
            if (xlsvConnectedUsers.InvokeRequired)
            {
                xlsvConnectedUsers.BeginInvoke(new AddClientToClientListDelegate(AddClientToClientList), username);
            }
            else
            {
                xlsvConnectedUsers.Items.Add(username);
            }
        }

        private void OnPrintChatMessage(object sender, string text)
        {
            if (xlbxChat.InvokeRequired)
            {
                xlbxChat.BeginInvoke(new PrintChatMessageDelegate(PrintChatMessage), text);
            }
            else
            {
                xlbxChat.Items.Add(text);
            }
        }

        private void OnShowMessagBox(object sender, ShowMessageBoxEventArgs showMessageBoxEventArgs)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ShowMessageBoxDelegate(ShowMessageBox), showMessageBoxEventArgs.message, showMessageBoxEventArgs.caption, showMessageBoxEventArgs.messageBoxButtons, showMessageBoxEventArgs.messageBoxIcon);
            }
            else
            {
                ShowMessageBox(showMessageBoxEventArgs.message, showMessageBoxEventArgs.caption, showMessageBoxEventArgs.messageBoxButtons, showMessageBoxEventArgs.messageBoxIcon);
            }
        }

        private void OnOpenMainMenu(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new OpenMainMenuDelegate(OpenMainMenu));
            }
            else
            {
                OpenMainMenu();
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (askToClose) // Prevents closing when returning to main menu
            {
                if (FrmHolder.processing.BeginDisconnect() == false)
                {
                    e.Cancel = true;
                }
            }
        }
        #endregion

        #region Form Event Handlers
        private void xtxtbxSendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string message = xtbxSendMessage.Text;
                message = message.Trim();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (FrmHolder.hosting || message[0] == '/')
                    {
                        PrintChatMessage($"{FrmHolder.username}: {message}");
                    }
                    if (FrmHolder.processing.ProcessCommand(message) == false)
                    {
                        if (FrmHolder.processing.connectedClients.Count > 0)
                        {
                            message = message.Trim();
                            for (int i = 0; i < FrmHolder.processing.connectedClients.Count; i++)
                            {
                                FrmHolder.processing.BeginWrite(FrmHolder.processing.connectedClients[i], FrmHolder.processing.ComposeMessage(FrmHolder.processing.connectedClients[i], 0, Message.MessageTypes.ChatMessage, $"{FrmHolder.username} {message}", null));
                            }
                        }
                    }
                    xtbxSendMessage.Clear();
                }
                e.SuppressKeyPress = true;
            }
        }

        private void xtxtbxSendMessage_Enter(object sender, EventArgs e)
        {
            if (xtbxSendMessage.ForeColor == Color.Gray)
            {
                xtbxSendMessage.ForeColor = Color.Black;
                xtbxSendMessage.Clear();
            }
        }

        private void xtxtbxSendMessage_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(xtbxSendMessage.Text))
            {
                xtbxSendMessage.ForeColor = Color.Gray;
                xtbxSendMessage.Text = "Enter a message...";
            }
        }

        private void xbtnDisconnect_Click(object sender, EventArgs e)
        {
            FrmHolder.processing.BeginDisconnect();
        }
        #endregion
    }
}