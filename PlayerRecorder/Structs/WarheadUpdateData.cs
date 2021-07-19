using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class WarheadUpdateData : IEventType
    {
        [ProtoMember(1)]
        public bool InProgress { get; set; }
        [ProtoMember(2)]
        public sbyte ResumeScenario { get; set; }
        [ProtoMember(3)]
        public byte StartScenario { get; set; }
        [ProtoMember(4)]
        public float TimeToDetonation { get; set; }
    }
}
