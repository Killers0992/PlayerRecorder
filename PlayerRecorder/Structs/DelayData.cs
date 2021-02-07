using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [MessagePack.MessagePackObject]
    public class DelayData
    {
        [MessagePack.Key(0)]
        public byte Event { get; set; } = (byte)PlayerRecorder.Enums.RecordEvents.Delay;
        [MessagePack.Key(1)]
        public float DelayTime { get; set; }
    }
}
