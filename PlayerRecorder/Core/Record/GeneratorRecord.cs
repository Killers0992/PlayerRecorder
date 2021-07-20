using Exiled.API.Features;
using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Core.Record
{
    public class GeneratorRecord : MonoBehaviour
    {
        public bool TabletConnected { get; set; } = false;
        public bool IsDoorOpen { get; set; } = false;
        public bool IsUnlocked { get; set; } = false;

        Generator079 _generator;
        public Generator079 generator
        {
            get
            {
                if (_generator == null)
                    _generator = GetComponent<Generator079>();
                return _generator;
            }
        }

        private void Update()
        {
            if (!MainClass.isRecording)
                return;
            if (TabletConnected != generator.NetworkisTabletConnected)
            {
                TabletConnected = generator.NetworkisTabletConnected;
                RecordCore.OnReceiveEvent(new GeneratorUpdateData()
                {
                    TabletConnected = TabletConnected,
                    TotalVoltage = generator.NetworktotalVoltage,
                    RemainingPowerup = generator.NetworkremainingPowerup,
                    Position = new Vector3Data() { x = generator.transform.position.x, y = generator.transform.position.y, z = generator.transform.position.z }
                });
            }
            if (IsUnlocked != generator.NetworkisDoorUnlocked)
            {
                IsUnlocked = generator.NetworkisDoorUnlocked;
                RecordCore.OnReceiveEvent(new UnlockGeneratorData()
                {
                    Position = new Vector3Data() { x = generator.transform.position.x, y = generator.transform.position.y, z = generator.transform.position.z }
                });
            }

            if (IsDoorOpen != generator.NetworkisDoorOpen)
            {
                IsDoorOpen = generator.NetworkisDoorOpen;
                RecordCore.OnReceiveEvent(new OpenCloseGeneratorData()
                {
                    IsOpen = IsDoorOpen,
                    Position = new Vector3Data() { x = generator.transform.position.x, y = generator.transform.position.y, z = generator.transform.position.z }
                });
            }
        }
    }
}
