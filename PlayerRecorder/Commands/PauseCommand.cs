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
            "unpause"
        };

        public string Description { get; } = "Pause/Unpause replay.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get((sender as PlayerCommandSender).ReferenceHub);
            if (!player.CheckPermission("playerrecorder.pause"))
            {
                response = "No Permission";
                return true;
            }
            if (MainClass.isReplaying)
            {
                MainClass.isReplayPaused = !MainClass.isReplayPaused;
                response = $"Replay {(MainClass.isReplayPaused ? "paused" : "started")}.";
            }
            else
            {
                response = "Replay not prepared.";
            }
            return true;
        }
    }
}
