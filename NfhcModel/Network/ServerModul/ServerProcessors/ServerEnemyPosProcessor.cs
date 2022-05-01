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
    public class ServerEnemyPosProcessor : BaseProcessor
    {
        public new Queue<EnemyPosition> IncomingMessages { get; set; } = new Queue<EnemyPosition>();

        public new Queue<EnemyPosition> OutgoingMessages { get; set; } = new Queue<EnemyPosition>();

        public override MessageTypes MessageType { get { return MessageTypes.EnemyTransform; } }


        public ServerEnemyPosProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {

            var EnemyPosition = BaseMessageType.Deserialize<EnemyPosition>(message);
            IncomingMessages.Enqueue(EnemyPosition);

            return true;

        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is EnemyPosition msg)
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
                EnemyPosition enemyData = IncomingMessages.Dequeue();

                var msg = BaseMessageType.Serialize(enemyData);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.Unreliable);
            }

            while (OutgoingMessages.Any())
            {
                EnemyPosition enemyData = OutgoingMessages.Dequeue();
                var msg = BaseMessageType.Serialize(enemyData);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.Unreliable);
            }
        }
    }
}
