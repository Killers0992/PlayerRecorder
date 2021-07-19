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
    public class QuaternionData
    {
        [ProtoMember(1)]
        public float x { get; set; }
        [ProtoMember(2)]
        public float y { get; set; }
        [ProtoMember(3)]
        public float z { get; set; }
        [ProtoMember(4)]
        public float w { get; set; }


        [ProtoIgnore]
        public Quaternion quaternion => new Quaternion(x,y,z,w);
    }
}
