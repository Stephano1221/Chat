using System.Text;

namespace Chat
{
    public class Message
    {
        public enum MessageTypes : uint
        {
            None = 0,
            Acknowledgement = 1,
            ChatMessage = 2,
            ClientDisconnect = 3,
            UsernameInUse = 4,
            UserConnected = 5,
            UserDisconnected = 6,
            ClearUserList = 7,
            AddToUserList = 8,
            Kicked = 9,
            OtherUserKicked = 10,
            Heartbeat = 11,
            OwnClientId = 12,
            OtherUserLostConnection = 13,
            MadeAdmin = 14,
            OtherUserMadeAdmin = 15,
            RemovedAdmin = 16,
            OtherUserRemovedAdmin = 17,
            SendMessageQueue = 18,
            ConnectionSetupComplete = 19,
            ClientId = 20,
            RequestVersionNumber = 21,
            ClientVersionNumber = 22,
            RequestUsername = 23,
            ClientUsername = 24,
            RequestClientId = 25,
            ServersMinimumSupportedClientVersionNumber = 26,
            ServersMaximumSupportedClientVersionNumber = 27,
            ServersPreReleaseSupport = 28,
            ServerVersionNumberCompatibility = 29,
            ServerVersionNumber = 30,
            FinishedSendingMessageQueue = 31,
            RankGiven = 32,
            RankTaken = 33,
            OtherUserRankGiven = 34,
            OtherUserRankTaken = 35,
            RequestAllRanks = 36,
            AllRanks = 37
        }

        public uint messageId;
        public MessageTypes messageType;
        public string messageText;
        public byte[] messageBytes;

        public int messageSendPriority;

        public Message(uint messageId, MessageTypes messageType, string messageText)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageText = messageText;

            MessageTextToOrFromBytes();
            SetPriorityLevelFromMessageType();
        }

        public Message(MessageTypes messageType, string messageText)
        {
            this.messageType = messageType;
            this.messageText = messageText;

            MessageTextToOrFromBytes();
            SetPriorityLevelFromMessageType();

        }

        public Message(uint messageId, MessageTypes messageType, string messageText, int messageSendPriority)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageText = messageText;

