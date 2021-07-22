using Exiled.API.Features;
using MEC;
using PlayerRecorder.Core.Record;
using PlayerRecorder.Core.Replay;
using PlayerRecorder.Interfaces;
using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.IO;

namespace PlayerRecorder
{
    public class MainClass : Plugin<PluginConfig>
    {
        public override string Author { get; } = "Killers0992";
        public override string Name { get; } = "PlayerRecorder";
        public override string Prefix { get; } = "playerrecorder";
        public override Version Version { get; } = new Version(1, 0, 1);
        public override Version RequiredExiledVersion { get; } = new Version(2,11,0);

        public RecordCore core;
        public ReplayCore core2;
        private EventHandlers eventHandlers;
        private ReplayHud hud;

        public static MainClass singleton;

        public static string pluginDir;

        private bool isLoaded = false;

        public override void OnEnabled()
        {
            if (isLoaded)
                return;
            pluginDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED", "Plugins", "PlayerRecorder");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);
            singleton = this;
            core = CustomNetworkManager.singleton.gameObject.AddComponent<RecordCore>();
            core2 = CustomNetworkManager.singleton.gameObject.AddComponent<ReplayCore>();
            eventHandlers = new EventHandlers(core, core2);
            hud = new ReplayHud();
            HarmonyLib.Harmony hrm = new HarmonyLib.Harmony($"playerrecorder.{DateTime.Now.Ticks}");
            hrm.PatchAll();
            isLoaded = true;
            base.OnEnabled();
        }
        public static CoroutineHandle replayHandler;

        public static int currentRoundID = 0;

        public static bool isRecording = false;
        public static bool isReplaying = false;

        public static bool isReplayEnded = false;

        public static bool isReplayReady = false;
        public static bool isReplayPaused = false;

        public static bool forceReplayStart = false;

        public static int bringSpectatorToTarget = -1;

        public static int LastFrame = 9999;
        public static int LastExecutedEvents = 0;

        public static int SeedID = -1;

        public static Dictionary<int, Dictionary<int, List<IEventType>>> recordFrames = new Dictionary<int, Dictionary<int, List<IEventType>>>();
        public static Dictionary<int, RecordPickup> recordPickups = new Dictionary<int, RecordPickup>();
        public static int framer;

        public static DateTime RoundTimestamp { get; set; } = DateTime.Now;
        
        public static Dictionary<int, ReplayPlayer> replayPlayers = new Dictionary<int, ReplayPlayer>();
        public static Dictionary<int, ReplayPickup> replayPickups = new Dictionary<int, ReplayPickup>();

        public static Dictionary<int, List<IEventType>> replayEvents = new Dictionary<int, List<IEventType>>();

    }
}
