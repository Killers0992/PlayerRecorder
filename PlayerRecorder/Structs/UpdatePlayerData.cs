using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class UpdatePlayerData : IEventType
    {
        [ProtoMember(1)]
        public sbyte PlayerID { get; set; }
        [ProtoMember(2)]
        public sbyte RoleID { get; set; }
        [ProtoMember(3)]
        public Vector3Data Position { get; set; }
        [ProtoMember(4)]
        public Vector2Data Rotation { get; set; }
    }
}
