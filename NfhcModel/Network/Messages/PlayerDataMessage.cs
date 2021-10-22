using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.Messages
{
    [ProtoContract]
    public class PlayerDataMessage : BaseMessageType
    {

        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public int ID { get; set; }

        [ProtoMember(3)]
        public bool OwnData { get; set; } = false;

        public PlayerDataMessage()
        {
            MsgType = MessageTypes.PlayerDataMessage;
        }
    }
}
