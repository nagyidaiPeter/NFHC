using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.Messages
{
    [ProtoContract]
    public class LevelDetailsSync : BaseMessageType
    {
        [ProtoMember(1)]
        public float LevelTime { get; set; }

        public LevelDetailsSync()
        {
            MsgType = MessageTypes.LevelDetailsSync;
        }
    }
}
