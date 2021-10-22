using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.Messages
{
    [ProtoContract]    
    public class ChatMessage : BaseMessageType
    {
        [ProtoMember(1)]
        public string MessageText { get; set; }

        public ChatMessage()
        {
            MsgType = MessageTypes.Chat;
        }

        public ChatMessage(string msg)
        {
            this.MessageText = msg;
            MsgType = MessageTypes.Chat;
        }
    }

    public static class ChatMessageHelper
    {
        public static ChatMessage ToChatMessage(this string value)
        {
            return new ChatMessage()
            {
                MessageText = value
            };
        }
    }
}
