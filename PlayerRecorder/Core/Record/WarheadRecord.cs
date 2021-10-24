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

        public bool CardEntered { get; set; } = false;

        public bool NukeEnabled { get; set; } = false;

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

        AlphaWarheadOutsitePanel _panel;
        public AlphaWarheadOutsitePanel panel
        {
            get
            {
                if (_panel == null)
                    _panel = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
                return _panel;
            }
        }

        private void Update()
        {
            if (!MainClass.isRecording)
                return;

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

            if (CardEntered != panel.NetworkkeycardEntered)
            {
                CardEntered = panel.NetworkkeycardEntered;
                RecordCore.OnReceiveEvent(new NukeOutsideKeycardEnteredData()
                {
                    IsEntered = CardEntered
                });
            }

            if (NukeEnabled != AlphaWarheadOutsitePanel.nukeside.Networkenabled)
            {
                NukeEnabled = AlphaWarheadOutsitePanel.nukeside.Networkenabled;
                RecordCore.OnReceiveEvent(new NukesiteSwitchData()
                {
                    IsEnabled = NukeEnabled
                });
            }
        }
    }
}
