using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class LiftData : IEventType
    {
        [ProtoMember(1)]
        public string Elevatorname { get; set; }
    }
}
