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
    public class Vector3Data
    {
        [Key(0)]
        public float x { get; set; }
        [Key(1)]
        public float y { get; set; }
        [Key(2)]
        public float z { get; set; }

        [SerializationConstructor]
        public Vector3Data(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 SetVector()
        {
            return new Vector3(x, y, z);
        }

        [IgnoreMember]
        public Vector3 vector;
    }
}
