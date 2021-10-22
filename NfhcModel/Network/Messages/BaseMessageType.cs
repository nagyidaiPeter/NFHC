using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Network.Messages
{
    public enum MessageTypes : byte
    {
        Chat,
        PlayerTransform,
        EnemyTransform,
        SceneLoadingSync,
        LevelDetailsSync,
        PlayerDataMessage,
        GameEntityMessage
    }

    [Serializable]
    [ProtoContract]
    public struct CustomVector
    {
        [ProtoMember(1)]
        public float X;

        [ProtoMember(2)]
        public float Y;
    }

    [ProtoContract]
    [ProtoInclude(500, typeof(ChatMessage))]
    [ProtoInclude(501, typeof(PlayerPosition))]
    [ProtoInclude(502, typeof(EnemyPosition))]
    [ProtoInclude(503, typeof(SceneLoadingSync))]
    [ProtoInclude(504, typeof(LevelDetailsSync))]
    [ProtoInclude(505, typeof(PlayerDataMessage))]
    [ProtoInclude(506, typeof(GameEntityMessage))]
    public class BaseMessageType
    {
        [ProtoMember(1)]
        public MessageTypes MsgType { get; set; }

        [ProtoMember(2)]
        public int SenderID { get; set; }

        public static T Deserialize<T>(byte[] message)
        {
            T msgOut;

            using (var stream = new MemoryStream(message))
            {
                msgOut = Serializer.Deserialize<T>(stream);
            }

            return msgOut;
        }

        public static byte[] Serialize<T>(T msg)
        {
            byte[] msgOut;

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, msg);
                msgOut = stream.ToArray();
            }

            return msgOut;
        }
    }
}
