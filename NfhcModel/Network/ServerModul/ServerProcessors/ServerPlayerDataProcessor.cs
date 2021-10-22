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
    public class ServerPlayerDataProcessor : BaseProcessor
    {
        public new Queue<PlayerDataMessage> IncomingMessages { get; set; } = new Queue<PlayerDataMessage>();

        public new Queue<PlayerDataMessage> OutgoingMessages { get; set; } = new Queue<PlayerDataMessage>();


        public ServerPlayerDataProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {

            var playerDataMessage = BaseMessageType.Deserialize<PlayerDataMessage>(message);
            IncomingMessages.Enqueue(playerDataMessage);

            return true;

        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is PlayerDataMessage msg)
            {
                Log.Info("Adding playerDataMsg out..");
                OutgoingMessages.Enqueue(msg);
                return true;
            }

            return false;
        }

        public override void Process()
        {
            while (IncomingMessages.Any())
            {
                PlayerDataMessage playerData = IncomingMessages.Dequeue();
                if (GetPlayerManager.Players[playerData.SenderID] is PlayerData player)
                {
                    player.Name = playerData.Name;
                    Log.Info($"Client's name: {playerData.Name}");
                    GetServer.ServerSay($"{playerData.Name} joined...");

                    foreach (var otherPlayer in GetPlayerManager.Players.Values)
                    {
                        var otherPlayerData = new PlayerDataMessage();
                        otherPlayerData.ID = otherPlayer.ID;
                        otherPlayerData.Name = otherPlayer.Name;
                        otherPlayerData.OwnData = false;
                        otherPlayerData.SenderID = -1;
                        otherPlayerData.MsgType = MessageTypes.PlayerDataMessage;

                        var otherMsg = BaseMessageType.Serialize(otherPlayerData);
                        NetDataWriter otherWriter = new NetDataWriter();
                        otherWriter.Put(otherMsg);

                        GetServer.server.SendToAll(otherWriter, DeliveryMethod.ReliableOrdered);
                    }

                }
            }

            while (OutgoingMessages.Any())
            {
                PlayerDataMessage playerData = OutgoingMessages.Dequeue();
                playerData.SenderID = -1;
                var msg = BaseMessageType.Serialize(playerData);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetPlayerManager.Players[playerData.ID].Connection.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
