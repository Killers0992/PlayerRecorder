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
    public class Vector2Data
    {
        [Key(0)]
        public float x;
        [Key(1)]
        public float y;

        [SerializationConstructor]
        public Vector2Data(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 GetVector()
        {
            return new Vector2(x, y);
        }
    }
}
