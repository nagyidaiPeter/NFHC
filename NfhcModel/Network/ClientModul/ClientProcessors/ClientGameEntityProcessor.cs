using LiteNetLib;
using LiteNetLib.Utils;
using NFH.DevTools;
using NFH.Game;
using NFH.Game.Logic;
using NfhcModel.Logger;
using NfhcModel.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NfhcModel.Network.ClientModul.ClientProcessors
{
    public class ClientGameEntityProcessor : BaseProcessor
    {
        public new Queue<GameEntityMessage> IncomingMessages { get; set; } = new Queue<GameEntityMessage>();

        public new Queue<GameEntityMessage> OutgoingMessages { get; set; } = new Queue<GameEntityMessage>();

        public ClientGameEntityProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {
            var GameEntityMessage = BaseMessageType.Deserialize<GameEntityMessage>(message);
            IncomingMessages.Enqueue(GameEntityMessage);

            return true;
        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is GameEntityMessage dataMessage)
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
                GameEntityMessage gameEntityMessage = IncomingMessages.Dequeue();
                if (!GetClient.IsServer)
                {
                    if (LogicController.HasInstance && LogicController.Instance.GetEntityByName(gameEntityMessage.WorldObjectName) is GameEntity gameEntity)
                    {
                        if (gameEntity.GetAnimByName(gameEntityMessage.Animation) is EntityAnimation animation)
                        {
                            gameEntity.PlayAnimFromTime(animation, gameEntityMessage.AnimTime);
                            gameEntity.IsHidden = gameEntityMessage.IsHidden;
                            gameEntity.IsLocked = gameEntityMessage.IsLocked;
                            gameEntity.gameObject.SetActive(gameEntityMessage.isActiveAndEnabled);
                            gameEntity.enabled = gameEntityMessage.isActiveAndEnabled;
                            gameEntity.gameObject.SetActive(gameEntityMessage.isActiveAndEnabled);
                        }
                    }
                }
            }

            while (OutgoingMessages.Any())
            {
                GameEntityMessage GameEntityMessage = OutgoingMessages.Dequeue();
                GameEntityMessage.SenderID = GetPlayerManager.LocalPlayer.ID;
                var msg = BaseMessageType.Serialize(GameEntityMessage);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetClient.client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
