using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class SeedData : IEventType
    {
        [ProtoMember(1)]
        public int Seed { get; set; }
    }
}
