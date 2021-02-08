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
    public class UpdatePlayerData : IEventType
    {
        [MessagePack.Key(0)]
        public sbyte PlayerID { get; set; }
        [MessagePack.Key(1)]
        public byte MoveState { get; set; }
        [MessagePack.Key(2)]
        public sbyte HoldingItem { get; set; }
        [MessagePack.Key(3)]
        public int CurrentAnim { get; set; }
        [MessagePack.Key(4)]
        public Vector2Data Speed { get; set; }
        [MessagePack.Key(5)]
        public Vector3Data Position { get; set; }
        [MessagePack.Key(6)]
        public Vector2Data Rotation { get; set; }
    }
}
