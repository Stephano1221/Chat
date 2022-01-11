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
    public partial class frmConnecting : Form
    {
        public frmConnecting()
        {
            InitializeComponent();
            SetIp(null);
        }

        public void SetIp(string serverName)
        {
            if (serverName == null)
            {
                xlblServerName.Text = FrmHolder.joinIP;
            }
            else
            {
                xlblServerName.Text = serverName;
            }
        }
    }
}
