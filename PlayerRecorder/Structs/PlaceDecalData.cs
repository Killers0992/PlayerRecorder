using PlayerRecorder.Interfaces;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class PlaceDecalData : IEventType
    {
        [ProtoMember(1)]
        public bool IsBlood { get; set; }
        [ProtoMember(2)]
        public sbyte Type { get; set; }
        [ProtoMember(3)]
        public Vector3Data Position { get; set; }
        [ProtoMember(4)]
        public QuaternionData Rotation { get; set; }
    }
}
