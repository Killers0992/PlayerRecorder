using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PlayerRecorder.Enums;
using PlayerRecorder.Structs;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder
{
    public class RecorderCore : MonoBehaviour
    {
        public static RecorderCore singleton;

        public static bool isRecording = false;
        public static bool isReplaying = false;

        public static bool isReplayReady = false;
        public static bool isReplayPaused = false;

        public int SeedID = -1;

        public static event EventHandler<RecordPlayer> RegisterRecordPlayer;
        public static event EventHandler<RecordPlayer> UnRegisterRecordPlayer;

        public static event EventHandler<RecordPickup> RegisterRecordPickup;
        public static event EventHandler<RecordPickup> UnRegisterRecordPickup;

        public static event EventHandler<ReplayPlayer> RegisterReplayPlayer;
        public static event EventHandler<ReplayPlayer> UnRegisterReplayPlayer;

        public static event EventHandler<ReplayPickup> RegisterReplayPickup;
        public static event EventHandler<ReplayPickup> UnRegisterReplayPickup;

        public static event EventHandler<IEventType> ReceiveEvent;

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

        public static void OnRegisterReplayPlayer(ReplayPlayer replayplayer)
        {
            RegisterReplayPlayer.Invoke(null, replayplayer);
        }

        public static void OnUnRegisterReplayPlayer(ReplayPlayer replayplayer)
        {
            UnRegisterReplayPlayer.Invoke(null, replayplayer);
        }

        public static void OnRegisterReplayPickup(ReplayPickup replaypickup)
        {
            RegisterReplayPickup.Invoke(null, replaypickup);
        }

        public static void OnUnRegisterReplayPickup(ReplayPickup replaypickup)
        {
            UnRegisterReplayPickup.Invoke(null, replaypickup);
        }

        public static void OnReceiveEvent(IEventType eventObject)
        {
            ReceiveEvent.Invoke(null, eventObject);
        }

        public EventHandlers handler;

        public float timeElapsed = 0f;

        void Start()
        {
            singleton = this;
            ReceiveEvent += EventReceived;
        }

        private void EventReceived(object sender, IEventType ev)
        {
            if (isRecording && !EventHandlers.waitingforplayers)
            {
                MessagePack.MessagePackSerializer.Serialize<IEventType>(recordingStream, new DelayData() { DelayTime = timeElapsed });
                timeElapsed = 0;
            }
            MessagePack.MessagePackSerializer.Serialize<IEventType>(recordingStream, ev);
            recordingStream.Flush();
        }

        void Update()
        {
            if (isRecording && !EventHandlers.waitingforplayers)
                timeElapsed += Time.deltaTime;
        }

        public void CreateFakePlayer(sbyte clientid, string name, string userId, RoleType RoleType)
        {
            try
            {
                GameObject obj =
                    UnityEngine.Object.Instantiate(
                        NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
                CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
                if (ccm == null)
                    return;
                ccm.CurClass = RoleType;
                obj.GetComponent<NicknameSync>().Network_myNickSync = string.IsNullOrEmpty(name) ? "[REC] Unknown name" :  $"[REC] {name}";
                var qp = obj.GetComponent<QueryProcessor>();
                qp.NetworkPlayerId = QueryProcessor._idIterator++;
                qp._ipAddress = "127.0.0.WAN";
                obj.transform.position = new Vector3(0f, 0f, 0f);
                NetworkServer.Spawn(obj);
                PlayerManager.AddPlayer(obj);
                var rplayer = obj.AddComponent<ReplayPlayer>();
                rplayer.uniqueId = clientid;
                replayPlayers.Add(clientid, rplayer);

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public FileStream recordingStream;

        public static Dictionary<int, ReplayPlayer> replayPlayers = new Dictionary<int, ReplayPlayer>();
        public static Dictionary<int, ReplayPickup> replayPickups = new Dictionary<int, ReplayPickup>();

        public static Dictionary<int, RecordPickup> recordPickups = new Dictionary<int, RecordPickup>();

        public void StartRecording()
        {   
            recordingStream = new FileStream(Path.Combine(MainClass.pluginDir, "RecorderData", $"{Server.Port}", $"Record_{DateTime.Now.Ticks}.rd"), FileMode.CreateNew);
            OnReceiveEvent(new SeedData()
            {
                Seed = UnityEngine.Object.FindObjectsOfType<SeedSynchronizer>()[0].Network_syncSeed
            });
        }

        public void LogData(string str, bool output = true)
        {
            if (MainClass.singleton.Config.debug)
                Log.Info(str);
            /*var list = File.ReadAllLines(Path.Combine(MainClass.pluginDir, output ? "debug_out.txt" : "debug_in.txt")).ToList();
            list.Add(str);
            File.WriteAllLines(Path.Combine(MainClass.pluginDir, output ? "debug_out.txt" : "debug_in.txt"), list);*/
        }

        public bool currentStatus = false;

        public IEnumerator<float> Replay(string path)
        {
            isReplaying = false;
            using (var stream = new FileStream(path, FileMode.Open))
            {

                while (true)
                {
                    if (isReplayPaused || (!isReplaying && isReplayReady))
                    {
                        yield return Timing.WaitForOneFrame;
                        continue;
                    }
                    IEventType data = null;
                    try
                    {
                        data = MessagePack.MessagePackSerializer.Deserialize<IEventType>(stream);
                    }
                    catch (Exception ex)
                    {
                        Log.Info(ex.ToString());
                        break;
                    }
                    try {
                        switch (data)
                        {
                            case DelayData delay:
                                //yield return Timing.WaitForSeconds(delay.DelayTime);
                                break;
                            case SeedData seed:
                                LogData($"Received seed {seed.Seed}");
                                RecorderCore.singleton.SeedID = seed.Seed;
                                isReplayReady = true;
                                ReferenceHub.HostHub.playerStats.Roundrestart();
                                break;
                            case PlayerInfoData pinfo:
                                LogData($"Player joined {pinfo.UserName} ({pinfo.UserID}) ({pinfo.PlayerID})");
                                CreateFakePlayer(pinfo.PlayerID, pinfo.UserName, pinfo.UserID, RoleType.Spectator);
                                break;
                            case CreatePickupData cpickup:
                                LogData($"Create pickup {(ItemType)cpickup.ItemType}");
                                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ReferenceHub.HostHub.inventory.pickupPrefab);
                                NetworkServer.Spawn(gameObject);
                                gameObject.GetComponent<Pickup>().SetupPickup((ItemType)cpickup.ItemType, -1f, ReferenceHub.HostHub.gameObject, new Pickup.WeaponModifiers(false, -1, -1, -1), cpickup.Position.SetVector(), Quaternion.Euler(new Vector3(0, 0, 0)));
                                var rpickup = gameObject.AddComponent<ReplayPickup>();
                                rpickup.uniqueId = cpickup.ItemID;
                                replayPickups.Add(cpickup.ItemID, rpickup);
                                break;
                            case UpdatePlayerData uplayer:
                                LogData($"Update player {uplayer.PlayerID}");
                                if (!replayPlayers.ContainsKey(uplayer.PlayerID))
                                    break;
                                replayPlayers[uplayer.PlayerID].UpdatePlayer(uplayer);
                                break;
                            case UpdatePickupData upickup:
                                LogData($"Update pickup {upickup.ItemID}");
                                if (!replayPickups.ContainsKey(upickup.ItemID))
                                    break;
                                replayPickups[upickup.ItemID].UpdatePickup(upickup);
                                break;
                            case LeaveData lplayer:
                                LogData($"Player leave {lplayer.PlayerID}");
                                if (!replayPlayers.ContainsKey(lplayer.PlayerID))
                                    break;
                                var rplayer = replayPlayers[lplayer.PlayerID];
                                NetworkServer.Destroy(rplayer.gameObject);
                                break;
                            case RemovePickupData rppickup:
                                LogData($"Pickup remove {rppickup.ItemID}");
                                if (!replayPickups.ContainsKey(rppickup.ItemID))
                                    break;
                                var rrpickup = replayPickups[rppickup.ItemID];
                                NetworkServer.Destroy(rrpickup.gameObject);
                                break;
                            case DoorData ddata:
                                LogData($"Open door");
                                var doorpos = ddata.Position.SetVector();

                                DoorVariant bestDoor = null;
                                float bestDistance = 999f;
                                foreach (var dor in Map.Doors)
                                {
                                    float distance = Vector3.Distance(dor.transform.position, doorpos);
                                    if (distance < bestDistance)
                                    {
                                        bestDoor = dor;
                                        bestDistance = distance;
                                    }
                                }
                                if (bestDoor != null)
                                {
                                    bestDoor.TargetState = ddata.State;
                                }
                                break;
                            case UpdateRoleData urole:
                                LogData($"Update role {(RoleType)urole.RoleID} for player {urole.PlayerID}");
                                if (!replayPlayers.ContainsKey(urole.PlayerID))
                                    break;
                                replayPlayers[urole.PlayerID].UpdateRole(urole);
                                break;
                            case LiftData ulift:
                                LogData($"Use lift");
                                foreach (var lift2 in Map.Lifts)
                                    if (lift2.elevatorName == ulift.Elevatorname)
                                        lift2.UseLift();
                                break;
                            case ShotWeaponData sweapon:
                                LogData($"Shot weapon {sweapon.PlayerID}");
                                if (!replayPlayers.ContainsKey(sweapon.PlayerID))
                                    break;
                                replayPlayers[sweapon.PlayerID].ShotWeapon();
                                break;
                            case ReloadWeaponData rweapon:
                                LogData($"Reload weapon {rweapon.PlayerID}");
                                if (!replayPlayers.ContainsKey(rweapon.PlayerID))
                                    break;
                                replayPlayers[rweapon.PlayerID].ReloadWeapon();
                                break;
                            case RoundEndData end:
                                LogData($"Round ended");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Info(ex.ToString());
                    }
                }
            }
            Log.Info("Replay ended");
            yield break;
        }
    }
}
