using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class RemovePickupData : IEventType
    {
        [ProtoMember(1)]
        public int ItemID { get; set; }
    }
}
