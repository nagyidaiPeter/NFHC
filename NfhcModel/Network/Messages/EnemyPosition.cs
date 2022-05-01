using NfhcModel.DataStructures;
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
    public class EnemyPosition : BaseMessageType
    {
        [ProtoMember(1)]
        public CustomVector Position { get; set; }

        [ProtoMember(2)]
        public string Room { get; set; }

        [ProtoMember(3)]
        public NpcTypes NpcType { get; set; }

        [ProtoMember(4)]
        public string Animation { get; set; }

        [ProtoMember(5)]
        public float AnimTime { get; set; }

        [ProtoMember(6)]
        public bool IsHidden { get; set; }

        [ProtoMember(7)]
        public string CurrentTask { get; set; }


        public EnemyPosition()
        {
            MsgType = MessageTypes.EnemyTransform;
        }

    }
}