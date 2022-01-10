namespace Chat
{
    class EventArgsData
    {
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public Client client;
        public Message message;

        public MessageReceivedEventArgs(Client client, Message message)
        {
            this.client = client;
            this.message = message;
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
