﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        public Encryption encryption;
        public int clientId = -1;
        public int nextAssignableMessageId = 0;
        public bool heartbeatReceieved = false;
        public int heartbeatFailures = 0;
        public bool encryptionEstablished = false;
        public bool connectionSetupComplete = false;
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
            encryption = new Encryption();
        }
    }
}
