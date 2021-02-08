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
    public class UpdatePickupData : IEventType
    {
        [Key(0)]
        public int ItemID { get; set; }
        [Key(1)]
        public int ItemType { get; set; }
        [Key(2)]
        public Vector3Data Position { get; set; }
        [Key(3)]
        public QuaternionData Rotation { get; set; }
    }
}
