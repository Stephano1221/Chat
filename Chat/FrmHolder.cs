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
    public partial class FrmHolder : Form
    {
        public static string username;
        public static bool hosting;
        public static string joinIP;
        public static int clientId = -1;
        public static string applicationWindowText = "Chat";

        private FrmLoginScreen loginScreen;

        public FrmHolder()
        {
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            IsMdiContainer = true;
            loginScreen = new FrmLoginScreen
            {
                MdiParent = this,
                Dock = DockStyle.Fill
            };
            loginScreen.Show();
        }

        public void SetWindowText(string text)
        {
            this.Text = text;
        }
    }
}
