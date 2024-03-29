﻿using PlayerRecorder.Interfaces;
using ProtoBuf;
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
        private Quaternion _quat;
        [ProtoIgnore]
        public Quaternion quaternion
        {
            get
            {
                return new Quaternion(x, y, z, w);
            }
        }
    }
}
