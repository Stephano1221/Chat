using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Chat
{
    class Network
    {
        public int port = 12210;
        public string publicIp;
        public string localIp;
        public int nextAssignableClientId = 0;
        public List<Client> connectedClients = new List<Client>();

        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
        public event EventHandler<string> PrintChatMessageEvent;
        public event EventHandler<Client> HeartbeatTimeoutEvent;
        public event EventHandler ClearClientListEvent;
        public event EventHandler<string> AddClientToClientListEvent;

        public Thread serverThread;
        public Thread clientThread;
        public System.Timers.Timer heartbeat = new System.Timers.Timer();

        public string GetLocalIp()
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

        public void StartServer(object obj)
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

        public void StartClient(object obj)
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

        public void StartHeartbeat()
        {
            heartbeat.Interval = 1000;
            heartbeat.Elapsed += Heartbeat_Tick;
            heartbeat.Start();
        }

        public void Heartbeat_Tick(object sender, EventArgs e)
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
                        HeartbeatTimeoutEvent.Invoke(this, client);
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

        public void HeartbeatTimeoutFailure(Client client)
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
                PrintChatMessageEvent(this, $"Lost connection to {client.username}...");
            }
            else
            {
                PrintChatMessageEvent(this, $"Lost connection to server...");
            }
        }

        public void SendFirstMessageInMessageQueue(Client client)
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

        public bool CheckAddMessageToQueue(Client client, Message message, bool sendFailed)
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

        public void AddMessageToMessageListBySendPriority(List<Message> list, Message message)
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

        public void RemoveMessageFromMessageListByTypeAndOrSendPriority(List<Message> list, int? messageType, int? messageSendPriority)
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

        public void ConnectClient(Client client)
        {
            client.encryptionEstablished = false;
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
            /*string clientIdPart = "";
            if (FrmHolder.clientId != -1)
            {
                clientIdPart = $" {Convert.ToString(FrmHolder.clientId)}";
            }*/
            client.encryption = new Encryption();
            client.encryption.keyContainerName = DateTime.Now.ToString();
            client.encryption.RsaGenerateKey(client.encryption.keyContainerName);
            string rsaKey = client.encryption.RsaExportXmlKey(client.encryption.keyContainerName, false);
            //SendMessage(connectedClients[0], ComposeMessage(connectedClients[0], -1, 0, $"{FrmHolder.username}{clientIdPart}"));
            SendMessage(connectedClients[0], ComposeMessage(connectedClients[0], -1, 20, $"{rsaKey}"));
        }

        public void ServerAcceptIncomingConnection(TcpListener tcpListener)
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

        public void LoopClientsForIncomingMessages()
        {
            for (int i = 0; i < connectedClients.Count(); i++)
            {
                ReceiveMessage(connectedClients[i]);
            }
        }

        public void ConvertLittleEndianToBigEndian(byte[] byteArray) // Converts byte array from Little-Endian/Host Byte Order to Big-Endian/Network Byte Order for network tranfer if host machine stores bytes in Little Endian (and back if needed)
        {
            if (byteArray != null)
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(byteArray);
                }
            }
        }

        public Message ComposeMessage(Client client, int messageId, int messageType, string messageText)
        {
            if (messageId == -1)
            {
                messageId = client.nextAssignableMessageId;
                client.nextAssignableMessageId += 2;
            }
            Message message = new Message(messageId, messageType, messageText);
            return message;
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
                                if (client.encryptionEstablished)
                                {
                                    textBuffer = client.encryption.AesEncryptDecrypt(Encoding.Unicode.GetBytes(message.messageText), client.encryption.AesGetKeyAndIv(client.encryption.aesEncryptedKey, client.encryption.aesEncryptedIv), true);
                                }
                                else
                                {
                                    textBuffer = Encoding.Unicode.GetBytes(message.messageText);
                                }
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
                            MessageReceivedEvent.Invoke(this, new MessageReceivedEventArgs(client, receivedMessage));
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.InvalidOperationException) // TODO: Avoid catching exceptions every time a read is done on a non-connected socket
            {
                return;
            }
        }

        public void SendToAll(List<Client> ignoredClients, int messageType, string messageText) //TODO: Replace messagType and messagText with Message class
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

        public Client MergeClient(Client clientMergeFrom, Client clientMergeTo)
        {
            //clientMergeTo.clientId = clientMergeFrom.clientId;
            clientMergeTo.nextAssignableMessageId = (clientMergeTo.nextAssignableMessageId + clientMergeFrom.nextAssignableMessageId);
            //clientMergeTo.username = clientMergeFrom.username;
            clientMergeTo.tcpClient = clientMergeFrom.tcpClient;
            clientMergeTo.encryption = clientMergeFrom.encryption;

            //clientMergeTo.admin = clientMergeFrom.admin;
            //clientMergeTo.serverMuted = clientMergeFrom.serverMuted;
            //clientMergeTo.serverDeafened = clientMergeFrom.serverDeafened;

            clientMergeTo.heartbeatReceieved = clientMergeFrom.heartbeatReceieved;
            clientMergeTo.heartbeatFailures = clientMergeFrom.heartbeatFailures;
            clientMergeTo.encryptionEstablished = clientMergeFrom.encryptionEstablished;
            //clientMergeTo.connectionSetupComplete = clientMergeFrom.connectionSetupComplete;
            //clientMergeTo.sendingMessageQueue = clientMergeFrom.sendingMessageQueue;
            //clientMergeTo.receivingMessageQueue = clientMergeFrom.receivingMessageQueue;

            //TODO: Add messages to queue based on priority
            clientMergeTo.messagesSent.AddRange(clientMergeFrom.messagesSent);
            clientMergeTo.messagesToBeSent.AddRange(clientMergeFrom.messagesToBeSent);
            clientMergeTo.messagesReceived.AddRange(clientMergeFrom.messagesReceived);

            connectedClients.Remove(clientMergeFrom);
            return clientMergeTo;
        }

        public string[] GetClientUsernames()
        {
            string[] usernames = new string[connectedClients.Count];
            for (int i = 0; i < connectedClients.Count; i++)
            {
                usernames[i] = connectedClients[i].username;
            }
            return usernames;
        }

        public List<Client> ClientSearch(string[] usernames, int[] clientIds)
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

        public void UpdateClientLists()
        {
            ClearClientListEvent.Invoke(this, null);
            SendToAll(null, 7, null);
            string[] usernames = GetClientUsernames();
            for (int i = 0; i < usernames.Length; i++)
            {
                AddClientToClientListEvent.Invoke(this, usernames[i]);
                SendToAll(null, 8, usernames[i]);
            }
        }

        public void SetAdmin(Client client, string setter, bool setAsAdmin, List<Client> ignoredClients)
        {
            client.admin = setAsAdmin;
            ignoredClients ??= new List<Client>();
            ignoredClients.Add(client);
            if (setAsAdmin)
            {
                SendMessage(client, ComposeMessage(client, -1, 14, setter));
                SendToAll(ignoredClients, 15, $"{client.username} {setter}");
                PrintChatMessageEvent.Invoke(this, $"You made {client.username} an Admin");
            }
            else
            {
                SendMessage(client, ComposeMessage(client, -1, 16, setter));
                SendToAll(ignoredClients, 17, $"{client.username} {setter}");
                PrintChatMessageEvent.Invoke(this, $"You removed {client.username} from Admin");
            }
        }

        public void SendDisconnect(Client client, bool kick, bool sendToAll)
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
    }
}
