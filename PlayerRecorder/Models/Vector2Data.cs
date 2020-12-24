using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Models
{
    public class Vector2Data
    {
        public float x { get; set; }
        public float y { get; set; }
        public Vector2Data Create(Vector2 quat)
        {
            this.x = quat.x;
            this.y = quat.y;
            return this;
        }
        [BsonIgnore]
        public Vector2 Vector
        {
            get
            {
                return new Vector2(x, y);
            }
        }
    }
}
