﻿using System;
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
        private CancellationTokenSource serverCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource clientCancellationTokenSource = new CancellationTokenSource();
        private int port = 12210;
        private string publicIp;
        private string localIp;
        private int nextAssignableClientId = 0;
        private List<Client> connectedClients = new List<Client>();
        private bool askToClose = true;
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
            BeginNetworkThreads();
        }

        private void BeginNetworkThreads()
        {
            localIp = GetLocalIp();
            xlsvConnectedUsers.Items.Add(FrmHolder.username);
            if (FrmHolder.hosting)
            {
                publicIp = new WebClient().DownloadString("https://ipv4.icanhazip.com/");
                //publicIp = localIp; // For use if unable to access internet/port forward
                publicIp = publicIp.Trim();

                serverThread = new Thread(new ParameterizedThreadStart(StartServer));
                serverThread.IsBackground = true;
                serverThread.Start(serverCancellationTokenSource.Token);

                PrintChatMessage($"Server started on: {publicIp}");
            }
            else
            {
                publicIp = FrmHolder.joinIP;

                clientThread = new Thread(new ParameterizedThreadStart(StartClient));
                clientThread.IsBackground = true;
                clientThread.Start(clientCancellationTokenSource.Token);

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

        private void StartServer(object obj)
        {
            Thread.Sleep(50);
            CancellationToken cancellationToken = (CancellationToken)obj;

            IPAddress iPAddress = IPAddress.Parse(localIp);
            TcpListener tcpListener = new TcpListener(iPAddress, port);

            Client client = new Client();
            client.clientId = nextAssignableClientId;
            nextAssignableClientId++;
            client.username = FrmHolder.username;
            client.admin = true;
            connectedClients.Add(client);

            tcpListener.Start();
            StartHeartbeat();

            while (cancellationToken.IsCancellationRequested == false)
            {
                ServerAcceptIncomingConnection(tcpListener);
                LoopClientsForIncomingMessages();
            }

            tcpListener.Stop();
            FrmHolder.clientId = -1;
            if (connectedClients.Count > 0)
            {
                SendDisconnect(null, false, true);
            }
            while (serverThread.IsAlive)
            {
                //Loops to keep application open to properly disconnect
            }
        }

        private void StartClient(object obj)
        {
            Thread.Sleep(50);
            CancellationToken cancellationToken = (CancellationToken)obj;
            Client client = new Client();

            ConnectClient(client);
            StartHeartbeat();

            while (cancellationToken.IsCancellationRequested == false)
            {
                LoopClientsForIncomingMessages();
            }
            FrmHolder.clientId = -1;
            if (connectedClients.Count > 0)
            {
                SendDisconnect(client, false, false);
            }
            while (clientThread.IsAlive)
            {
                //Loops to keep application open to properly disconnect
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
                    if (FrmHolder.hosting && connectedClients[0].Equals(client))
                    {
                        continue;
                    }
                    if (client.heartbeatReceieved == false)
                    {
                        client.heartbeatFailures++;
                    }
                    if (client.heartbeatFailures == 5 && FrmHolder.hosting == false)
                    {
                        ConnectClient(client);
                    }
                    else if ((client.heartbeatFailures >= 12 && FrmHolder.hosting) || (client.heartbeatFailures >= 10 && FrmHolder.hosting == false))
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
                            SendMessage(client, ComposeMessage(client, -1, 11, null)); // Send heartbeat
                        }
                    }
                    client.heartbeatReceieved = false;
                }
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
                SendToAll(ignoredClients, 13, client.username);
                UpdateClientLists();
                PrintChatMessage($"Lost connection to {client.username}...");
            }
            else
            {
                PrintChatMessage($"Lost connection to server...");
            }
        }

        private void SendFirstMessageInMessageQueue(Client client)
        {
            if (client.messagesToBeSent.Count > 0 && client.sendingMessageQueue)
            {
                Message message = client.messagesToBeSent[0];
                SendMessage(client, message);
            }
            else
            {
                client.sendingMessageQueue = false;
                if (FrmHolder.hosting == false)
                {
                    SendMessage(client, ComposeMessage(client, -1, 18, null));
                    client.receivingMessageQueue = true;
                }
            }
        }

        private void ConnectClient(Client client)
        {
            client.connectionSetupComplete = false;
            client.sendingMessageQueue = false;
            client.receivingMessageQueue = false;
            if (client.tcpClient != null)
            {
                client.tcpClient.Close();
            }
            if (connectedClients.Contains(client) == false)
            {
                connectedClients.Add(client);
            }
            client.tcpClient = new TcpClient();
            client.tcpClient.Connect(publicIp, port);
            string clientIdPart = "";
            if (FrmHolder.clientId != -1)
            {
                clientIdPart = $" {Convert.ToString(FrmHolder.clientId)}";
            }
            SendMessage(connectedClients[0], ComposeMessage(connectedClients[0], -1, 0, $"{FrmHolder.username}{clientIdPart}"));
        }

        private void ServerAcceptIncomingConnection(TcpListener tcpListener)
        {
            if (tcpListener.Pending())
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Client client = new Client();
                client.tcpClient = tcpClient;
                client.nextAssignableMessageId = 1;
                connectedClients.Add(client);
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

        public void SendMessage(Client client, Message message)
        {
            if (CheckAddMessageToQueue(client, message, false))
            {
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

                            // Message text
                            byte[] textBuffer = null;
                            if (message.messageText != null)
                            {
                                textBuffer = Encoding.Unicode.GetBytes(message.messageText);
                            }

                            // Message length
                            byte[] lengthBuffer = new byte[4];
                            if (message.messageText != null)
                            {
                                lengthBuffer = BitConverter.GetBytes(textBuffer.Length);
                            }

                            ConvertLittleEndianToBigEndian(idBuffer);
                            ConvertLittleEndianToBigEndian(typeBuffer);
                            ConvertLittleEndianToBigEndian(textBuffer);
                            ConvertLittleEndianToBigEndian(lengthBuffer);

                            networkStream.Write(idBuffer, 0, 4); // Message ID
                            networkStream.Write(typeBuffer, 0, 4); // Message type
                            networkStream.Write(lengthBuffer, 0, 4); // Message length
                            if (message.messageText != null)
                            {
                                networkStream.Write(textBuffer, 0, textBuffer.Length); // Message text
                            }
                            if (message.messageType != 1 && message.messageType != 3 && message.messageType != 11)
                            {
                                AddMessageToMessageListBySendPriority(client.messagesToBeSent, message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.InvalidOperationException)
            {
                CheckAddMessageToQueue(client, message, true);
            }
        }

        public void ReceiveMessage(Client client)
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

                            // Message text
                            string messageText = null;
                            int receivedLength = 0;
                            int totalReceivedLength = 0;
                            if (messageLength > 0)
                            {
                                byte[] textBuffer = new byte[messageLength];
                                byte[] tempTextBuffer = new byte[messageLength];
                                while (totalReceivedLength < messageLength)
                                {
                                    receivedLength = 0;
                                    receivedLength += networkStream.Read(tempTextBuffer, 0, messageLength - totalReceivedLength);
                                    Array.Resize(ref tempTextBuffer, receivedLength);
                                    tempTextBuffer.CopyTo(textBuffer, totalReceivedLength);
                                    tempTextBuffer = new byte[messageLength];
                                    totalReceivedLength += receivedLength;
                                }
                                ConvertLittleEndianToBigEndian(textBuffer);
                                messageText = Encoding.Unicode.GetString(textBuffer);
                            }

                            Message receivedMessage = ComposeMessage(client, messageId, messageType, messageText);
                            if (xlbxChat.InvokeRequired)
                            {
                                xlbxChat.BeginInvoke(new MessageDelegate(ProcessMessage), client, receivedMessage);
                            }
                            else
                            {
                                ProcessMessage(client, receivedMessage);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.InvalidOperationException) // TODO: Avoid catching exceptions every time a read is done on a non-connected socket
            {
                return;
            }
        }

        private bool CheckAddMessageToQueue(Client client, Message message, bool sendFailed)
        {
            if (client.messagesToBeSent.Count > 0 && client.messagesToBeSent[0] == message)
            {
                return false;
            }
            if (message.messageType != 1 && message.messageType != 11)
            {
                if (sendFailed)
                {
                    AddMessageToMessageListBySendPriority(client.messagesToBeSent, message);
                    return true;
                }
                else
                {
                    if (client.messagesToBeSent.Count > 0)
                    {
                        if (message.messageSendPriority > 0)
                        {
                            if (client.sendingMessageQueue || client.receivingMessageQueue)
                            {
                                AddMessageToMessageListBySendPriority(client.messagesToBeSent, message);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private Message ComposeMessage(Client client, int messageId, int messageType, string messageText)
        {
            if (messageId == -1)
            {
                messageId = client.nextAssignableMessageId;
                client.nextAssignableMessageId += 2;
            }
            Message message = new Message(messageId, messageType, messageText);
            return message;
        }

        private void ProcessMessage(Client client, Message message)
        {
            client.heartbeatReceieved = true;
            client.heartbeatFailures = 0;
            if (message.messageType != 1 && message.messageType != 3 && message.messageType != 11)
            {
                SendMessage(client, ComposeMessage(client, message.messageId, 1, null)); // Acknowledge received message
                foreach (Message alreadyReceivedMessage in client.messagesReceived)
                {
                    if (message.messageId == alreadyReceivedMessage.messageId)
                    {
                        return;
                    }
                }
                client.messagesReceived.Add(message);
            }

            if (message.messageType == 0) // Connection Request [username, clientId (if reconnecting)]
            {
                string[] parts = message.messageText.Split(' ', 2);
                string username = parts[0];
                int clientId = -1;
                if (parts.Length > 1)
                {
                    clientId = Convert.ToInt32(parts[1]);
                }
                bool usernameAlreadyInUse = false;
                for (int i = 0; i < connectedClients.Count; i++)
                {
                    if (clientId != -1)
                    {
                        if (clientId == connectedClients[i].clientId)
                        {
                            client = MergeClient(client, connectedClients[i]);
                        }
                    }
                    if (string.Equals(connectedClients[i].username, username, StringComparison.OrdinalIgnoreCase))
                    {
                        if (clientId != connectedClients[i].clientId)
                        {
                            usernameAlreadyInUse = true;
                            SendMessage(client, ComposeMessage(client, -1, 4, null));
                            break;
                        }
                    }
                }
                if (usernameAlreadyInUse == false)
                {
                    client.username = username;
                    if (client.clientId == -1)
                    {
                        client.clientId = nextAssignableClientId;
                        nextAssignableClientId++;
                        SendMessage(client, ComposeMessage(client, -1, 12, client.clientId.ToString()));

                        List<Client> ignoredClients = new List<Client>();
                        ignoredClients.Add(client);
                        PrintChatMessage($"{client.username} connected");
                        SendToAll(ignoredClients, 5, client.username);
                        UpdateClientLists();
                    }
                    client.connectionSetupComplete = true;
                    SendMessage(client, ComposeMessage(client, -1, 19, null));
                    SendMessage(client, ComposeMessage(client, -1, 18, null));
                    client.receivingMessageQueue = true;
                }
            }
            else if (message.messageType == 1) // Message Acknowledgement
            {
                foreach (Message item in client.messagesToBeSent)
                {
                    if (item.messageId == message.messageId)
                    {
                        client.messagesSent.Add(item);
                        client.messagesToBeSent.Remove(item);
                        break;
                    }
                }
                if (client.sendingMessageQueue)
                {
                    SendFirstMessageInMessageQueue(client);
                }
            }
            else if (message.messageType == 2) // Message recieve [username, message]
            {
                string[] parts = message.messageText.Split(' ', 2);
                string username = parts[0];
                string messageText = parts[1];
                PrintChatMessage($"{username}: {messageText}");
                if (FrmHolder.hosting)
                {
                    SendToAll(null, 2, message.messageText);
                }
            }
            else if (message.messageType == 3) // Disconnect
            {
                if (FrmHolder.hosting)
                {
                    connectedClients.Remove(client);
                    if (client.tcpClient != null)
                    {
                        client.tcpClient.Close();
                    }
                    if (client.username != null)
                    {
                        PrintChatMessage($"{client.username} disconnected");
                        SendToAll(null, 6, client.username);
                    }
                    UpdateClientLists();
                }
                else
                {
                    if (clientThread != null && clientThread.IsAlive)
                    {
                        clientCancellationTokenSource.Cancel();
                    }
                    MessageBox.Show("The server was closed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OpenMainMenu();
                }
            }
            else if (message.messageType == 4) // Username already used
            {
                clientCancellationTokenSource.Cancel();
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
                    clientCancellationTokenSource.Cancel();
                }
                string[] parts = message.messageText.Split(' ', 2);
                string username = parts[0];
                string reason = parts[1];
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
                string[] parts = message.messageText.Split(' ', 3);
                string username = parts[0];
                string kickerUsername = parts[1];
                string reason = parts[2];
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
            else if (message.messageType == 11) // Heartbeat
            {
                if (FrmHolder.hosting)
                {
                    SendMessage(client, ComposeMessage(client, -1, 11, null));
                }
            }
            else if (message.messageType == 12) // Set clientId
            {
                FrmHolder.clientId = Convert.ToInt32(message.messageText);
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
                string[] parts = message.messageText.Split(' ', 2);
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
                string[] parts = message.messageText.Split(' ', 2);
                string username = parts[0];
                string setterUsername = parts[1];
                PrintChatMessage($"{username} has been removed from Admin by {setterUsername}");
            }
            else if (message.messageType == 18) // Send message queue
            {
                client.sendingMessageQueue = true;
                client.receivingMessageQueue = false;
                SendFirstMessageInMessageQueue(client);
            }
            else if (message.messageType == 19) // Connection setup complete
            {
                client.connectionSetupComplete = true;
            }
        }

        private void AddMessageToMessageListBySendPriority(List<Message> list, Message message)
        {
            bool added = false;
            if (list.Contains(message) == false)
            {
                for (int i = 0; i < list.Count(); i++)
                {
                    if (list[i].messageSendPriority > message.messageSendPriority)
                    {
                        list.Insert(i, message);
                        added = true;
                        break;
                    }
                }
                if (added == false)
                {
                    list.Add(message);
                }
            }
        }

        private void RemoveMessageFromMessageListByTypeAndOrSendPriority(List<Message> list, int? messageType, int? messageSendPriority)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                if (messageType != null && messageSendPriority != null)
                {
                    if (list[i].messageType == messageType && list[i].messageSendPriority == messageSendPriority)
                    {
                        list.RemoveAt(i);
                    }
                }
                else if (messageType != null)
                {
                    if (list[i].messageType == messageType)
                    {
                        list.RemoveAt(i);
                    }
                }
                else if (messageSendPriority != null)
                {
                    if (list[i].messageSendPriority == messageSendPriority)
                    {
                        list.RemoveAt(i);
                    }
                }
            }
        }

        private Client MergeClient(Client clientMergeFrom, Client clientMergeTo)
        {
            //clientMergeTo.clientId = clientMergeFrom.clientId;
            clientMergeTo.nextAssignableMessageId = (clientMergeTo.nextAssignableMessageId + clientMergeFrom.nextAssignableMessageId);
            //clientMergeTo.username = clientMergeFrom.username;
            clientMergeTo.tcpClient = clientMergeFrom.tcpClient;

            //clientMergeTo.admin = clientMergeFrom.admin;
            //clientMergeTo.serverMuted = clientMergeFrom.serverMuted;
            //clientMergeTo.serverDeafened = clientMergeFrom.serverDeafened;

            clientMergeTo.heartbeatReceieved = clientMergeFrom.heartbeatReceieved;
            clientMergeTo.heartbeatFailures = clientMergeFrom.heartbeatFailures;

            clientMergeTo.messagesSent.AddRange(clientMergeFrom.messagesSent);
            clientMergeTo.messagesToBeSent.AddRange(clientMergeFrom.messagesToBeSent);
            clientMergeTo.messagesReceived.AddRange(clientMergeFrom.messagesReceived);

            connectedClients.Remove(clientMergeFrom);
            return clientMergeTo;
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
            SendToAll(null, 7, null);
            string[] usernames = GetClientUsernames();
            for (int i = 0; i < usernames.Length; i++)
            {
                xlsvConnectedUsers.Items.Add(usernames[i]);
                SendToAll(null, 8, usernames[i]);
            }
        }

        private void LoopClientsForIncomingMessages()
        {
            for (int i = 0; i < connectedClients.Count(); i++)
            {
                ReceiveMessage(connectedClients[i]);
            }
        }

        private void SendToAll(List<Client> ignoredClients, int messageType, string messageText) //TODO: Replace messagType and messagText with Message class
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                if (ignoredClients != null)
                {
                    for (int j = 0; j < ignoredClients.Count; j++)
                    {
                        if (connectedClients[i] != ignoredClients[j])
                        {
                            SendMessage(connectedClients[i], ComposeMessage(connectedClients[i], -1, messageType, messageText));
                            break;
                        }
                    }
                }
                else
                {
                    SendMessage(connectedClients[i], ComposeMessage(connectedClients[i], -1, messageType, messageText));
                }
            }
        }

        private void SendDisconnect(Client client, bool kick, bool sendToAll)
        {
            int type = kick == false ? 3 : 9;
            if (sendToAll)
            {
                List<Client> ignoredClients = new List<Client>();
                ignoredClients.Add(client);
                SendToAll(ignoredClients, type, null);
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
                SendMessage(client, ComposeMessage(client, -1, type, null));
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
                if (BeginDisconnect() == false)
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
            List<Client> ignoredClients = new List<Client>();
            ignoredClients.Add(clients[0]);
            SendMessage(clients[0], ComposeMessage(clients[0], -1, 9, $"{FrmHolder.username} {reason}")); // Kick client
            SendToAll(ignoredClients, 10, $"{username[0]} {FrmHolder.username} {reason}");
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
            ignoredClients ??= new List<Client>();
            ignoredClients.Add(client);
            if (setAsAdmin)
            {
                SendMessage(client, ComposeMessage(client, -1, 14, setter));
                SendToAll(ignoredClients, 15, $"{client.username} {setter}");
                PrintChatMessage($"You made {client.username} an Admin");
            }
            else
            {
                SendMessage(client, ComposeMessage(client, -1, 16, setter));
                SendToAll(ignoredClients, 17, $"{client.username} {setter}");
                PrintChatMessage($"You removed {client.username} from Admin");
            }
        }

        private bool BeginDisconnect()
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
                        serverCancellationTokenSource.Cancel();
                    }
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    return false;
                }
            }
            if (clientThread != null && clientThread.IsAlive)
            {
                clientCancellationTokenSource.Cancel();
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

        private void xtxtbxSendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string message = xtbxSendMessage.Text;
                message = message.Trim();
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
                                SendMessage(connectedClients[i], ComposeMessage(connectedClients[i], -1, 2, $"{FrmHolder.username} {message}"));
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
            BeginDisconnect();
        }
    }
}