using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Models
{
    public class PickupData : InvokeEvent
    {
        public int ID { get; set; }
        public ItemType Item { get; set; }
        public Vector3Data Position { get; set; }
        public QuaternionData Rotation { get; set; }
    }
}
