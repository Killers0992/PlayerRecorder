using Exiled.API.Features;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Evs = Exiled.Events;
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
            foreach (MethodBase bas in Evs.Events.Instance.Harmony.GetPatchedMethods())
            {
                if (bas.Name.Equals("TransmitData"))
                {
                    Exiled.Events.Events.DisabledPatchesHashSet.Add(bas);
                }
            }
            Evs.Events.Instance.ReloadDisabledPatches();
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
