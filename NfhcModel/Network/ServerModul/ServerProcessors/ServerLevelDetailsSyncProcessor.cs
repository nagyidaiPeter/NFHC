using LiteNetLib;
using LiteNetLib.Utils;
using NfhcModel.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.ServerModul.ServerProcessors
{
    public class ServerLevelDetailsSyncProcessor : BaseProcessor
    {
        public new Queue<LevelDetailsSync> IncomingMessages { get; set; } = new Queue<LevelDetailsSync>();

        public new Queue<LevelDetailsSync> OutgoingMessages { get; set; } = new Queue<LevelDetailsSync>();

        public ServerLevelDetailsSyncProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {
            var LevelDetailsSync = BaseMessageType.Deserialize<LevelDetailsSync>(message);
            IncomingMessages.Enqueue(LevelDetailsSync);
            return true;
        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is LevelDetailsSync dataMessage)
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
                LevelDetailsSync LevelDetailsSync = IncomingMessages.Dequeue();

            }

            while (OutgoingMessages.Any())
            {
                LevelDetailsSync LevelDetailsSync = OutgoingMessages.Dequeue();

                LevelDetailsSync.SenderID = -1;
                var msg = BaseMessageType.Serialize(LevelDetailsSync);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}

