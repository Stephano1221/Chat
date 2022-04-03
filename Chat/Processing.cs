using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Chat
{
    public class Processing
    {
        private Network network;

        #region Event Handlers
        public event EventHandler<FirstConnectionAttemptResultEventArgs> FirstConnectionAttemptResultEvent;
        public event EventHandler OpenMainMenuEvent;
        public event EventHandler<string> PrintChatMessageEvent;
        public event EventHandler<Client> HeartbeatTimeoutEvent;
        public event EventHandler ClearClientListEvent;
        public event EventHandler<string> AddClientToClientListEvent;
        public event EventHandler<ShowMessageBoxEventArgs> ShowMessageBoxEvent;
        #endregion

        #region Certificates
        public X509Certificate2 x509Certificate;
        public string certificateName = "chatappserver.ddns.net";
        public string certificateFilePath = "C:\\Certbot\\live\\chatappserver.ddns.net\\fullchain.pem";
        public string keyFilePath = "C:\\Certbot\\live\\chatappserver.ddns.net\\privkey.pem";
        #endregion

        #region Connection Info
        public int port = 12210;
        public string publicIp;
        public int serverType = 1; // 0: Officially-Hosted, 1: User-Hosted
        public string localIp;
        public uint nextAssignableClientId = 1;
        public List<Client> connectedClients = new List<Client>();
        #endregion

        #region Threads
        public Thread serverThread;
        public Thread clientThread;
        public CancellationTokenSource serverCancellationTokenSource = new CancellationTokenSource();
        public CancellationTokenSource clientCancellationTokenSource = new CancellationTokenSource();
        #endregion

        private frmManageRanks frmManageRanks;
        public Ranks ranks = new Ranks();

        public System.Timers.Timer heartbeat = new System.Timers.Timer();

        private string kickFormat = "/kick [Username] [Reason (optional)]";
        private string adminFormat = "/admin [Username] [True/False (optional)]";

        public Processing()
        {
            ranks.AddFirstRank();
            network = new Network();
            SetNetworkEventHandlers();
            BeginNetworkThreads();
        }

        private void SetNetworkEventHandlers()
        {
            network.FirstConnectionAttemptResultEvent += OnFirstConnectionAttemptResult;
            network.MessageReceivedEvent += OnMessageReceived;
            network.ShowMessageBoxEvent += OnShowMessageBox;
            network.AcceptTcpClientEvent += OnAcceptTcpClient;
        }

        public void BeginNetworkThreads()
        {
            localIp = GetLocalIp();
            InvokeAddClientToClientListEvent(this, FrmHolder.username);
            if (FrmHolder.hosting)
            {
                try
                {
                    Task<string> publicIpTask = new HttpClient().GetStringAsync("https://ipv4.icanhazip.com/"); //TODO: Implement backup URLs
                    publicIp = publicIpTask.Result;
                }
                catch (Exception ex) when (ex.InnerException is TaskCanceledException || ex.InnerException is HttpRequestException)
                {
                    publicIp = $"(Local IP) {localIp}";
                }
                publicIp = publicIp.Trim();

                serverThread = new Thread(new ParameterizedThreadStart(StartServer));
                serverThread.IsBackground = true;
                serverThread.Start(serverCancellationTokenSource.Token);
            }
            else
            {
                publicIp = FrmHolder.joinIP;

                clientThread = new Thread(new ParameterizedThreadStart(StartClient));
                clientThread.IsBackground = true;
                clientThread.Start(clientCancellationTokenSource.Token);
            }
#if RELEASE
            StartHeartbeat();
#endif
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
                InvokeAddClientToClientListEvent(this, FrmHolder.username);
                InvokePrintChatMessageEvent(this, $"Server started on: {publicIp}");

                while (cancellationToken.IsCancellationRequested == false)
                {
                    network.BeginAcceptTcpClient(tcpListener);
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

            network.BeginConnect(client);
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
            network.BeginWrite(client, message);
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
                        network.BeginConnect(client);
                    }
                    else if ((client.heartbeatFailures >= 12 && FrmHolder.hosting) || (client.heartbeatFailures >= 10 && FrmHolder.hosting == false))
                    {
                        if (client.disconnectHandled == false)
                        {
                            InvokeHeartbeatTimeoutEvent(this, client);
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
                    BeginWriteToAll(ignoredClients, Message.MessageTypes.OtherUserLostConnection, client.username, null);
                    UpdateClientLists();
                    InvokePrintChatMessageEvent(this, $"Lost connection to {client.username}...");
                }
                else
                {
                    InvokePrintChatMessageEvent(this, $"Lost connection to server...");
                }
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
                BeginWriteToAll(ignoredClients, Message.MessageTypes.OtherUserMadeAdmin, $"{client.username} {setter}", null);
                InvokePrintChatMessageEvent(this, $"You made {client.username} an Admin");
            }
            else
            {
                BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.RemovedAdmin, setter, null));
                BeginWriteToAll(ignoredClients, Message.MessageTypes.OtherUserRemovedAdmin, $"{client.username} {setter}", null);
                InvokePrintChatMessageEvent(this, $"You removed {client.username} from Admin");
            }
        }

        public void SendDisconnect(Client client, bool kick, bool sendToAll)
        {
            Message.MessageTypes type = kick == false ? Message.MessageTypes.ClientDisconnect : Message.MessageTypes.Kicked;
            if (sendToAll)
            {
                List<Client> ignoredClients = new List<Client>();
                ignoredClients.Add(client);
                BeginWriteToAll(ignoredClients, type, null, null);
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

        public void BeginWriteToAll(List<Client> ignoredClients, Message.MessageTypes messageType, string messageText, byte[] messageBytes) //TODO: Replace messagType and messagText with Message class
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
            InvokeClearClientListEvent(this, null);
            BeginWriteToAll(null, Message.MessageTypes.ClearUserList, null, null);
            string[] usernames = GetClientUsernames();
            for (int i = 0; i < usernames.Length; i++)
            {
                InvokeAddClientToClientListEvent(this, usernames[i]);
                BeginWriteToAll(null, Message.MessageTypes.AddToUserList, usernames[i], null);
            }
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ProcessMessage(this, e);
            if (e.client.connectionSetupComplete == false)
            {
                NextStepInConnectionSetupAsServer(e.client);
            }
        }

        public bool ProcessCommand(string message)
        {
            if (message[0] == '/')
            {
                if (FrmHolder.hosting == false)
                {
                    InvokePrintChatMessageEvent(this, "You must be an admin to execute commands"); //TODO: Allow non-admin commands (e.g. /help)
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
                else if (commandParts[0] == "giverank")
                {
                    RunCommandGiveRank(commandParts);
                }
                else if (commandParts[0] == "takerank")
                {
                    RunCommandTakeRank(commandParts);
                }
                else if (commandParts[0] == "ranks")
                {
                    RunCommandRanks(commandParts);
                }
                else if (commandParts[0] == "manageranks")
                {
                    RunCommandManageRanks();
                }
                return true;
            }
            return false; //true if commmand
        }

        private void NextStepInConnectionSetupAsServer(Client client)
        {
            if (FrmHolder.hosting == false || client.connectionSetupComplete)
            {
                return;
            }
            if (client.receivedApplicationVersionNumber == false)
            {
                if (client.requestedApplicationVersionNumber == false)
                {
                    BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.RequestVersionNumber, null, null));
                    client.requestedApplicationVersionNumber = true;
                }
                return;
            }
            if (client.receivedClientId == false)
            {
                if (client.requestedClientId == false)
                {
                    BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.RequestClientId, null, null));
                    client.requestedClientId = true;
                }
                return;
            }
            if (client.receivedUsername == false)
            {
                if (client.requestedUsername == false)
                {
                    BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.RequestUsername, null, null));
                    client.requestedUsername = true;
                }
                return;
            }
            if (client.sessionFirstConnection)
            {
                BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.AllRanks, JsonSerializer.Serialize<List<Ranks.Rank>>(ranks.RankList), null));
                List<Client> ignoredClients = new List<Client>();
                ignoredClients.Add(client);
                InvokePrintChatMessageEvent(this, $"{client.username} connected");
                BeginWriteToAll(ignoredClients, Message.MessageTypes.UserConnected, client.username, null);
                UpdateClientLists();
            }
            client.connectionSetupComplete = true;
            BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.ConnectionSetupComplete, null, null));
            BeginWrite(client, ComposeMessage(client, 0, Message.MessageTypes.SendMessageQueue, null, null));
            client.receivingMessageQueue = true;
        }

        public List<Ranks.Rank> GetRanksFromDatabase()
        {
            throw new NotImplementedException();
        }

        public void SaveRanksAsServer(Client requestingClient, Ranks.Changes changes)
        {
            if (ValidateRanks(changes.NewRanks) == false || ValidateRanks(changes.ModifiedRanks) == false)
            {
                if (requestingClient == null) //Server is the caller
                {
                    SavingRanksFailedInvalidRanks();
                }
                else
                {
                    BeginWrite(requestingClient, ComposeMessage(requestingClient, 0, Message.MessageTypes.InvalidRanks, null, null));
                }
            }
            //TODO: Read ranks from database
            //TODO: Save to database
            List<Ranks.Rank> newRanks = changes.NewRanks;
            changes.NewRanks = new List<Ranks.Rank>();
            foreach (Ranks.Rank newRank in newRanks)
            {
                changes.NewRanks.Add(new Ranks.Rank(IdGenerator.GenerateId(), newRank.Name, newRank.Color, newRank.Level, newRank.PermissionNumber));
            }
            ranks.UpdateRanksList(changes); //TODO: Replace with reading all ranks from database
            string json = changes.SerializeToJson();
            BeginWriteToAll(null, Message.MessageTypes.ChangedRanks, json, null);
        }

        public void SaveRanksAsClient(Ranks.Changes changes)
        {
            string json = ranks.SerializeToJson();
            BeginWrite(connectedClients[0], ComposeMessage(connectedClients[0], 0, Message.MessageTypes.AllRanks, json, null));
        }

        private bool ValidateRanks(List<Ranks.Rank> ranks)
        {
            foreach (Ranks.Rank rank in ranks)
            {
                if (Ranks.IsValidRank(rank) == false)
                {
                    return false;
                }
            }
            return true;
        }

        private void SavingRanksFailedInvalidRanks()
        {
            InvokeShowMessageBoxEvent(this, new ShowMessageBoxEventArgs("An error occured when saving ranks: Invalid ranks.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning));
        }

        private List<Commands.Command> possibleMatchingCommands(string command)
        {
            if (command != null)
            {
                List<Commands.Command> matchingCommands = new List<Commands.Command>();
                foreach (Commands.Command defaultCommand in Commands.defaultCommandsInfo)
                {
                    foreach (string commandName in defaultCommand.Names)
                    {
                        if (commandName.Contains(command, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingCommands.Add(defaultCommand);
                        }
                    }
                }
                return matchingCommands;
            }
            return null;
        }

        private void RunCommandGiveRank(string[] commandParts)
        {
            if (commandParts.Length == 1)
            {
                InvokePrintChatMessageEvent(this, $"A user must be specified.");
                return;
            }
            else if (commandParts.Length == 2)
            {
                InvokePrintChatMessageEvent(this, $"A rank must be specified.");
                return;
            }
            string[] username = { commandParts[1] };
            List<Client> matchingClients = ClientSearch(username, null);
            if (matchingClients.Count() == 0)
            {
                InvokePrintChatMessageEvent(this, $"This user could not be found.");
                return;
            }
            string rankName = commandParts[2];
            List<Ranks.Rank> matchingRanks = FrmHolder.processing.ranks.GetRanksMatchingName(rankName);
            if (matchingClients == null || matchingRanks.Count() == 0)
            {
                InvokePrintChatMessageEvent(this, $"This rank could not be found.");
                return;
            }
            else
            {
                foreach (Ranks.Rank rank in matchingClients[0].ranks)
                {
                    if (rank.Name == rankName)
                    {
                        InvokePrintChatMessageEvent(this, $"This user already has this rank.");
                        return;
                    }
                }
                matchingClients[0].ranks.Add(matchingRanks[0]);
                InvokePrintChatMessageEvent(this, $"{username[0]} has been given the rank {rankName}");

                List<Client> ignoredClients = new List<Client>();
                ignoredClients.Add(matchingClients[0]);
                BeginWrite(matchingClients[0], ComposeMessage(matchingClients[0], 0, Message.MessageTypes.RankGiven, $"{FrmHolder.username} {rankName}", null));
                BeginWriteToAll(ignoredClients, Message.MessageTypes.OtherUserRankGiven, $"{username[0]} {FrmHolder.username} {rankName}", null);
            }
        }

        private void RunCommandTakeRank(string[] commandParts)
        {
            if (commandParts.Length == 1)
            {
                InvokePrintChatMessageEvent(this, $"A user must be specified.");
                return;
            }
            else if (commandParts.Length == 2)
            {
                InvokePrintChatMessageEvent(this, $"A rank must be specified.");
                return;
            }
            string[] username = { commandParts[1] };
            List<Client> matchingClients = ClientSearch(username, null);
            if (matchingClients.Count() == 0)
            {
                InvokePrintChatMessageEvent(this, $"This user could not be found.");
                return;
            }
            string rankName = commandParts[2];
            foreach (Ranks.Rank rank in matchingClients[0].ranks)
            {
                if (rank.Name == rankName)
                {
                    matchingClients[0].ranks.Remove(rank);
                    InvokePrintChatMessageEvent(this, $"You removed {username[0]}'s rank of {rankName}.");

                    List<Client> ignoredClients = new List<Client>();
                    ignoredClients.Add(matchingClients[0]);
                    BeginWrite(matchingClients[0], ComposeMessage(matchingClients[0], 0, Message.MessageTypes.RankTaken, $"{FrmHolder.username} {rankName}", null));
                    BeginWriteToAll(ignoredClients, Message.MessageTypes.OtherUserRankTaken, $"{username[0]} {FrmHolder.username} {rankName}", null);
                    return;
                }
            }
            InvokePrintChatMessageEvent(this, "The user does not have this rank.");
            return;
        }

        private void RunCommandRanks(string[] commandParts)
        {
            if (commandParts.Length == 1)
            {
                List<string> rankNames = new List<string>();
                foreach (Ranks.Rank rank in FrmHolder.processing.ranks.RankList)
                {
                    rankNames.Add(rank.Name);
                }
                if (rankNames == null || rankNames.Count() == 0)
                {
                    InvokePrintChatMessageEvent(this, $"There are no ranks on the server.");
                    return;
                }
                string rankNamesCombined = String.Join(", ", rankNames);
                InvokePrintChatMessageEvent(this, $"Available ranks are: {rankNamesCombined}.");
                return;
            }
            else if (commandParts.Length == 2)
            {
                string[] username = { commandParts[1] };
                List<Client> matchingClients = ClientSearch(username, null);
                if (matchingClients.Count() == 0)
                {
                    InvokePrintChatMessageEvent(this, $"This user could not be found.");
                    return;
                }
                List<string> rankNames = new List<string>();
                foreach (Ranks.Rank rank in matchingClients[0].ranks)
                {
                    rankNames.Add(rank.Name);
                }
                if (rankNames == null || rankNames.Count() == 0)
                {
                    InvokePrintChatMessageEvent(this, $"This user has no ranks.");
                    return;
                }
                string rankNamesCombined = String.Join(", ", rankNames);
                InvokePrintChatMessageEvent(this, $"{username[0]} has the ranks: {rankNamesCombined}.");
                return;
            }
        }

        private void RunCommandManageRanks()
        {
            frmManageRanks = new frmManageRanks
            {
                MdiParent = FrmHolder.ActiveForm,
                Dock = DockStyle.Fill
            };
            frmManageRanks.Show();
        }

        private void RunCommandHelp(string[] commandParts)
        {
            if (commandParts.Length == 1)
            {
                InvokePrintChatMessageEvent(this, $"Available commands are 'kick' and 'admin'. Type '/help [command]' for an explanation of the command.");
            }
            else if (commandParts[1] == "kick")
            {
                InvokePrintChatMessageEvent(this, $"Explanation: Kicks a user. Format: {kickFormat}");
            }
            else if (commandParts[1] == "admin")
            {
                InvokePrintChatMessageEvent(this, $"Explanation: Adds/removes a user from Admin. Format: {adminFormat}");
            }
            else
            {
                InvokePrintChatMessageEvent(this, $"No command {commandParts[1]} exists.");
            }
        }

        private bool RunCommandKick(string[] commandParts)
        {
            if (commandParts[1] == null)
            {
                InvokePrintChatMessageEvent(this, $"The format is: {kickFormat}");
                return true;
            }
            string[] username = { commandParts[1] };
            List<Client> clients = ClientSearch(username, null);
            if (clients.Count == 0)
            {
                InvokePrintChatMessageEvent(this, $"No user with the username {username[0]} exists");
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
                InvokePrintChatMessageEvent(this, $"You kicked {username[0]} with reason: {reason}");
            }
            else
            {
                InvokePrintChatMessageEvent(this, $"You kicked {username[0]}");
            }
            List<Client> ignoredClients = new List<Client>();
            ignoredClients.Add(clients[0]);
            BeginWrite(clients[0], ComposeMessage(clients[0], 0, Message.MessageTypes.Kicked, $"{FrmHolder.username} {reason}", null)); // Kick client
            BeginWriteToAll(ignoredClients, Message.MessageTypes.OtherUserKicked, $"{username[0]} {FrmHolder.username} {reason}", null);
            return false;
        }

        private bool RunCommandAdmin(string[] commandParts)
        {
            if (commandParts.Length < 2 || commandParts[1] == null)
            {
                InvokePrintChatMessageEvent(this, $"The format is: {adminFormat}");
                return true;
            }
            string[] username = { commandParts[1] };
            List<Client> clients = ClientSearch(username, null);
            if (clients.Count == 0)
            {
                InvokePrintChatMessageEvent(this, $"No user with the username {username[0]} exists");
                return true;
            }
            bool setAsAdmin = false;
            if (commandParts.Length > 2 && commandParts[2] != null)
            {
                if (string.Equals(commandParts[2], "True", StringComparison.OrdinalIgnoreCase))
                {
                    if (clients[0].admin)
                    {
                        InvokePrintChatMessageEvent(this, $"This user is already an Admin");
                        return true;
                    }
                    setAsAdmin = true;
                }
                else if (string.Equals(commandParts[2], "False", StringComparison.OrdinalIgnoreCase))
                {
                    if (clients[0].admin == false)
                    {
                        InvokePrintChatMessageEvent(this, $"This user is already not an Admin");
                        return true;
                    }
                    setAsAdmin = false;
                }
                else
                {
                    InvokePrintChatMessageEvent(this, $"The format is: {adminFormat}");
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

        public bool BeginDisconnect()
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
                InvokeOpenMainMenuEvent(this, null);
            }
            return true;
        }

        private void ProcessMessage(object sender, MessageReceivedEventArgs e)
        {
            Message message = ComposeMessage(e.client, e.messageId, e.messageType, e.messageText, e.messageBytes);
            e.client.heartbeatReceieved = true;
            e.client.heartbeatFailures = 0;
            if (message.messageBytes != null)
            {
                if (message.CheckIfCanConvertToText())
                {
                    message.MessageTextToOrFromBytes();
                }
            }
#if DEBUG && messageReceivedUpdates
            if (message.messageType != MessagmessageTypes.Heartbeat)
            {
                if (message.messageText != null)
                {
                    InvokePrintChatMessageEvent(this, $"[RECEIVED] Type: {message.messageType}. ID: {message.messageId}. Text: {message.messageText}");
                }
                else
                {
                    InvokePrintChatMessageEvent(this, $"[RECEIVED] Type: {message.messageType}. ID: {message.messageId}");
                }
            }
#endif
            if (message.messageType != Message.MessageTypes.Acknowledgement && message.messageType != Message.MessageTypes.ClientDisconnect && message.messageType != Message.MessageTypes.Heartbeat)
            {
                BeginWrite(e.client, ComposeMessage(e.client, message.messageId, Message.MessageTypes.Acknowledgement, null, null)); // Acknowledge received message
                if (e.client.connectionSetupComplete)
                {
                    foreach (Message alreadyReceivedMessage in e.client.messagesReceived)
                    {
                        if (message.messageId == alreadyReceivedMessage.messageId)
                        {
                            return;
                        }
                    }
                }
                e.client.messagesReceived.Add(message);
            }

            if (message.messageType == Message.MessageTypes.None)
            {
                return;
            }
            else if (message.messageType == Message.MessageTypes.Acknowledgement)
            {
                foreach (Message item in e.client.messagesToBeSent)
                {
                    if (item.messageId == message.messageId)
                    {
                        e.client.messagesSent.Add(item);
                        e.client.messagesToBeSent.Remove(item);
                        break;
                    }
                }
                if (e.client.sendingMessageQueue)
                {
                    SendFirstMessageInMessageQueue(e.client);
                }
            }
            else if (message.messageType == Message.MessageTypes.ChatMessage)
            {
                string[] parts = message.messageText.Split(' ', 2);
                string username = parts[0];
                string messageText = parts[1];
                if (messageText[0] != '/')
                {
                    InvokePrintChatMessageEvent(this, $"{username}: {messageText}");
                }
                if (FrmHolder.hosting)
                {
                    BeginWriteToAll(null, Message.MessageTypes.ChatMessage, message.messageText, null);
                }
            }
            else if (message.messageType == Message.MessageTypes.ClientDisconnect)
            {
                if (FrmHolder.hosting)
                {
                    connectedClients.Remove(e.client);
                    if (e.client.tcpClient != null)
                    {
                        e.client.tcpClient.Close();
                    }
                    if (e.client.username != null)
                    {
                        InvokePrintChatMessageEvent(this, $"{e.client.username} disconnected");
                        BeginWriteToAll(null, Message.MessageTypes.UserDisconnected, e.client.username, null);
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
                    InvokeOpenMainMenuEvent(this, null);
                }
            }
            else if (message.messageType == Message.MessageTypes.UsernameInUse)
            {
                clientCancellationTokenSource.Cancel();
                MessageBox.Show("This username is already in use", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                InvokeOpenMainMenuEvent(this, null);
            }
            else if (message.messageType == Message.MessageTypes.UserConnected)
            {
                InvokePrintChatMessageEvent(this, $"{message.messageText} connected");
            }
            else if (message.messageType == Message.MessageTypes.UserDisconnected)
            {
                InvokePrintChatMessageEvent(this, $"{message.messageText} disconnected");
            }
            else if (message.messageType == Message.MessageTypes.ClearUserList)
            {
                InvokeClearClientListEvent(this, null);
            }
            else if (message.messageType == Message.MessageTypes.AddToUserList)
            {
                InvokeAddClientToClientListEvent(this, message.messageText);
            }
            else if (message.messageType == Message.MessageTypes.Kicked)
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
                InvokeOpenMainMenuEvent(this, null);
            }
            else if (message.messageType == Message.MessageTypes.OtherUserKicked)
            {
                string[] parts = message.messageText.Split(' ', 3);
                string username = parts[0];
                string kickerUsername = parts[1];
                string reason = parts[2];
                reason = reason.Trim();
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    InvokePrintChatMessageEvent(this, $"{username} was kicked by {kickerUsername} with reason: {reason}");
                }
                else
                {
                    InvokePrintChatMessageEvent(this, $"{username} was kicked by {kickerUsername}");
                }
            }
            else if (message.messageType == Message.MessageTypes.Heartbeat)
            {
                if (FrmHolder.hosting)
                {
                    BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.Heartbeat, null, null));
                }
            }
            else if (message.messageType == Message.MessageTypes.OwnClientId)
            {
                FrmHolder.clientId = Convert.ToUInt32(message.messageText);
            }
            else if (message.messageType == Message.MessageTypes.OtherUserLostConnection)
            {
                InvokePrintChatMessageEvent(this, $"{message.messageText} has lost connection...");
            }
            else if (message.messageType == Message.MessageTypes.MadeAdmin)
            {
                InvokePrintChatMessageEvent(this, $"You have been made an Admin by {message.messageText}");
            }
            else if (message.messageType == Message.MessageTypes.OtherUserMadeAdmin)
            {
                string[] parts = message.messageText.Split(' ', 2);
                string username = parts[0];
                string setterUsername = parts[1];
                InvokePrintChatMessageEvent(this, $"{username} has been made an Admin by {setterUsername}");
            }
            else if (message.messageType == Message.MessageTypes.RemovedAdmin)
            {
                InvokePrintChatMessageEvent(this, $"You have been removed from Admin by {message.messageText}");
            }
            else if (message.messageType == Message.MessageTypes.OtherUserRemovedAdmin)
            {
                string[] parts = message.messageText.Split(' ', 2);
                string username = parts[0];
                string setterUsername = parts[1];
                InvokePrintChatMessageEvent(this, $"{username} has been removed from Admin by {setterUsername}");
            }
            else if (message.messageType == Message.MessageTypes.SendMessageQueue)
            {
                e.client.sendingMessageQueue = true;
                e.client.receivingMessageQueue = false;
                SendFirstMessageInMessageQueue(e.client);
            }
            else if (message.messageType == Message.MessageTypes.ConnectionSetupComplete)
            {
                e.client.connectionSetupComplete = true;
            }
            else if (message.messageType == Message.MessageTypes.ClientId)
            {
                if (string.IsNullOrWhiteSpace(message.messageText) || message.messageText == "0")
                {
                    e.client.clientId = nextAssignableClientId;
                    nextAssignableClientId++;
                    BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.OwnClientId, e.client.clientId.ToString(), null));
                    e.client.receivedClientId = true;
                    return;
                }
                uint clientId;
                bool converted = uint.TryParse(message.messageText, out clientId);
                if (converted)
                {
                    for (int i = 0; i < connectedClients.Count(); i++)
                    {
                        if (clientId == connectedClients[i].clientId)
                        {
                            e.client.sessionFirstConnection = false;
                            e.client = MergeClient(e.client, connectedClients[i]);
                            break;
                        }
                    }
                    e.client.clientId = clientId;
                    e.client.receivedClientId = true;
                }
            }
            else if (message.messageType == Message.MessageTypes.RequestVersionNumber)
            {
                BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.ClientVersionNumber, VersionNumber.applicationVersion, null));
            }
            else if (message.messageType == Message.MessageTypes.ClientVersionNumber)
            {
                e.client.applicationVersionNumber = (message.messageText);
                e.client.receivedApplicationVersionNumber = true;
                char versionDifference = VersionNumber.CheckVersionCompatibility(VersionNumber.minimumSupportedClientVersion, VersionNumber.maximumSupportedClientVersion, e.client.applicationVersionNumber, VersionNumber.allowClientPreRelease);
                BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.ServersMinimumSupportedClientVersionNumber, VersionNumber.minimumSupportedClientVersion, null));
                BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.ServersMaximumSupportedClientVersionNumber, VersionNumber.maximumSupportedClientVersion, null));
                BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.ServersPreReleaseSupport, VersionNumber.allowClientPreRelease.ToString(), null));
                BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.ServerVersionNumberCompatibility, versionDifference.ToString(), null));
                if (versionDifference == '<' || versionDifference == '>')
                {
                    connectedClients.Remove(e.client);
                    if (e.client.sslStream != null)
                    {
                        e.client.sslStream.Close();
                    }
                }
            }
            else if (message.messageType == Message.MessageTypes.RequestUsername)
            {
                BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.ClientUsername, FrmHolder.username, null));
            }
            else if (message.messageType == Message.MessageTypes.ClientUsername)
            {
                string requestedUsername = message.messageText;
                bool usernameAlreadyInUse = false;
                for (int i = 0; i < connectedClients.Count(); i++)
                {
                    if (string.Equals(connectedClients[i].username, requestedUsername, StringComparison.OrdinalIgnoreCase))
                    {
                        if (e.client.clientId != connectedClients[i].clientId)
                        {
                            usernameAlreadyInUse = true;
                            BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.UsernameInUse, null, null));
                            break;
                        }
                    }
                }
                if (usernameAlreadyInUse == false)
                {
                    e.client.username = requestedUsername;
                    e.client.receivedUsername = true;
                }
            }
            else if (message.messageType == Message.MessageTypes.RequestClientId)
            {
                string clientId = FrmHolder.clientId == 0 ? null : FrmHolder.clientId.ToString();
                BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.ClientId, clientId, null));
            }
            else if (message.messageType == Message.MessageTypes.ServersMinimumSupportedClientVersionNumber)
            {
                string serverMinimumSupportedClientApplicationVersionNumber = message.messageText;
                e.client.serverMinimumSupportedClientApplicationVersionNumber = serverMinimumSupportedClientApplicationVersionNumber;
            }
            else if (message.messageType == Message.MessageTypes.ServersMaximumSupportedClientVersionNumber)
            {
                string serverMaximumSupportedClientApplicationVersionNumber = message.messageText;
                e.client.serverMaximumSupportedClientApplicationVersionNumber = serverMaximumSupportedClientApplicationVersionNumber;
            }
            else if (message.messageType == Message.MessageTypes.ServersPreReleaseSupport)
            {
                bool serverSupportsClientPreReleaseVersionNumbers = message.messageText == "0" ? true : false;
                e.client.serverSupportsClientPreReleaseAppplicationVersionNumber = serverSupportsClientPreReleaseVersionNumbers;
            }
            else if (message.messageType == Message.MessageTypes.ServerVersionNumberCompatibility)
            {
                char clientApplicationNumberServerCompatibility = '=';
                bool converted = Char.TryParse(message.messageText, out clientApplicationNumberServerCompatibility);
                if (converted)
                {
                    e.client.clientToServerVersionNumberCompatibility = clientApplicationNumberServerCompatibility;
                    bool unsupportedVersion = false;
                    if (e.client.clientToServerVersionNumberCompatibility == '<')
                    {
                        unsupportedVersion = true;
                    }
                    else if (e.client.clientToServerVersionNumberCompatibility == '>')
                    {
                        unsupportedVersion = true;
                    }
                    if (unsupportedVersion)
                    {
                        string difference = clientApplicationNumberServerCompatibility == '<' ? "an older" : "a newer";
                        MessageBox.Show($"You are running {difference} version ({VersionNumber.applicationVersion}) than that which is supported by the server ({e.client.serverMaximumSupportedClientApplicationVersionNumber}).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BeginDisconnect();
                    }
                }
                else
                {
                    MessageBox.Show($"Unable to determine whether the client is on a version supported by the server.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    BeginDisconnect();
                }
            }
            else if (message.messageType == Message.MessageTypes.ServerVersionNumber)
            {
                string serverApplicationVersionNumber = message.messageText;
                e.client.serverApplicationVersionNumber = serverApplicationVersionNumber;
            }
            else if (message.messageType == Message.MessageTypes.FinishedSendingMessageQueue)
            {
                e.client.receivingMessageQueue = false;
            }
            else if (message.messageType == Message.MessageTypes.RankGiven)
            {
                string[] parts = message.messageText.Split(' ', 2);
                string issuer = parts[0];
                string rank = parts[1];
                InvokePrintChatMessageEvent(this, $"You have been given the rank {rank} by {issuer}");
            }
            else if (message.messageType == Message.MessageTypes.RankTaken)
            {
                string[] parts = message.messageText.Split(' ', 2);
                string issuer = parts[0];
                string rank = parts[1];
                InvokePrintChatMessageEvent(this, $"You have been removed from the rank {rank} by {issuer}");
            }
            else if (message.messageType == Message.MessageTypes.OtherUserRankGiven)
            {
                string[] parts = message.messageText.Split(' ', 3);
                string username = parts[0];
                string issuer = parts[1];
                string rank = parts[2];
                InvokePrintChatMessageEvent(this, $"{username} has been given the rank {rank} by {issuer}");
            }
            else if (message.messageType == Message.MessageTypes.OtherUserRankTaken)
            {
                string[] parts = message.messageText.Split(' ', 3);
                string username = parts[0];
                string issuer = parts[1];
                string rank = parts[2];
                InvokePrintChatMessageEvent(this, $"{username} has been removed from the rank {rank} by {issuer}");
            }
            else if (message.messageType == Message.MessageTypes.RequestAllRanks)
            {
                string json = ranks.SerializeToJson();
                BeginWrite(e.client, ComposeMessage(e.client, 0, Message.MessageTypes.AllRanks, json, null));
            }
            else if (message.messageType == Message.MessageTypes.AllRanks)
            {
                if (FrmHolder.hosting)
                {
                    Ranks.Changes changes = Ranks.Changes.DeserializeFromJson(message.messageText);
                    SaveRanksAsServer(e.client, changes);
                }
                else
                {
                    List<Ranks.Rank> receivedRanks = JsonSerializer.Deserialize<List<Ranks.Rank>>(message.messageText);
                    ranks.UpdateRanksList(receivedRanks);
                }
            }
            else if (message.messageType == Message.MessageTypes.ChangedRanks)
            {
                if (FrmHolder.hosting == false)
                {
                    Ranks.Changes changes = Ranks.Changes.DeserializeFromJson(message.messageText);
                    ranks.UpdateRanksList(changes);
                }
            }
            else if (message.messageType == Message.MessageTypes.InvalidRanks)
            {
                SavingRanksFailedInvalidRanks();
            }
        }

#region Invoke Events
        private void InvokeFirstConnectionAttemptResultEvent(object sender, FirstConnectionAttemptResultEventArgs e)
        {
            if (FirstConnectionAttemptResultEvent != null)
            {
                FirstConnectionAttemptResultEvent.Invoke(sender, e);
            }
        }

        private void InvokeOpenMainMenuEvent(object sender, EventArgs e)
        {
            if (OpenMainMenuEvent != null)
            {
                OpenMainMenuEvent.Invoke(sender, e);
            }
        }

        private void InvokePrintChatMessageEvent(object sender, string e)
        {
            if (PrintChatMessageEvent != null)
            {
                PrintChatMessageEvent.Invoke(sender, e);
            }
        }

        private void InvokeHeartbeatTimeoutEvent(object sender, Client e)
        {
            if (HeartbeatTimeoutEvent != null)
            {
                HeartbeatTimeoutEvent.Invoke(sender, e);
            }
        }

        private void InvokeClearClientListEvent(object sender, EventArgs e)
        {
            if (ClearClientListEvent != null)
            {
                ClearClientListEvent.Invoke(sender, e);
            }
        }

        private void InvokeAddClientToClientListEvent(object sender, string e)
        {
            if (AddClientToClientListEvent != null)
            {
                AddClientToClientListEvent.Invoke(sender, e);
            }
        }

        private void InvokeShowMessageBoxEvent(object sender, ShowMessageBoxEventArgs e)
        {
            if (ShowMessageBoxEvent != null)
            {
                ShowMessageBoxEvent.Invoke(sender, e);
            }
        }
#endregion

        private void OnFirstConnectionAttemptResult(object sender, FirstConnectionAttemptResultEventArgs e)
        {
            InvokeFirstConnectionAttemptResultEvent(this, e);
        }

        private void OnShowMessageBox(object sender, ShowMessageBoxEventArgs showMessageBoxEventArgs)
        {
            ShowMessageBoxEvent.Invoke(this, showMessageBoxEventArgs);
        }

        private void OnAcceptTcpClient(object sender, Client client)
        {
            NextStepInConnectionSetupAsServer(client);
        }
    }
}
