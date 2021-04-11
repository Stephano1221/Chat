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
    public partial class FrmEnterJoinIp : Form
    {
        public FrmEnterJoinIp()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            xlblError.Hide();
        }

        private void XbtnJoin_Click(object sender, EventArgs e)
        {
            if (CheckIP())
            {
                this.DialogResult = DialogResult.OK;
                FrmHolder.joinIP = xtbxIp.Text;
                this.Close();
            }
            else
            {
                xlblError.Text = "Please enter a valid IP address";
                xlblError.Show();
            }
        }

        private void xtxtbxIP_TextChanged(object sender, EventArgs e)
        {
            xlblError.Hide();
        }

        private bool CheckIP()
        {
            int count = 0;
            foreach (char c in xtbxIp.Text)
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
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
