using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using RemoteAdmin;
using SharpCompress.Archives.Zip;
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

        public byte[] GetFileByID(string path, string id)
        {
            var files = Directory.GetFiles(path);
            foreach(var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var exten = Path.GetExtension(file);
                switch (exten.ToUpper())
                {
                    case ".RD":
                        fileName = fileName.Replace("Record_", "");
                        if (fileName == id)
                        {
                            return File.ReadAllBytes(file);
                        }
                        break;
                    case ".ZIP":
                        using (var zip = ZipArchive.Open(file))
                        {
                            foreach(var entry in zip.Entries)
                            {
                                fileName = entry.Key.Replace("Record_", "").Replace(".rd","");
                                if (fileName == id)
                                {
                                    using(var mem = new MemoryStream())
                                    {
                                        entry.OpenEntryStream().CopyTo(mem);
                                        return mem.ToArray();
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            return new byte[0];
        }

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
                byte[] bytes = GetFileByID(Path.Combine(MainClass.pluginDir, "RecorderData", arguments.At(0)), arguments.At(1));
                if (bytes.Length != 0)
                {
                    response = "Start prepare";
                    MainClass.replayHandler = Timing.RunCoroutine(MainClass.singleton.core2.Replay(bytes));
                }
                else
                {
                    response = "File not found.";
                }
            }
            else if (arguments.Count == 4)
            {
                byte[] bytes = GetFileByID(Path.Combine(MainClass.pluginDir, "RecorderData", arguments.At(0)), arguments.At(1));

                if (bytes.Length != 0)
                {
                    response = "Start prepare";
                    MainClass.replayHandler = Timing.RunCoroutine(MainClass.singleton.core2.Replay(bytes, int.Parse(arguments.At(2)), int.Parse(arguments.At(3))));
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
