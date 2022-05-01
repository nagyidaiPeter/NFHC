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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NfhcModel.Network.ClientModul.ClientProcessors
{
    public class ClientPlayerPosProcessor : BaseProcessor
    {
        public new Queue<PlayerPosition> IncomingMessages { get; set; } = new Queue<PlayerPosition>();

        public new Queue<PlayerPosition> OutgoingMessages { get; set; } = new Queue<PlayerPosition>();

        public override MessageTypes MessageType { get { return MessageTypes.PlayerTransform; } }

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

                        var woodyInstance = Transform.Instantiate(originalWoody.gameObject, originalWoody.transform.parent);//new GameObject("woody");
                        woodyInstance.transform.parent = originalWoody.transform.parent;
                        woodyInstance.name = "Woody-" + GetPlayerManager.Players.Count;

                        var oldActionHandler = originalWoody.GetComponent<EntityActionHandler>();

                        var actor = woodyInstance.GetComponent<Actor>();
                        var actorBrain = woodyInstance.GetComponent<ActorBrain>();
                        var actionHandler = woodyInstance.GetComponent<EntityActionHandler>();
                        var entity = woodyInstance.GetComponent<GameEntity>();

                        var brainProp = actor.GetType().GetField("_brain", BindingFlags.NonPublic | BindingFlags.Instance);
                        brainProp.SetValue(actor, actorBrain);

                        entity.WorldObjectName = woodyInstance.name;

                        var prop = actionHandler.GetType().GetField("_entity", BindingFlags.NonPublic | BindingFlags.Instance);
                        prop.SetValue(actionHandler, entity);

                        actionHandler.Actions.Clear();

                        foreach (var action in oldActionHandler.Actions)
                        {
                            EntityAction newAction = new EntityAction(action.ActionName);

                            action.CopyFieldsTo(newAction);

                            if (newAction.ActorName == "woody")
                            {
                                var nameProp = newAction.GetType().GetField("_actorName", BindingFlags.NonPublic | BindingFlags.Instance);
                                nameProp.SetValue(newAction, woodyInstance.name);
                            }

                            if (newAction.ActorBrain == originalWoody.gameObject.GetComponent<ActorBrain>())
                            {
                                newAction.ActorBrain = actorBrain;
                            }

                            var handlerProp = newAction.GetType().GetField("_handler", BindingFlags.NonPublic | BindingFlags.Instance);
                            handlerProp.SetValue(newAction, actionHandler);

                            actionHandler.Actions.Add(newAction);
                        }

                        actor.SetCurrentRoom(originalWoody.CurrentRoom);

                        LevelRoom room = originalWoody.CurrentRoom;
                        if (woodyInstance.gameObject.GetComponent<GameEntity>() is GameEntity gameEntity && room != null)
                        {
                            gameEntity.SetCurrentRoom(room);
                            woodyInstance.gameObject.transform.localPosition = new Vector3(originalWoody.transform.localPosition.x, originalWoody.transform.localPosition.y, 0);

                            if (LogicController.Instance.TheTriggerHandler is TriggerHandler triggerHandler)
                            {
                                LogicController.Instance.InitializeLevel(LogicController.Instance.CurrentLevel);
                            }
                            else
                            {
                                Log.Info($"Failed to get trigger handler..");
                            }

                            LogicController.Instance.AddEntityToWorld(gameEntity, room);

                            LogicController.Instance.GetAllPortals(true);
                            LogicController.Instance.GetAllItems(true);
                            LogicController.Instance.GetAllContainers(true);
                            LogicController.Instance.GetAllActors(true);
                            LogicController.Instance.GetAllActorBrains(true);
                        }
                        else
                        {
                            Log.Info($"Failed to get Room: {room == null} or Entity: {woodyInstance.GetComponent<GameEntity>() == null}");
                        }

                        var neighbour = LogicController.Instance.GetAllActorBrains().FirstOrDefault(x => x.IsNeighbor);
                        var woody = LogicController.Instance.GetAllActorBrains().FirstOrDefault(x => x.IsWoody);

                        Log.Info($"Successfully added new test woody!");

                        entity.SetCurrentRoom(room);
                        woodyInstance.gameObject.transform.localPosition = new Vector3(playerPosition.Position.X, playerPosition.Position.Y, 0);
                        GetPlayerManager.Players[playerPosition.SenderID].Woody = woodyInstance.gameObject;
                        LogicController.Instance.AddEntityToWorld(entity, room);
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
