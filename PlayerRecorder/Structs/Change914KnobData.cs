using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class Change914KnobData : IEventType
    {
        [ProtoMember(1)]
        public sbyte KnobSetting { get; set; }
    }
}
