using PlayerRecorder.Enums;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class DoorData : IEventType
    {
        [ProtoMember(1)]
        public bool State { get; set; }
        [ProtoMember(2)]
        public Vector3Data Position { get; set; }
    }
}
