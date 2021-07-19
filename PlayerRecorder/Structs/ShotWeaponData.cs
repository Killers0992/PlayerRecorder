using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class ShotWeaponData : IEventType
    {
        [ProtoMember(1)]
        public sbyte PlayerID { get; set; }
    }
}