            MessageTextToOrFromBytes();
            this.messageSendPriority = messageSendPriority;
        }

        public Message(MessageTypes messageType, string messageText, int messageSendPriority)
        {
            this.messageType = messageType;
            this.messageText = messageText;

            MessageTextToOrFromBytes();
            this.messageSendPriority = messageSendPriority;
        }

        public Message(uint messageId, MessageTypes messageType, byte[] messageBytes)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageBytes = messageBytes;

            MessageTextToOrFromBytes();
            SetPriorityLevelFromMessageType();
        }

        public Message(MessageTypes messageType, byte[] messageBytes)
        {
            this.messageType = messageType;
            this.messageBytes = messageBytes;

            MessageTextToOrFromBytes();
            SetPriorityLevelFromMessageType();

        }

        public Message(uint messageId, MessageTypes messageType, byte[] messageBytes, int messageSendPriority)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageBytes = messageBytes;

            MessageTextToOrFromBytes();
            this.messageSendPriority = messageSendPriority;
        }

        public Message(MessageTypes messageType, byte[] messageBytes, int messageSendPriority)
        {
            this.messageType = messageType;
            this.messageBytes = messageBytes;

            MessageTextToOrFromBytes();
            this.messageSendPriority = messageSendPriority;
        }

        public void MessageTextToOrFromBytes()
        {
            if (messageText != null && messageBytes == null)
            {
                messageBytes = Encoding.Unicode.GetBytes(messageText);
            }
            else if (messageBytes != null && CheckIfCanConvertToText())
            {
                messageText = Encoding.Unicode.GetString(messageBytes);
            }
        }

        public void SetPriorityLevelFromMessageType()
        {
            switch (messageType)
            {
                case MessageTypes.None: messageSendPriority = 0; break;
                case MessageTypes.Acknowledgement: messageSendPriority = 0; break;
                case MessageTypes.ChatMessage: messageSendPriority = 1; break;
                case MessageTypes.ClientDisconnect: messageSendPriority = 0; break;
                case MessageTypes.UsernameInUse: messageSendPriority = 0; break;
                case MessageTypes.UserConnected: messageSendPriority = 1; break;
                case MessageTypes.UserDisconnected: messageSendPriority = 1; break;
                case MessageTypes.ClearUserList: messageSendPriority = 1; break;
                case MessageTypes.AddToUserList: messageSendPriority = 1; break;
                case MessageTypes.Kicked: messageSendPriority = 0; break;
                case MessageTypes.OtherUserKicked: messageSendPriority = 1; break;
                case MessageTypes.Heartbeat: messageSendPriority = 0; break;
                case MessageTypes.OwnClientId: messageSendPriority = 0; break;
                case MessageTypes.OtherUserLostConnection: messageSendPriority = 1; break;
                case MessageTypes.MadeAdmin: messageSendPriority = 1; break;
                case MessageTypes.OtherUserMadeAdmin: messageSendPriority = 1; break;
                case MessageTypes.RemovedAdmin: messageSendPriority = 1; break;
                case MessageTypes.OtherUserRemovedAdmin: messageSendPriority = 1; break;
                case MessageTypes.SendMessageQueue: messageSendPriority = 0; break;
                case MessageTypes.ConnectionSetupComplete: messageSendPriority = 0; break;
                case MessageTypes.ClientId: messageSendPriority = 0; break;
                case MessageTypes.RequestVersionNumber: messageSendPriority = 0; break;
                case MessageTypes.ClientVersionNumber: messageSendPriority = 0; break;
                case MessageTypes.RequestUsername: messageSendPriority = 0; break;
                case MessageTypes.ClientUsername: messageSendPriority = 0; break;
                case MessageTypes.RequestClientId: messageSendPriority = 0; break;
                case MessageTypes.ServersMinimumSupportedClientVersionNumber: messageSendPriority = 0; break;
                case MessageTypes.ServersMaximumSupportedClientVersionNumber: messageSendPriority = 0; break;
                case MessageTypes.ServersPreReleaseSupport: messageSendPriority = 0; break;
                case MessageTypes.ServerVersionNumberCompatibility: messageSendPriority = 0; break;
                case MessageTypes.ServerVersionNumber: messageSendPriority = 0; break;
                case MessageTypes.FinishedSendingMessageQueue: messageSendPriority = 0; break;
                case MessageTypes.RankGiven: messageSendPriority = 1; break;
                case MessageTypes.RankTaken: messageSendPriority = 1; break;
                case MessageTypes.OtherUserRankGiven: messageSendPriority = 1; break;
                case MessageTypes.OtherUserRankTaken: messageSendPriority = 1; break;
                case MessageTypes.RequestAllRanks: messageSendPriority = 1; break;
                case MessageTypes.AllRanks: messageSendPriority = 1; break;
                default: messageSendPriority = 0; break;
            }
        }

        public bool CheckIfCanConvertToText()
        {
            switch (messageType)
            {
                case MessageTypes.None: return true;
                case MessageTypes.Acknowledgement: return true;
                case MessageTypes.ChatMessage: return true;
                case MessageTypes.ClientDisconnect: return true;
                case MessageTypes.UsernameInUse: return true;
                case MessageTypes.UserConnected: return true;
                case MessageTypes.UserDisconnected: return true;
                case MessageTypes.ClearUserList: return true;
                case MessageTypes.AddToUserList: return true;
                case MessageTypes.Kicked: return true;
                case MessageTypes.OtherUserKicked: return true;
                case MessageTypes.Heartbeat: return true;
                case MessageTypes.OwnClientId: return true;
                case MessageTypes.OtherUserLostConnection: return true;
                case MessageTypes.MadeAdmin: return true;
                case MessageTypes.OtherUserMadeAdmin: return true;
                case MessageTypes.RemovedAdmin: return true;
                case MessageTypes.OtherUserRemovedAdmin: return true;
                case MessageTypes.SendMessageQueue: return true;
                case MessageTypes.ConnectionSetupComplete: return true;
                case MessageTypes.ClientId: return true;
                case MessageTypes.RequestVersionNumber: return true;
                case MessageTypes.ClientVersionNumber: return true;
                case MessageTypes.RequestUsername: return true;
                case MessageTypes.ClientUsername: return true;
                case MessageTypes.RequestClientId: return true;
                case MessageTypes.ServersMinimumSupportedClientVersionNumber: return true;
                case MessageTypes.ServersMaximumSupportedClientVersionNumber: return true;
                case MessageTypes.ServersPreReleaseSupport: return true;
                case MessageTypes.ServerVersionNumberCompatibility: return true;
                case MessageTypes.ServerVersionNumber: return true;
                case MessageTypes.FinishedSendingMessageQueue: return true;
                case MessageTypes.RankGiven: return true;
                case MessageTypes.RankTaken: return true;
                case MessageTypes.OtherUserRankGiven: return true;
                case MessageTypes.OtherUserRankTaken: return true;
                case MessageTypes.RequestAllRanks: return true;
                case MessageTypes.AllRanks: return true;
                default: return false;
            }
        }
    }
}
