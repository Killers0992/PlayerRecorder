using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder
{
    public class PluginConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        [Description("Speed of recording. Default ( 0.1 )")]
        public float recordDelay { get; set; } = 0.1f;
        [Description("Speed of replay. Default ( 0.1 )")]
        public float replayDelay { get; set; } = 0.1f;
        [Description("Compress files into 7z after x files. Default ( 5 )")]
        public int compressAfter { get; set; } = 5;
        [Description("Webhook url for discord ( send info about records )")]
        public string webhookUrl { get; set; } = "";
        [Description("Amount of seconds before report on which replay should start. Default ( 5 )")]
        public int secondsBeforeReport { get; set; } = 5;
        public bool debug { get; set; } = false;
    }
}
