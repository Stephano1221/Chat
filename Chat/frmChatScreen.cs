using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Chat
{
    public partial class frmChatScreen : Form
    {
        private int port = 12210;
        private bool connected;
        private ThreadStart ts;
        private Thread thread;
        private bool hosting;
        private string response;
        private string publicIp;
        private string localIp;
        private List<string> connectedClients = new List<string>();

        public frmChatScreen()
        {
            InitializeComponent();
            CheckHost();
        }

        private void CheckHost()
        {
            publicIp = new WebClient().DownloadString("https://ipv4.icanhazip.com/");
            publicIp = publicIp.TrimEnd();
            localIp = GetLocalIp();
            if (frmHolderForm.hosting == true)
            {
                ts = new ThreadStart(StartServer);
                thread = new Thread(ts);
                thread.Start();
                Thread.Sleep(100);
                xlbxChat.Items.Add($"Server started on: {publicIp}");
            }
            else
            {
                ts = new ThreadStart(StartClient);
                thread = new Thread(ts);
                thread.Start();
            }
        }

        private string GetLocalIp()
        {
            string ip;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                ip = endPoint.Address.ToString();
            }
            return ip;
        }

        private void StartServer()
        {
            IPAddress iPAddress = IPAddress.Parse(localIp);
            TcpListener tcpListener = new TcpListener(iPAddress, port);
            tcpListener.Start();
            while (true)
            {
                Recieve(tcpListener);
            }
            tcpListener.Stop();
        }

        private void StartClient()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(publicIp, port);
            NetworkStream networkStream = tcpClient.GetStream();
            Send(networkStream, $"<connect.localIp={localIp}><EOF>");
            byte[] buffer = new byte[1024];
            int size = networkStream.Read(buffer, 0, buffer.Length);
            string recievedData = null;
            for (int i = 0; i < size; i++)
            {
                recievedData += Convert.ToChar(buffer[i]);
            }
            MessageBox.Show("Client: " + recievedData);
        }

        private void Recieve(TcpListener tcpListener)
        {
            using (Socket socket = tcpListener.AcceptSocket())
            {
                byte[] buffer = new byte[1024];
                int size = socket.Receive(buffer);
                string recievedData = null;
                for (int i = 0; i < size; i++)
                {
                    recievedData += Convert.ToChar(buffer[i]);
                }
                ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
                socket.Send(aSCIIEncoding.GetBytes("<RECIEVED><EOF>"));
                MessageBox.Show("Server: " + recievedData);
            }
        }

        private void Send(NetworkStream networkStream, string message)
        {
            ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
            byte[] buffer = aSCIIEncoding.GetBytes(message);
            networkStream.Write(buffer, 0, buffer.Length);
            //byte[] bb = new byte[1024];
            //int k = networkStream.Read(bb, 0, 1024);
        }

        private void xtxxtbxSendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //SendMessage();
                xtbxSendMessage.Clear();
                e.SuppressKeyPress = true;
            }
        }

        private void xtxtbxSendMessage_Enter(object sender, EventArgs e)
        {
            if (xtbxSendMessage.ForeColor == Color.Gray)
            {
                xtbxSendMessage.ForeColor = Color.Black;
                xtbxSendMessage.Clear();
            }
        }

        private void xtxtbxSendMessage_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(xtbxSendMessage.Text))
            {
                xtbxSendMessage.ForeColor = Color.Gray;
                xtbxSendMessage.Text = "Enter a message...";
            }
        }

    }
}