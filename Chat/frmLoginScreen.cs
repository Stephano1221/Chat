namespace Chat
{
    public partial class FrmLoginScreen : Form
    {
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
                    FrmChatScreen frmChatScreen = new FrmChatScreen
                    {
                        MdiParent = this.ParentForm,
                        Dock = DockStyle.Fill
                    };
                    frmChatScreen.Show();
                    this.Close();
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

        private void xtbxUsername_TextChanged(object sender, EventArgs e)
        {
            xlblUsernameError.Hide();
        }
    }
}
