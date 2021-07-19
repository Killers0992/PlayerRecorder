using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class CreateRagdollData : IEventType
    {
        [ProtoMember(1)]
        public int PlayerID { get; set; }
        [ProtoMember(2)]
        public string OwnerNick { get; set; }
        [ProtoMember(3)]
        public string OwnerID { get; set; }
        [ProtoMember(4)]
        public int ClassID { get; set; }
        [ProtoMember(5)]
        public int ToolID { get; set; }
        [ProtoMember(6)]
        public Vector3Data Velocity { get; set; }
        [ProtoMember(7)]
        public Vector3Data Position { get; set; }
        [ProtoMember(8)]
        public QuaternionData Rotation { get; set; }
    }
}
