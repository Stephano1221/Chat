//#define messageReceivedUpdates
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
        private Network network = new Network();
        private bool askToClose = true;

        private delegate void MessageDelegate(object sender, MessageReceivedEventArgs e);
        private delegate void HeartbeatDelegate(Client client);
        private delegate void ClearClientClistDelegate();
        private delegate void AddClientToClientListDelegate(string username);
        private delegate void PrintChatMessageDelegate(string text);
        private delegate void NextConnectionSetupStepDelegate(Client client);
        private delegate DialogResult ShowMessageBoxDelegate(string message, string caption, MessageBoxButtons messageBoxButtons, MessageBoxIcon messageBoxIcon);

        private string kickFormat = "/kick [Username] [Reason (optional)]";
        private string adminFormat = "/admin [Username] [True/False (optional)]";

        public FrmChatScreen()
        {
            InitializeComponent();
            SetNetworkEventHandlers();
            SetFormEventHandlers();
            xlsvConnectedUsers.Columns[0].Width = xlsvConnectedUsers.Width - 5;
            network.BeginNetworkThreads();
        }

        private void SetFormEventHandlers()
        {
            this.Load += new EventHandler(FrmChatScreen_Load);
            this.FormClosing += new FormClosingEventHandler(OnClosing);
        }

        private void SetNetworkEventHandlers()
        {
            network.MessageReceivedEvent += OnMessageReceived;
            network.HeartbeatTimeoutEvent += OnHeartbeatTimeoutFailure;
            network.PrintChatMessageEvent += OnPrintChatMessage;
            network.ClearClientListEvent += OnClearClientList;
            network.AddClientToClientListEvent += OnAddClientToClientList;
            network.NextConnectionSetupStep += OnNextConnectionSetupStep;
            network.ShowMessageBoxEvent += OnShowMessagBox;
        }

        private void ProcessMessage(object sender, MessageReceivedEventArgs e)
        {
            e.client.heartbeatReceieved = true;
            e.client.heartbeatFailures = 0;
            if (e.message.messageBytes != null)
            {
                if (e.message.CheckIfCanConvertToText())
                {
                    e.message.MessageTextToOrFromBytes();
                }
            }
#if DEBUG && messageReceivedUpdates
            if (e.message.messageType != 11)
            {
                if (e.message.messageText != null)
                {
                    PrintChatMessage($"[RECEIVED] Type: {e.message.messageType}. ID: {e.message.messageId}. Text: {e.message.messageText}");
                }
                else
                {
                    PrintChatMessage($"[RECEIVED] Type: {e.message.messageType}. ID: {e.message.messageId}");
                }
            }
#endif
            if (e.message.messageType != Message.MessageTypes.Acknowledgement && e.message.messageType != Message.MessageTypes.ClientDisconnect && e.message.messageType != Message.MessageTypes.Heartbeat)
            {
                network.BeginWrite(e.client, network.ComposeMessage(e.client, e.message.messageId, Message.MessageTypes.Acknowledgement, null, null)); // Acknowledge received message
                if (e.client.connectionSetupComplete)
                {
                    foreach (Message alreadyReceivedMessage in e.client.messagesReceived)
                    {
                        if (e.message.messageId == alreadyReceivedMessage.messageId)
                        {
                            return;
                        }
                    }
                }
                e.client.messagesReceived.Add(e.message);
            }

            if (e.message.messageType == Message.MessageTypes.None)
            {
                return;
            }
            else if (e.message.messageType == Message.MessageTypes.Acknowledgement)
            {
                foreach (Message item in e.client.messagesToBeSent)
                {
                    if (item.messageId == e.message.messageId)
                    {
                        e.client.messagesSent.Add(item);
                        e.client.messagesToBeSent.Remove(item);
                        break;
                    }
                }
                if (e.client.sendingMessageQueue)
                {
                    network.SendFirstMessageInMessageQueue(e.client);
                }
            }
            else if (e.message.messageType == Message.MessageTypes.ChatMessage)
            {
                string[] parts = e.message.messageText.Split(' ', 2);
                string username = parts[0];
                string messageText = parts[1];
                PrintChatMessage($"{username}: {messageText}");
                if (FrmHolder.hosting)
                {
                    network.SendToAll(null, Message.MessageTypes.ChatMessage, e.message.messageText, null);
                }
            }
            else if (e.message.messageType == Message.MessageTypes.ClientDisconnect)
            {
                if (FrmHolder.hosting)
                {
                    network.connectedClients.Remove(e.client);
                    if (e.client.tcpClient != null)
                    {
                        e.client.tcpClient.Close();
                    }
                    if (e.client.username != null)
                    {
                        PrintChatMessage($"{e.client.username} disconnected");
                        network.SendToAll(null, Message.MessageTypes.UserDisconnected, e.client.username, null);
                    }
                    network.UpdateClientLists();
                }
                else
                {
                    if (network.clientThread != null && network.clientThread.IsAlive)
                    {
                        network.clientCancellationTokenSource.Cancel();
                    }
                    MessageBox.Show("The server was closed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OpenMainMenu();
                }
            }
            else if (e.message.messageType == Message.MessageTypes.UsernameInUse)
            {
                network.clientCancellationTokenSource.Cancel();
                MessageBox.Show("This username is already in use", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OpenMainMenu();
            }
            else if (e.message.messageType == Message.MessageTypes.UserConnected)
            {
                PrintChatMessage($"{e.message.messageText} connected");
            }
            else if (e.message.messageType == Message.MessageTypes.UserDisconnected)
            {
                PrintChatMessage($"{e.message.messageText} disconnected");
            }
            else if (e.message.messageType == Message.MessageTypes.ClearUserList)
            {
                xlsvConnectedUsers.Items.Clear();
            }
            else if (e.message.messageType == Message.MessageTypes.AddToUserList)
            {
                xlsvConnectedUsers.Items.Add(e.message.messageText);
            }
            else if (e.message.messageType == Message.MessageTypes.Kicked)
            {
                if (network.clientThread != null && network.clientThread.IsAlive)
                {
                    network.clientCancellationTokenSource.Cancel();
                }
                string[] parts = e.message.messageText.Split(' ', 2);
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
            else if (e.message.messageType == Message.MessageTypes.OtherUserKicked)
            {
                string[] parts = e.message.messageText.Split(' ', 3);
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
            else if (e.message.messageType == Message.MessageTypes.Heartbeat)
            {
                if (FrmHolder.hosting)
                {
                    network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.Heartbeat, null, null));
                }
            }
            else if (e.message.messageType == Message.MessageTypes.OwnClientId)
            {
                FrmHolder.clientId = Convert.ToUInt32(e.message.messageText);
            }
            else if (e.message.messageType == Message.MessageTypes.OtherUserLostConnection)
            {
                PrintChatMessage($"{e.message.messageText} has lost connection...");
            }
            else if (e.message.messageType == Message.MessageTypes.MadeAdmin)
            {
                PrintChatMessage($"You have been made an Admin by {e.message.messageText}");
            }
            else if (e.message.messageType == Message.MessageTypes.OtherUserMadeAdmin)
            {
                string[] parts = e.message.messageText.Split(' ', 2);
                string username = parts[0];
                string setterUsername = parts[1];
                PrintChatMessage($"{username} has been made an Admin by {setterUsername}");
            }
            else if (e.message.messageType == Message.MessageTypes.RemovedAdmin)
            {
                PrintChatMessage($"You have been removed from Admin by {e.message.messageText}");
            }
            else if (e.message.messageType == Message.MessageTypes.OtherUserRemovedAdmin)
            {
                string[] parts = e.message.messageText.Split(' ', 2);
                string username = parts[0];
                string setterUsername = parts[1];
                PrintChatMessage($"{username} has been removed from Admin by {setterUsername}");
            }
            else if (e.message.messageType == Message.MessageTypes.SendMessageQueue)
            {
                e.client.sendingMessageQueue = true;
                e.client.receivingMessageQueue = false;
                network.SendFirstMessageInMessageQueue(e.client);
            }
            else if (e.message.messageType == Message.MessageTypes.ConnectionSetupComplete)
            {
                e.client.connectionSetupComplete = true;
            }
            else if (e.message.messageType == Message.MessageTypes.ClientId)
            {
                if (string.IsNullOrWhiteSpace(e.message.messageText) || e.message.messageText == "0")
                {
                    e.client.clientId = network.nextAssignableClientId;
                    network.nextAssignableClientId++;
                    network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.OwnClientId, e.client.clientId.ToString(), null));
                    e.client.receivedClientId = true;
                    return;
                }
                uint clientId;
                bool converted = uint.TryParse(e.message.messageText, out clientId);
                if (converted)
                {
                    for (int i = 0; i < network.connectedClients.Count(); i++)
                    {
                        if (clientId == network.connectedClients[i].clientId)
                        {
                            e.client.sessionFirstConnection = false;
                            e.client = network.MergeClient(e.client, network.connectedClients[i]);
                            break;
                        }
                    }
                    e.client.clientId = clientId;
                    e.client.receivedClientId = true;
                }
            }
            else if (e.message.messageType == Message.MessageTypes.RequestVersionNumber)
            {
                network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.ClientVersionNumber, FrmHolder.applicationVersion, null));
            }
            else if (e.message.messageType == Message.MessageTypes.ClientVersionNumber)
            {
                e.client.applicationVersionNumber = (e.message.messageText);
                e.client.receivedApplicationVersionNumber = true;
                char versionDifference = CheckVersionCompatibility(FrmHolder.minimumSupportedClientVersion, FrmHolder.maximumSupportedClientVersion, e.client.applicationVersionNumber, FrmHolder.allowClientPreRelease);
                network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.ServersMinimumSupportedClientVersionNumber, FrmHolder.minimumSupportedClientVersion, null));
                network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.ServersMaximumSupportedClientVersionNumber, FrmHolder.maximumSupportedClientVersion, null));
                network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.ServersPreReleaseSupport, FrmHolder.allowClientPreRelease.ToString(), null));
                network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.ServerVersionNumberCompatibility, versionDifference.ToString(), null));
                if (versionDifference == '<' || versionDifference == '>')
                {
                    network.connectedClients.Remove(e.client);
                    if (e.client.sslStream != null)
                    {
                        e.client.sslStream.Close();
                    }
                }
            }
            else if (e.message.messageType == Message.MessageTypes.RequestUsername)
            {
                network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.ClientUsername, FrmHolder.username, null));
            }
            else if (e.message.messageType == Message.MessageTypes.ClientUsername)
            {
                string requestedUsername = e.message.messageText;
                bool usernameAlreadyInUse = false;
                for (int i = 0; i < network.connectedClients.Count(); i++)
                {
                    if (string.Equals(network.connectedClients[i].username, requestedUsername, StringComparison.OrdinalIgnoreCase))
                    {
                        if (e.client.clientId != network.connectedClients[i].clientId)
                        {
                            usernameAlreadyInUse = true;
                            network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.UsernameInUse, null, null));
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
            else if (e.message.messageType == Message.MessageTypes.RequestClientId)
            {
                string clientId = FrmHolder.clientId == 0 ? null : FrmHolder.clientId.ToString();
                network.BeginWrite(e.client, network.ComposeMessage(e.client, 0, Message.MessageTypes.ClientId, clientId, null));
            }
            else if (e.message.messageType == Message.MessageTypes.ServersMinimumSupportedClientVersionNumber)
            {
                string serverMinimumSupportedClientApplicationVersionNumber = e.message.messageText;
                e.client.serverMinimumSupportedClientApplicationVersionNumber = serverMinimumSupportedClientApplicationVersionNumber;
            }
            else if (e.message.messageType == Message.MessageTypes.ServersMaximumSupportedClientVersionNumber)
            {
                string serverMaximumSupportedClientApplicationVersionNumber = e.message.messageText;
                e.client.serverMaximumSupportedClientApplicationVersionNumber = serverMaximumSupportedClientApplicationVersionNumber;
            }
            else if (e.message.messageType == Message.MessageTypes.ServersPreReleaseSupport)
            {
                bool serverSupportsClientPreReleaseVersionNumbers = e.message.messageText == "0" ? true : false;
                e.client.serverSupportsClientPreReleaseAppplicationVersionNumber = serverSupportsClientPreReleaseVersionNumbers;
            }
            else if (e.message.messageType == Message.MessageTypes.ServerVersionNumberCompatibility)
            {
                char clientApplicationNumberServerCompatibility = '=';
                bool converted = Char.TryParse(e.message.messageText, out clientApplicationNumberServerCompatibility);
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
                        MessageBox.Show($"You are running {difference} version ({FrmHolder.applicationVersion}) than that which is supported by the server ({e.client.serverMaximumSupportedClientApplicationVersionNumber}).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BeginDisconnect();
                    }
                }
                else
                {
                    MessageBox.Show($"Unable to determine whether the client is on a version supported by the server.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    BeginDisconnect();
                }
            }
            else if (e.message.messageType == Message.MessageTypes.ServerVersionNumber)
            {
                string serverApplicationVersionNumber = e.message.messageText;
                e.client.serverApplicationVersionNumber = serverApplicationVersionNumber;
            }
            else if (e.message.messageType == Message.MessageTypes.FinishedSendingMessageQueue)
            {
                e.client.receivingMessageQueue = false;
            }
        }

        private void ClearClientList()
        {
            xlsvConnectedUsers.Items.Clear();
        }

        private void AddClientToClientList(string username)
        {
            xlsvConnectedUsers.Items.Add(username);
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
                    this.ParentForm.Text = $"{FrmHolder.applicationWindowText} - {FrmHolder.username} hosting on {network.publicIp}";
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

        private int[] SplitVersionNumberPrefix(string versionNumberToSplit)
        {
            if (string.IsNullOrWhiteSpace(versionNumberToSplit))
            {
                return null;
            }
            string versionNumberToSplitPrefix = versionNumberToSplit.Split('-', 2)[0];
            string[] versionNumbersSplitAsString = versionNumberToSplitPrefix.Split('.');
            int[] VersionNumbersSplitAsInt = new int[versionNumbersSplitAsString.Count()];
            for (int i = 0; i < versionNumbersSplitAsString.Count(); i++)
            {
                int.TryParse(versionNumbersSplitAsString[i], out VersionNumbersSplitAsInt[i]);
            }
            return VersionNumbersSplitAsInt;
        }

        private string GetPreReleaseNumberFromVersionNumber(string versionNumberToSplit)
        {
            if (string.IsNullOrWhiteSpace(versionNumberToSplit))
            {
                return versionNumberToSplit;
            }
            string preReleaseNumber = null;
            char seperator = '-';
            string[] preReleaseNumberParts = versionNumberToSplit.Split(seperator, 2);
            if (preReleaseNumberParts.Count() > 1)
            {
                preReleaseNumber = preReleaseNumberParts[1];
            }
            return preReleaseNumber;
        }


        private string removeBuildInfoFromVersionNumber(string versionNumberToSplit)
        {
            if (string.IsNullOrWhiteSpace(versionNumberToSplit))
            {
                return versionNumberToSplit;
            }
            string versionNumberWithoutBuildInfo = versionNumberToSplit;
            char seperator = '+';
            string[] versionNumberParts = versionNumberToSplit.Split(seperator, 2);
            if (versionNumberWithoutBuildInfo.Count() > 1)
            {
                versionNumberWithoutBuildInfo = versionNumberParts[0];
            }
            return versionNumberWithoutBuildInfo;
        }

        private char DeterminePreReleaseVersionNumberPrecedence(string basePreReleaseVersionNumber, string challengePreReleaseVersionNumber, bool allowPreRelease)
        {
            if (basePreReleaseVersionNumber == null && challengePreReleaseVersionNumber == null)
            {
                return '=';
            }
            else if ((basePreReleaseVersionNumber == null || allowPreRelease == false) && challengePreReleaseVersionNumber != null)
            {
                return '<';
            }
            else if (basePreReleaseVersionNumber != null && challengePreReleaseVersionNumber == null)
            {
                return '>';
            }
            char identifierSeperator = '.';
            string[] basePreReleaseVersionNumberIdentifiers = basePreReleaseVersionNumber.Split(identifierSeperator);
            string[] challengePreReleaseVersionNumberIdentifiers = challengePreReleaseVersionNumber.Split(identifierSeperator);
            int[] preReleaseIdentifierCount = { basePreReleaseVersionNumberIdentifiers.Count(), challengePreReleaseVersionNumberIdentifiers.Count() };
            int smallestPreReleaseIdentifierCount = preReleaseIdentifierCount.Min();
            for (int i = 0; i < smallestPreReleaseIdentifierCount; i++)
            {
                int basePreReleaseVersionNumberIdentifier;
                int challengePreReleaseVersionNumberIdentifier;
                if (Int32.TryParse(basePreReleaseVersionNumberIdentifiers[i], out basePreReleaseVersionNumberIdentifier) && Int32.TryParse(challengePreReleaseVersionNumberIdentifiers[i], out challengePreReleaseVersionNumberIdentifier))
                {
                    if (basePreReleaseVersionNumberIdentifier > challengePreReleaseVersionNumberIdentifier)
                    {
                        return '<';
                    }
                    else if (basePreReleaseVersionNumberIdentifier < challengePreReleaseVersionNumberIdentifier)
                    {
                        return '>';
                    }
                }
                else
                {
                    int[] preReleaseIdentifierCharacterCount = { basePreReleaseVersionNumberIdentifiers[i].Count(), challengePreReleaseVersionNumberIdentifiers[i].Count() };
                    int smallestPreReleaseIdentifierCharacterCount = preReleaseIdentifierCharacterCount.Min();
                    for (int j = 0; j < smallestPreReleaseIdentifierCharacterCount; j++)
                    {
                        int basePreReleaseCharacterCode = basePreReleaseVersionNumberIdentifiers[i][j];
                        int challengePreReleaseCharacterCode = challengePreReleaseVersionNumberIdentifiers[i][j];
                        if (basePreReleaseCharacterCode > challengePreReleaseCharacterCode)
                        {
                            return '<';
                        }
                        else if (basePreReleaseCharacterCode < challengePreReleaseCharacterCode)
                        {
                            return '>';
                        }
                    }
                    if (preReleaseIdentifierCharacterCount[0] > preReleaseIdentifierCharacterCount[1])
                    {
                        return '<';
                    }
                    else if (preReleaseIdentifierCharacterCount[0] < preReleaseIdentifierCharacterCount[1])
                    {
                        return '>';
                    }
                }
            }
            if (preReleaseIdentifierCount[0] > preReleaseIdentifierCount[1])
            {
                return '<';
            }
            else if (preReleaseIdentifierCount[0] < preReleaseIdentifierCount[1])
            {
                return '>';
            }
            return '=';
        }

        private char CheckVersionCompatibility(string minimumVersionNumber, string maximumVersionNumber, string challengeVersionNumber, bool allowPreRelease)
        {
            char minimumVersionNumberDifference = CompareVersionNumber(minimumVersionNumber, challengeVersionNumber, allowPreRelease);
            if (minimumVersionNumberDifference == '<')
            {
                return minimumVersionNumberDifference;
            }
            char maximumVersionNumberDifference = CompareVersionNumber(maximumVersionNumber, challengeVersionNumber, allowPreRelease);
            if (maximumVersionNumberDifference == '>')
            {
                return maximumVersionNumberDifference;
            }
            return '=';
        }

        private char CompareVersionNumber(string baseVersionNumber, string challengeVersionNumber, bool allowPreRelease)
        {
            if (string.IsNullOrWhiteSpace(baseVersionNumber) || string.IsNullOrWhiteSpace(challengeVersionNumber))
            {
                return '=';
            }
            string baseVersionNumberWithoutBuildInfo = removeBuildInfoFromVersionNumber(baseVersionNumber);
            string challengeVersionNumberWithoutBuildInfo = removeBuildInfoFromVersionNumber(challengeVersionNumber);
            int[] baseVersionNumberSplit = SplitVersionNumberPrefix(baseVersionNumberWithoutBuildInfo);
            int[] challengeVersionNumberSplit = SplitVersionNumberPrefix(challengeVersionNumberWithoutBuildInfo);
            if (baseVersionNumberSplit[0] != challengeVersionNumberSplit[0]) // Incompatible
            {
                char versionDifference = CompareIndividualVersionNumber(baseVersionNumberSplit[0], challengeVersionNumberSplit[0]);
                return versionDifference;
            }
            if (baseVersionNumberSplit[0] == 0 || challengeVersionNumberSplit[0] == 0)
            {
                if (baseVersionNumberSplit[1] != challengeVersionNumberSplit[1]) // Incompatible
                {
                    char versionDifference = CompareIndividualVersionNumber(baseVersionNumberSplit[1], challengeVersionNumberSplit[1]);
                    return versionDifference;
                }
            }
            int[] versionNumberPrefixIdentifierCount = { baseVersionNumberSplit.Count(), challengeVersionNumberSplit.Count() };
            int smallestVersionNumberPrefixIdentifierCount = versionNumberPrefixIdentifierCount.Min();
            for (int i = 1; i < smallestVersionNumberPrefixIdentifierCount; i++)
            {
                char VersionDifference = CompareIndividualVersionNumber(baseVersionNumberSplit[i], challengeVersionNumberSplit[i]);
                if (VersionDifference != '=') // Incompatible
                {
                    return VersionDifference;
                }
            }
            string basePreReleaseVersionNumber = GetPreReleaseNumberFromVersionNumber(baseVersionNumberWithoutBuildInfo);
            if (basePreReleaseVersionNumber != null)
            {
                allowPreRelease = true;
            }
            string challengePreReleaseVersionNumber = GetPreReleaseNumberFromVersionNumber(challengeVersionNumberWithoutBuildInfo);
            char preReleaseVersionDifference = DeterminePreReleaseVersionNumberPrecedence(basePreReleaseVersionNumber, challengePreReleaseVersionNumber, allowPreRelease);
            return preReleaseVersionDifference;
        }

        private char CompareIndividualVersionNumber(int minimumVersionNumber, int challengeVersionNumber)
        {
            if (minimumVersionNumber < challengeVersionNumber)
            {
                return '>';
            }
            else if (minimumVersionNumber > challengeVersionNumber)
            {
                return '<';
            }
            return '=';
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
                    network.BeginWrite(client, network.ComposeMessage(client, 0, Message.MessageTypes.RequestVersionNumber, null, null));
                    client.requestedApplicationVersionNumber = true;
                }
                return;
            }
            if (client.receivedClientId == false)
            {
                if (client.requestedClientId == false)
                {
                    network.BeginWrite(client, network.ComposeMessage(client, 0, Message.MessageTypes.RequestClientId, null, null));
                    client.requestedClientId = true;
                }
                return;
            }
            if (client.receivedUsername == false)
            {
                if (client.requestedUsername == false)
                {
                    network.BeginWrite(client, network.ComposeMessage(client, 0, Message.MessageTypes.RequestUsername, null, null));
                    client.requestedUsername = true;
                }
                return;
            }
            if (client.sessionFirstConnection)
            {
                List<Client> ignoredClients = new List<Client>();
                ignoredClients.Add(client);
                PrintChatMessage($"{client.username} connected");
                network.SendToAll(ignoredClients, Message.MessageTypes.UserConnected, client.username, null);
                network.UpdateClientLists();
            }
            client.connectionSetupComplete = true;
            network.BeginWrite(client, network.ComposeMessage(client, 0, Message.MessageTypes.ConnectionSetupComplete, null, null));
            network.BeginWrite(client, network.ComposeMessage(client, 0, Message.MessageTypes.SendMessageQueue, null, null));
            client.receivingMessageQueue = true;
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
            List<Client> clients = network.ClientSearch(username, null);
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
            network.BeginWrite(clients[0], network.ComposeMessage(clients[0], 0, Message.MessageTypes.Kicked, $"{FrmHolder.username} {reason}", null)); // Kick client
            network.SendToAll(ignoredClients, Message.MessageTypes.OtherUserKicked, $"{username[0]} {FrmHolder.username} {reason}", null);
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
            List<Client> clients = network.ClientSearch(username, null);
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
                    if (network.ClientSearch(username, null)[0].admin)
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
                network.SetAdmin(clients[0], FrmHolder.username, setAsAdmin, null);
            }
            return false;
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
                    if (network.serverThread != null && network.serverThread.IsAlive)
                    {
                        network.serverCancellationTokenSource.Cancel();
                    }
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    return false;
                }
            }
            if (network.clientThread != null && network.clientThread.IsAlive)
            {
                network.clientCancellationTokenSource.Cancel();
            }
            if (serverClose || FrmHolder.hosting == false)
            {
                OpenMainMenu();
            }
            return true;
        }


        private DialogResult ShowMessageBox(string message, string caption, MessageBoxButtons messageBoxButtons, MessageBoxIcon messageBoxIcon)
        {
            message = message == null ? "" : message;
            caption = caption == null ? "" : caption;
            return DialogResult = MessageBox.Show(this, message, caption, messageBoxButtons, messageBoxIcon);
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

        #region Event handlers
        private void OnHeartbeatTimeoutFailure(object sender, Client client)
        {
            if (xlbxChat.InvokeRequired)
            {
                xlbxChat.BeginInvoke(new HeartbeatDelegate(network.HeartbeatTimeoutFailure), client);
            }
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (xlbxChat.InvokeRequired)
            {
                xlbxChat.BeginInvoke(new MessageDelegate(ProcessMessage), this, e);
                if (e.client.connectionSetupComplete == false)
                {
                    xlbxChat.BeginInvoke(new NextConnectionSetupStepDelegate(NextStepInConnectionSetupAsServer), e.client);
                }
            }
            else
            {
                ProcessMessage(this, e);
                if (e.client.connectionSetupComplete == false)
                {
                    NextStepInConnectionSetupAsServer(e.client);
                }
            }
        }

        private void OnClearClientList(object sender, EventArgs e)
        {
            if (xlsvConnectedUsers.InvokeRequired)
            {
                xlsvConnectedUsers.BeginInvoke(new ClearClientClistDelegate(ClearClientList));
            }
            else
            {
                xlsvConnectedUsers.Items.Clear();
            }
        }

        private void OnAddClientToClientList(object sender, string username)
        {
            if (xlsvConnectedUsers.InvokeRequired)
            {
                xlsvConnectedUsers.BeginInvoke(new AddClientToClientListDelegate(AddClientToClientList), username);
            }
            else
            {
                xlsvConnectedUsers.Items.Add(username);
            }
        }

        private void OnPrintChatMessage(object sender, string text)
        {
            if (xlbxChat.InvokeRequired)
            {
                xlbxChat.BeginInvoke(new PrintChatMessageDelegate(PrintChatMessage), text);
            }
            else
            {
                xlbxChat.Items.Add(text);
            }
        }

        private void OnNextConnectionSetupStep(object sender, Client client)
        {
            if (xlbxChat.InvokeRequired)
            {
                xlbxChat.BeginInvoke(new NextConnectionSetupStepDelegate(NextStepInConnectionSetupAsServer), client);
            }
            else
            {
                NextStepInConnectionSetupAsServer(client);
            }
        }

        private void OnShowMessagBox(object sender, ShowMessageBoxEventArgs showMessageBoxEventArgs)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ShowMessageBoxDelegate(ShowMessageBox), showMessageBoxEventArgs.message, showMessageBoxEventArgs.caption, showMessageBoxEventArgs.messageBoxButtons, showMessageBoxEventArgs.messageBoxIcon);
            }
            else
            {
                ShowMessageBox(showMessageBoxEventArgs.message, showMessageBoxEventArgs.caption, showMessageBoxEventArgs.messageBoxButtons, showMessageBoxEventArgs.messageBoxIcon);
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
        #endregion

        #region Form Event Handlers
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
                        if (network.connectedClients.Count > 0)
                        {
                            message = message.Trim();
                            for (int i = 0; i < network.connectedClients.Count; i++)
                            {
                                network.BeginWrite(network.connectedClients[i], network.ComposeMessage(network.connectedClients[i], 0, Message.MessageTypes.ChatMessage, $"{FrmHolder.username} {message}", null));
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
        #endregion
    }
}