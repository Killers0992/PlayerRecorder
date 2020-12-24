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
    public class LiftData
    {
        [Key(0)]
        public byte EventID { get; set; } = (byte)RecordEvents.UseLift;
        [Key(1)]
        public string Elevatorname { get; set; }
    }
}
