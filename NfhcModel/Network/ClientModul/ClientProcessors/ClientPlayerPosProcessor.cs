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
    public class ClientPlayerPosProcessor : BaseProcessor
    {
        public new Queue<PlayerPosition> IncomingMessages { get; set; } = new Queue<PlayerPosition>();

        public new Queue<PlayerPosition> OutgoingMessages { get; set; } = new Queue<PlayerPosition>();

        public ClientPlayerPosProcessor()
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
            if (message is PlayerPosition dataMessage)
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
                PlayerPosition playerPosition = IncomingMessages.Dequeue();

                if (GetPlayerManager.Players.ContainsKey(playerPosition.SenderID) && playerPosition.SenderID != GetPlayerManager.LocalPlayer.ID)
                {
                    if (GetPlayerManager.Players[playerPosition.SenderID].Woody == null)
                    {
                        ActorBrain originalWoody = GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsWoody);

                        GameObject woodyInstance = new GameObject("woody");

                        woodyInstance.AddComponent<CapsuleCollider2D>();
                        var actor = woodyInstance.AddComponent<Actor>();
                        ActorMovementBase.AddRigidBody(actor);

                        woodyInstance.AddComponent<ActorCollisionHandler>();


                        woodyInstance.AddComponent(originalWoody.GetComponent<EntityActionHandler>());


                        actor.AnimSet = originalWoody.GetComponent<Actor>().AnimSet;
                        var actorMovBase = woodyInstance.AddComponent<WoodyMovement>();

                        foreach (Transform child in originalWoody.transform)
                        {
                            GameObject.Instantiate(child.gameObject, woodyInstance.transform);
                        }

                        woodyInstance.AddComponent<DynamicSpriteSorter>();

                        LevelRoom room = null;

                        if (LogicController.HasInstance)
                        {
                            room = LogicController.Instance.GetRoomByName(playerPosition.Room);
                        }

                        if (woodyInstance.gameObject.GetComponent<GameEntity>() is GameEntity gameEntity && room != null)
                        {
                            gameEntity.SetCurrentRoom(room);
                            woodyInstance.gameObject.transform.localPosition = new Vector3(playerPosition.Position.X, playerPosition.Position.Y, 0);
                            GetPlayerManager.Players[playerPosition.SenderID].Woody = woodyInstance.gameObject;
                        }
                        else
                        {
                            //Log.Info($"Failed to get Room: {room == null} or Entity: {woodyInstance.GetComponent<GameEntity>() == null}");
                        }
                    }
                    else
                    {
                        var theWoody = GetPlayerManager.Players[playerPosition.SenderID].Woody;
                        LevelRoom room = null;
                        if (LogicController.HasInstance)
                        {
                            room = LogicController.Instance.GetRoomByName(playerPosition.Room);
                        }

                        var woodyActor = theWoody.GetComponent<Actor>();

                        if (woodyActor?.CurrentRoom?.RoomName != playerPosition.Room
                            && room != null)
                        {
                            woodyActor.SetCurrentRoom(room);
                        }

                        var woodyMovement = theWoody.GetComponent<WoodyMovement>();
                        var spriteAnimPlayer = theWoody.GetComponentInChildren<SpriteAnimPlayer>();
                        
                        woodyActor.PlayAnimFromTime(woodyActor.GetAnimByName(playerPosition.Animation), playerPosition.AnimTime);

                        theWoody.transform.localPosition =
                            new Vector3(playerPosition.Position.X, playerPosition.Position.Y, 0);
                    }
                }
            }

            while (OutgoingMessages.Any())
            {
                PlayerPosition PlayerPosition = OutgoingMessages.Dequeue();
                PlayerPosition.SenderID = GetPlayerManager.LocalPlayer.ID;
                var msg = BaseMessageType.Serialize(PlayerPosition);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetClient.client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
