using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using PlayerRecorder.Extensions;
using PlayerRecorder.Models;
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

        public List<RecordFile> GetFiles()
        {
            var files = new DirectoryInfo(Path.Combine(MainClass.pluginDir, "RecorderData", Server.Port.ToString())).GetFiles();
            List<RecordFile> items = new List<RecordFile>();
            foreach (var file in files)
            {
                switch (file.Extension.ToUpper())
                {
                    case ".RD":
                        items.Add(new RecordFile() { FullName = file.Name.Replace(".rd", ""), FileSize = file.Length, Time = new DateTime(long.Parse(file.Name.Replace(".rd", "").Replace("Record_", "")))});
                        break;
                    case ".ZIP":
                        using (var zip = ZipArchive.Open(file.FullName))
                        {
                            foreach (var entry in zip.Entries)
                            {
                                if (entry.Key.EndsWith(".rd"))
                                {
                                    items.Add(new RecordFile() { FullName = entry.Key.Replace(".rd", ""), FileSize = entry.Size, Time = new DateTime(long.Parse(entry.Key.Replace("Record_", "").Replace(".rd", ""))) });
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
                response += $" - {file.FullName} | Time: {file.Time.ToString()} | Size: {file.FileSize.BytesToString()}\n";
            }
            return true;
        }
    }
}
