﻿using Exiled.API.Features;
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
                RecorderCore.OnReceiveEvent(new GeneratorUpdateData()
                {
                    TabletConnected = TabletConnected,
                    TotalVoltage = generator.NetworktotalVoltage,
                    Position = new Vector3Data() { x = generator.transform.position.x, y = generator.transform.position.y, z = generator.transform.position.z }
                });
            }
        }
    }
}