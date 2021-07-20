using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ReplayCommand : ParentCommand
    {
		public ReplayCommand() => LoadGeneratedCommands();

		public override string Command { get; } = "replay";

		public override string[] Aliases { get; } = new string[]
		{
			"rep"
		};

		public override string Description { get; } = "Controls the LCZ decontamination";

		public static ReplayCommand Create()
		{
			ReplayCommand cmd = new ReplayCommand();
			cmd.LoadGeneratedCommands();
			return cmd;
		}

		protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			response = string.Concat(
				"Replay Commands:",
				Environment.NewLine,
				"- REPLAY end - End replay",
				Environment.NewLine,
				"- REPLAY pause - Pause/unpause replay",
				Environment.NewLine,
				"- REPLAY prepare <port> <id> - Prepare replay",
				Environment.NewLine,
				"- REPLAY setspeed <speed> - Set replay speed (default: 0.1)",
				Environment.NewLine,
				"- REPLAY start - Start replay");
			return false;
		}

		public override void LoadGeneratedCommands()
		{
			this.RegisterCommand(new EndCommand());
			this.RegisterCommand(new PauseCommand());
			this.RegisterCommand(new StartCommand());
			this.RegisterCommand(new PrepareCommand());
			this.RegisterCommand(new SetSpeedCommand());
		}
	}
}
