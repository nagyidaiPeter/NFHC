using LiteNetLib;
using LiteNetLib.Utils;
using NfhcModel.Core;
using NfhcModel.DataStructures;
using NfhcModel.Logger;
using NfhcModel.Network.Messages;
using NfhcModel.Network.ServerModul.ServerProcessors;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NfhcModel.Network.ServerModul
{
    public class Server
    {
        private Dictionary<MessageTypes, IProcessor> MessageProcessors = new Dictionary<MessageTypes, IProcessor>();

        public bool IsServerRunning = true;

        public NetManager server;

        private PlayerManager playerManager;
        private ServerConfig serverConfig;

        public Server(ServerConfig serverConfig, PlayerManager playerManager)
        {
            this.serverConfig = serverConfig;
            this.playerManager = playerManager;

            //Get all server message processors filtered with namespace
            Type[] processors = typeof(IProcessor).Assembly.GetTypes()
              .Where(t => t.Namespace != null)
              .Where(t => t.Namespace.StartsWith("NfhcModel.Network.ServerModul.ServerProcessors", StringComparison.Ordinal))
              .Where(t => t.IsSubclassOf(typeof(BaseProcessor)))
              .Where(t => !t.IsAbstract)
              .ToArray();

            foreach (var proc in processors)
            {
                if (NfhcServiceLocator.LocateService(proc) is IProcessor resolvedProc)
                {
                    MessageProcessors.Add(resolvedProc.MessageType, resolvedProc);
                }
            }

            EventBasedNetListener listener = new EventBasedNetListener();
            server = new NetManager(listener);
            server.Start(serverConfig.Port);

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < serverConfig.MaxPlayers)
                    request.AcceptIfKey(serverConfig.ServerKey);
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                Log.Info($"We got connection: {peer.EndPoint}");
                PlayerIntroduction(peer);
            };

            listener.NetworkReceiveEvent += (peer, reader, deliveryMethod) =>
            {
                byte[] received = new byte[reader.AvailableBytes];
                reader.GetBytes(received, received.Length);

                var baseMessage = BaseMessageType.Deserialize<BaseMessageType>(received);


                if (!playerManager.Players.ContainsKey(baseMessage.SenderID) && baseMessage.MsgType != MessageTypes.PlayerDataMessage
                || peer.Id != baseMessage.SenderID)
                {
                    //Not known player or has wrong id, plus it's not an intro PlayerDataMessage
                    //send them PlayerDataMessage to give them their id and request username, etc..
                    PlayerIntroduction(peer);
                    return;
                }

                MessageProcessors[baseMessage.MsgType].AddMessage(received, playerManager.Players[baseMessage.SenderID]);
            };
        }

        private void PlayerIntroduction(NetPeer peer)
        {
            Log.Info($"Sending new connection introduction request..");

            PlayerData playerData = new PlayerData();
            playerData.Connection = peer;
            playerData.ID = peer.Id;
            playerData.Name = "New Woody";

            playerManager.Players.Add(peer.Id, playerData);

            PlayerDataMessage playerDataMsg = new PlayerDataMessage();
            playerDataMsg.Name = "Woody";
            playerDataMsg.ID = peer.Id;
            playerDataMsg.OwnData = true;
            playerDataMsg.SenderID = -1;

            MessageProcessors[MessageTypes.PlayerDataMessage].SendMessage(playerDataMsg, playerData);
        }

        public IEnumerator RunServer()
        {
            IsServerRunning = true;
            Log.Info($"Server running!");
            while (IsServerRunning)
            {
                server.PollEvents();
                foreach (var processor in MessageProcessors.Values)
                {
                   processor.Process();                    
                }
                yield return new WaitForSeconds(serverConfig.RefreshTime);

            }
            server.Stop();
            Log.Info($"Server stopped...");
        }

        public void ServerSay(string message)
        {
            ChatMessage chatMessage = new ChatMessage();

            chatMessage.MessageText = "Server: " + message;
            chatMessage.SenderID = -1;
            chatMessage.MsgType = MessageTypes.Chat;

            var msg = BaseMessageType.Serialize(chatMessage);

            NetDataWriter writer = new NetDataWriter();
            writer.Put(msg);
            server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        public void StopServer()
        {
            IsServerRunning = false;
        }
    }
}
