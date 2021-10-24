using PlayerRecorder.Interfaces;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class NukeOutsideKeycardEnteredData : IEventType
    {
        [ProtoMember(1)]
        public bool IsEntered { get; set; }
    }
}
