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
    public class PlayerInfoData
    {
        [Key(0)]
        public byte EventID { get; set; } = (byte)RecordEvents.PlayerInfo;
        [Key(1)]
        public sbyte PlayerID { get; set; }
        [Key(2)]
        public string UserName { get; set; }
        [Key(3)]
        public string UserID { get; set; }
    }
}
