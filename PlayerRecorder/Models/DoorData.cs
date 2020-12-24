using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Models
{
    public class DoorData : InvokeEvent
    {
        public bool state { get; set; }
        public Vector3Data Position { get; set; }
    }
}
