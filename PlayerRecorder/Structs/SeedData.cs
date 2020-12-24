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
    public class SeedData
    {
        [Key(0)]
        public byte EventID { get; set; } = (byte)RecordEvents.ReceiveSeed;
        [Key(1)]
        public int Seed { get; set; }
    }
}
