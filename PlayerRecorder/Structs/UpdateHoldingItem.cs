using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class UpdateHoldingItem : IEventType
    {
        [ProtoMember(1)]
        public sbyte PlayerID { get; set; }
        [ProtoMember(2)]
        public sbyte HoldingItem { get; set; }
    }
}
