using System;
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
            //RsaEncryptionTest();
            //AesEncryptionTest();
        }

        private void RsaEncryptionTest()
        {
            encryption.keyContainerName = DateTime.Now.ToString();
            encryption.RsaDeleteKey(encryption.keyContainerName);
            encryption.RsaGenerateKey(encryption.keyContainerName);
            string key1 = encryption.RsaExportXmlKey(encryption.keyContainerName, false);
            encryption.RsaGenerateKey(encryption.keyContainerName);
            string key2 = encryption.RsaExportXmlKey(encryption.keyContainerName, true);

            try
            {
                encryption.RsaImportXmlKey("Import", key2);
                string key3 = encryption.RsaExportXmlKey("Import", false);
                byte[] bytes = Encoding.Unicode.GetBytes("Hello World!");
                byte[] encrypted = encryption.RsaEncryptDecrypt(bytes, "Import", encryption.rsaParametersPublicKey, true, true);
                byte[] decrypted = encryption.RsaEncryptDecrypt(encrypted, encryption.keyContainerName, encryption.rsaParametersPrivateAndPublicKey, true, false);
                string xml = Encoding.Unicode.GetString(decrypted);
            }
            catch
            {
                throw;
            }
            finally
            {
                encryption.RsaDeleteKey(encryption.keyContainerName);
                encryption.RsaDeleteKey("Import");
            }
        }

        private void AesEncryptionTest()
        {
            encryption.AesGenerateKey();
            (byte[] aesDecryptedKey, byte[] aesDecryptedIv) keyAndIv = encryption.AesExportKeyAndIv(encryption.aesEncryptedKey, encryption.aesEncryptedIv);

            byte[] bytes = Encoding.Unicode.GetBytes("Hello World!");
            byte[] encrypted = encryption.AesEncryptDecrypt(bytes, encryption.AesExportKeyAndIv(encryption.aesEncryptedKey, encryption.aesEncryptedIv), true);

            encryption.AesImportKeyOrIv(keyAndIv.aesDecryptedKey, true);
            encryption.AesImportKeyOrIv(keyAndIv.aesDecryptedIv, false);

            byte[] decrypted = encryption.AesEncryptDecrypt(encrypted, encryption.AesExportKeyAndIv(encryption.aesEncryptedKey, encryption.aesEncryptedIv), false);
            string result = Encoding.Unicode.GetString(decrypted);
        }
    }
}
