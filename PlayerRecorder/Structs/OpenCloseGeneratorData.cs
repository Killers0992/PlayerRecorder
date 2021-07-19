using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class OpenCloseGeneratorData : IEventType
    {
        [ProtoMember(1)]
        public bool IsOpen { get; set; }
        [ProtoMember(2)]
        public Vector3Data Position { get; set; }
    }
}
