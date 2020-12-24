using LiteDB;
using PlayerRecorder.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Models
{
    public class InvokeEvent
    {
        public Guid Id { get; set; }
        public int frameId { get; set; }
        public EventType Event { get; set; }
    }
}
