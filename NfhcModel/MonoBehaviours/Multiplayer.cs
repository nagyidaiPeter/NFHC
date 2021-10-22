using NFH.Game;
using NFH.Game.Logic;
using NfhcModel.Core;
using NfhcModel.Logger;
using NfhcModel.Network;
using NfhcModel.Network.ClientModul;
using NfhcModel.Network.ClientModul.ClientProcessors;
using NfhcModel.Network.Messages;
using NfhcModel.Network.ServerModul;
using NfhcModel.Network.ServerModul.ServerProcessors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NfhcModel.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        private PlayerManager playerManager;

        private ServerConfig serverConfig;

        private ClientPlayerPosProcessor clientPlayerPos;
        private ServerEnemyPosProcessor serverEnemyPos;
        private ServerGameEntityProcessor serverGameEntity;

        public Server server { get; set; }
        public Client client { get; set; }

        private GameObject neighbor;

        void Start()
        {
            try
            {
                playerManager = NfhcServiceLocator.LocateService<PlayerManager>();
                serverConfig = NfhcServiceLocator.LocateService<ServerConfig>();

                clientPlayerPos = NfhcServiceLocator.LocateService<ClientPlayerPosProcessor>();
                serverEnemyPos = NfhcServiceLocator.LocateService<ServerEnemyPosProcessor>();
                serverGameEntity = NfhcServiceLocator.LocateService<ServerGameEntityProcessor>();

                server = NfhcServiceLocator.LocateService<Server>();
                client = NfhcServiceLocator.LocateService<Client>();
            }
            catch (Exception ex)
            {
                Log.Info($"Failed to Start Multiplayer monobehaviour: {ex}");
                this.enabled = false;
                return;
            }

            StartCoroutine(NetworkingTick());
        }

        private IEnumerator NetworkingTick()
        {
            while (true)
            {
                //Send pos
                if (client.IsClientRunning && playerManager.LocalPlayer.Woody != null)
                {
                    try
                    {
                        if (playerManager.LocalPlayer.Woody.GetComponent<ActorBrain>() is ActorBrain woodyBrain && woodyBrain.CurrentRoom != null)
                        {
                            PlayerPosition playerPosition = new PlayerPosition();
                            playerPosition.Position = new CustomVector()
                            {
                                X = playerManager.LocalPlayer.Woody.transform.localPosition.x,
                                Y = playerManager.LocalPlayer.Woody.transform.localPosition.y
                            };

                            playerPosition.Animation = woodyBrain.Entity.CurrentAnimName;
                            playerPosition.AnimTime = woodyBrain.Entity.SpriteAnimPlayer.CurrentAnimTime;

                            playerPosition.Room = woodyBrain.CurrentRoom.RoomName;
                            playerPosition.SenderID = playerManager.LocalPlayer.ID;
                            playerPosition.IsHidden = woodyBrain.Entity.IsHidden;
                            clientPlayerPos.SendMessage(playerPosition, playerManager.LocalPlayer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Info(ex);
                    }
                }

                if (server.IsServerRunning && client.IsServer)
                {
                    if (neighbor != null)
                    {
                        try
                        {
                            if (neighbor.GetComponent<ActorBrain>() is ActorBrain enemyBrain && enemyBrain.CurrentRoom != null)
                            {
                                EnemyPosition enemyPosition = new EnemyPosition();
                                enemyPosition.Position = new CustomVector()
                                {
                                    X = neighbor.transform.localPosition.x,
                                    Y = neighbor.transform.localPosition.y
                                };

                                enemyPosition.Room = enemyBrain.CurrentRoom.RoomName;
                                enemyPosition.SenderID = playerManager.LocalPlayer.ID;
                                enemyPosition.Animation = enemyBrain.Entity.CurrentAnimName;
                                enemyPosition.AnimTime = enemyBrain.Entity.SpriteAnimPlayer.CurrentAnimTime;
                                enemyPosition.IsHidden = enemyBrain.Entity.IsHidden;
                                serverEnemyPos.SendMessage(enemyPosition, playerManager.LocalPlayer);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Info(ex);
                        }
                    }
                    else if (GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsNeighbor) is ActorBrain enemy)
                    {
                        Log.Info($"Success finding neighbor!");
                        neighbor = enemy.gameObject;
                    }

                    foreach (var entity in GameObject.FindObjectsOfType<GameEntity>().Where(x => !x.IsActor))
                    {                        
                        GameEntityMessage gameEntityMessage = new GameEntityMessage();
                        gameEntityMessage.WorldObjectName = entity.WorldObjectName;
                        gameEntityMessage.Animation = entity.CurrentAnimName;
                        gameEntityMessage.AnimTime = entity.SpriteAnimPlayer.CurrentAnimTime;
                        gameEntityMessage.IsHidden = entity.IsHidden;
                        gameEntityMessage.IsLocked = entity.IsLocked;
                        gameEntityMessage.isActiveAndEnabled = entity.isActiveAndEnabled;

                        var actionHandler = entity.GetComponent<EntityActionHandler>();
                        


                        serverGameEntity.SendMessage(gameEntityMessage, playerManager.LocalPlayer);                        
                    }
                }

                yield return new WaitForSeconds(serverConfig.RefreshTime);
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
