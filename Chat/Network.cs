//#define messageSentUpdates
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Chat
{
    class Network
    {
        public X509Certificate2 x509Certificate;
        public string certificateName = "chatappserver.ddns.net";
        public string certificateFilePath = "C:\\Certbot\\live\\chatappserver.ddns.net\\fullchain.pem";
        public string keyFilePath = "C:\\Certbot\\live\\chatappserver.ddns.net\\privkey.pem";

        #region Connection Info
        public int port = 12210;
        public string publicIp;
        public int serverType = 1; // 0: Officially-Hosted, 1: User-Hosted
        public string localIp;
        public uint nextAssignableClientId = 1;
        public List<Client> connectedClients = new List<Client>();
        #endregion

        #region Event Handlers
        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
        public event EventHandler<string> PrintChatMessageEvent;
        public event EventHandler<Client> HeartbeatTimeoutEvent;
        public event EventHandler ClearClientListEvent;
        public event EventHandler<string> AddClientToClientListEvent;
        public event EventHandler<Client> NextConnectionSetupStep;
        public event EventHandler<ShowMessageBoxEventArgs> ShowMessageBoxEvent;
        #endregion

        #region Threads
        public Thread serverThread;
        public Thread clientThread;
        public CancellationTokenSource serverCancellationTokenSource = new CancellationTokenSource();
        public CancellationTokenSource clientCancellationTokenSource = new CancellationTokenSource();
        private AutoResetEvent writeAutoResetEvent = new AutoResetEvent(true);
        private AutoResetEvent acceptTcpClientResetEvent = new AutoResetEvent(true);
        private AutoResetEvent connectAutoResetEvent = new AutoResetEvent(true);
        #endregion

        #region Timers
        public System.Timers.Timer heartbeat = new System.Timers.Timer();
        #endregion

        public void BeginNetworkThreads()
        {
            localIp = GetLocalIp();
            AddClientToClientListEvent(this, FrmHolder.username);
            if (FrmHolder.hosting)
            {
                publicIp = new WebClient().DownloadString("https://ipv4.icanhazip.com/"); //TODO: Implement backup URLs
                //publicIp = localIp; // For use if unable to access internet/port forward
                publicIp = publicIp.Trim();

                serverThread = new Thread(new ParameterizedThreadStart(StartServer));
                serverThread.IsBackground = true;
                serverThread.Start(serverCancellationTokenSource.Token);

                PrintChatMessageEvent(this, $"Server started on: {publicIp}");
            }
            else
            {
                publicIp = FrmHolder.joinIP;

                clientThread = new Thread(new ParameterizedThreadStart(StartClient));
                clientThread.IsBackground = true;
                clientThread.Start(clientCancellationTokenSource.Token);

                PrintChatMessageEvent(this, $"Connected to server on: {publicIp}"); //TODO: Only if succesfully connected - use acknowledgement
            }
            //network.StartHeartbeat();
        }

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

            using (x509Certificate = ImportCertificateFromStoreOrFile(certificateName, false, certificateFilePath, keyFilePath, serverType == 0 ? false : true))
            {
                tcpListener.Start();
                StartHeartbeat();

                while (cancellationToken.IsCancellationRequested == false)
                {
                    BeginAcceptTcpClient(tcpListener);
                }
            }

            tcpListener.Stop();
            FrmHolder.clientId = 0;
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

            BeginConnect(client);
            StartHeartbeat();

            while (cancellationToken.IsCancellationRequested == false)
            {

            }
            FrmHolder.clientId = 0;
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
            //heartbeat.Start();
        }

        private void StopHeartbeat()
        {
            heartbeat.Stop();
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
                        BeginConnect(client);
                    }
                    else if ((client.heartbeatFailures >= 12 && FrmHolder.hosting) || (client.heartbeatFailures >= 10 && FrmHolder.hosting == false))
                    {
                        if (client.disconnectHandled == false)
                        {
                            HeartbeatTimeoutEvent.Invoke(this, client);
                        }
                    }
                    if (FrmHolder.hosting == false)
                    {
                        BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.Heartbeat, null, null)); // Send heartbeat
                    }
                    client.heartbeatReceieved = false;
                }
            }
        }

        public void HeartbeatTimeoutFailure(Client client)
        {
            if (client.disconnectHandled == false)
            {
                client.disconnectHandled = true;
                if (client.sslStream != null)
                {
                    client.sslStream.Close();
                }
                connectedClients.Remove(client);
                if (FrmHolder.hosting)
                {
                    List<Client> ignoredClients = new List<Client>();
                    ignoredClients.Add(client);
                    SendToAll(ignoredClients, Message.MessageTypes.OtherUserLostConnection, client.username, null);
                    UpdateClientLists();
                    PrintChatMessageEvent(this, $"Lost connection to {client.username}...");
                }
                else
                {
                    PrintChatMessageEvent(this, $"Lost connection to server...");
                }
            }
        }

        public X509Certificate2 ImportCertificateFromStoreOrFile(string certificateName, bool inStore, string certificateFilePath, string keyFilePath, bool useSelfSigned)
        {
            if (useSelfSigned)
            {
                return GenerateSelfSignedCertificate();
            }
            else if (inStore)
            {
                using (X509Store x509Store = new X509Store(StoreLocation.LocalMachine))
                {
                    x509Store.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection allCertificates = x509Store.Certificates;
                    X509Certificate2Collection validCertificates = allCertificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                    X509Certificate2Collection matchingCertificates = validCertificates.Find(X509FindType.FindBySubjectDistinguishedName, "CN=" + certificateName, false);
                    if (matchingCertificates.Count == 0)
                    {
                        throw new CertificateNotFoundException($"No valid certificate matching the name '{certificateName}' found in the certificate store.");
                    }
                    return matchingCertificates[0];
                }
            }
            else
            {
                try
                {
                    X509Certificate2 matchingCertificate = X509Certificate2.CreateFromPemFile(certificateFilePath, keyFilePath);
                    return new X509Certificate2(matchingCertificate.Export(X509ContentType.Pkcs12));
                }
                catch
                {
                    throw new CertificateNotFoundException($"No valid certificate matching the name '{Path.GetFileName(certificateFilePath)}' found at {Path.GetDirectoryName(certificateFilePath)}.");
                }
            }
        }

        private X509Certificate2 GenerateSelfSignedCertificate()
        {
            ECDsa eCDsa = ECDsa.Create();
            CertificateRequest certificateRequest = new CertificateRequest("CN=chat", eCDsa, HashAlgorithmName.SHA256);
            X509Certificate2 x509Certificate2 = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.Now.AddYears(1));
            byte[] x509Certificate2Bytes = x509Certificate2.Export(X509ContentType.Pkcs12);
            X509Certificate2 x509Certificate2Imported = new X509Certificate2(x509Certificate2Bytes);
            return x509Certificate2Imported;
        }

        public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None || serverType == 1)
            {
                return true;
            }
            return false;
        }

        public void SendFirstMessageInMessageQueue(Client client)
        {
            if (client.messagesToBeSent.Count > 0 && client.sendingMessageQueue)
            {
                Message message = client.messagesToBeSent[0];
                BeginWrite(client, message);
            }
            else
            {
                client.sendingMessageQueue = false;
                BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.FinishedSendingMessageQueue, null, null));
                if (FrmHolder.hosting == false)
                {
                    BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.SendMessageQueue, null, null));
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
            if (message.messageType != Message.MessageTypes.Acknowledgement && message.messageType != Message.MessageTypes.Heartbeat)
            {
                if (sendFailed)
                {
                    AddMessageToMessageListBySendPriority(client.messagesToBeSent, message, true);
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
                                AddMessageToMessageListBySendPriority(client.messagesToBeSent, message, true);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void AddMessageToMessageListBySendPriority(List<Message> messageList, Message message, bool addToEndOfPriorityGroup)
        {
            if (messageList != null)
            {
                if (messageList.Count() == 0)
                {
                    messageList.Add(message);
                }
                else if (messageList.Contains(message) == false)
                {
                    bool added = false;
                    if (addToEndOfPriorityGroup)
                    {
                        for (int i = 0; i < messageList.Count(); i++)
                        {
                            if (messageList[i].messageSendPriority > message.messageSendPriority)
                            {
                                messageList.Insert(i, message);
                                added = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = messageList.Count() - 1; i >= 0; i--)
                        {
                            if (messageList[i].messageSendPriority < message.messageSendPriority)
                            {
                                messageList.Insert(i + 1, message);
                                added = true;
                                break;
                            }
                        }
                    }
                    if (added == false)
                    {
                        messageList.Add(message);
                    }
                }
            }
        }

        public void RemoveMessageFromMessageListByTypeAndOrSendPriority(List<Message> messageList, Message.MessageTypes messageType, int? messageSendPriority)
        {
            for (int i = 0; i < messageList.Count(); i++)
            {
                if (messageType != Message.MessageTypes.None && messageSendPriority != null)
                {
                    if (messageList[i].messageType == messageType && messageList[i].messageSendPriority == messageSendPriority)
                    {
                        messageList.RemoveAt(i);
                    }
                }
                else if (messageType != Message.MessageTypes.None)
                {
                    if (messageList[i].messageType == messageType)
                    {
                        messageList.RemoveAt(i);
                    }
                }
                else if (messageSendPriority != null)
                {
                    if (messageList[i].messageSendPriority == messageSendPriority)
                    {
                        messageList.RemoveAt(i);
                    }
                }
            }
        }

        public void BeginConnect(Client client)
        {
            client.connectionSetupComplete = false;
            client.disconnectHandled = false;
            client.sendingMessageQueue = false;
            client.receivingMessageQueue = false;

            if (client.sslStream != null)
            {
                client.sslStream.Close();
            }
            if (connectedClients.Contains(client) == false)
            {
                connectedClients.Add(client);
            }

            client.tcpClient = new TcpClient();
            connectAutoResetEvent.WaitOne();
            client.tcpClient.BeginConnect(publicIp, port, ConnectCallback, client);
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            Client client = asyncResult.AsyncState as Client;
            client.tcpClient.EndConnect(asyncResult);
            connectAutoResetEvent.Set();
            client.sslStream = new SslStream(client.tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            try
            {
                client.sslStream.AuthenticateAsClient("chatappserver.ddns.net");
                if (client.sslStream.IsEncrypted == false || client.sslStream.IsSigned == false || client.sslStream.IsAuthenticated == false)
                {
                    client.sslStream.Close();
                    connectedClients.Remove(client);
                    ShowMessageBoxEvent.Invoke(this, new ShowMessageBoxEventArgs("Unable to establish a secure connection to the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                    return;
                }
            }
            catch (AuthenticationException ex)
            {
                client.sslStream.Close();
                connectedClients.Remove(client);
                ShowMessageBoxEvent.Invoke(this, new ShowMessageBoxEventArgs(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                return;
            }
            BeginRead(client);
        }

        public void BeginAcceptTcpClient(TcpListener tcpListener)
        {
            if (tcpListener.Pending())
            {
                acceptTcpClientResetEvent.WaitOne();
                tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, tcpListener);
            }
        }

        private void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {
            TcpListener tcpListener = asyncResult.AsyncState as TcpListener;
            TcpClient tcpClient = tcpListener.EndAcceptTcpClient(asyncResult);

            Client client = new Client();
            client.tcpClient = tcpClient;
            client.nextAssignableMessageId = 1;
            connectedClients.Add(client);
            acceptTcpClientResetEvent.Set();

            client.sslStream = new SslStream(client.tcpClient.GetStream(), false);
            try
            {
                client.sslStream.AuthenticateAsServer(x509Certificate, false, true);
                if (client.sslStream.IsEncrypted == false || client.sslStream.IsSigned == false || client.sslStream.IsAuthenticated == false)
                {
                    client.sslStream.Close();
                    connectedClients.Remove(client);
                    //MessageBoxEvent.Invoke(this, new ShowMessageBoxEventArgs("Unable to establish a secure connection to client.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                }
            }
            catch (AuthenticationException ex)
            {
                client.sslStream.Close();
                connectedClients.Remove(client);
                //MessageBoxEvent.Invoke(this, new ShowMessageBoxEventArgs(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning));
            }
            BeginRead(client);
            NextConnectionSetupStep.Invoke(this, client);
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

        public Message ComposeMessage(Client client, uint messageId, Message.MessageTypes messageType, string messageText, byte[] messageBytes)
        {
            if (messageId == 0)
            {
                messageId = client.nextAssignableMessageId;
                client.nextAssignableMessageId += 2;
            }
            Message message;
            if (messageText != null)
            {
                message = new Message(messageId, messageType, messageText);
            }
            else
            {
                message = new Message(messageId, messageType, messageBytes);
            }

            return message;
        }

        public void BeginWrite(Client client, Message message)
        {
            if (CheckAddMessageToQueue(client, message, false))
            {
                return;
            }
            try
            {
                if (client.sslStream != null)
                {
                    {
                        if (client.sslStream.CanWrite && client.sslStream.CanRead)
                        {
                            // Message ID
                            byte[] idBuffer = new byte[4];
                            idBuffer = BitConverter.GetBytes(message.messageId);

                            // Message type
                            byte[] typeBuffer = new byte[4];
                            typeBuffer = BitConverter.GetBytes(((uint)message.messageType));

                            // Message bytes
                            byte[] bytesBuffer = null;
                            if (message.messageBytes != null && message.messageBytes.Count() > 0)
                            {
                                bytesBuffer = message.messageBytes;
                            }

                            // Message length
                            byte[] lengthBuffer = new byte[4];
                            if (bytesBuffer != null)
                            {
                                lengthBuffer = BitConverter.GetBytes(bytesBuffer.Length);
                            }

                            ConvertLittleEndianToBigEndian(idBuffer);
                            ConvertLittleEndianToBigEndian(typeBuffer);
                            ConvertLittleEndianToBigEndian(bytesBuffer);
                            ConvertLittleEndianToBigEndian(lengthBuffer);

                            int messageLength = idBuffer.Length + typeBuffer.Length + lengthBuffer.Length;
                            if (bytesBuffer != null)
                            {
                                messageLength += bytesBuffer.Length;
                            }
                            byte[] writeBuffer = new byte[messageLength];
                            idBuffer.CopyTo(writeBuffer, 0);
                            typeBuffer.CopyTo(writeBuffer, idBuffer.Length);
                            lengthBuffer.CopyTo(writeBuffer, idBuffer.Length + typeBuffer.Length);
                            if (bytesBuffer != null)
                            {
                                bytesBuffer.CopyTo(writeBuffer, idBuffer.Length + typeBuffer.Length + lengthBuffer.Length);
                            }

                            ClientStateObject clientStateObject = new ClientStateObject(client);
                            clientStateObject.message = message;
                            writeAutoResetEvent.WaitOne();
                            client.sslStream.BeginWrite(writeBuffer, 0, writeBuffer.Length, new AsyncCallback(WriteCallback), clientStateObject);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.InvalidOperationException)
            {
                CheckAddMessageToQueue(client, message, true);
            }
        }

        private void WriteCallback(IAsyncResult asyncResult)
        {
            ClientStateObject clientStateObject = asyncResult.AsyncState as ClientStateObject;
            try
            {
                clientStateObject.client.sslStream.EndWrite(asyncResult);
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.InvalidOperationException)
            {
                return;
            }
            writeAutoResetEvent.Set();
            if (clientStateObject.message.messageType != Message.MessageTypes.Acknowledgement && clientStateObject.message.messageType != Message.MessageTypes.ClientDisconnect && clientStateObject.message.messageType != Message.MessageTypes.Heartbeat)
            {
                AddMessageToMessageListBySendPriority(clientStateObject.client.messagesToBeSent, clientStateObject.message, true);
            }
#if DEBUG && messageSentUpdates
                            if (message.messageType != 11)
                            {
                                string text;
                                if (message.messageText != null)
                                {
                                    text = $"[SENT] Type: {message.messageType}. ID: {message.messageId}. Text: {message.messageText}";
                                }
                                else
                                {
                                    text = $"[SENT] Type: {message.messageType}. ID: {message.messageId}";
                                }
                                PrintChatMessageEvent.Invoke(this, text);
                            }
#endif
        }

        private void BeginRead(Client client)
        {
            try
            {
                if (client.sslStream != null)
                {
                    {
                        if (client.sslStream.CanRead && client.sslStream.CanWrite)
                        {
                            ClientStateObject clientStateObject = new ClientStateObject(client);
                            client.sslStream.BeginRead(clientStateObject.byteBuffer, 0, clientStateObject.byteBuffer.Count(), new AsyncCallback(ReadCallback), clientStateObject);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.InvalidOperationException)
            {
                return;
            }
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            ClientStateObject clientStateObject = asyncResult.AsyncState as ClientStateObject;
            uint bytesRead = 0;
            try
            {
                bytesRead = (uint)clientStateObject.client.sslStream.EndRead(asyncResult);
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.InvalidOperationException)
            {
                return;
            }
            clientStateObject.bytesRead += bytesRead;
            if (bytesRead > 0)
            {
                clientStateObject.client.streamUnprocessedBytes.Write(clientStateObject.byteBuffer, 0, (int)bytesRead);
                if (clientStateObject.client.streamUnprocessedBytes.Length >= clientStateObject.headerLength)
                {
                    long writePosition = clientStateObject.client.streamUnprocessedBytes.Position;
                    if (clientStateObject.readHeader == false)
                    {
                        clientStateObject.client.streamUnprocessedBytes.Position = 0;
                        clientStateObject.client.streamUnprocessedBytes.Read(clientStateObject.idBuffer, 0, clientStateObject.idBuffer.Count());
                        clientStateObject.client.streamUnprocessedBytes.Read(clientStateObject.typeBuffer, 0, clientStateObject.typeBuffer.Count());
                        clientStateObject.client.streamUnprocessedBytes.Read(clientStateObject.lengthBuffer, 0, clientStateObject.lengthBuffer.Count());

                        ConvertLittleEndianToBigEndian(clientStateObject.idBuffer);
                        ConvertLittleEndianToBigEndian(clientStateObject.typeBuffer);
                        ConvertLittleEndianToBigEndian(clientStateObject.lengthBuffer);

                        clientStateObject.messageId = BitConverter.ToUInt32(clientStateObject.idBuffer, 0);
                        uint messageType = BitConverter.ToUInt32(clientStateObject.typeBuffer, 0);
                        clientStateObject.messageType = (Message.MessageTypes)messageType;
                        clientStateObject.messageLength = BitConverter.ToUInt32(clientStateObject.lengthBuffer, 0);

                        clientStateObject.messageBytes = new byte[clientStateObject.messageLength.GetValueOrDefault()];
                        clientStateObject.readHeader = true;
                    }
                    if (clientStateObject.client.streamUnprocessedBytes.Length >= clientStateObject.messageLength + clientStateObject.headerLength)
                    {
                        clientStateObject.client.streamUnprocessedBytes.Position = clientStateObject.headerLength;
                        clientStateObject.client.streamUnprocessedBytes.Read(clientStateObject.messageBytes, 0, clientStateObject.messageBytes.Count());
                        ConvertLittleEndianToBigEndian(clientStateObject.messageBytes);
                    }
                    clientStateObject.client.streamUnprocessedBytes.Position = writePosition;
                    TruncateBytesPrecedingPositionInMemoryStream(clientStateObject.client);
                }
                Message receivedMessage = ComposeMessage(clientStateObject.client, clientStateObject.messageId.GetValueOrDefault(), clientStateObject.messageType, null, clientStateObject.messageBytes);
                MessageReceivedEvent.Invoke(this, new MessageReceivedEventArgs(clientStateObject.client, receivedMessage));
                BeginRead(clientStateObject.client);
            }
        }

        private void TruncateBytesPrecedingPositionInMemoryStream(Client client)
        {
            long streamUnprocessedBytesPosition = client.streamUnprocessedBytes.Position;
            byte[] unprocessedBytes = new byte[client.streamUnprocessedBytes.Length - streamUnprocessedBytesPosition];
            client.streamUnprocessedBytes.Read(unprocessedBytes, 0, unprocessedBytes.Length);
            client.streamUnprocessedBytes.Position = 0;
            client.streamUnprocessedBytes.Write(unprocessedBytes);
            client.streamUnprocessedBytes.SetLength(unprocessedBytes.Count());
        }

        public void SendToAll(List<Client> ignoredClients, Message.MessageTypes messageType, string messageText, byte[] messageBytes) //TODO: Replace messagType and messagText with Message class
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                if (ignoredClients != null)
                {
                    bool ignoreClient = false;
                    for (int j = 0; j < ignoredClients.Count; j++)
                    {
                        if (connectedClients[i] == ignoredClients[j])
                        {
                            ignoreClient = true;
                            break;
                        }
                    }
                    if (ignoreClient == false)
                    {
                        BeginWrite(connectedClients[i], ComposeMessage(connectedClients[i], 0, messageType, messageText, messageBytes));
                        continue;
                    }
                }
                else
                {
                    BeginWrite(connectedClients[i], ComposeMessage(connectedClients[i], 0, messageType, messageText, messageBytes));
                    continue;
                }
            }
        }

        public Client MergeClient(Client clientMergeFrom, Client clientMergeTo)
        {
            //clientMergeTo.clientId = clientMergeFrom.clientId;
            clientMergeTo.nextAssignableMessageId = (clientMergeTo.nextAssignableMessageId + clientMergeFrom.nextAssignableMessageId);
            //clientMergeTo.username = clientMergeFrom.username;
            clientMergeTo.tcpClient = clientMergeFrom.tcpClient;
            clientMergeTo.sslStream = clientMergeFrom.sslStream;

            //clientMergeTo.admin = clientMergeFrom.admin;
            //clientMergeTo.serverMuted = clientMergeFrom.serverMuted;
            //clientMergeTo.serverDeafened = clientMergeFrom.serverDeafened;

            clientMergeTo.heartbeatReceieved = clientMergeFrom.heartbeatReceieved;
            clientMergeTo.heartbeatFailures = clientMergeFrom.heartbeatFailures;
            clientMergeTo.connectionSetupComplete = clientMergeFrom.connectionSetupComplete;
            clientMergeTo.disconnectHandled = clientMergeFrom.disconnectHandled;
            clientMergeTo.sendingMessageQueue = clientMergeFrom.sendingMessageQueue;
            clientMergeTo.receivingMessageQueue = clientMergeFrom.receivingMessageQueue;

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
            SendToAll(null, Message.MessageTypes.ClearUserList, null, null);
            string[] usernames = GetClientUsernames();
            for (int i = 0; i < usernames.Length; i++)
            {
                AddClientToClientListEvent.Invoke(this, usernames[i]);
                SendToAll(null, Message.MessageTypes.AddToUserList, usernames[i], null);
            }
        }

        public void SetAdmin(Client client, string setter, bool setAsAdmin, List<Client> ignoredClients)
        {
            client.admin = setAsAdmin;
            ignoredClients ??= new List<Client>();
            ignoredClients.Add(client);
            if (setAsAdmin)
            {
                BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.MadeAdmin, setter, null));
                SendToAll(ignoredClients, Message.MessageTypes.OtherUserMadeAdmin, $"{client.username} {setter}", null);
                PrintChatMessageEvent.Invoke(this, $"You made {client.username} an Admin");
            }
            else
            {
                BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.RemovedAdmin, setter, null));
                SendToAll(ignoredClients, Message.MessageTypes.OtherUserRemovedAdmin, $"{client.username} {setter}", null);
                PrintChatMessageEvent.Invoke(this, $"You removed {client.username} from Admin");
            }
        }

        public void SendDisconnect(Client client, bool kick, bool sendToAll)
        {
            Message.MessageTypes type = kick == false ? Message.MessageTypes.ClientDisconnect : Message.MessageTypes.Kicked;
            if (sendToAll)
            {
                List<Client> ignoredClients = new List<Client>();
                ignoredClients.Add(client);
                SendToAll(ignoredClients, type, null, null);
                for (int i = 0; i < connectedClients.Count; i++)
                {
                    if (connectedClients[i].sslStream != null)
                    {
                        connectedClients[i].sslStream.Close();
                    }
                }
                connectedClients.Clear();
            }
            else
            {
                BeginWrite(client, ComposeMessage(client, 0, type, null, null));
                if (client.sslStream != null)
                {
                    client.sslStream.Close();
                }
                connectedClients.Remove(client);
            }
        }
    }
}
