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
        private Thread serverThread;
        private Thread clientThread;
        private int port = 12210;
        private string publicIp;
        private string localIp;
        private int nextAssignableClientId = 0;
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
                serverThread.IsBackground = true;
                serverThread.Start();
                PrintChatMessage($"Server started on: {publicIp}");
            }
            else
            {
                publicIp = FrmHolder.joinIP;
                clientThread = new Thread(new ThreadStart(StartClient));
                clientThread.IsBackground = true;
                clientThread.Start();
                PrintChatMessage($"Connected to server on: {publicIp}"); //TODO: Only if succesfully connected - use acknowledgement
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
            client.clientId = nextAssignableClientId;
            nextAssignableClientId++;
            client.username = FrmHolder.username;
            client.admin = true;
            connectedClients.Add(client);
            try
            {
                tcpListener.Start();
                StartHeartbeat();
                while (true)
                {
                    ServerAcceptIncomingConnection(tcpListener);
                    LoopClientsForIncomingMessages();
                }
            }
            catch (ThreadAbortException)
            {
                if (connectedClients.Count > 0)
                {
                    SendDisconnect(null, false, true);
                }
                tcpListener.Stop();
                while (serverThread.IsAlive)
                {
                    //Loops to keep application open to properly disconnect
                }
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
                SendMessage(connectedClients[0], 0, $"{FrmHolder.username}", true, -1);
                StartHeartbeat();
                while (true)
                {
                    LoopClientsForIncomingMessages();
                }
            }
            catch (ThreadAbortException)
            {
                if (connectedClients.Count > 0)
                {
                    SendDisconnect(client, false, false);
                }
                while (clientThread.IsAlive)
                {
                    //Loops to keep application open to properly disconnect
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
                foreach (Client client in connectedClients)
                {
                    if (FrmHolder.hosting && connectedClients.ElementAtOrDefault(0).Equals(client))
                    {
                        continue;
                    }
                    if (client.heartbeatReceieved == false)
                    {
                        client.heartbeatFailures++;
                    }
                    if ((client.heartbeatFailures == 6 && FrmHolder.hosting) || (client.heartbeatFailures == 5 && FrmHolder.hosting == false))
                    {
                        if (xlbxChat.InvokeRequired)
                        {
                            xlbxChat.BeginInvoke(new HeartbeatDelegate(HeartbeatTimeoutFailure), client);
                        }
                    }
                    if (FrmHolder.hosting == false)
                    {
                        if (client.tcpClient != null)
                        {
                            SendMessage(client, 11, null, true, -1); // Send heartbeat
                        }
                    }
                    client.heartbeatReceieved = false;
                }
            }
        }

        /*private void Heartbeat_Tick(object sender, EventArgs e)
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
                            SendMessage(client, 11, null, true, -1);
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
                                xlbxChat.BeginInvoke(new HeartbeatDelegate(HeartbeatTimeoutFailure), client);
                            }
                        }
                        client.heartbeatReceieved = false;
                    }
                }
            }
        }*/

        private void ServerAcceptIncomingConnection(TcpListener tcpListener)
        {
            if (tcpListener.Pending())
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Client client = new Client();
                client.tcpClient = tcpClient;
                client.clientId = nextAssignableClientId;
                nextAssignableClientId++;
                client.nextAssignableMessageId = 1;
                connectedClients.Add(client);
                SendMessage(connectedClients[0], 1, null, true, 0); // Acknowledgement ID?
            }
        }

        private void ConvertLittleEndianToBigEndian(byte[] byteArray) // Converts byte array from Little-Endian/Host Byte Order to Big-Endian/Network Byte Order for network tranfer if host machine stores bytes in Little Endian (and back if needed)
        {
            if (byteArray != null)
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(byteArray);
                }
            }
        }

        public void SendMessage(Client client, int messageType, string messageText, bool process, int acknowledgementMessageId)
        {
            if (messageType >= 0 && messageType <= 255)
            {
                Message message;
                if (messageType == 1) // Acknowledgement
                {
                    message = ComposeMessage(acknowledgementMessageId, messageType, messageText);
                }
                else
                {
                    message = ComposeMessage(client.nextAssignableMessageId, messageType, messageText);
                    client.nextAssignableMessageId += 2;
                }
                if (client.messageToBeSent.Count > 0 && message.messageType != 11) //Heartbeat
                {
                    client.messageToBeSent.Add(message);
                    return;
                }
                try
                {
                    if (client.tcpClient != null)
                    {
                        NetworkStream networkStream = client.tcpClient.GetStream();
                        {
                            if (networkStream.CanWrite && networkStream.CanRead)
                            {
                                // Message ID
                                byte[] idBuffer = new byte[4];
                                idBuffer = BitConverter.GetBytes(message.messageId);

                                // Message type
                                byte[] typeBuffer = new byte[4];
                                typeBuffer = BitConverter.GetBytes(message.messageType);

                                // Message content
                                byte[] messageBuffer = null;
                                if (messageText != null)
                                {
                                    messageBuffer = Encoding.ASCII.GetBytes(message.messageText);
                                }

                                // Message length
                                byte[] lengthBuffer = new byte[4];
                                if (messageText != null)
                                {
                                    lengthBuffer = BitConverter.GetBytes(messageBuffer.Length);
                                }

                                ConvertLittleEndianToBigEndian(idBuffer);
                                ConvertLittleEndianToBigEndian(typeBuffer);
                                ConvertLittleEndianToBigEndian(messageBuffer);
                                ConvertLittleEndianToBigEndian(lengthBuffer);

                                networkStream.Write(idBuffer, 0, 4); // Message ID
                                networkStream.Write(typeBuffer, 0, 4); // Message type
                                networkStream.Write(lengthBuffer, 0, 4); // Message length
                                if (messageText != null)
                                {
                                    networkStream.Write(messageBuffer, 0, messageBuffer.Length); // Message content
                                }
                                if (messageType != 1 && messageType != 11)
                                {
                                    client.messageSentNotAcknowledged.Add(message);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is System.IO.IOException || ex is System.InvalidOperationException)
                {
                    if (message.messageType != 11) // Heartbeat
                    {
                        client.messageToBeSent.Add(message);
                    }
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("'messageType' must be between 0 and 255");
            }
        }

        public void ReceiveMessage(Client client, bool process)
        {
            try
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

                            // Message type
                            byte[] typeBuffer = new byte[4];
                            networkStream.Read(typeBuffer, 0, 4);

                            // Message length
                            byte[] lengthBuffer = new byte[4];
                            networkStream.Read(lengthBuffer, 0, 4);

                            ConvertLittleEndianToBigEndian(idBuffer);
                            ConvertLittleEndianToBigEndian(typeBuffer);
                            ConvertLittleEndianToBigEndian(lengthBuffer);

                            int messageId = BitConverter.ToInt32(idBuffer, 0);
                            int messageType = BitConverter.ToInt32(typeBuffer, 0);
                            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                            // Message content
                            string messageText = null;
                            if (messageLength > 0)
                            {
                                byte[] messageBuffer = new byte[messageLength];
                                networkStream.Read(messageBuffer, 0, messageLength);
                                ConvertLittleEndianToBigEndian(messageBuffer);
                                messageText = Encoding.ASCII.GetString(messageBuffer);
                            }

                            Message recievedMessage = ComposeMessage(messageId, messageType, messageText);
                            if (process)
                            {
                                if (xlbxChat.InvokeRequired)
                                {
                                    xlbxChat.BeginInvoke(new MessageDelegate(ClientProcessMessage), client, recievedMessage);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.InvalidOperationException)
            {
                return;
            }
        }

        private Message ComposeMessage(int messageId, int messageType, string messageText)
        {
            Message message = new Message(messageId, messageType, messageText);
            return message;
        }

        private void ClientProcessMessage(Client client, Message message)
        {
            if (message.messageType != 1 && message.messageType != 3 && message.messageType != 11)
            {
                client.messageReceived.Add(message);
                SendMessage(client, 1, null, true, message.messageId); // Acknowledge received message
            }

            //0 = client-server connection request; 1 = message acknowledgement; 2 = recieve message; 3 = disconnect; 4 = server-client username already used
            if (message.messageType == 0) // Connection Request [username]
            {
                bool usernameInUse = false;
                for (int i = 0; i < connectedClients.Count; i++)
                {
                    if (string.Equals(connectedClients[i].username, message.messageText, StringComparison.OrdinalIgnoreCase) || FrmHolder.username == message.messageText)
                    {
                        usernameInUse = true;
                        SendMessage(client, 4, null, true, -1);
                        break;
                    }
                }
                if (usernameInUse == false)
                {
                    client.username = message.messageText;
                    List<Client> exceptions = new List<Client>();
                    exceptions.Add(client);
                    PrintChatMessage($"{client.username} connected");
                    SendToAll(exceptions, 5, client.username, true);
                    UpdateClientLists();
                }
            }
            else if (message.messageType == 1) // Message Acknowledgement
            {
                foreach(Message item in client.messageSentNotAcknowledged)
                {
                    if (item.messageId == message.messageId)
                    {
                        client.messageSentAcknowledged.Add(item);
                        client.messageSentNotAcknowledged.Remove(item);
                        break;
                    }
                }
            }
            else if (message.messageType == 2) // Message recieve [username, message]
            {
                string[] parts = message.messageText.Split(' ');
                string username = parts[0];
                string messageText = "";
                for (int i = 1; i < parts.Length; i++)
                {
                    messageText += parts[i] + ' ';
                }
                messageText = messageText.Trim();
                PrintChatMessage($"{username}: {messageText}");
                if (FrmHolder.hosting)
                {
                    SendToAll(null, 2, message.messageText, true);
                }
            }
            else if (message.messageType == 3) // Disconnect
            {
                if (FrmHolder.hosting)
                {
                    if (client.username != null)
                    {
                        List<Client> ignoredClients = new List<Client>();
                        ignoredClients.Add(client);
                        PrintChatMessage($"{client.username} disconnected");
                        SendToAll(ignoredClients, 6, client.username, true);
                    }
                    connectedClients.Remove(client);
                    //SendDisconnect(client, false, false);
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
                PrintChatMessage($"{message.messageText} connected");
            }
            else if (message.messageType == 6) // Client disconnected
            {
                PrintChatMessage($"{message.messageText} disconnected");
            }
            else if (message.messageType == 7) // Clear user list
            {
                xlsvConnectedUsers.Items.Clear();
            }
            else if (message.messageType == 8) // Add to user list
            {
                xlsvConnectedUsers.Items.Add(message.messageText);
            }
            else if (message.messageType == 9) // Kicked
            {
                if (clientThread != null && clientThread.IsAlive)
                {
                    clientThread.Abort();
                }
                string[] parts = message.messageText.Split(' ');
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
                string[] parts = message.messageText.Split(' ');
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
                    PrintChatMessage($"{username} was kicked by {kickerUsername} with reason: {reason}");
                }
                else
                {
                    PrintChatMessage($"{username} was kicked by {kickerUsername}");
                }
            }
            else if (message.messageType == 11) // Heartbeat received
            {
                client.heartbeatReceieved = true;
                client.heartbeatFailures = 0;
                if (FrmHolder.hosting)
                {
                    SendMessage(client, 11, null, true, -1);
                }
            }
            else if (message.messageType == 12) // Heartbeat failed
            {
                HeartbeatTimeoutFailure(client);
            }
            else if (message.messageType == 13) // Another client heartbeat failed
            {
                PrintChatMessage($"{message.messageText} has lost connection...");
            }
            else if (message.messageType == 14) // Made admin
            {
                PrintChatMessage($"You have been made an Admin by {message.messageText}");
            }
            else if (message.messageType == 15) // Another made admin
            {
                string[] parts = message.messageText.Split(' ');
                string username = parts[0];
                string setterUsername = parts[1];
                PrintChatMessage($"{username} has been made an Admin by {setterUsername}");
            }
            else if (message.messageType == 16) // Removed admin
            {
                PrintChatMessage($"You have been removed from Admin by {message.messageText}");
            }
            else if (message.messageType == 17) // Another removed admin
            {
                string[] parts = message.messageText.Split(' ');
                string username = parts[0];
                string setterUsername = parts[1];
                PrintChatMessage($"{username} has been removed from Admin by {setterUsername}");
            }
        }

        private void HeartbeatTimeoutFailure(Client client)
        {
            if (client.tcpClient != null)
            {
                client.tcpClient.Close();
            }
            connectedClients.Remove(client);
            if (FrmHolder.hosting)
            {
                List<Client> ignoredClients = new List<Client>();
                ignoredClients.Add(client);
                //SendMessage(client, 12, null, true, -1);
                SendToAll(ignoredClients, 13, client.username, true);
                UpdateClientLists();
                PrintChatMessage($"Lost connection to {client.username}...");
            }
            else
            {
                PrintChatMessage($"Lost connection to server...");
            }
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

        private void LoopClientsForIncomingMessages()
        {
            for (int i = 0; i < connectedClients.Count(); i++)
            {
                ReceiveMessage(connectedClients[i], true);
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
                            SendMessage(connectedClients[i], messageType, message, process, -1);
                            break;
                        }
                    }
                }
                else
                {
                    SendMessage(connectedClients[i], messageType, message, process, -1);
                }
            }
        }

        private void SendDisconnect(Client client, bool kick, bool sendToAll)
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
                SendMessage(client, type, null, true, -1);
                if (client.tcpClient != null)
                {
                    client.tcpClient.Close();
                }
                connectedClients.Remove(client);
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (askToClose) // Prevents closing when returning to main menu
            {
                if (BeginDisconnect(false) == false)
                {
                    e.Cancel = true;
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
                    PrintChatMessage("You must be an admin to execute commands"); //TODO: Allow non-admin commands (e.g. /help)
                    return true;
                }
                string command = message.Substring(1, message.Length - 1);
                string[] commandParts = command.Split(' ');
                if (commandParts[0] == "help" || commandParts[0] == "commands")
                {
                    RunCommandHelp(commandParts);
                }
                else if (commandParts[0] == "kick") //TODO: Admin only
                {
                    return RunCommandKick(commandParts);
                }
                else if (commandParts[0] == "admin" && FrmHolder.hosting)
                {
                    return RunCommandAdmin(commandParts);
                }
                return true;
            }
            return false; //true if commmand
        }

        private void PrintChatMessage(string chatMessage)
        {
            xlbxChat.Items.Add(chatMessage);
        }

        private void RunCommandHelp(string[] commandParts)
        {
            if (commandParts.Length == 1)
            {
                PrintChatMessage($"Available commands are 'kick' and 'admin'. Type '/help [command]' for an explanation of the command.");
            }
            else if (commandParts[1] == "kick")
            {
                PrintChatMessage($"Explanation: Kicks a user. Format: {kickFormat}");
            }
            else if (commandParts[1] == "admin")
            {
                PrintChatMessage($"Explanation: Adds/removes a user from Admin. Format: {adminFormat}");
            }
            else
            {
                PrintChatMessage($"No command {commandParts[1]} exists.");
            }
        }

        private bool RunCommandKick(string[] commandParts)
        {
            if (commandParts[1] == null)
            {
                PrintChatMessage($"The format is: {kickFormat}");
                return true;
            }
            string[] username = { commandParts[1] };
            List<Client> clients = ClientSearch(username, null);
            if (clients.Count == 0)
            {
                PrintChatMessage($"No user with the username {username[0]} exists");
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
                PrintChatMessage($"You kicked {username[0]} with reason: {reason}");
            }
            else
            {
                PrintChatMessage($"You kicked {username[0]}");
            }
            List<Client> exceptions = new List<Client>();
            exceptions.Add(clients[0]);
            SendMessage(clients[0], 9, $"{FrmHolder.username} {reason}", true, -1); // Kick client
            SendToAll(exceptions, 10, $"{username[0]} {FrmHolder.username} {reason}", true);
            return false;
        }

        private bool RunCommandAdmin(string[] commandParts)
        {
            if (commandParts.Length < 2 || commandParts[1] == null)
            {
                PrintChatMessage($"The format is: {adminFormat}");
                return true;
            }
            string[] username = { commandParts[1] };
            List<Client> clients = ClientSearch(username, null);
            if (clients.Count == 0)
            {
                PrintChatMessage($"No user with the username {username[0]} exists");
                return true;
            }
            bool setAsAdmin = false;
            if (commandParts.Length > 2 && commandParts[2] != null)
            {
                if (string.Equals(commandParts[2], "True", StringComparison.OrdinalIgnoreCase))
                {
                    if (clients[0].admin)
                    {
                        PrintChatMessage($"This user is already an Admin");
                        return true;
                    }
                    setAsAdmin = true;
                }
                else if (string.Equals(commandParts[2], "False", StringComparison.OrdinalIgnoreCase))
                {
                    if (clients[0].admin == false)
                    {
                        PrintChatMessage($"This user is already not an Admin");
                        return true;
                    }
                    setAsAdmin = false;
                }
                else
                {
                    PrintChatMessage($"The format is: {adminFormat}");
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
                SendMessage(client, 14, setter, true, -1);
                SendToAll(ignoredClients, 15, $"{client.username} {setter}", true);
                PrintChatMessage($"You made {client.username} an Admin");
            }
            else
            {
                SendMessage(client, 16, setter, true, -1);
                SendToAll(ignoredClients, 17, $"{client.username} {setter}", true);
                PrintChatMessage($"You removed {client.username} from Admin");
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
                        PrintChatMessage($"{FrmHolder.username}: {message}");
                    }
                    if (ProcessCommand(message) == false)
                    {
                        if (connectedClients.Count > 0)
                        {
                            message = message.Trim();
                            for (int i = 0; i < connectedClients.Count; i++)
                            {
                                SendMessage(connectedClients[i], 2, $"{FrmHolder.username} {message}", true, -1);
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
            BeginDisconnect(true);
        }

        private bool BeginDisconnect(bool returnToMainMenu)
        {
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
                else if (dialogResult == DialogResult.Cancel)
                {
                    return false;
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
            return true;
        }

        private void OpenMainMenu()
        {
            askToClose = false;
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
        public int nextAssignableMessageId = 0;
        public string username;
        public TcpClient tcpClient;

        public bool admin = false;
        public bool serverMuted = false;
        public bool serverDeafened = false;

        public bool heartbeatReceieved = false;
        public int heartbeatFailures = 0;

        public List<Message> messageSentNotAcknowledged = new List<Message>();
        public List<Message> messageSentAcknowledged = new List<Message>();
        public List<Message> messageToBeSent = new List<Message>();
        public List<Message> messageReceived = new List<Message>();
    }

    public class Message
    {
        public int messageId;
        public int messageType;
        public string messageText;

        public Message(int messageId, int messageType, string messageText)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageText = messageText;
        }

        public Message(int messageType, string messageText)
        {
            this.messageType = messageType;
            this.messageText = messageText;
        }
    }
}