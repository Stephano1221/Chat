using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Chat
{
    public class Client
    {
        public int clientId = -1;
        public int nextAssignableMessageId = 0;
        public string username;
        public TcpClient tcpClient;

        public Encryption encryption;

        public bool admin = false;
        public bool serverMuted = false;
        public bool serverDeafened = false;

        public bool heartbeatReceieved = false;
        public int heartbeatFailures = 0;
        public bool connectionSetupComplete = false;
        public bool sendingMessageQueue = false;
        public bool receivingMessageQueue = false;

        public List<Message> messagesSent = new List<Message>();
        public List<Message> messagesToBeSent = new List<Message>();
        public List<Message> messagesReceived = new List<Message>();

        public Client()
        {
            encryption = new Encryption();
        }
    }
}
