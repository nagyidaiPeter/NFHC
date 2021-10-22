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
    public class ServerPlayerPosProcessor : BaseProcessor
    {
        public new Queue<PlayerPosition> IncomingMessages { get; set; } = new Queue<PlayerPosition>();

        public new Queue<PlayerPosition> OutgoingMessages { get; set; } = new Queue<PlayerPosition>();


        public ServerPlayerPosProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {

            var PlayerPosition = BaseMessageType.Deserialize<PlayerPosition>(message);
            IncomingMessages.Enqueue(PlayerPosition);

            return true;

        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is PlayerPosition msg)
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
                PlayerPosition playerData = IncomingMessages.Dequeue();

                var msg = BaseMessageType.Serialize(playerData);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.Unreliable);
            }

            while (OutgoingMessages.Any())
            {
                PlayerPosition playerData = OutgoingMessages.Dequeue();
                var msg = BaseMessageType.Serialize(playerData);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.Unreliable);
            }
        }
    }
}
