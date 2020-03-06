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
        private string publicIp;
        private string localIp;
        private List<Client> connectedClients = new List<Client>();

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
            if (frmHolderForm.hosting)
            {
                Thread thread = new Thread(new ThreadStart(StartServer));
                thread.Start();
                xlbxChat.Items.Add($"Server started on: {publicIp}");
            }
            else
            {
                Thread thread = new Thread(new ThreadStart(StartClient));
                thread.Start();
            }
        }

        private string GetLocalIp()
        {
            string ip;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = (IPEndPoint)socket.LocalEndPoint;
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
                AcceptIncomingConnection(tcpListener);
                RecieveLoopClients();
            }
            //tcpListener.Stop();
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

        private void AcceptIncomingConnection(TcpListener tcpListener)
        {
            if (tcpListener.Pending())
            {
                using (TcpClient tcpClient = tcpListener.AcceptTcpClient())
                {
                    using (NetworkStream networkStream = tcpClient.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int size = networkStream.Read(buffer, 0, 4);
                        string recievedData = Encoding.ASCII.GetString(buffer);
                        //Send acknowledement
                        Send(networkStream, "<RECIEVED><EOF>");
                        MessageBox.Show("Server: " + recievedData);
                        //Initialise new Client
                    }
                }
            }
        }

        private void Recieve(TcpClient tcpClient)
        {
            using (NetworkStream networkStream = tcpClient.GetStream())
            {
                if (networkStream.CanRead && networkStream.CanWrite && networkStream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    int size = networkStream.Read(buffer, 0, /*recieve length of next packet and only read by that amount*/);
                    string recievedData = Encoding.ASCII.GetString(buffer);
                    //Send acknowledement
                    Send(networkStream, "<RECIEVED><EOF>");
                    MessageBox.Show("Server: " + recievedData);
                }
            }
        }

        private void RecieveLoopClients()
        {
            for (int i = 0; i < connectedClients.Count(); i++)
            {
                Recieve(connectedClients[i].tcpClient);
            }
        }

        private void Send(NetworkStream networkStream, string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            networkStream.Write(buffer, 0, buffer.Length);
            //Read acknowledgement
        }

        private void xtxxtbxSendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Send(connectedClients[0].networkStream, xtbxSendMessage.Text);
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

    public class Client
    {
        public int clientId;
        public string username;
        public TcpClient tcpClient;
        public NetworkStream networkStream;
    }
}