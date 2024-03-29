﻿using CommandSystem;
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
    public class StartCommand : ICommand
    {
        public string Command { get; } = "start";

        public string[] Aliases { get; } = new string[]
        {
            "stop"
        };

        public string Description { get; } = "Start replay.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("playerrecorder.start"))
            {
                response = "No Permission";
                return false;
            }

            if (MainClass.isReplayReady && !MainClass.isReplaying)
            {
                MainClass.isReplaying = true;
                response = "Replay started.";
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
