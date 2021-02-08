using MessagePack;
using PlayerRecorder.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [MessagePackObject]
    public class LiftData : IEventType
    {
        [Key(0)]
        public string Elevatorname { get; set; }
    }
}
