using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NfhcModel.DataStructures;
using NfhcModel.Network.Messages;

namespace NfhcModel.Network
{
    public interface IProcessor
    {
        Queue<BaseMessageType> IncomingMessages { get; set; }

        Queue<BaseMessageType> OutgoingMessages { get; set; }

        bool AddMessage(byte[] message, DataStructures.PlayerData player);

        bool SendMessage(object message, DataStructures.PlayerData player);

        void Process();
    }
}
