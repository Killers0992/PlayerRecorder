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
    public class RoundEndData
    {
        [Key(0)]
        public byte EventID { get; set; } = (byte)RecordEvents.RoundEnd;
    }
}
