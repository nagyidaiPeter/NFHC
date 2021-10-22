using LiteNetLib;
using NFH.Game;
using NFH.Game.Logic;
using NfhcModel.Logger;
using NfhcModel.MonoBehaviours.Gui;
using NfhcModel.Network.ClientModul.ClientProcessors;
using NfhcModel.Network.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NfhcModel.Network.ClientModul
{
    public class Client
    {
        Dictionary<MessageTypes, IProcessor> MessageProcessors = new Dictionary<MessageTypes, IProcessor>();

        public ClientConfig clientConfig;

        public bool IsClientRunning = false;

        private PlayerManager playerManager;

        public ChatHandler chatHandler;

        public bool IsServer = false;

        public NetManager client;
        public EventBasedNetListener listener;
        public Client(ClientConfig clientConfig, PlayerManager playerManager, ClientPlayerDataProcessor playerDataProcessor,
            ClientChatProcessor clientChatProcessor, ClientSceneSyncProcessor clientSceneSync, ClientPlayerPosProcessor clientPlayerPos,
            ClientEnemyPosProcessor enemyPosProcessor, ClientGameEntityProcessor gameEntityProcessor)
        {
            this.clientConfig = clientConfig;
            this.playerManager = playerManager;
            MessageProcessors.Add(MessageTypes.PlayerDataMessage, playerDataProcessor);
            MessageProcessors.Add(MessageTypes.Chat, clientChatProcessor);
            MessageProcessors.Add(MessageTypes.SceneLoadingSync, clientSceneSync);
            MessageProcessors.Add(MessageTypes.PlayerTransform, clientPlayerPos);
            MessageProcessors.Add(MessageTypes.EnemyTransform, enemyPosProcessor);
            MessageProcessors.Add(MessageTypes.GameEntityMessage, gameEntityProcessor);

        }

        public IEnumerator RunClient()
        {
            IsClientRunning = true;

            listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.Start();
            client.Connect(clientConfig.IP, clientConfig.Port, clientConfig.ServerKey);

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                byte[] received = new byte[dataReader.AvailableBytes];
                dataReader.GetBytes(received, received.Length);

                var baseMessage = BaseMessageType.Deserialize<BaseMessageType>(received);
                MessageProcessors[baseMessage.MsgType].AddMessage(received, null);

                dataReader.Recycle();
            };

            listener.PeerConnectedEvent += (server) =>
            {
                chatHandler.EnableChat = true;
            };

            listener.PeerDisconnectedEvent += (peer, dcInfo) =>
            {
                IsServer = false;
                chatHandler.EnableChat = false;
            };

            while (IsClientRunning)
            {
                client.PollEvents();
                foreach (var processor in MessageProcessors.Values)
                {
                    try
                    {
                        processor.Process();
                    }
                    catch (Exception ex)
                    {
                        Log.Info(ex);
                    }
                }
                yield return new WaitForSeconds(clientConfig.RefreshTime);
            }

            client.Stop();
            Log.Info($"Client stopped...");
        }

        public void SendChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            MessageProcessors[MessageTypes.Chat].SendMessage(message.ToChatMessage(), playerManager.LocalPlayer);
        }

        public void StopClient()
        {
            IsClientRunning = false;
        }
    }
}
