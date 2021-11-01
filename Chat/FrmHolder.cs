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
        public static string applicationVersion = "0.1.0-alpha"; //Format: [Major].[Minor].[Patch](optional:-[Pre-Release]). Follows semantic versioning 2.0.0. To use versions from the assembly (i.e. AssemblyInfo.cs): Application.ProductVersion;
        public static string minimumSupportedClientVersion = "0.1.0-alpha"; //Format: [Major].[Minor].[Patch](optional:-[Pre-Release]). Follows semantic versioning 2.0.0. As a server, clients connecting must be of version equal to or greater than this.
        public static string maximumSupportedClientVersion = minimumSupportedClientVersion;
        public static bool allowClientPreRelease = true;
        public static string minumumSupportedServerVersion = "0.1.0"; //Format: [Major].[Minor].[Patch](optional:-[Pre-Release]). Follows semantic versioning 2.0.0. As a client, servers connecting must be of version equal to or greater than this.
        public static string maximumSupportedServerVersion = minumumSupportedServerVersion;
        public static bool allowServerPreRelease = true;

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
