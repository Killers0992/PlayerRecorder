using CommandSystem;
using Mirror;
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
                foreach (var player in MainClass.replayPlayers)
                {
                    NetworkServer.Destroy(player.Value.gameObject);
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
