using PlayerRecorder.Interfaces;
using ProtoBuf;
using UnityEngine;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class Vector2Data
    {
        [ProtoMember(1)]
        public float x { get; set; }
        [ProtoMember(2)]
        public float y { get; set; }

        [ProtoIgnore]
        private Vector2 _vec;

        [ProtoIgnore]
        public Vector2 vector
        {
            get
            {
                if (_vec == null)
                    _vec = new Vector2(x, y);
                return _vec;
            }
        }
    }
}
