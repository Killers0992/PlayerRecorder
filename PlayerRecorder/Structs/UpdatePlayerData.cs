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
    public class UpdatePlayerData
    {
        [Key(0)]
        public byte EventID { get; set; } = (byte)RecordEvents.UpdatePlayer;
        [MessagePack.Key(1)]
        public sbyte PlayerID { get; set; }
        [MessagePack.Key(2)]
        public byte MoveState { get; set; }
        [MessagePack.Key(3)]
        public sbyte HoldingItem { get; set; }
        [MessagePack.Key(4)]
        public int CurrentAnim { get; set; }
        [MessagePack.Key(5)]
        public Vector2Data Speed { get; set; }
        [MessagePack.Key(6)]
        public Vector3Data Position { get; set; }
        [MessagePack.Key(7)]
        public Vector2Data Rotation { get; set; }
    }
}
