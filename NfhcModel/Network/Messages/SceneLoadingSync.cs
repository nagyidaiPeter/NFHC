using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.Messages
{
    [ProtoContract]
    public class SceneLoadingSync : BaseMessageType
    {
        [ProtoMember(1)]
        public string SceneName { get; set; }

        [ProtoMember(2)]
        public int LoadingMode { get; set; }

        public SceneLoadingSync()
        {
            MsgType = MessageTypes.SceneLoadingSync;
        }
    }
}
