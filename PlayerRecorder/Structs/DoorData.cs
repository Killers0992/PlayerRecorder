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
    public class DoorData : IEventType
    {
        [Key(0)]
        public bool State { get; set; }
        [Key(1)]
        public Vector3Data Position { get; set; }
    }
}
