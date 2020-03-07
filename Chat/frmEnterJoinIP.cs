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
    public partial class FrmEnterJoinIP : Form
    {
        public FrmEnterJoinIP()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            xlblError.Hide();
        }

        private void XbtnJoin_Click(object sender, EventArgs e)
        {
            CheckIP();
        }

        private void xtxtbxIP_TextChanged(object sender, EventArgs e)
        {
            xlblError.Hide();
        }

        private void CheckIP()
        {
            int count = 0;
            foreach (char c in xtxtbxIP.Text)
            {
                if (c == '.')
                {
                    count++;
                }
                else if (!(char.IsDigit(c)))
                {
                    count = 0;
                    break;
                }
            }
            if (count == 3)
            {
                this.DialogResult = DialogResult.OK;
                FrmHolder.joinIP = xtxtbxIP.Text;
                this.Close();
            }
            else
            {
                xlblError.Text = "Please enter a valid IP address";
                xlblError.Show();
                return;
            }
        }
    }
}
