using PlayerRecorder.Interfaces;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class ScpTerminationData : IEventType
    {
        [ProtoMember(1)]
        public string RoleFullName { get; set; }
        [ProtoMember(2)]
        public int ToolID { get; set; }
        [ProtoMember(3)]
        public string GroupID { get; set; }
    }
}
