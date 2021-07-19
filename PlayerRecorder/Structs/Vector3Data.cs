using PlayerRecorder.Interfaces;
using ProtoBuf;
using UnityEngine;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class Vector3Data
    {
        [ProtoMember(1)]
        public float x { get; set; }
        [ProtoMember(2)]
        public float y { get; set; }
        [ProtoMember(3)]
        public float z { get; set; }

        [ProtoIgnore]
        private Vector3 _vec;

        [ProtoIgnore]
        public Vector3 vector
        {
            get
            {
                return new Vector3(x, y, z);
            }
        }
    }
}
