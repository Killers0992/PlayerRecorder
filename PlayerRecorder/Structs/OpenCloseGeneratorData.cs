using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class OpenCloseGeneratorData : IEventType
    {
        [ProtoMember(1)]
        public bool IsOpen { get; set; }
        [ProtoMember(2)]
        public Vector3Data Position { get; set; }
    }
}
