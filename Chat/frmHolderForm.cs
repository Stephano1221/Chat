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
    public partial class frmHolderForm : Form
    { 
        public static string username;
        public static bool hosting;
        public static string joinIP;

        public frmHolderForm()
        {
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            IsMdiContainer = true;
            frmLoginScreen loginScreen = new frmLoginScreen
            {
                MdiParent = this,
                Dock = DockStyle.Fill
            };
            loginScreen.Show();
        }
    }
}
