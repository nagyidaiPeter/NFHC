using ProtoBuf;

namespace NfhcModel.Network.Messages
{
    [ProtoContract]    
    public class ChatMessage : BaseMessageType
    {
        [ProtoMember(1)]
        public string PlayerName { get; set; }

        [ProtoMember(2)]
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
