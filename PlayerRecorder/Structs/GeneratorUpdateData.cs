using PlayerRecorder.Interfaces;
using ProtoBuf;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class GeneratorUpdateData : IEventType
    {
        [ProtoMember(1)]
        public bool TabletConnected { get; set; }
        [ProtoMember(2)]
        public byte TotalVoltage { get; set; }
        [ProtoMember(3)]
        public float RemainingPowerup { get; set; }
        [ProtoMember(4)]
        public Vector3Data Position { get; set; }
    }
}
