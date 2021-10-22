using LiteNetLib;
using LiteNetLib.Utils;
using NfhcModel.Network.Messages;
using NfhcModel.Core;
using NfhcModel.DataStructures;
using NfhcModel.Network.ClientModul;
using NfhcModel.Network.ServerModul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network
{
    public class BaseProcessor : IProcessor
    {
        public Queue<BaseMessageType> IncomingMessages { get; set; } = new Queue<BaseMessageType>();

        public Queue<BaseMessageType> OutgoingMessages { get; set; } = new Queue<BaseMessageType>();


        private Server _server;
        internal Server GetServer
        {
            get
            {
                _server = _server ?? NfhcServiceLocator.LocateService<Server>();
                return _server;
            }
        }

        private Client _client;
        internal Client GetClient
        {
            get
            {
                _client = _client ?? NfhcServiceLocator.LocateService<Client>();
                return _client;
            }
        }

        private PlayerManager _playerManager;
        internal PlayerManager GetPlayerManager
        {
            get
            {
                _playerManager = _playerManager ?? NfhcServiceLocator.LocateService<PlayerManager>();
                return _playerManager;
            }
        }

        public virtual bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {
            throw new NotImplementedException();
        }

        public virtual bool SendMessage(object message, DataStructures.PlayerData player)
        {
            throw new NotImplementedException();
        }

        public async virtual void Process()
        {
            throw new NotImplementedException();
        }
    }
}

