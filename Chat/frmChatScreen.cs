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
        //TODO: Implement clientId
        //TODO: Implement message ID's (each client has its own next message IDs)
        private Thread serverThread;
        private Thread clientThread;
        private int port = 12210;
        private string publicIp;
        private string localIp;
        private int nextAssignedClientId = 0;
        private int nextAssignedMessageId = 0;
        private List<Client> connectedClients = new List<Client>();
        bool askToClose = true;
        private System.Timers.Timer heartbeat = new System.Timers.Timer();

        private string kickFormat = "/kick [Username] [Reason (optional)]";
        private string adminFormat = "/admin [Username] [True/False (optional)]";

        private delegate void MessageDelegate(Client client, Message message);
        private delegate void HeartbeatDelegate(Client client);

        public FrmChatScreen()
        {
            InitializeComponent();
            this.Load += new EventHandler(FrmChatScreen_Load);
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
                //publicIp = localIp; // For use if unable to access internet/port forward
                publicIp = publicIp.Trim();
                serverThread = new Thread(new ThreadStart(StartServer));
                serverThread.IsBackground = true; //TODO: Abort thread if parentform closes, so that Disconnect() can be properly called (For both client and server)
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
            //StartHeartbeat();
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
            Thread.Sleep(50);
            IPAddress iPAddress = IPAddress.Parse(localIp);
            TcpListener tcpListener = new TcpListener(iPAddress, port);
            Client client = new Client();
            client.clientId = nextAssignedClientId;
            nextAssignedClientId++;
            client.username = FrmHolder.username;
            client.admin = true;
            connectedClients.Add(client);
            try
            {
                tcpListener.Start();
                //StartHeartbeat();
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
            Thread.Sleep(50);
            Client client = new Client();
            try
            {
                client.tcpClient = new TcpClient();
                client.tcpClient.Connect(publicIp, port);
                connectedClients.Add(client);
                Send(connectedClients[0], 0, $"{FrmHolder.username}", true);
                //StartHeartbeat();
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

        private void StartHeartbeat()
        {
            heartbeat.Interval = 1000;
            heartbeat.Elapsed += Heartbeat_Tick;
            heartbeat.Start();
        }

        private void Heartbeat_Tick(object sender, EventArgs e)
        {
            if (connectedClients != null)
            {
                for (int i = 0; i < connectedClients.Count; i++)
                {
                    Client client = connectedClients[i];
                    if (client.tcpClient != null)
                    {
                        if (FrmHolder.hosting == false)
                        {
                            Send(client, 11, null, true);
                        }
                        Thread.Sleep(50);
                        if (client.heartbeatReceieved == false)
                        {
                            client.heartbeatFailures++;
                        }
                        if (client.heartbeatFailures == 3)
                        {
                            if (xlbxChat.InvokeRequired)
                            {
                                xlbxChat.BeginInvoke(new HeartbeatDelegate(HeartbeatFailure), client);
                            }
                        }
                        client.heartbeatReceieved = false;
                    }
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
                client.clientId = nextAssignedClientId;
                nextAssignedClientId++;
                connectedClients.Add(client);
                Send(connectedClients[0], 1, $"{client.clientId}", true);
                //TODO: Send acknowledement
            }
        }

        public Message Send(Client client, int messageType, string message, bool process)
        {
            if (messageType >= 0 && messageType <= 255)
            {
                if (client.tcpClient != null)
                {
                    NetworkStream networkStream = client.tcpClient.GetStream();
                    {
                        if (networkStream.CanWrite && networkStream.CanRead)
                        {
                            // Message ID
                            byte[] idBuffer = new byte[4];
                            idBuffer = BitConverter.GetBytes(nextAssignedMessageId);
                            nextAssignedMessageId++;
                            // Message type
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
                            networkStream.Write(idBuffer, 0, 4); // Message ID
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
            if (client.tcpClient != null)
            {
                NetworkStream networkStream = client.tcpClient.GetStream();
                {
                    if (networkStream.CanRead && networkStream.CanWrite && networkStream.DataAvailable)
                    {
                        // Message ID
                        byte[] idBuffer = new byte[4];
                        networkStream.Read(idBuffer, 0, 4);
                        int messageId = BitConverter.ToInt32(idBuffer, 0);
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
                        Message recievedMessage = new Message(messageId, messageType, message);
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
                    if (string.Equals(connectedClients[i].username, message.message, StringComparison.OrdinalIgnoreCase) || FrmHolder.username == message.message)
                    {
                        usernameInUse = true;
                        Send(client, 4, null, true);
                        break;
                    }
                }
                if (usernameInUse == false)
                {
                    client.username = message.message;
                    List<Client> exceptions = new List<Client>();
                    exceptions.Add(client);
                    xlbxChat.Items.Add($"{client.username} connected");
                    SendToAll(exceptions, 5, client.username, true);
                    UpdateClientLists();
                }
            }
            else if (message.messageType == 1) // ClientId set [clientId]
            {
                client.clientId = Convert.ToInt32(message.message);
            }
            else if (message.messageType == 2) // Message recieve [username, message]
            {
                string[] parts = message.message.Split(' ');
                string username = parts[0];
                string messageText = "";
                for (int i = 1; i < parts.Length; i++)
                {
                    messageText += parts[i] + ' ';
                }
                messageText = messageText.Trim();
                xlbxChat.Items.Add($"{username}: {messageText}");
                if (FrmHolder.hosting)
                {
                    SendToAll(null, 2, message.message, true);
                }
            }
            else if (message.messageType == 3) // Disconnect
            {
                if (FrmHolder.hosting)
                {
                    if (client.username != null)
                    {
                        List<Client> exceptions = new List<Client>();
                        exceptions.Add(client);
                        xlbxChat.Items.Add($"{client.username} disconnected");
                        SendToAll(exceptions, 6, client.username, true);
                    }
                    connectedClients.Remove(client);
                    Disconnect(client, false, false);
                    UpdateClientLists();
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
                reason = reason.Trim();
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    MessageBox.Show($"You have been kicked by {username} with reason: {reason}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"You have been kicked by {username}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                OpenMainMenu();
            }
            else if (message.messageType == 10) // Another client kicked
            {
                string[] parts = message.message.Split(' ');
                string username = parts[0];
                string kickerUsername = parts[1];
                string reason = "";
                for (int i = 2; i < parts.Length; i++)
                {
                    reason += parts[i] + ' ';
                }
                reason = reason.Trim();
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    xlbxChat.Items.Add($"{username} was kicked by {kickerUsername} with reason: {reason}");
                }
                else
                {
                    xlbxChat.Items.Add($"{username} was kicked by {kickerUsername}");
                }
            }
            else if (message.messageType == 11) // Heartbeat received
            {
                client.heartbeatReceieved = true;
                Send(client, 11, null, true);
            }
            else if (message.messageType == 12) // Heartbeat failed
            {
                HeartbeatFailure(client);
            }
            else if (message.messageType == 13) // Another client heartbeat failed
            {
                xlbxChat.Items.Add($"{message.message} has lost connection...");
            }
            else if (message.messageType == 14) // Made admin
            {
                xlbxChat.Items.Add($"You have been made an Admin by {message.message}");
            }
            else if (message.messageType == 15) // Another made admin
            {
                string[] parts = message.message.Split(' ');
                string username = parts[0];
                string setterUsername = parts[1];
                xlbxChat.Items.Add($"{username} has been made an Admin by {setterUsername}");
            }
            else if (message.messageType == 16) // Removed admin
            {
                xlbxChat.Items.Add($"You have been removed from Admin by {message.message}");
            }
            else if (message.messageType == 17) // Another removed admin
            {
                string[] parts = message.message.Split(' ');
                string username = parts[0];
                string setterUsername = parts[1];
                xlbxChat.Items.Add($"{username} has been removed from Admin by {setterUsername}");
            }
        }

        private void HeartbeatFailure(Client client)
        {
            if (FrmHolder.hosting)
            {
                List<Client> exceptions = new List<Client>();
                exceptions.Add(client);
                Send(client, 12, null, true);
                SendToAll(exceptions, 13, client.username, true);
                UpdateClientLists();
                xlbxChat.Items.Add($"Lost connection to {client.username}...");
            }
            else
            {
                xlbxChat.Items.Add($"Lost connection to server...");
            }
            if (client.tcpClient != null)
            {
                client.tcpClient.Close();
            }
            connectedClients.Remove(client);
        }

        private string[] GetClientUsernames()
        {
            string[] usernames = new string[connectedClients.Count];
            for (int i = 0; i < connectedClients.Count; i++)
            {
                usernames[i] = connectedClients[i].username;
            }
            return usernames;
        }

        private void UpdateClientLists()
        {
            xlsvConnectedUsers.Items.Clear();
            SendToAll(null, 7, null, true);
            string[] usernames = GetClientUsernames();
            for (int i = 0; i < usernames.Length; i++)
            {
                xlsvConnectedUsers.Items.Add(usernames[i]);
                SendToAll(null, 8, usernames[i], true);
            }
        }

        private void RecieveLoopClients()
        {
            for (int i = 0; i < connectedClients.Count(); i++)
            {
                Recieve(connectedClients[i], true);
            }
        }

        private void SendToAll(List<Client> ignoredClients, int messageType, string message, bool process)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                if (ignoredClients != null)
                {
                    for (int j = 0; j < ignoredClients.Count; j++)
                    {
                        if (connectedClients[i] != ignoredClients[j])
                        {
                            Send(connectedClients[i], messageType, message, process);
                            break;
                        }
                    }
                }
                else
                {
                    Send(connectedClients[i], messageType, message, process);
                }
            }
        }

        private void Disconnect(Client client, bool kick, bool sendToAll)
        {
            int type = kick == false ? 3 : 9;
            if (sendToAll)
            {
                List<Client> exceptions = new List<Client>();
                exceptions.Add(client);
                SendToAll(exceptions, type, null, true);
                for (int i = 0; i < connectedClients.Count; i++)
                {
                    if (connectedClients[i].tcpClient != null)
                    {
                        connectedClients[i].tcpClient.Close();
                    }
                }
                connectedClients.Clear();
            }
            else
            {
                Send(client, type, null, true);
                Thread.Sleep(50);
                if (client.tcpClient != null)
                {
                    client.tcpClient.Close();
                }
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

        private void FrmChatScreen_Load(object sender, EventArgs e)
        {
            SetParentFormWindowText(true);
        }

        private void SetParentFormWindowText(bool showConnectionInformation)
        {
            if (showConnectionInformation)
            {
                if (FrmHolder.hosting)
                {
                    this.ParentForm.Text = $"{FrmHolder.applicationWindowText} - {FrmHolder.username} hosting on {publicIp}";
                }
                else
                {
                    this.ParentForm.Text = $"{FrmHolder.applicationWindowText} - {FrmHolder.username} on {FrmHolder.joinIP}";
                }
            }
            else
            {
                this.ParentForm.Text = $"{FrmHolder.applicationWindowText}";
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
                    xlbxChat.Items.Add($"You must be an admin to execute commands"); //TODO: Allow non-admin commands (e.g. /help)
                    return true;
                }
                string command = message.Substring(1, message.Length - 1);
                string[] commandParts = command.Split(' ');
                if (commandParts[0] == "help" || commandParts[0] == "commands")
                {
                    CommandHelp(commandParts);
                }
                else if (commandParts[0] == "kick") //TODO: Admin only
                {
                    return CommandKick(commandParts);
                }
                else if (commandParts[0] == "admin" && FrmHolder.hosting)
                {
                    return CommandAdmin(commandParts);
                }
                return true;
            }
            return false; //true if commmand
        }

        private void CommandHelp(string[] commandParts)
        {
            if (commandParts.Length == 1)
            {
                xlbxChat.Items.Add($"Available commands are 'kick' and 'admin'. Type '/help [command]' for an explanation of the command.");
            }
            else if (commandParts[1] == "kick")
            {
                xlbxChat.Items.Add($"Explanation: Kicks a user. Format: {kickFormat}");
            }
            else if (commandParts[1] == "admin")
            {
                xlbxChat.Items.Add($"Explanation: Adds/removes a user from Admin. Format: {adminFormat}");
            }
        }

        private bool CommandKick(string[] commandParts)
        {
            if (commandParts[1] == null)
            {
                xlbxChat.Items.Add($"The format is: {kickFormat}");
                return true;
            }
            string[] username = { commandParts[1] };
            List<Client> clients = ClientSearch(username, null);
            if (clients.Count == 0)
            {
                xlbxChat.Items.Add($"No user with the username {username[0]} exists");
                return true;
            }
            string reason = "";
            for (int i = 2; i < commandParts.Length; i++)
            {
                reason += commandParts[i] + ' ';
            }
            reason = reason.Trim();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                xlbxChat.Items.Add($"You kicked {username[0]} with reason: {reason}");
            }
            else
            {
                xlbxChat.Items.Add($"You kicked {username[0]}");
            }
            List<Client> exceptions = new List<Client>();
            exceptions.Add(clients[0]);
            Send(clients[0], 9, $"{FrmHolder.username} {reason}", true); // Kick client
            SendToAll(exceptions, 10, $"{username[0]} {FrmHolder.username} {reason}", true);
            return false;
        }

        private bool CommandAdmin(string[] commandParts)
        {
            if (commandParts.Length < 2 || commandParts[1] == null)
            {
                xlbxChat.Items.Add($"The format is: {adminFormat}");
                return true;
            }
            string[] username = { commandParts[1] };
            List<Client> clients = ClientSearch(username, null);
            if (clients.Count == 0)
            {
                xlbxChat.Items.Add($"No user with the username {username[0]} exists");
                return true;
            }
            bool setAsAdmin = false;
            if (commandParts.Length > 2 && commandParts[2] != null)
            {
                if (string.Equals(commandParts[2], "True", StringComparison.OrdinalIgnoreCase))
                {
                    if (clients[0].admin)
                    {
                        xlbxChat.Items.Add($"This user is already an Admin");
                        return true;
                    }
                    setAsAdmin = true;
                }
                else if (string.Equals(commandParts[2], "False", StringComparison.OrdinalIgnoreCase))
                {
                    if (clients[0].admin == false)
                    {
                        xlbxChat.Items.Add($"This user is already not an Admin");
                        return true;
                    }
                    setAsAdmin = false;
                }
                else
                {
                    xlbxChat.Items.Add($"The format is: {adminFormat}");
                    return true;
                }
            }
            else
            {
                if (FrmHolder.hosting)
                {
                    if (ClientSearch(username, null)[0].admin)
                    {
                        setAsAdmin = false;
                    }
                    else
                    {
                        setAsAdmin = true;
                    }
                }
            }
            if (FrmHolder.hosting)
            {
                clients[0].admin = setAsAdmin;
                SetAdmin(clients[0], FrmHolder.username, setAsAdmin, null);
            }
            return false;
        }

        private void SetAdmin(Client client, string setter, bool setAsAdmin, List<Client> ignoredClients)
        {
            client.admin = setAsAdmin;
            if (ignoredClients == null)
            {
                ignoredClients = new List<Client>();
            }
            ignoredClients.Add(client);
            if (setAsAdmin)
            {
                Send(client, 14, setter, true);
                SendToAll(ignoredClients, 15, $"{client.username} {setter}", true);
                xlbxChat.Items.Add($"You made {client.username} an Admin");
            }
            else
            {
                Send(client, 16, setter, true);
                SendToAll(ignoredClients, 17, $"{client.username} {setter}", true);
                xlbxChat.Items.Add($"You removed {client.username} from Admin");
            }
        }

        private void xtxtbxSendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string message = xtbxSendMessage.Text;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (FrmHolder.hosting || message[0] == '/')
                    {
                        xlbxChat.Items.Add($"{FrmHolder.username}: {message}");
                    }
                    if (ProcessCommand(message) == false)
                    {
                        if (connectedClients.Count > 0)
                        {
                            message = message.Trim();
                            for (int i = 0; i < connectedClients.Count; i++)
                            {
                                Send(connectedClients[i], 2, $"{FrmHolder.username} {message}", true);
                            }
                        }
                    }
                    xtbxSendMessage.Clear();
                }
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
            bool serverClose = false;
            if (FrmHolder.hosting)
            {
                DialogResult dialogResult = MessageBox.Show("This will terminate the server. Are you sure?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.OK)
                {
                    serverClose = true;
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
            if (serverClose || FrmHolder.hosting == false)
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
            SetParentFormWindowText(false);
            frmLoginScreen.Show();
            this.Close();
        }
    }

    public class Client
    {
        public int clientId;
        public string username;
        public TcpClient tcpClient;
        public bool admin = false;
        public bool serverMuted = false;
        public bool serverDeafened = false;
        public bool heartbeatReceieved = false;
        public int heartbeatFailures = 0;
    }

    public class Message
    {
        public int messageId;
        public int messageType;
        public string message;

        public Message(int messageId, int messageType, string message)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.message = message;
        }

        public Message(int messageType, string message)
        {
            this.messageType = messageType;
            this.message = message;
        }
    }
}