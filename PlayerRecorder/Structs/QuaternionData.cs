using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Structs
{
    [MessagePackObject]
    public class QuaternionData
    {
        [Key(0)]
        public float x { get; set; }
        [Key(1)]
        public float y { get; set; }
        [Key(2)]
        public float z { get; set; }
        [Key(3)]
        public float w { get; set; }

        [SerializationConstructor]
        public QuaternionData(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public void SetQuaternion()
        {
            quaternion = new Quaternion(x, y,z,w);
        }

        [IgnoreMember]
        public Quaternion quaternion;
    }
}
