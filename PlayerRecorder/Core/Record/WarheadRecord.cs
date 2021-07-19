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
    public class WarheadRecord : MonoBehaviour
    {
        public bool InProgress { get; set; } = false;

        AlphaWarheadController _controller;
        public AlphaWarheadController controller
        {
            get
            {
                if (_controller == null)
                    _controller = GetComponent<AlphaWarheadController>();
                return _controller;
            }
        }

        private void Update()
        {
            if (InProgress != controller.inProgress)
            {
                InProgress = controller.inProgress;
                RecordCore.OnReceiveEvent(new WarheadUpdateData()
                {
                    InProgress = controller.inProgress,
                    TimeToDetonation = controller.timeToDetonation,
                    ResumeScenario = controller.NetworksyncResumeScenario,
                    StartScenario = controller.NetworksyncStartScenario
                });
            }
        }
    }
}
