using Exiled.API.Features;
using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder
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

        void Awake()
        {
            Log.Info($"Warhead record init.");
        }

        private void Update()
        {
            if (InProgress != controller.inProgress)
            {
                InProgress = controller.inProgress;
                RecorderCore.OnReceiveEvent(new WarheadUpdateData()
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
