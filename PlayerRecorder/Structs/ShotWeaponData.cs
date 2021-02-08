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
    public class ShotWeaponData : IEventType
    {
        [Key(0)]
        public sbyte PlayerID { get; set; }
    }
}
