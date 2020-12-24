using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Models
{
    public class Vector3Data
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public Vector3Data Create(Vector3 quat)
        {
            this.x = quat.x;
            this.y = quat.y;
            this.z = quat.z;
            return this;
        }
        [BsonIgnore]
        public Vector3 Vector
        {
            get
            {
                return new Vector3(x, y, z);
            }
        }
    }
}
