using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat
{
    public partial class LoginScreen : Form
    {
        public LoginScreen()
        {
            InitializeComponent();
            DisableTextboxContextMenu();
            xlblUsernameError.Hide();
        }

        private void DisableTextboxContextMenu()
        {
            xtxtbxUsername.ContextMenu = new ContextMenu();
        }

        private void XbtnHost_Click(object sender, EventArgs e)
        {
            if (CheckUsername() == true)
            {
                HolderForm.username = xtxtbxUsername.Text;
                HolderForm.hosting = true;
                ChatScreen chatScreen = new ChatScreen()
                {
                    MdiParent = this.ParentForm,
                    Dock = DockStyle.Fill
                };
                chatScreen.Show();
                this.Close();
            }
        }

        private void XbtnJoin_Click(object sender, EventArgs e)
        {
            if (CheckUsername() == true)
            {
                HolderForm.username = xtxtbxUsername.Text;
                HolderForm.hosting = false;
                EnterJoinIP enterJoinIP = new EnterJoinIP();
                DialogResult dialogResult = enterJoinIP.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    ChatScreen chatScreen = new ChatScreen()
                    {
                        MdiParent = this.ParentForm,
                        Dock = DockStyle.Fill
                    };
                    chatScreen.Show();
                    this.Close();
                }
                enterJoinIP.Close();
            }
        }

        private bool CheckUsername()
        {
            string username = xtxtbxUsername.Text;
            if (string.IsNullOrWhiteSpace(username))
            {
                xlblUsernameError.Show();
                xlblUsernameError.Text = "Please insert a username";
                return false;
            }
            return true;
        }

        private void xtxtbxUsername_TextChanged(object sender, EventArgs e)
        {
            xlblUsernameError.Hide();
        }
    }
}
