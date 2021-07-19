using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class UnlockGeneratorData : IEventType
    {
        [ProtoMember(1)]
        public Vector3Data Position { get; set; }
    }
}
