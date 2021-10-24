using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class GeneratorFlagsData : IEventType
    {
        [ProtoMember(1)]
        public byte Flags { get; set; }
        [ProtoMember(2)]
        public Vector3Data Position { get; set; }
    }
}
