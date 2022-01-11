namespace Chat
{
    public partial class FrmEnterJoinIp : Form
    {
        public string ip;

        public FrmEnterJoinIp()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            xlblError.Hide();
        }

        private void xbtnJoin_Click(object sender, EventArgs e)
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

        private void xtbxIp_TextChanged(object sender, EventArgs e)
        {
            xlblError.Hide();
        }

        private bool CheckIP()
        {
            int count = 0;
            ip = xtbxIp.Text;
            foreach (char c in ip)
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
