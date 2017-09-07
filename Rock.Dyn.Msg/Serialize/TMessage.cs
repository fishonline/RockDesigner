using System;
using System.Collections.Generic;
using System.Text;

namespace Rock.Dyn.Msg
{
    public struct TMessage
    {
        private string name;
        private TMessageType type;
        private string msgID;
        private short liveLife;
        private string sender;
        private string senderQueueName;
        private string receiver;
        private string receiverQueueName;

        public TMessage(string name, TMessageType type, string msgID, short liveLife, string sender, string receiver, string senderQueueName, string receiverQueueName)
            : this()
        {
            this.name = name;
            this.type = type;
            this.msgID = msgID;
            this.liveLife = liveLife;
            this.sender = sender;
            this.receiver = receiver;
            this.senderQueueName = senderQueueName;
            this.receiverQueueName = receiverQueueName;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public TMessageType Type
        {
            get { return type; }
            set { type = value; }
        }

        public string MsgID
        {
            get { return msgID; }
            set { msgID = value; }
        }

        public short LiveLife
        {
            get { return liveLife; }
            set { liveLife = value; }
        }

        public string Sender
        {
            get { return sender; }
            set { sender = value; }
        }

        public string SenderQueueName
        {
            get { return senderQueueName; }
            set { senderQueueName = value; }
        }

        public string Receiver
        {
            get { return receiver; }
            set { receiver = value; }
        }

        public string ReceiverQueueName
        {
            get { return receiverQueueName; }
            set { receiverQueueName = value; }
        }
    }
}
