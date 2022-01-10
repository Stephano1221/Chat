namespace Chat
{
    public partial class FrmHolder : Form
    {
        public static string username;
        public static bool hosting;
        public static string joinIP;
        public static uint clientId = 0;
        public static string applicationWindowText = "Chat";

        private FrmLoginScreen loginScreen;
        public static Network network;

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
