using PlayerRecorder.Interfaces;
using ProtoBuf;


namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class DoorData : IEventType
    {
        [ProtoMember(1)]
        public bool State { get; set; }
        [ProtoMember(2)]
        public Vector3Data Position { get; set; }
    }
}
