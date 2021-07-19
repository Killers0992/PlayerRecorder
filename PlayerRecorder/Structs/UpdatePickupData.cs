using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class UpdatePickupData : IEventType
    {
        [ProtoMember(1)]
        public int ItemID { get; set; }
        [ProtoMember(2)]
        public int ItemType { get; set; }
        [ProtoMember(3)]
        public Vector3Data Position { get; set; }
        [ProtoMember(4)]
        public QuaternionData Rotation { get; set; }
    }
}
