using MessagePack;
using PlayerRecorder.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Structs
{
    [MessagePackObject]
    public class DoorData
    {
        [Key(0)]
        public byte EventID { get; set; } = (byte)RecordEvents.DoorState;
        [Key(1)]
        public bool State { get; set; }
        [Key(2)]
        public Vector3Data Position { get; set; }
    }
}
