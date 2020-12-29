using Exiled.API.Features;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder
{
    public class MainClass : Plugin<PluginConfig>
    {
        public override string Author { get; } = "Killers0992";
        public override string Name { get; } = "PlayerRecorder";
        public override string Prefix { get; } = "playerrecorder";

        private RecorderCore core;
        private EventHandlers eventHandlers;

        public static MainClass singleton;

        public override void OnEnabled()
        {
            singleton = this;
            core = new RecorderCore();
            eventHandlers = new EventHandlers(core);
            core.handler = eventHandlers;
            HarmonyLib.Harmony hrm = new HarmonyLib.Harmony("Patcher.recorder");
            hrm.PatchAll();
            base.OnEnabled();
        }
    }
}
