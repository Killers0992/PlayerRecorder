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
        public byte MoveState { get; set; }
        [ProtoMember(3)]
        public int CurrentAnim { get; set; }
        [ProtoMember(4)]
        public Vector2Data Speed { get; set; }
        [ProtoMember(5)]
        public Vector3Data Position { get; set; }
        [ProtoMember(6)]
        public Vector2Data Rotation { get; set; }
    }
}
