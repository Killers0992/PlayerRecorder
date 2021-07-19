using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class ReloadWeaponData : IEventType
    {
        [ProtoMember(1)]
        public sbyte PlayerID { get; set; }
    }
}
