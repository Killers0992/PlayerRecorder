using Exiled.API.Features;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static string pluginDir;

        public override void OnEnabled()
        {
            pluginDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED", "Plugins", "PlayerRecorder");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);
            singleton = this;
            core = CustomNetworkManager.singleton.gameObject.AddComponent<RecorderCore>();
            eventHandlers = new EventHandlers(core);
            core.handler = eventHandlers;
            HarmonyLib.Harmony hrm = new HarmonyLib.Harmony("Patcher.recorder");
            hrm.PatchAll();
            base.OnEnabled();
        }
    }
}
