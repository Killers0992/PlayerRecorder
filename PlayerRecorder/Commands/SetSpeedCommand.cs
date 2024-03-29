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
    public class SetSpeedCommand : ICommand
    {
        public string Command { get; } = "setspeed";

        public string[] Aliases { get; } = new string[]
        {
            "ss"
        };

        public string Description { get; } = "Set replay speed.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("playerrecorder.setspeed"))
            {
                response = "No Permission";
                return false;
            }

            if (arguments.Count == 1)
            {
                if (float.TryParse(arguments.At(0), out float speed))
                {
                    MainClass.singleton.Config.replayDelay = speed;
                    response = $"Set replay speed to {speed}";
                    return true;
                }
            }
            response = $"Failed while setting speed.";
            return false;
        }
    }
}
