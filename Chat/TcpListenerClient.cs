/*using System;
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
    public partial class ChatScreen : Form
    {
        private int port = 12210;
        private bool connected;
        private ThreadStart ts;
        private Thread thread;
        private bool hosting;
        Server server;
        Client client;

        public ChatScreen()
        {
            InitializeComponent();
            CheckHost();
        }

        private void CheckHost()
        {
            if (HolderForm.hosting == true)
            {
                ts = new ThreadStart(StartServer);
                thread = new Thread(ts);
                thread.Start();
            }
            else
            {
                ts = new ThreadStart(StartClient);
                thread = new Thread(ts);
                thread.Start();
            }
        }

        private void StartServer()
        {
            hosting = true;
            server = new Server();
            server.username = HolderForm.username;
            server.StartServer(port);
        }

        private void StartClient()
        {
            hosting = false;
            client = new Client();
            client.username = HolderForm.username;
            client.StartClient(port);
        }

        private void SendMessage()
        {
            if (!(string.IsNullOrWhiteSpace(xtxtbxSendMessage.Text) && xtxtbxSendMessage.ForeColor == Color.Gray))
            {
                if (hosting == false)
                {
                    client.Send(xtxtbxSendMessage.Text);
                }
            }
        }

        private void xtxxtbxSendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage();
                xtxtbxSendMessage.Clear();
                e.SuppressKeyPress = true;
            }
        }

        private void xtxtbxSendMessage_Enter(object sender, EventArgs e)
        {
            if (xtxtbxSendMessage.ForeColor == Color.Gray)
            {
                xtxtbxSendMessage.ForeColor = Color.Black;
                xtxtbxSendMessage.Clear();
            }
        }

        private void xtxtbxSendMessage_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(xtxtbxSendMessage.Text))
            {
                xtxtbxSendMessage.ForeColor = Color.Gray;
                xtxtbxSendMessage.Text = "Enter a message...";
            }
        }
    }

    public class Server
    {
        public TcpListener server;
        public IPHostEntry iPHostEntry;
        public IPAddress iPAddress;
        public IPEndPoint iPEndPoint;
        public TcpClient client;
        public NetworkStream ns;
        public string recievedData;
        public string username;
        public bool connected;

        public void StartServer(int port)
        {
            try
            {
                iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
                iPAddress = iPHostEntry.AddressList[0];
                iPEndPoint = new IPEndPoint(IPAddress.Any, port);

                server = new TcpListener(iPAddress, port);
                server.Start();

                Connect();
                while (true)
                {
                    if (connected == true)
                    {
                        CheckForRecievedData();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                server.Stop();
            }
        }

        public async void Connect()
        {
            client = await server.AcceptTcpClientAsync();
            ns = client.GetStream();
            connected = true;
        }

        public void CheckForRecievedData()
        {
            byte[] buffer = new byte[1024];
            int i = ns.Read(buffer, 0, buffer.Length);
            while (!(i == 0))
            {
                recievedData = Encoding.ASCII.GetString(buffer, 0, i);
            }
        }

        public void Send(string responseIn)
        {
            byte[] response = Encoding.ASCII.GetBytes(responseIn);
            ns.Write(response, 0, response.Length);
        }

        public void Process()
        {

        }
    }

    public class Client
    {
        public TcpClient client;
        public IPHostEntry iPHostEntry;
        public IPAddress iPAddress;
        public IPEndPoint iPEndPoint;
        public NetworkStream ns;
        public string recievedData;
        public string username;

        public void StartClient(int port)
        {
            iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            iPAddress = iPHostEntry.AddressList[0];
            iPEndPoint = new IPEndPoint(IPAddress.Any, port);

            while (true)
            {
                if (Connect(port) == true)
                {
                    break;
                }
            }

            while (true)
            {
                CheckForRecievedData();
            }
        }

        public bool Connect(int portIn)
        {
            client = new TcpClient(iPAddress.ToString(), portIn);
            //Send($"<CONNECTED><USER={username}");
            //Todo: Handshake to confirm connection
            return true;
        }

        public void CheckForRecievedData()
        {
            byte[] buffer = new byte[1024];
            int i = ns.Read(buffer, 0, buffer.Length);
            while (!(i == 0))
            {
                recievedData = Encoding.ASCII.GetString(buffer, 0, i);
            }
        }

        public void Send(string responseIn)
        {
            byte[] response = Encoding.ASCII.GetBytes(responseIn);
            ns.Write(response, 0, response.Length);
        }
    }
}*/