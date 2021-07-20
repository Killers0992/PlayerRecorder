using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class LiftData : IEventType
    {
        [ProtoMember(1)]
        public string Elevatorname { get; set; }
        [ProtoMember(2)]
        public byte StatusID { get; set; }
        [ProtoMember(3)]
        public bool IsLocked { get; set; }
    }
}
