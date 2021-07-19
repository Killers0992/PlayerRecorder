using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Vector3Data Position { get; set; }
    }
}
