using PlayerRecorder.Enums;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class SeedData : IEventType
    {
        [ProtoMember(1)]
        public int Seed { get; set; }
    }
}
