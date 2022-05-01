using LiteNetLib;
using LiteNetLib.Utils;
using NfhcModel.DataStructures;
using NfhcModel.Logger;
using NfhcModel.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.ServerModul.ServerProcessors
{
    public class ServerGameEntityProcessor : BaseProcessor
    {
        public new Queue<GameEntityMessage> IncomingMessages { get; set; } = new Queue<GameEntityMessage>();

        public new Queue<GameEntityMessage> OutgoingMessages { get; set; } = new Queue<GameEntityMessage>();

        public override MessageTypes MessageType { get { return MessageTypes.GameEntityMessage; } }


        public ServerGameEntityProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {

            var gameEntityMessage = BaseMessageType.Deserialize<GameEntityMessage>(message);
            IncomingMessages.Enqueue(gameEntityMessage);

            return true;

        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is GameEntityMessage msg)
            {
                OutgoingMessages.Enqueue(msg);
                return true;
            }

            return false;
        }

        public override void Process()
        {
            while (IncomingMessages.Any())
            {
                GameEntityMessage gameEntityMsg = IncomingMessages.Dequeue();

                var msg = BaseMessageType.Serialize(gameEntityMsg);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.Unreliable);
            }

            while (OutgoingMessages.Any())
            {
                GameEntityMessage gameEntityMsg = OutgoingMessages.Dequeue();
                var msg = BaseMessageType.Serialize(gameEntityMsg);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.Unreliable);
            }
        }
    }
}
