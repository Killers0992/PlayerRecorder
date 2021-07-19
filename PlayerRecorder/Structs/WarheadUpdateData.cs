using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class WarheadUpdateData : IEventType
    {
        [ProtoMember(1)]
        public bool InProgress { get; set; }
        [ProtoMember(2)]
        public sbyte ResumeScenario { get; set; }
        [ProtoMember(3)]
        public byte StartScenario { get; set; }
        [ProtoMember(4)]
        public float TimeToDetonation { get; set; }
    }
}
