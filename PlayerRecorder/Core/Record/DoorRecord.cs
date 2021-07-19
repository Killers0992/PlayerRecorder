using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Core.Record
{
    public class DoorRecord : MonoBehaviour
    {
        public bool IsOpen { get; set; } = false;

        DoorVariant _door;
        public DoorVariant door
        {
            get
            {
                if (_door == null)
                    _door = GetComponent<DoorVariant>();
                return _door;
            }
        }

        private void Update()
        {
            if (!MainClass.isRecording)
                return;
            if (IsOpen != door.TargetState)
            {
                IsOpen = door.TargetState;
                RecorderCore.OnReceiveEvent(new DoorData()
                {
                    State = IsOpen,
                    Position = new Vector3Data() { x = door.transform.position.x, y = door.transform.position.y, z = door.transform.position.z },
                });
            }
        }
    }
}
