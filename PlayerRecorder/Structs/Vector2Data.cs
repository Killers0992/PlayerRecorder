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
        public Vector2 vector => new Vector2(x,y);
    }
}
