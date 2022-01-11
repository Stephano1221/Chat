//#define messageSentUpdates
using System.Net.Security;
using System.Security.Authentication;
using System.Net.Sockets;

namespace Chat
{
    public class Network
    {
        public event EventHandler<FirstConnectionAttemptResultEventArgs> FirstConnectionAttemptResultEvent;
        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
        public event EventHandler<ShowMessageBoxEventArgs> ShowMessageBoxEvent;
        public event EventHandler<Client> AcceptTcpClientEvent;

        #region Reset Events
        private AutoResetEvent writeAutoResetEvent = new AutoResetEvent(true);
        private AutoResetEvent acceptTcpClientResetEvent = new AutoResetEvent(true);
        private AutoResetEvent connectAutoResetEvent = new AutoResetEvent(true);
        #endregion

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
            if (FrmHolder.processing.connectedClients.Contains(client) == false)
            {
                FrmHolder.processing.connectedClients.Add(client);
            }

            client.tcpClient = new TcpClient();
            connectAutoResetEvent.WaitOne();
            client.tcpClient.BeginConnect(FrmHolder.processing.publicIp, FrmHolder.processing.port, ConnectCallback, client);
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            Client client = asyncResult.AsyncState as Client;
            try
            {
                client.tcpClient.EndConnect(asyncResult);
            }
            catch (Exception ex) when (ex is SocketException)
            {
                InvokeFirstConnectionAttemptResultEvent(this, new FirstConnectionAttemptResultEventArgs(false, "Unable to reach the server.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information));
                return;
            }
            connectAutoResetEvent.Set();
            client.sslStream = new SslStream(client.tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(FrmHolder.processing.ValidateServerCertificate), null);
            try
            {
                client.sslStream.AuthenticateAsClient("chatappserver.ddns.net");
                if (client.sslStream.IsEncrypted == false || client.sslStream.IsSigned == false || client.sslStream.IsAuthenticated == false)
                {
                    client.sslStream.Close();
                    FrmHolder.processing.connectedClients.Remove(client);
                    InvokeFirstConnectionAttemptResultEvent(this, new FirstConnectionAttemptResultEventArgs(false, "Unable to establish a secure connection to the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                    return;
                }
            }
            catch (Exception ex) when (ex is AuthenticationException || ex is IOException)
            {
                client.sslStream.Close();
                FrmHolder.processing.connectedClients.Remove(client);
                InvokeFirstConnectionAttemptResultEvent(this, new FirstConnectionAttemptResultEventArgs(false, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                return;
            }
            BeginRead(client);
            InvokeFirstConnectionAttemptResultEvent(this, new FirstConnectionAttemptResultEventArgs(true));
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
            FrmHolder.processing.connectedClients.Add(client);
            acceptTcpClientResetEvent.Set();

            client.sslStream = new SslStream(client.tcpClient.GetStream(), false);
            try
            {
                client.sslStream.AuthenticateAsServer(FrmHolder.processing.x509Certificate, false, true);
                if (client.sslStream.IsEncrypted == false || client.sslStream.IsSigned == false || client.sslStream.IsAuthenticated == false)
                {
                    client.sslStream.Close();
                    FrmHolder.processing.connectedClients.Remove(client);
                    //MessageBoxEvent.Invoke(this, new ShowMessageBoxEventArgs("Unable to establish a secure connection to client.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                }
            }
            catch (Exception ex) when (ex is AuthenticationException || ex is IOException)
            {
                client.sslStream.Close();
                FrmHolder.processing.connectedClients.Remove(client);
                //MessageBoxEvent.Invoke(this, new ShowMessageBoxEventArgs(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning));
            }
            BeginRead(client);
            AcceptTcpClientEvent.Invoke(this, client);
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

        public void BeginWrite(Client client, Message message)
        {
            if (FrmHolder.processing.CheckAddMessageToQueue(client, message, false))
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
                FrmHolder.processing.CheckAddMessageToQueue(client, message, true);
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
                FrmHolder.processing.AddMessageToMessageListBySendPriority(clientStateObject.client.messagesToBeSent, clientStateObject.message, true);
            }
#if DEBUG && messageSentUpdates
                            if (clientStateObject.message.messageType != Message.MessageTypes.Heartbeat)
                            {
                                string text;
                                if (clientStateObject.message.messageText != null)
                                {
                                    text = $"[SENT] Type: {clientStateObject.message.messageType}. ID: {clientStateObject.message.messageId}. Text: {clientStateObject.message.messageText}";
                                }
                                else
                                {
                                    text = $"[SENT] Type: {clientStateObject.message.messageType}. ID: {clientStateObject.message.messageId}";
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
                while (clientStateObject.client.streamUnprocessedBytes.Length >= clientStateObject.headerLength)
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
                        TruncateBytesPrecedingPositionInMemoryStream(clientStateObject.client);
                        clientStateObject.readHeader = false;
                        ConvertLittleEndianToBigEndian(clientStateObject.messageBytes);
                        MessageReceivedEvent.Invoke(this, new MessageReceivedEventArgs(clientStateObject.client, clientStateObject.messageId.GetValueOrDefault(), clientStateObject.messageType, null, clientStateObject.messageBytes));
                    }
                    else
                    {
                        clientStateObject.client.streamUnprocessedBytes.Position = writePosition;
                        break;
                    }
                }
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

        private void InvokeFirstConnectionAttemptResultEvent(object sender, FirstConnectionAttemptResultEventArgs e)
        {
            if (FirstConnectionAttemptResultEvent != null)
            {
                FirstConnectionAttemptResultEvent.Invoke(sender, e);
            }
        }
    }
}
