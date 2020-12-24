using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder
{
    public class PluginConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public float recordDelay { get; set; } = 0.1f;
        public float replayDelay { get; set; } = 0.1f;
    }
}
