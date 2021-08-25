using Exiled.API.Features;
using Exiled.API.Features.Items;
using MapGeneration;
using MEC;
using PlayerRecorder.Interfaces;
using PlayerRecorder.Structs;
using ProtoBuf;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Utf8Json;

namespace PlayerRecorder.Core.Record
{
    public class RecordCore : MonoBehaviour
    {
        public static RecordCore singleton;

        public static void OnReceiveEvent(IEventType eventObject)
        {
            if (!MainClass.recordFrames.ContainsKey(MainClass.currentRoundID))
                MainClass.recordFrames.Add(MainClass.currentRoundID, new Dictionary<int, List<IEventType>>());
            if (MainClass.recordFrames[MainClass.currentRoundID].ContainsKey(MainClass.framer))
            {
                MainClass.recordFrames[MainClass.currentRoundID][MainClass.framer].Add(eventObject);
            }
            else
            {
                MainClass.recordFrames[MainClass.currentRoundID].Add(MainClass.framer, new List<IEventType>() { eventObject });
            }
        }

        private CoroutineHandle pickupsCoroutine, frameCoroutine;

        void Awake()
        {
            singleton = this;
            pickupsCoroutine = Timing.RunCoroutine(PickupsWatcher());
            frameCoroutine = Timing.RunCoroutine(FrameRecord());
        }

        private void OnDestroy()
        {
            if (pickupsCoroutine != null)
                Timing.KillCoroutines(pickupsCoroutine);
            if (frameCoroutine != null)
                Timing.KillCoroutines(frameCoroutine);
        }

        public IEnumerator<float> PickupsWatcher()
        {
            while (true)
            {
                if (!MainClass.isRecording)
                    continue;
                try
                {
                    foreach (var item in Map.Pickups)
                    {
                        if (!item.Base.gameObject.TryGetComponent<RecordPickup>(out _))
                        {
                            item.Base.gameObject.AddComponent<RecordPickup>();
                        }
                    }
                }
                catch (Exception) { }
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        public static TimeSpan GetTimeFromFrames(int frames)
        {
            return TimeSpan.FromSeconds(frames * MainClass.singleton.Config.replayDelay);
        }


        public static IEnumerator<float> Process(int round, DateTime timeStamp)
        {
            if (MainClass.recordFrames.TryGetValue(round, out Dictionary<int, List<IEventType>> events))
            {
                var recordingStream = new FileStream(Path.Combine(MainClass.pluginDir, "RecorderData", $"{Server.Port}", $"Record_{timeStamp.Ticks}.rd"), FileMode.CreateNew);

                Serializer.Serialize(recordingStream, events);
                recordingStream.Flush();
                recordingStream.Close();
                if (!string.IsNullOrEmpty(MainClass.singleton.Config.webhookUrl))
                {
                    TimeSpan recordLength = GetTimeFromFrames(events.Last().Key);
                    HttpQuery.Post(MainClass.singleton.Config.webhookUrl, "payload_json=" + JsonSerializer.ToJsonString<DiscordWebhook>(new DiscordWebhook(string.Empty, "Player Recorder", "https://cdn.discordapp.com/attachments/742563439918055510/867318607826386954/recording-icon-15.png", false, new DiscordEmbed[]
                    {
                        new DiscordEmbed("Round record", "rich", $"New record on server ``{Server.IpAddress}:{Server.Port}``", CheaterReport.WebhookColor, new DiscordEmbedField[]
                        {
                            new DiscordEmbedField("Info", string.Concat($"Length ``{(recordLength.TotalMinutes == 0 ? "" : $"{recordLength.TotalMinutes.ToString("F0")} minutes ")}{recordLength.Seconds.ToString("F0")} seconds``"), false),
                            new DiscordEmbedField("Command", $"||replay prepare {Server.Port} {timeStamp.Ticks}||", false)
                        })
                    })));
                }

                MainClass.recordFrames.Remove(round);
                if (MainClass.singleton.Config.compressAfter != -1)
                {
                    var files = Directory.GetFiles(Path.Combine(MainClass.pluginDir, "RecorderData", $"{Server.Port}"), "*.rd");
                    if (files.Length < MainClass.singleton.Config.compressAfter)
                        yield break;
                    string firstFile = Path.GetFileNameWithoutExtension(files.First()).Replace("Record_", "");
                    string lastFile = Path.GetFileNameWithoutExtension(files.Last()).Replace("Record_", "");
                    using (var archive = ZipArchive.Create())
                    {
                        archive.AddAllFromDirectory(Path.Combine(MainClass.pluginDir, "RecorderData", $"{Server.Port}"), "*.rd");
                        archive.SaveTo(Path.Combine(MainClass.pluginDir, "RecorderData", $"{Server.Port}", $"RecordCompress_{firstFile}-{lastFile}.zip"), CompressionType.Deflate);
                    }
                    foreach (var file in files)
                        File.Delete(file);
                    Log.Info($"Compressed {files.Length} records into zip file.");
                }
            }
            yield break;
        }



        public IEnumerator<float> FrameRecord()
        {
            while (true)
            {
                if (MainClass.isRecording)
                    MainClass.framer++;
                yield return Timing.WaitForSeconds(MainClass.singleton.Config.recordDelay);
            }
        }

        public void StartRecording()
        {
            OnReceiveEvent(new SeedData()
            {
                Seed = SeedSynchronizer._singleton.Network_syncSeed
            });
        }
    }
}
