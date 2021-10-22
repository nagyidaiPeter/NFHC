using LiteNetLib;
using LiteNetLib.Utils;
using NfhcModel.Core;
using NfhcModel.DataStructures;
using NfhcModel.Logger;
using NfhcModel.Network.ClientModul;
using NfhcModel.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.ServerModul.ServerProcessors
{
    public class ServerChatProcessor : BaseProcessor
    {
        public new Queue<ChatMessage> IncomingMessages { get; set; } = new Queue<ChatMessage>();

        public new Queue<ChatMessage> OutgoingMessages { get; set; } = new Queue<ChatMessage>();


        public ServerChatProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {
            var chatMessage = BaseMessageType.Deserialize<ChatMessage>(message);
            chatMessage.MessageText = player.Name + ": " + chatMessage.MessageText;
            IncomingMessages.Enqueue(chatMessage);
            return true;
        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is ChatMessage dataMessage)
            {
                OutgoingMessages.Enqueue(dataMessage);
                return true;
            }
            return false;
        }

        public override void Process()
        {
            while (IncomingMessages.Any())
            {
                ChatMessage chatMessage = IncomingMessages.Dequeue();

                var msg = BaseMessageType.Serialize(chatMessage);

                Log.Info($"Servre got chat: {chatMessage.MessageText}");

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            }

            while (OutgoingMessages.Any())
            {
                ChatMessage chatMessage = OutgoingMessages.Dequeue();

            }
        }
    }
}
