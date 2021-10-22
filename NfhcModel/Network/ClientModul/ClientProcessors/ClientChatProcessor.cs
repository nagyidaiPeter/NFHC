using LiteNetLib;
using LiteNetLib.Utils;
using NfhcModel.Core;
using NfhcModel.DataStructures;
using NfhcModel.Logger;
using NfhcModel.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.ClientModul.ClientProcessors
{
    public class ClientChatProcessor : BaseProcessor
    {
        public new Queue<ChatMessage> IncomingMessages { get; set; } = new Queue<ChatMessage>();

        public new Queue<ChatMessage> OutgoingMessages { get; set; } = new Queue<ChatMessage>();

        public ClientChatProcessor()
        {
            
        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {
            ChatMessage chatMessage;
            try
            {
                chatMessage = BaseMessageType.Deserialize<ChatMessage>(message);
            }
            catch (InvalidCastException ex)
            {
                return false;
            }

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
                Log.Info($"Chat: {chatMessage.MessageText}");
                GetClient.chatHandler.AddMessage(chatMessage.MessageText, chatMessage.SenderID);
            }

            while (OutgoingMessages.Any())
            {
                ChatMessage chatMessage = OutgoingMessages.Dequeue();

                chatMessage.SenderID = GetPlayerManager.LocalPlayer.ID;
                var msg = BaseMessageType.Serialize(chatMessage);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetClient.client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
