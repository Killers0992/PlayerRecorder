using Exiled.API.Features;
using MapGeneration.Distributors;
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
        public byte Flags { get; set; }
        public short SyncTime { get; set; }

        Scp079Generator _generator;
        public Scp079Generator generator
        {
            get
            {
                if (_generator == null)
                    _generator = GetComponent<Scp079Generator>();
                return _generator;
            }
        }         

        private void Update()
        {
            if (!MainClass.isRecording)
                return;
            if (Flags != generator.Network_flags)
            {
                Flags = generator.Network_flags;
                RecordCore.OnReceiveEvent(new GeneratorFlagsData()
                {
                    Flags = Flags,
                    Position = new Vector3Data() { x = generator.transform.position.x, y = generator.transform.position.y, z = generator.transform.position.z }
                });
            }
            if (SyncTime != generator.Network_syncTime)
            {
                SyncTime = generator.Network_syncTime;
                RecordCore.OnReceiveEvent(new GeneratorTimeData()
                {
                    Time = SyncTime,
                    Position = new Vector3Data() { x = generator.transform.position.x, y = generator.transform.position.y, z = generator.transform.position.z }
                });
            }
        }
    }
}
