using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class UpdateHoldingItem : IEventType
    {
        [ProtoMember(1)]
        public sbyte PlayerID { get; set; }
        [ProtoMember(2)]
        public sbyte HoldingItem { get; set; }
    }
}
