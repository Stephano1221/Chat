using System.Net.Security;
using System.Net.Sockets;

namespace Chat
{
    public class Client
    {
        #region User properties
        public string username;
        public bool admin = false;
        public List<Ranks.Rank> ranks = new List<Ranks.Rank>();
        public bool serverMuted = false;
        public bool serverDeafened = false;
        public string applicationVersionNumber;
        public string serverApplicationVersionNumber;
        public string serverMinimumSupportedClientApplicationVersionNumber;
        public string serverMaximumSupportedClientApplicationVersionNumber;
        public bool serverSupportsClientPreReleaseAppplicationVersionNumber;
        public char clientToServerVersionNumberCompatibility;
        #endregion

        #region Connection
        public TcpClient tcpClient;
        public SslStream sslStream;
        public MemoryStream streamUnprocessedBytes = new MemoryStream();
        public uint clientId = 0;
        public uint nextAssignableMessageId = 1;
        public bool heartbeatReceieved = false;
        public int heartbeatFailures = 0;
        public bool connectionSetupComplete = false;
        public bool disconnectHandled = false;
        public bool sendingMessageQueue = false;
        public bool receivingMessageQueue = false;

        public bool receivedApplicationVersionNumber = false;
        public bool requestedApplicationVersionNumber = false;
        public bool receivedClientId = false;
        public bool requestedClientId = false;
        public bool receivedUsername = false;
        public bool requestedUsername = false;
        public bool sessionFirstConnection = true;
        #endregion

        #region Message Queues
        public List<Message> messagesSent = new List<Message>();
        public List<Message> messagesToBeSent = new List<Message>();
        public List<Message> messagesReceived = new List<Message>();
        #endregion

        public Client()
        {

        }
    }

    public class ClientStateObject
    {
        public Client client;
        public Message message;

        public uint headerLength = 0;
        public byte[] byteBuffer = null;
        public byte[] idBuffer = new byte[4];
        public byte[] typeBuffer = new byte[4];
        public byte[] lengthBuffer = new byte[4];
        public uint? messageId = null;
        public Message.MessageTypes messageType = Message.MessageTypes.None;
        public uint? messageLength = null;
        public bool readHeader = false;

        public byte[] messageBytes = null;
        public uint bytesRead = 0;

        public ClientStateObject(Client client)
        {
            this.client = client;
            headerLength = (uint)idBuffer.Count() + (uint)typeBuffer.Count() + (uint)lengthBuffer.Count();
            int byteBufferSize = client.tcpClient.ReceiveBufferSize;
            byteBuffer = new byte[byteBufferSize];
        }
    }
}
