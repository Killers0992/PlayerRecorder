using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Commands
{
    [CommandHandler(typeof(ReplayCommand))]
    public class PauseCommand : ICommand
    {
        public string Command { get; } = "pause";

        public string[] Aliases { get; } = new string[]
        {
            "pause"
        };

        public string Description { get; } = "Pause/Unpause replay.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("playerrecorder.pause"))
            {
                response = "No Permission";
                return false;
            }

            if (MainClass.isReplaying)
            {
                MainClass.isReplayPaused = !MainClass.isReplayPaused;
                if (MainClass.isReplayPaused)
                {
                    if (Warhead.IsInProgress)
                        MainClass.singleton.core2.lastDetonation = Warhead.DetonationTimer;
                }
                response = $"Replay {(MainClass.isReplayPaused ? "paused" : "unpaused")}.";
            }
            else
            {
                response = "Replay not prepared.";
                return false;
            }
            return true;
        }
    }
}
