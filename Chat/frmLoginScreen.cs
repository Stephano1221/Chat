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
    public partial class frmLoginScreen : Form
    {
        public frmLoginScreen()
        {
            InitializeComponent();
            DisableTextboxContextMenu();
            xlblUsernameError.Hide();
        }

        private void DisableTextboxContextMenu()
        {
            xtxtbxUsername.ContextMenu = new ContextMenu();
        }

        private void xbtnHost_Click(object sender, EventArgs e)
        {
            if (CheckUsername() == true)
            {
                frmHolderForm.username = xtxtbxUsername.Text;
                frmHolderForm.hosting = true;
                frmChatScreen chatScreen = new frmChatScreen
                {
                    MdiParent = this.ParentForm,
                    Dock = DockStyle.Fill
                };
                chatScreen.Show();
                this.Close();
            }
        }

        private void xbtnJoin_Click(object sender, EventArgs e)
        {
            if (CheckUsername() == true)
            {
                frmHolderForm.username = xtxtbxUsername.Text;
                frmHolderForm.hosting = false;
                frmEnterJoinIP enterJoinIP = new frmEnterJoinIP();
                DialogResult dialogResult = enterJoinIP.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    frmChatScreen chatScreen = new frmChatScreen
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
