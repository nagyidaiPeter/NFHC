using LiteNetLib;
using LiteNetLib.Utils;
using NFH.Game;
using NFH.Game.UI;
using NfhcModel.Logger;
using NfhcModel.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NfhcModel.Network.ClientModul.ClientProcessors
{
    public class ClientLevelDetailsSyncProcessor : BaseProcessor
    {
        public new Queue<LevelDetailsSync> IncomingMessages { get; set; } = new Queue<LevelDetailsSync>();

        public new Queue<LevelDetailsSync> OutgoingMessages { get; set; } = new Queue<LevelDetailsSync>();

        public override MessageTypes MessageType { get { return MessageTypes.LevelDetailsSync; } }


        public ClientLevelDetailsSyncProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {
            var LevelDetailsSync = BaseMessageType.Deserialize<LevelDetailsSync>(message);
            IncomingMessages.Enqueue(LevelDetailsSync);
            return true;
        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is LevelDetailsSync dataMessage)
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
                LevelDetailsSync LevelDetailsSync = IncomingMessages.Dequeue();

                if (!GetClient.IsServer)
                {
                    
                }
            }

            while (OutgoingMessages.Any())
            {
                LevelDetailsSync LevelDetailsSync = OutgoingMessages.Dequeue();

            }
        }
    }
}
