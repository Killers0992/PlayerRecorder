using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Core.Record
{
    public class LiftRecord : MonoBehaviour
    {
        public byte StatusID { get; set; } = 0;
        public bool IsLocked { get; set; } = false;

        Lift _lift;
        public Lift lift
        {
            get
            {
                if (_lift == null)
                    _lift = GetComponent<Lift>();
                return _lift;
            }
        }

        private void Update()
        {
            if (!MainClass.isRecording)
                return;
            if (StatusID != lift.NetworkstatusID || IsLocked != lift.Network_locked)
            {
                StatusID = lift.NetworkstatusID;
                IsLocked = lift.Network_locked;
                RecordCore.OnReceiveEvent(new LiftData()
                {
                    StatusID = StatusID,
                    IsLocked = IsLocked,
                    Elevatorname = lift.elevatorName
                });
            }
        }
    }
}
