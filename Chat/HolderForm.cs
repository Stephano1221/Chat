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
    public partial class HolderForm : Form
    { 
        public static string username;
        public static bool hosting;
        public static string joinIP;

        public HolderForm()
        {
            InitializeComponent();
            LoginScreen loginScreen = new LoginScreen()
            {
                MdiParent = this,
                Dock = DockStyle.Fill
            };
            loginScreen.Show();
        }
    }
}
