using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    public class Message
    {
        public int messageId;
        public int messageType;
        public string messageText;
        public byte[] messageBytes;

        public int messageSendPriority;

        public Message(int messageId, int messageType, string messageText)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageText = messageText;

            MessageTextToOrFromBytes();
            SetPriorityLevelFromMessageType();
        }

        public Message(int messageType, string messageText)
        {
            this.messageType = messageType;
            this.messageText = messageText;

            MessageTextToOrFromBytes();
            SetPriorityLevelFromMessageType();

        }

        public Message(int messageId, int messageType, string messageText, int messageSendPriority)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageText = messageText;

            MessageTextToOrFromBytes();
            this.messageSendPriority = messageSendPriority;
        }

        public Message(int messageType, string messageText, int messageSendPriority)
        {
            this.messageType = messageType;
            this.messageText = messageText;

            MessageTextToOrFromBytes();
            this.messageSendPriority = messageSendPriority;
        }

        public Message(int messageId, int messageType, byte[] messageBytes)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageBytes = messageBytes;

            MessageTextToOrFromBytes();
            SetPriorityLevelFromMessageType();
        }

        public Message(int messageType, byte[] messageBytes)
        {
            this.messageType = messageType;
            this.messageBytes = messageBytes;

            MessageTextToOrFromBytes();
            SetPriorityLevelFromMessageType();

        }

        public Message(int messageId, int messageType, byte[] messageBytes, int messageSendPriority)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageBytes = messageBytes;

            MessageTextToOrFromBytes();
            this.messageSendPriority = messageSendPriority;
        }

        public Message(int messageType, byte[] messageBytes, int messageSendPriority)
        {
            this.messageType = messageType;
            this.messageBytes = messageBytes;

            MessageTextToOrFromBytes();
            this.messageSendPriority = messageSendPriority;
        }

        public void MessageTextToOrFromBytes()
        {
                if (messageText != null)
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
            switch(messageType)
            {
                case 0: messageSendPriority = 0; break;
                case 1: messageSendPriority = 0; break;
                case 2: messageSendPriority = 1; break;
                case 3: messageSendPriority = 0; break;
                case 4: messageSendPriority = 0; break;
                case 5: messageSendPriority = 1; break;
                case 6: messageSendPriority = 1; break;
                case 7: messageSendPriority = 1; break;
                case 8: messageSendPriority = 1; break;
                case 9: messageSendPriority = 0; break;
                case 10: messageSendPriority = 1; break;
                case 11: messageSendPriority = 0; break;
                case 12: messageSendPriority = 0; break;
                case 13: messageSendPriority = 1; break;
                case 14: messageSendPriority = 1; break;
                case 15: messageSendPriority = 1; break;
                case 16: messageSendPriority = 1; break;
                case 17: messageSendPriority = 1; break;
                case 18: messageSendPriority = 0; break;
                case 19: messageSendPriority = 0; break;
                default: messageSendPriority = 0; break;
            }
        }

        public bool CheckIfCanConvertToText()
        {
            switch (messageType)
            {
                case 0: return true;
                case 1: return true;
                case 2: return true;
                case 3: return true;
                case 4: return true;
                case 5: return true;
                case 6: return true;
                case 7: return true;
                case 8: return true;
                case 9: return true;
                case 10: return true;
                case 11: return true;
                case 12: return true;
                case 13: return true;
                case 14: return true;
                case 15: return true;
                case 16: return true;
                case 17: return true;
                case 18: return true;
                case 19: return true;
                default: return false;
            }
        }
    }
}
