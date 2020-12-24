using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Models
{
    public class QuaternionData
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }

        public QuaternionData Create(Quaternion quat)
        {
            this.x = quat.x;
            this.y = quat.y;
            this.z = quat.z;
            this.w = quat.w;
            return this;
        }

        [BsonIgnore]
        public Quaternion Rotation
        {
            get
            {
                return new Quaternion(x, y, z, w);
            }
        }
    }
}
