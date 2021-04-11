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
        private Thread serverThread;
        private Thread clientThread;
        private int port = 12210;
        private string publicIp;
        private string localIp;
        private List<Client> connectedClients = new List<Client>();
        bool askToClose = true;
        private System.Windows.Forms.Timer heartbeat = new System.Windows.Forms.Timer();

        private delegate void MessageDelegate(Client client, Message message);

        public FrmChatScreen()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(OnClosing);
            xlsvConnectedUsers.Columns[0].Width = xlsvConnectedUsers.Width - 5;
            CheckHost();
        }

        private void CheckHost()
        {
            localIp = GetLocalIp();
            xlsvConnectedUsers.Items.Add(FrmHolder.username);
            if (FrmHolder.hosting)
            {
                publicIp = new WebClient().DownloadString("https://ipv4.icanhazip.com/");
                publicIp = publicIp.TrimEnd();
                serverThread = new Thread(new ThreadStart(StartServer));
                serverThread.IsBackground = true; //TODO: Abort thread if parentform closes, so that Client.Disconnect() can be properly called
                serverThread.Start();
                xlbxChat.Items.Add($"Server started on: {publicIp}");
            }
            else
            {
                publicIp = FrmHolder.joinIP;
                clientThread = new Thread(new ThreadStart(StartClient));
                clientThread.IsBackground = true;
                clientThread.Start();
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
                if (connectedClients.Count > 0)
                {
                    Disconnect(null, false, true);
                }
                tcpListener.Stop();
            }
        }

        private void StartClient()
        {
            Client client = new Client();
            try
            {
                client.tcpClient = new TcpClient();
                client.tcpClient.Connect(publicIp, port);
                connectedClients.Add(client);
                Send(connectedClients[0], 0, $"{FrmHolder.username}", true);
                heartbeat.Interval = 5000;
                heartbeat.Tick += Heartbeat_Tick;
                heartbeat.Start();
                while (true)
                {
                    RecieveLoopClients();
                }
            }
            catch (ThreadAbortException)
            {
                if (connectedClients.Count > 0)
                {
                    Disconnect(client, false, false);
                }
            }
        }

        private void Heartbeat_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ServerAcceptIncomingConnection(TcpListener tcpListener)
        {
            if (tcpListener.Pending())
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Client client = new Client();
                client.tcpClient = tcpClient;
                client.clientId = connectedClients.Count;
                connectedClients.Add(client);
                Send(connectedClients[0], 1, $"{client.clientId}", true);
                //TODO: Send acknowledement
            }
        }

        public Message Send(Client client, int messageType, string message, bool process)
        {
            if (messageType >= 0 && messageType <= 255)
            {
                NetworkStream networkStream = client.tcpClient.GetStream();
                {
                    if (networkStream.CanWrite && networkStream.CanRead)
                    {
                        //Mmessage type
                        byte[] typeBuffer = new byte[1];
                        typeBuffer = BitConverter.GetBytes(messageType);
                        // Message content
                        byte[] messageBuffer = null;
                        if (message != null)
                        {
                            messageBuffer = Encoding.ASCII.GetBytes(message);
                        }
                        // Message length
                        byte[] lengthBuffer = new byte[4];
                        if (message != null)
                        {
                            lengthBuffer = BitConverter.GetBytes(messageBuffer.Length);
                        }
                        if (BitConverter.IsLittleEndian)
                        {
                            /*Array.Reverse(typeBuffer);
                            Array.Reverse(messageBuffer);
                            Array.Reverse(lengthBuffer);*/
                        }
                        networkStream.Write(typeBuffer, 0, 1); // Message type
                        networkStream.Write(lengthBuffer, 0, 4); // Message length
                        if (message != null)
                        {
                            networkStream.Write(messageBuffer, 0, messageBuffer.Length); // Message content
                        }
                        //Read acknowledgement
                        return null; //Response
                    }
                }
                return null;
            }
            else
            {
                throw new ArgumentOutOfRangeException("'messageType' must be between 0 and 255");
            }
        }

        public Message Recieve(Client client, bool process)
        {
            NetworkStream networkStream = client.tcpClient.GetStream();
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
                    if (messageLength > 1 * 1024 * 1024)
                    {
                        throw new ArgumentOutOfRangeException("Length cannot be greater than 1*1024*1024");
                    }
                    // Message content
                    string message = null;
                    if (messageLength > 0)
                    {
                        byte[] messageBuffer = new byte[messageLength];
                        networkStream.Read(messageBuffer, 0, messageLength);
                        message = Encoding.ASCII.GetString(messageBuffer);
                    }
                    //TODO: Send acknowledement
                    Message recievedMessage = new Message(messageType, message);
                    if (process)
                    {
                        if (xlbxChat.InvokeRequired)
                        {
                            xlbxChat.BeginInvoke(new MessageDelegate(ClientProcessMessage), client, recievedMessage);
                        }
                    }
                    return recievedMessage;
                }
            }
            return null;
        }

        private void ClientProcessMessage(Client client, Message message)
        {
            //0 = client-server connection request; 1 = server-client set clientId; 2 = recieve message; 3 = disconnect; 4 = server-client username already used
            if (message.messageType == 0) // Connection Request [username]
            {
                bool usernameInUse = false;
                for (int i = 0; i < connectedClients.Count; i++)
                {
                    if (connectedClients[i].username == message.message || FrmHolder.username == message.message)
                    {
                        usernameInUse = true;
                        Send(client, 4, null, true);
                    }
                }
                if (usernameInUse == false)
                {
                    client.username = message.message;
                    xlsvConnectedUsers.Items.Add(client.username);
                    xlbxChat.Items.Add($"{client.username} connected");
                    SendToAll(5, client.username, true);
                    SendToAll(7, null, true);
                    for (int i = 0; i < xlsvConnectedUsers.Items.Count; i++)
                    {
                        SendToAll(8, xlsvConnectedUsers.Items[i].Text, true);
                    }
                }
            }
            else if (message.messageType == 1) // ClientId set [clientId]
            {
                client.clientId = Convert.ToInt32(message.message);
            }
            else if (message.messageType == 2) // Message recieve [username, message]
            {
                string username = "";
                string messageText = "";
                bool inUsername = false;
                bool inMessageText = false;
                for (int i = 0; i < message.message.Length; i++)
                {
                    if (message.message[i] == '<') { inUsername = true; continue; }
                    else if (message.message[i] == '>') { inUsername = false; inMessageText = true; continue; }
                    if (inUsername) { username += message.message[i]; }
                    else if (inMessageText) { messageText += message.message[i]; }
                }
                xlbxChat.Items.Add($"{username}: {messageText}");
                if (FrmHolder.hosting)
                {
                    SendToAll(2, message.message, true);
                }
            }
            else if (message.messageType == 3) // Disconnect
            {
                if (FrmHolder.hosting)
                {
                    if (client.username != null)
                    {
                        xlsvConnectedUsers.Items.RemoveAt(connectedClients.IndexOf(client) + 1);
                        xlbxChat.Items.Add($"{client.username} disconnected");
                        SendToAll(6, client.username, true);
                    }
                    connectedClients.Remove(client);
                    Disconnect(client, false, false);
                    SendToAll(7, null, true);
                    for (int i = 0; i < xlsvConnectedUsers.Items.Count; i++)
                    {
                        SendToAll(8, xlsvConnectedUsers.Items[i].Text, true);
                    }
                }
                else
                {
                    if (clientThread != null && clientThread.IsAlive)
                    {
                        clientThread.Abort();
                    }
                    MessageBox.Show("The server was closed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OpenMainMenu();
                }
            }
            else if (message.messageType == 4) // Username already used
            {
                clientThread.Abort();
                MessageBox.Show("This username is already in use", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OpenMainMenu();
            }
            else if (message.messageType == 5) // Client connected
            {
                xlbxChat.Items.Add($"{message.message} connected");
            }
            else if (message.messageType == 6) // Client disconnected
            {
                xlbxChat.Items.Add($"{message.message} disconnected");
            }
            else if (message.messageType == 7) // Clear user list
            {
                xlsvConnectedUsers.Items.Clear();
            }
            else if (message.messageType == 8) // Add to user list
            {
                xlsvConnectedUsers.Items.Add(message.message);
            }
            else if (message.messageType == 9) // Kicked
            {
                if (clientThread != null && clientThread.IsAlive)
                {
                    clientThread.Abort();
                }
                string[] parts = message.message.Split(' ');
                string username = parts[0];
                string reason = "";
                for (int i = 1; i < parts.Length; i++)
                {
                    reason += parts[i] + ' ';
                }
                reason.Trim();
                if (reason != "" && reason != " ")
                {
                    MessageBox.Show($"You have been kicked by {username} with reason: {reason}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"You have been kicked by {username}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                OpenMainMenu();
            }
            else if (message.messageType == 10)
            {
                string[] parts = message.message.Split(' ');
                string username = parts[0];
                string kickerUsername = parts[1];
                string reason = "";
                for (int i = 2; i < parts.Length; i++)
                {
                    reason += parts[i] + ' ';
                }
                reason.Trim();
                if (reason != "" && reason != " ")
                {
                    xlbxChat.Items.Add($"{username} was kicked by {kickerUsername} with reason: {reason}");
                }
                else
                {
                    xlbxChat.Items.Add($"{username} was kicked by {kickerUsername}");
                }
            }
        }

        private void RecieveLoopClients()
        {
            for (int i = 0; i < connectedClients.Count(); i++)
            {
                Recieve(connectedClients[i], true);
            }
        }

        private void SendToAll(int messageType, string message, bool process)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                Send(connectedClients[i], messageType, message, process);
            }
        }

        private void Disconnect(Client client, bool kick, bool sendToAll)
        {
            int type = kick == false ? 3 : 9;
            if (sendToAll)
            {
                SendToAll(type, null, true);
                for (int i = 0; i < connectedClients.Count; i++)
                {
                    connectedClients[i].tcpClient.Close();
                }
                connectedClients.Clear();
            }
            else
            {
                Send(client, type, null, true);
                client.tcpClient.Close();
                connectedClients.Remove(client);
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (askToClose)
            {
                if (FrmHolder.hosting)
                {
                    DialogResult dialogResult = MessageBox.Show("This will terminate the server. Are you sure?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.OK)
                    {
                        if (serverThread != null && serverThread.IsAlive)
                        {
                            serverThread.Abort();
                        }
                    }
                    else if (dialogResult == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
                if (clientThread != null && clientThread.IsAlive)
                {
                    clientThread.Abort();
                }
            }
        }

        private List<Client> ClientSearch(string[] usernames, int[] clientIds)
        {
            List<Client> clients = new List<Client>();
            if (usernames != null)
            {
                for (int i = 0; i < usernames.Length; i++)
                {
                    for (int j = 0; j < connectedClients.Count; j++)
                    {
                        if (usernames[i] == connectedClients[j].username)
                        {
                            clients.Add(connectedClients[j]);
                            break;
                        }
                    }
                }
            }
            if (clientIds != null)
            {
                for (int i = 0; i < clientIds.Length; i++)
                {
                    for (int j = 0; j < connectedClients.Count; j++)
                    {
                        if (clientIds[i] == connectedClients[j].clientId && clients.Contains(connectedClients[j]) == false)
                        {
                            clients.Add(connectedClients[j]);
                            break;
                        }
                    }
                }
            }
            return clients;
        }

        private bool ProcessCommand(string message)
        {
            if (message[0] == '/')
            {
                if (FrmHolder.hosting == false)
                {
                    xlbxChat.Items.Add($"You must be an admin to execute commands");
                    return true;
                }
                string command = message.Substring(1, message.Length - 1);
                string[] commandParts = command.Split(' ');
                string[] usernames = { commandParts[1] };
                if (commandParts[0] == "kick")
                {
                    List<Client> clients = ClientSearch(usernames, null);
                    if (clients.Count == 0)
                    {
                        xlbxChat.Items.Add($"No user with the username {usernames[0]} exists");
                        return true;
                    }
                    string reason = "";
                    for (int i = 2; i < commandParts.Length; i++)
                    {
                        reason += commandParts[i] + ' ';
                    }
                    reason.Trim();
                    if (reason != "")
                    {
                        xlbxChat.Items.Add($"You kicked {usernames[0]} with reason: {reason}");
                    }
                    else
                    {
                        xlbxChat.Items.Add($"You kicked {usernames[0]}");
                    }
                    Send(clients[0], 9, $"{FrmHolder.username} {reason}", true); // Kick client
                    SendToAll(10, $"{usernames[0]} {FrmHolder.username} {reason}", true);
                }
                return true;
            }
            return false; //true if commmand
        }

        private void xtxxtbxSendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string message = xtbxSendMessage.Text;
                if (FrmHolder.hosting || message[0] == '/')
                {
                    xlbxChat.Items.Add($"{FrmHolder.username}: {message}");
                }
                if (ProcessCommand(message) == false)
                {
                    if (connectedClients.Count > 0)
                    {
                        for (int i = 0; i < connectedClients.Count; i++)
                        {
                            Send(connectedClients[i], 2, $"<{FrmHolder.username}>{message}", true);
                        }
                    }
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

        private void xbtnDisconnect_Click(object sender, EventArgs e)
        {
            askToClose = false;
            bool close = false;
            if (FrmHolder.hosting)
            {
                DialogResult dialogResult = MessageBox.Show("This will terminate the server. Are you sure?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.OK)
                {
                    close = true;
                    if (serverThread != null && serverThread.IsAlive)
                    {
                        serverThread.Abort();
                    }
                }
            }
            if (clientThread != null && clientThread.IsAlive)
            {
                clientThread.Abort();
            }
            if (close || FrmHolder.hosting == false)
            {
                OpenMainMenu();
            }
        }

        private void OpenMainMenu()
        {
            FrmLoginScreen frmLoginScreen = new FrmLoginScreen
            {
                MdiParent = this.ParentForm,
                Dock = DockStyle.Fill
            };
            frmLoginScreen.Show();
            this.Close();
        }
    }

    public class Client
    {
        public int clientId;
        public string username;
        public TcpClient tcpClient;
    }

    public class Message
    {
        public int messageType;
        public string message;

        public Message(int messageType, string message)
        {
            this.messageType = messageType;
            this.message = message;
        }
    }
}