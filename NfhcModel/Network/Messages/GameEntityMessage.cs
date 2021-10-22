using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NfhcModel.Network.Messages
{
    [ProtoContract]
    public class GameEntityMessage : BaseMessageType
    {
        [ProtoMember(1)]
        public string WorldObjectName { get; set; }

        [ProtoMember(2)]
        public string Animation { get; set; }

        [ProtoMember(3)]
        public float AnimTime { get; set; }

        [ProtoMember(4)]
        public bool IsHidden { get; set; }

        [ProtoMember(5)]
        public bool IsLocked { get; set; }

        [ProtoMember(6)]
        public bool isActiveAndEnabled { get; set; }
        
        public GameEntityMessage()
        {
            MsgType = MessageTypes.GameEntityMessage;
        }
    }
}
