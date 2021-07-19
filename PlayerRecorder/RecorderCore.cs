using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using NPCS;
using PlayerRecorder.Enums;
using PlayerRecorder.Structs;
using ProtoBuf;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder
{
    public class RecorderCore : MonoBehaviour
    {
        public static RecorderCore singleton;

        public static int currentRoundID = 0;

        public static bool isRecording = false;
        public static bool isReplaying = false;

        public static bool isReplayReady = false;
        public static bool isReplayPaused = false;

        public int SeedID = -1;

        public static event EventHandler<RecordPlayer> RegisterRecordPlayer;
        public static event EventHandler<RecordPlayer> UnRegisterRecordPlayer;

        public static event EventHandler<RecordPickup> RegisterRecordPickup;
        public static event EventHandler<RecordPickup> UnRegisterRecordPickup;

        public static void OnRegisterRecordPlayer(RecordPlayer recordplayer)
        {
            RegisterRecordPlayer.Invoke(null, recordplayer);
        }

        public static void OnUnRegisterRecordPlayer(RecordPlayer recordplayer)
        {
            UnRegisterRecordPlayer.Invoke(null, recordplayer);
        }

        public static void OnRegisterRecordPickup(RecordPickup recordpickup)
        {
            RegisterRecordPickup.Invoke(null, recordpickup);
        }

        public static void OnUnRegisterRecordPickup(RecordPickup recordpickup)
        {
            UnRegisterRecordPickup.Invoke(null, recordpickup);
        }

        public static void OnReceiveEvent(IEventType eventObject)
        {
            if (!curDir.ContainsKey(currentRoundID))
                curDir.Add(currentRoundID, new Dictionary<int, List<IEventType>>());
            if (curDir.ContainsKey(framer))
            {
                curDir[currentRoundID][framer].Add(eventObject);
            }
            else
            {
                curDir[currentRoundID].Add(framer, new List<IEventType>() { eventObject });
            }
        }

        public EventHandlers handler;

        void Awake()
        {
            singleton = this;
            Timing.RunCoroutine(PickupsWatcher());
        }

        public IEnumerator<float> PickupsWatcher()
        {
            while (true)
            {
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

        public IEnumerator<float> Process()
        {
            int round = currentRoundID;
            currentRoundID++;
            if (curDir.TryGetValue(round, out Dictionary<int, List<IEventType>> events))
            {
                Log.Info("Process started");
                var recordingStream = new FileStream(Path.Combine(MainClass.pluginDir, "RecorderData", $"{Server.Port}", $"Record_{DateTime.Now.Ticks}.rd"), FileMode.CreateNew);
                Serializer.Serialize(recordingStream, events);
                recordingStream.Flush();
                recordingStream.Close();
                Log.Info("Process ended, saved " + events.Count + " frames.");
                curDir.Remove(round);
            }
            yield break;
        }

        public static Dictionary<int, Dictionary<int, List<IEventType>>> curDir = new Dictionary<int, Dictionary<int, List<IEventType>>>();


        public static int framer;

        void Update()
        {
            if (isRecording)
                framer++;
        }

        public static Dictionary<int, RecordPickup> recordPickups = new Dictionary<int, RecordPickup>();


        public void StartRecording()
        {
            OnReceiveEvent(new SeedData()
            {
                Seed = UnityEngine.Object.FindObjectsOfType<SeedSynchronizer>()[0].Network_syncSeed
            });
        }
    }
}
