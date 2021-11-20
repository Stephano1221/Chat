using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Chat
{
    public class Client
    {
        #region User properties
        public string username;
        public bool admin = false;
        public bool serverMuted = false;
        public bool serverDeafened = false;
        #endregion

        #region Connection
        public TcpClient tcpClient;
        public SslStream sslStream;
        public MemoryStream streamUnprocessedBytes = new MemoryStream();
        public int clientId = -1;
        public int nextAssignableMessageId = 0;
        public bool heartbeatReceieved = false;
        public int heartbeatFailures = 0;
        public bool connectionSetupComplete = false;
        public bool disconnectHandled = false;
        public bool sendingMessageQueue = false;
        public bool receivingMessageQueue = false;
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

        public int headerLength = 0;
        public byte[] byteBuffer = null;
        public byte[] idBuffer = new byte[4];
        public byte[] typeBuffer = new byte[4];
        public byte[] lengthBuffer = new byte[4];
        public int? messageId = null;
        public int? messageType = null;
        public int? messageLength = null;
        public bool readHeader = false;

        public byte[] messageBytes = null;
        public int bytesRead = 0;

        public ClientStateObject(Client client)
        {
            this.client = client;
            headerLength = idBuffer.Count() + typeBuffer.Count() + lengthBuffer.Count();
            int byteBufferSize = client.tcpClient.ReceiveBufferSize;
            byteBuffer = new byte[byteBufferSize];
        }
    }
}
