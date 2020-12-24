using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Models
{
    public class WeaponData : InvokeEvent
    {
        public string UserID { get; set; }
        public sbyte WeaponID { get; set; }
    }
}
