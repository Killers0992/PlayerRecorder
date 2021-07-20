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

        void Awake()
        {
            singleton = this;
            Timing.RunCoroutine(PickupsWatcher());
            Timing.RunCoroutine(FrameRecord());
        }

        public IEnumerator<float> PickupsWatcher()
        {
            while (true)
            {
                if (!MainClass.isRecording)
                    goto skipFor;
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
                skipFor:
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        public static IEnumerator<float> Process(int round)
        {
            if (MainClass.recordFrames.TryGetValue(round, out Dictionary<int, List<IEventType>> events))
            {
                var recordingStream = new FileStream(Path.Combine(MainClass.pluginDir, "RecorderData", $"{Server.Port}", $"Record_{MainClass.RoundTimestamp.Ticks}.rd"), FileMode.CreateNew);
                Serializer.Serialize(recordingStream, events);
                recordingStream.Flush();
                recordingStream.Close();
                MainClass.recordFrames.Remove(round);
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
