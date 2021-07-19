using Exiled.API.Features;
using PlayerRecorder.Core.Record;
using PlayerRecorder.Core.Replay;
using PlayerRecorder.Structs;
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
        private ReplayCore core2;
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
            core2 = CustomNetworkManager.singleton.gameObject.AddComponent<ReplayCore>();
            eventHandlers = new EventHandlers(core, core2);
            HarmonyLib.Harmony hrm = new HarmonyLib.Harmony("Patcher.recorder");
            hrm.PatchAll();
            base.OnEnabled();
        }


        public static int currentRoundID = 0;

        public static bool isRecording = false;
        public static bool isReplaying = false;

        public static bool isReplayReady = false;
        public static bool isReplayPaused = false;

        public static int SeedID = -1;

        public static Dictionary<int, Dictionary<int, List<IEventType>>> curDir = new Dictionary<int, Dictionary<int, List<IEventType>>>();
        public static Dictionary<int, RecordPickup> recordPickups = new Dictionary<int, RecordPickup>();
        public static int framer;

        public static Dictionary<int, ReplayPlayer> replayPlayers = new Dictionary<int, ReplayPlayer>();
        public static Dictionary<int, ReplayPickup> replayPickups = new Dictionary<int, ReplayPickup>();

        public static Dictionary<int, List<IEventType>> replayEvents = new Dictionary<int, List<IEventType>>();

    }
}
