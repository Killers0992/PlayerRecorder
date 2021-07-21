using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Commands
{
	[CommandHandler(typeof(ReplayCommand))]
	public class EndCommand : ICommand
	{
		public string Command { get; } = "end";

		public string[] Aliases { get; } = new string[]
		{
			"exit"
		};

		public string Description { get; } = "End replay.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender a)
            {
                Player player = Player.Get(a.ReferenceHub);
                if (!player.CheckPermission("playerrecorder.end"))
                {
                    response = "No Permission";
                    return true;
                }
            }

            if (MainClass.isReplaying)
            {
                MainClass.isReplaying = false;
                MainClass.isReplayReady = false;
                MainClass.SeedID = -1;
                foreach (var item in MainClass.replayPickups)
                {
                    NetworkServer.Destroy(item.Value.gameObject);
                }
                MainClass.replayPickups.Clear();
                foreach (var player2 in MainClass.replayPlayers)
                {
                    NetworkServer.Destroy(player2.Value.gameObject);
                }
                MainClass.replayPlayers.Clear();
                response = "Replay ended.";
            }
            else
            {
                response = "Replay not prepared.";
            }
            return true;
        }
    }
}
