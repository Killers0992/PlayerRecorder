using Exiled.API.Features;
using Interactables.Interobjects;
using MapGeneration;
using MEC;
using PlayerRecorder.Interfaces;
using PlayerRecorder.Structs;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PlayerRecorder.Core.Record
{
    public class RecordCore : MonoBehaviour
    {
        public static RecordCore singleton;

        public static void OnReceiveEvent(IEventType eventObject)
        {
            if (!MainClass.curDir.ContainsKey(MainClass.currentRoundID))
                MainClass.curDir.Add(MainClass.currentRoundID, new Dictionary<int, List<IEventType>>());
            if (MainClass.curDir[MainClass.currentRoundID].ContainsKey(MainClass.framer))
            {
                MainClass.curDir[MainClass.currentRoundID][MainClass.framer].Add(eventObject);
            }
            else
            {
                MainClass.curDir[MainClass.currentRoundID].Add(MainClass.framer, new List<IEventType>() { eventObject });
            }
        }

        void Start()
        {
            DontDestroyOnLoad(this);
            singleton = this;
            Timing.RunCoroutine(PickupsWatcher());
        }

        public IEnumerator<float> PickupsWatcher()
        {
            while (true)
            {
                if (!MainClass.isRecording)
                {
                    yield return Timing.WaitForSeconds(0.1f);
                    continue;
                }
                try
                {
                    foreach (var item in UnityEngine.Object.FindObjectsOfType<Pickup>())
                    {
                        if (!item.TryGetComponent<RecordPickup>(out RecordPickup _))
                        {
                            item.gameObject.AddComponent<RecordPickup>();
                        }
                    }
                }
                catch (Exception) { }
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        public static IEnumerator<float> Process(int round)
        {
            Log.Info("Save frames from round " + round);
            if (MainClass.curDir.TryGetValue(round, out Dictionary<int, List<IEventType>> events))
            {
                Log.Info("Process started");
                var recordingStream = new FileStream(Path.Combine(MainClass.pluginDir, "RecorderData", $"{Server.Port}", $"Record_{DateTime.Now.Ticks}.rd"), FileMode.CreateNew);
                Serializer.Serialize(recordingStream, events);
                recordingStream.Flush();
                recordingStream.Close();
                Log.Info("Process ended, saved " + events.Count + " frames.");
                MainClass.curDir.Remove(round);
            }
            else
            {
                Log.Info("Frames not found in round " + round);
            }
            yield break;
        }

        void Update()
        {
            if (MainClass.isRecording)
                MainClass.framer++;
        }



        public void StartRecording()
        {
            OnReceiveEvent(new SeedData()
            {
                Seed = UnityEngine.Object.FindObjectsOfType<SeedSynchronizer>()[0].Network_syncSeed
            });
        }
    }
}
