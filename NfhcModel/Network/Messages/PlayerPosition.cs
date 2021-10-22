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
    public class PlayerPosition : BaseMessageType
    { 
        [ProtoMember(1)]
        public CustomVector Position { get; set; }

        [ProtoMember(2)]
        public string Room { get; set; }

        [ProtoMember(3)]
        public string Animation { get; set; }

        [ProtoMember(4)]
        public float AnimTime { get; set; }

        [ProtoMember(5)]
        public bool IsHidden { get; set; }

        [ProtoMember(6)]
        public bool IsActive { get; set; }

        public PlayerPosition()
        {
            MsgType = MessageTypes.PlayerTransform;
        }
    }
}
