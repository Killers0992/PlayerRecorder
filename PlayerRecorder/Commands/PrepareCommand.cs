using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Commands
{
    [CommandHandler(typeof(ReplayCommand))]
    public class PrepareCommand : ICommand
    {
        public string Command { get; } = "prepare";

        public string[] Aliases { get; } = new string[]
        {
            "prep"
        };

        public string Description { get; } = "Prepare replay.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender a)
            {
                Player player = Player.Get(a.ReferenceHub);
                if (!player.CheckPermission("playerrecorder.prepare"))
                {
                    response = "No Permission";
                    return true;
                }
            }
            if (arguments.Count == 2)
            {
                if (File.Exists(Path.Combine(MainClass.pluginDir, "RecorderData", arguments.At(0), "Record_" + arguments.At(1) + ".rd")))
                {
                    response = "Start prepare";
                    MainClass.replayHandler = Timing.RunCoroutine(MainClass.singleton.core2.Replay(Path.Combine(MainClass.pluginDir, "RecorderData", arguments.At(0), "Record_" + arguments.At(1) + ".rd")));
                }
                else
                {
                    response = "File not found.";
                }
            }
            else if (arguments.Count == 4)
            {
                if (File.Exists(Path.Combine(MainClass.pluginDir, "RecorderData", arguments.At(0), "Record_" + arguments.At(1) + ".rd")))
                {
                    response = "Start prepare";
                    MainClass.replayHandler = Timing.RunCoroutine(MainClass.singleton.core2.Replay(Path.Combine(MainClass.pluginDir, "RecorderData", arguments.At(0), "Record_" + arguments.At(1) + ".rd"), int.Parse(arguments.At(2)), int.Parse(arguments.At(3))));
                }
                else
                {
                    response = "File not found.";
                }
            }
            else
            {
                response = "Syntax: PREPARE <port> <recordId>";
            }
            return true;
        }
    }
}
