namespace Chat
{
    public partial class FrmLoginScreen : Form
    {
        private delegate void FirstConnectionAttemptResultDelegate(FirstConnectionAttemptResultEventArgs firstConnectionAttemptResultEventArgs);
        frmConnecting frmConnecting;

        public FrmLoginScreen()
        {
            InitializeComponent();
            DisableTextboxContextMenu();
            xlblUsernameError.Hide();
        }

        private void DisableTextboxContextMenu()
        {
            xtbxUsername.ContextMenuStrip = new ContextMenuStrip();
        }

        private void xbtnHost_Click(object sender, EventArgs e)
        {
            if (CheckUsername() == true)
            {
                FrmHolder.username = xtbxUsername.Text;
                FrmHolder.hosting = true;
                FrmHolder.processing = new Processing();
                FrmChatScreen frmLoginScreen = new FrmChatScreen
                {
                    MdiParent = this.ParentForm,
                    Dock = DockStyle.Fill
                };
                frmLoginScreen.Show();
                this.Close();
            }
        }

        private void xbtnJoin_Click(object sender, EventArgs e)
        {
            if (CheckUsername() == true)
            {
                FrmHolder.username = xtbxUsername.Text;
                FrmHolder.hosting = false;
                FrmEnterJoinIp frmEnterJoinIp = new FrmEnterJoinIp();
                DialogResult dialogResult = frmEnterJoinIp.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    FrmHolder.processing = new Processing();
                    FrmHolder.processing.FirstConnectionAttemptResultEvent += OnFirstConnectionAttemptResult;
                    frmConnecting = new frmConnecting
                    {
                        MdiParent = this.ParentForm,
                        Dock = DockStyle.Fill
                    };
                    frmConnecting.Show();
                }
                frmEnterJoinIp.Close();
            }
        }

        private bool CheckUsername()
        {
            string username = xtbxUsername.Text;
            if (string.IsNullOrWhiteSpace(username))
            {
                xlblUsernameError.Show();
                xlblUsernameError.Text = "Please insert a username";
                return false;
            }
            for (int i = 0; i < username.Length; i++)
            {
                if (username[i] == ' ')
                {
                    xlblUsernameError.Show();
                    xlblUsernameError.Text = "Username cannot contain spaces";
                    return false;
                }
            }
            return true;
        }

        private void FirstConnectionAttemptResult(FirstConnectionAttemptResultEventArgs e)
        {
            if (e.firstConnectionAttemptResult == false)
            {
                if (e.message != null)
                {
                    MessageBox.Show(e.message, e.caption, e.messageBoxButtons, e.messageBoxIcon);
                }
            }
            else
            {
                FrmHolder.chatScreen = new FrmChatScreen
                {
                    MdiParent = this.ParentForm,
                    Dock = DockStyle.Fill
                };
                FrmHolder.chatScreen.Show();
            }
            frmConnecting.Close();
        }

        private void xtbxUsername_TextChanged(object sender, EventArgs e)
        {
            xlblUsernameError.Hide();
        }
        private void OnFirstConnectionAttemptResult(object sender, FirstConnectionAttemptResultEventArgs firstConnectionAttemptResultEventArgs)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new FirstConnectionAttemptResultDelegate(FirstConnectionAttemptResult), firstConnectionAttemptResultEventArgs);
            }
            else
            {
                FirstConnectionAttemptResult(firstConnectionAttemptResultEventArgs);
            }
        }
    }
}
