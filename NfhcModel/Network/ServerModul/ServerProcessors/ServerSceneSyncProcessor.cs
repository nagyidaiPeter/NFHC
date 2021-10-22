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
    public class ServerSceneSyncProcessor : BaseProcessor
    {
        public new Queue<SceneLoadingSync> IncomingMessages { get; set; } = new Queue<SceneLoadingSync>();

        public new Queue<SceneLoadingSync> OutgoingMessages { get; set; } = new Queue<SceneLoadingSync>();

        public ServerSceneSyncProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {
            var SceneLoadingSync = BaseMessageType.Deserialize<SceneLoadingSync>(message);
            IncomingMessages.Enqueue(SceneLoadingSync);
            return true;
        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is SceneLoadingSync dataMessage)
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
                SceneLoadingSync SceneLoadingSync = IncomingMessages.Dequeue();

            }

            while (OutgoingMessages.Any())
            {
                SceneLoadingSync SceneLoadingSync = OutgoingMessages.Dequeue();

                SceneLoadingSync.SenderID = -1;
                var msg = BaseMessageType.Serialize(SceneLoadingSync);

                GetServer.ServerSay($"Changing level to {SceneLoadingSync.SceneName}");

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}

