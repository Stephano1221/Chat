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

        public int messageSendPriority;

        public Message(int messageId, int messageType, string messageText)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageText = messageText;

            SetPriorityLevelFromMessageType();
        }

        public Message(int messageType, string messageText)
        {
            this.messageType = messageType;
            this.messageText = messageText;

            SetPriorityLevelFromMessageType();

        }

        public Message(int messageId, int messageType, string messageText, int messageSendPriority)
        {
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageText = messageText;

            this.messageSendPriority = messageSendPriority;
        }

        public Message(int messageType, string messageText, int messageSendPriority)
        {
            this.messageType = messageType;
            this.messageText = messageText;

            this.messageSendPriority = messageSendPriority;
        }

        private void SetPriorityLevelFromMessageType()
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
            }
        }
    }
}
