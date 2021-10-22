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
    public class ClientPlayerDataProcessor : BaseProcessor
    {
        public new Queue<PlayerDataMessage> IncomingMessages { get; set; } = new Queue<PlayerDataMessage>();

        public new Queue<PlayerDataMessage> OutgoingMessages { get; set; } = new Queue<PlayerDataMessage>();


        public ClientPlayerDataProcessor()
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
            if (message is PlayerDataMessage dataMessage)
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
                PlayerDataMessage playerDataMessage = IncomingMessages.Dequeue();

                if (playerDataMessage.OwnData)
                {
                    GetPlayerManager.LocalPlayer.ID = playerDataMessage.ID;
                    playerDataMessage.Name = GetPlayerManager.LocalPlayer.Name;
                    SendMessage(playerDataMessage, GetPlayerManager.LocalPlayer);
                    Log.Info($"Got ID from server: {playerDataMessage.ID}");
                }
                else if(playerDataMessage.ID != GetPlayerManager.LocalPlayer.ID)
                {
                    if (GetPlayerManager.Players.ContainsKey(playerDataMessage.ID))
                    {
                        GetPlayerManager.Players[playerDataMessage.ID].Name = playerDataMessage.Name;                        
                    }
                    else
                    {
                        PlayerData newPlayer = new PlayerData();
                        newPlayer.ID = playerDataMessage.ID;
                        newPlayer.Name = playerDataMessage.Name;

                        GetPlayerManager.Players.Add(playerDataMessage.ID, newPlayer);
                        Log.Info($"Successfully got new player!");
                    }
                }

            }

            while (OutgoingMessages.Any())
            {
                PlayerDataMessage PlayerDataMessage = OutgoingMessages.Dequeue();
                PlayerDataMessage.SenderID = GetPlayerManager.LocalPlayer.ID;
                var msg = BaseMessageType.Serialize(PlayerDataMessage);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                Log.Info($"Sending data back to server with own data!");
                GetClient.client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
