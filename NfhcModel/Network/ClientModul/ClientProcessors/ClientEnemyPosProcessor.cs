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
    public class ClientEnemyPosProcessor : BaseProcessor
    {
        public new Queue<EnemyPosition> IncomingMessages { get; set; } = new Queue<EnemyPosition>();

        public new Queue<EnemyPosition> OutgoingMessages { get; set; } = new Queue<EnemyPosition>();

        private ActorBrain neighbor;

        public override MessageTypes MessageType { get { return MessageTypes.EnemyTransform; } }

        public ClientEnemyPosProcessor()
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
            if (message is EnemyPosition dataMessage)
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
                EnemyPosition enemyPosition = IncomingMessages.Dequeue();

                if (!GetClient.IsServer)
                {
                    if (neighbor != null)
                    {
                        LevelRoom room = null;

                        if (GameObject.FindObjectOfType<LogicController>() is LogicController logicController)
                        {
                            room = logicController.GetRoomByName(enemyPosition.Room);
                        }

                        if (neighbor.gameObject.GetComponent<GameEntity>() is GameEntity gameEntity && room != null)
                        {
                            gameEntity.SetCurrentRoom(room);
                            gameEntity.gameObject.transform.localPosition = new Vector3(enemyPosition.Position.X, enemyPosition.Position.Y, 0);
                            gameEntity.PlayAnimFromTime(gameEntity.GetAnimByName(enemyPosition.Animation), enemyPosition.AnimTime);
                        }

                        if (neighbor?.GetBrainScript() is BrainScriptBase brainScript)
                        {
                            MethodInfo dynMethod = brainScript.GetType().GetMethod(enemyPosition.CurrentTask, BindingFlags.NonPublic | BindingFlags.Instance);

                            BrainScriptBase.State method = Delegate.CreateDelegate(typeof(BrainScriptBase.State), brainScript, dynMethod) as BrainScriptBase.State;
                            brainScript.EnterIdleState();
                            neighbor.StopAllJobs();
                            brainScript.Stop(neighbor);
                            brainScript.SetState(method);
                            brainScript.Run(neighbor);
                        }
                    }
                    else if(GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsNeighbor) is ActorBrain enemy)
                    {
                        neighbor = enemy;
                        neighbor?.StopAllJobs();
                    }
                }
            }

            while (OutgoingMessages.Any())
            {
                EnemyPosition enemyPosition = OutgoingMessages.Dequeue();
                enemyPosition.SenderID = GetPlayerManager.LocalPlayer.ID;
                var msg = BaseMessageType.Serialize(enemyPosition);

                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetClient.client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            }
        }

        private void Logic_OnLogicStarted()
        {
            throw new NotImplementedException();
        }
    }
}
