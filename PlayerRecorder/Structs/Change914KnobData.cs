using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class Change914KnobData : IEventType
    {
        [ProtoMember(1)]
        public sbyte KnobSetting { get; set; }
    }
}
