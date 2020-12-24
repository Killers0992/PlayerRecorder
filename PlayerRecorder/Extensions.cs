using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder
{
    public static class Extensions
    {
        public static QuaternionData GetData(this Quaternion qat)
        {
            return new QuaternionData(qat.x, qat.y, qat.z, qat.w);
        }

        public static Vector2Data GetData(this Vector2 qat)
        {
            return new Vector2Data(qat.x, qat.y);
        }

        public static Vector3Data GetData(this Vector3 qat)
        {
            return new Vector3Data(qat.x, qat.y, qat.z);
        }
    }
}
