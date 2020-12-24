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
    public class RemovePickupData
    {
        [Key(0)]
        public byte EventID { get; set; } = (byte)RecordEvents.RemovePickup;
        [Key(1)]
        public int ItemID { get; set; }
    }
}
