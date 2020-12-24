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
    public class LeaveData
    {
        [Key(0)]
        public byte EventID { get; set; } = (byte)RecordEvents.PlayerLeave;
        [Key(1)]
        public sbyte PlayerID { get; set; }
    }
}
