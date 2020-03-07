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
    public partial class FrmChatScreen : Form
    {
        private int port = 12210;
        private bool connected;
        private ThreadStart ts;
        private Thread thread;
        private bool hosting;
        private string publicIp;
        private string localIp;
        private List<Client> connectedClients = new List<Client>();

        public FrmChatScreen()
        {
            InitializeComponent();
            CheckHost();
        }

        private void CheckHost()
        {
            localIp = GetLocalIp();
            if (FrmHolder.hosting)
            {
                publicIp = new WebClient().DownloadString("https://ipv4.icanhazip.com/");
                publicIp = publicIp.TrimEnd();
                Thread thread = new Thread(new ThreadStart(StartServer));
                thread.IsBackground = true; //TODO: Abort thread if parentform closes, so that Client.Disconnect() can be properly called
                thread.Start();
                xlbxChat.Items.Add($"Server started on: {publicIp}");
            }
            else
            {
                publicIp = FrmHolder.joinIP;
                Thread thread = new Thread(new ThreadStart(StartClient));
                thread.IsBackground = true;
                thread.Start();
                xlbxChat.Items.Add($"Connected to server on: {publicIp}");
            }
        }

        private string GetLocalIp()
        {
            string localIp;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = (IPEndPoint)socket.LocalEndPoint;
                localIp = endPoint.Address.ToString();
            }
            return localIp;
        }

        private void StartServer()
        {
            IPAddress iPAddress = IPAddress.Parse(localIp);
            TcpListener tcpListener = new TcpListener(iPAddress, port);
            try
            {
                tcpListener.Start();
                while (true)
                {
                    ServerAcceptIncomingConnection(tcpListener);
                    RecieveLoopClients();
                }
            }
            catch (ThreadAbortException)
            {
                foreach (Client client in connectedClients)
                {
                    Disconnect(client);
                }
                tcpListener.Stop();
            }
        }

        private void StartClient()
        {
            try
            {
                Client client = new Client();
                client.tcpClient = new TcpClient();
                client.tcpClient.Connect(publicIp, port);
                connectedClients.Add(client);
                connectedClients[0].Send(0, $"{FrmHolder.username}");
                while (true)
                {
                    RecieveLoopClients();
                }
                //MessageBox.Show("Client: " + recievedData);
            }
            catch (ThreadAbortException)
            {
                foreach (Client client in connectedClients)
                {
                    Disconnect(client);
                }
            }
        }

        private void ServerAcceptIncomingConnection(TcpListener tcpListener)
        {
            if (tcpListener.Pending())
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Client client = new Client();
                client.tcpClient = tcpClient;
                connectedClients.Add(client);
                connectedClients[0].Send(1, $"{connectedClients.Count - 1}");
                //TODO: Send acknowledement
                //MessageBox.Show("Server: " + recievedData);
            }
        }

        private void RecieveLoopClients()
        {
            for (int i = 0; i < connectedClients.Count(); i++)
            {
                connectedClients[i].Recieve();
            }
        }

        private void Disconnect(Client client)
        {
            client.Disconnect();
            connectedClients.Remove(client);
        }

        private void xtxxtbxSendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (connectedClients.Count > 0)
                {
                    connectedClients[0].Send(2, xtbxSendMessage.Text);
                }
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

        public void Send(int messageType, string message)
        {
            if (messageType >= 0 && messageType <= 255)
            {
                NetworkStream networkStream = tcpClient.GetStream();
                {
                    if (networkStream.CanWrite && networkStream.CanRead)
                    {
                        //Mmessage type
                        byte[] typeBuffer = new byte[1];
                        typeBuffer = BitConverter.GetBytes(messageType);
                        // Message content
                        byte[] messageBuffer = Encoding.ASCII.GetBytes(message);
                        // Message length
                        byte[] lengthBuffer = new byte[4];
                        lengthBuffer = BitConverter.GetBytes(messageBuffer.Length);
                        if (BitConverter.IsLittleEndian)
                        {
                            /*Array.Reverse(typeBuffer);
                            Array.Reverse(messageBuffer);
                            Array.Reverse(lengthBuffer);*/
                        }
                        networkStream.Write(typeBuffer, 0, 1); // Message type
                        networkStream.Write(lengthBuffer, 0, 4); // Message length
                        networkStream.Write(messageBuffer, 0, messageBuffer.Length); // Message content
                        //Read acknowledgement
                    }
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("'messageType' must be between 0 and 255");
            }
        }

        public void Recieve()
        {
            NetworkStream networkStream = tcpClient.GetStream();
            {
                if (networkStream.CanRead && networkStream.CanWrite && networkStream.DataAvailable)
                {
                    // Message type
                    byte[] typeBuffer = new byte[1];
                    networkStream.Read(typeBuffer, 0, 1);
                    int messageType = typeBuffer[0];
                    // Message length
                    byte[] lengthBuffer = new byte[4];
                    networkStream.Read(lengthBuffer, 0, 4);
                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (messageLength > 1*1024*1024)
                    {
                        throw new ArgumentOutOfRangeException("Length cannot be greater than 1*1024*1024");
                    }
                    // Message content
                    byte[] messageBuffer = new byte[messageLength];
                    networkStream.Read(messageBuffer, 0, messageLength);
                    string message = Encoding.ASCII.GetString(messageBuffer);
                    //TODO: Send acknowledement
                }
            }
        }

        public void Disconnect()
        {
            tcpClient.Close();
        }
    }
}