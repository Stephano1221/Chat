using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class EventArgsData
    {
    }

    class MessageReceivedEventArgs : EventArgs
    {
        public Client client;
        public Message message;

        public MessageReceivedEventArgs(Client client, Message message)
        {
            this.client = client;
            this.message = message;
        }
    }
}
