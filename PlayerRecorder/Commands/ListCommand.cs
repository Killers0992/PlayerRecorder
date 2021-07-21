using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
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
    public class ListCommand : ICommand
    {
        public string Command { get; } = "list";

        public string[] Aliases { get; } = new string[]
        {
            "list"
        };

        public string Description { get; } = "Records list.";

        public Dictionary<string, DateTime> GetFiles()
        {
            var files = Directory.GetFiles(Path.Combine(MainClass.pluginDir, "RecorderData", Server.Port.ToString()));
            Dictionary<string, DateTime> items = new Dictionary<string, DateTime>();
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var exten = Path.GetExtension(file);
                switch (exten.ToUpper())
                {
                    case ".RD":
                        items.Add(fileName, new DateTime(long.Parse(fileName.Replace("Record_", ""))));
                        break;
                    case ".ZIP":
                        using (var zip = ZipArchive.Open(file))
                        {
                            foreach (var entry in zip.Entries)
                            {
                                if (entry.Key.EndsWith(".rd"))
                                {
                                    items.Add(entry.Key.Replace(".rd", ""), new DateTime(long.Parse(entry.Key.Replace("Record_", "").Replace(".rd", ""))));
                                }
                            }
                        }
                        break;
                }
            }
            return items;
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender a)
            {
                Player player = Player.Get(a.ReferenceHub);
                if (!player.CheckPermission("playerrecorder.list"))
                {
                    response = "No Permission";
                    return true;
                }
            }
            var files =GetFiles();
            response = "Records list:\n";
            foreach(var file in files)
            {
                response += " - " + file.Key + " | Time: " + file.Value.ToString() + "\n";
            }
            return true;
        }
    }
}
