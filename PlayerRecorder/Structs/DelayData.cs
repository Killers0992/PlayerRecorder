using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [MessagePackObject]
    public class DelayData : IEventType
    {
        [Key(0)]
        public float DelayTime { get; set; }
    }
}
