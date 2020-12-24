using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Models
{
    public class ThrowGrenadeData : InvokeEvent
    {
        public string UserID { get; set; }
        public int Grenadeid { get; set; }
        public double Fusetime { get; set; }
        public bool SlowThrow { get; set; }
    }
}
