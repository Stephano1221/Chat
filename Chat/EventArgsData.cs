namespace Chat
{
    class EventArgsData
    {
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public Client client;
        public uint messageId;
        public Message.MessageTypes messageType;
        public string messageText;
        public byte[] messageBytes;

        public MessageReceivedEventArgs(Client client, uint messageId, Message.MessageTypes messageType, string messageText, byte[] messageBytes)
        {
            this.client = client;
            this.messageId = messageId;
            this.messageType = messageType;
            this.messageText = messageText;
            this.messageBytes = messageBytes;
        }
    }

    public class ShowMessageBoxEventArgs : EventArgs
    {
        public string message;
        public string caption;
        public MessageBoxButtons messageBoxButtons;
        public MessageBoxIcon messageBoxIcon;
        public DialogResult dialogResult;

        public ShowMessageBoxEventArgs(string message, string caption, MessageBoxButtons messageBoxButtons, MessageBoxIcon messageBoxIcon)
        {
            this.message = message;
            this.caption = caption;
            this.messageBoxButtons = messageBoxButtons;
            this.messageBoxIcon = messageBoxIcon;
        }
    }
}
