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

        public static event EventHandler<object> ReceiveEvent;

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

        public static void OnReceiveEvent(object eventObject)
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

        private void EventReceived(object sender, object ev)
        {
            try
            {
                MessagePack.MessagePackSerializer.Serialize<DelayData>(recordingStream, new DelayData() { DelayTime = timeElapsed });
                timeElapsed = 0;
                if (ev is PlayerInfoData data)
                {
                    MessagePack.MessagePackSerializer.Serialize<PlayerInfoData>(recordingStream, data);
                }
                else if (ev is LeaveData data1)
                {
                    MessagePack.MessagePackSerializer.Serialize<LeaveData>(recordingStream, data1);
                }
                else if (ev is UpdateRoleData data2)
                {
                    MessagePack.MessagePackSerializer.Serialize<UpdateRoleData>(recordingStream, data2);
                }
                else if (ev is DoorData data3)
                {
                    MessagePack.MessagePackSerializer.Serialize<DoorData>(recordingStream, data3);
                }
                else if (ev is LiftData data4)
                {
                    MessagePack.MessagePackSerializer.Serialize<LiftData>(recordingStream, data4);
                }
                else if (ev is CreatePickupData data5)
                {
                    MessagePack.MessagePackSerializer.Serialize<CreatePickupData>(recordingStream, data5);
                }
                else if (ev is ReloadWeaponData data6)
                {
                    MessagePack.MessagePackSerializer.Serialize<ReloadWeaponData>(recordingStream, data6);
                }
                else if (ev is ShotWeaponData data7)
                {
                    MessagePack.MessagePackSerializer.Serialize<ShotWeaponData>(recordingStream, data7);
                }
                else if (ev is RemovePickupData data8)
                {
                    MessagePack.MessagePackSerializer.Serialize<RemovePickupData>(recordingStream, data8);
                }
                else if (ev is RoundEndData data9)
                {
                    MessagePack.MessagePackSerializer.Serialize<RoundEndData>(recordingStream, data9);
                }
                else if (ev is SeedData data10)
                {
                    MessagePack.MessagePackSerializer.Serialize<SeedData>(recordingStream, data10);
                }
                else if (ev is UpdatePickupData data11)
                {
                    MessagePack.MessagePackSerializer.Serialize<UpdatePickupData>(recordingStream, data11);
                }
                else if (ev is UpdatePlayerData data12)
                {
                    MessagePack.MessagePackSerializer.Serialize<UpdatePlayerData>(recordingStream, data12);
                }
                else if (ev is DelayData data123)
                {
                    MessagePack.MessagePackSerializer.Serialize<DelayData>(recordingStream, data123);
                }
                recordingStream.Flush();
            }catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }
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
                obj.GetComponent<QueryProcessor>().NetworkPlayerId = QueryProcessor._idIterator++;
                obj.GetComponent<QueryProcessor>()._ipAddress = "127.0.0.WAN";
                obj.transform.position = new Vector3(0f, 0f, 0f);
                NetworkServer.Spawn(obj);
                Player ply_obj = new Player(obj);
                Player.Dictionary.Add(obj, ply_obj);
                Player.IdsCache.Add(obj.GetComponent<QueryProcessor>().NetworkPlayerId, ply_obj);
                var rplayer = obj.AddComponent<ReplayPlayer>();
                rplayer.uniqueId = clientid;
                replayPlayers.Add(clientid, rplayer);

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public bool endInstance = false;

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

        public IEnumerator<float> Replay(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {

                while (true)
                {
                    scam:
                    if (isReplayPaused || (!isReplaying && isReplayReady))
                    {
                        yield return Timing.WaitForOneFrame;
                        Log.Info("Waiting");
                        goto scam;
                    }
                    var oldPos = stream.Position;
                    object[] data = null;
                    try
                    {
                        data = (MessagePack.MessagePackSerializer.Deserialize<object[]>(stream));
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    switch ((RecordEvents)data[0])
                    {
                        case RecordEvents.ReceiveSeed:
                            stream.Position = oldPos;
                            var seed = MessagePack.MessagePackSerializer.Deserialize<SeedData>(stream);
                            RecorderCore.singleton.SeedID = seed.Seed;
                            isReplayReady = true;
                            ReferenceHub.HostHub.playerStats.Roundrestart();
                            Log.Info($"Received seed, {seed.Seed}");
                            break;
                        case RecordEvents.PlayerInfo:
                            stream.Position = oldPos;
                            var pinfo = MessagePack.MessagePackSerializer.Deserialize<PlayerInfoData>(stream);
                            Log.Info($"Create player");
                            CreateFakePlayer(pinfo.PlayerID, pinfo.UserName, pinfo.UserID, RoleType.Spectator);
                            break;
                        case RecordEvents.CreatePickup:
                            stream.Position = oldPos;
                            var cpickup = MessagePack.MessagePackSerializer.Deserialize<CreatePickupData>(stream);
                            Log.Info($"Create pickup");
                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ReferenceHub.HostHub.inventory.pickupPrefab);
                            NetworkServer.Spawn(gameObject);
                            gameObject.GetComponent<Pickup>().SetupPickup((ItemType)cpickup.ItemType, -1f, ReferenceHub.HostHub.gameObject, new Pickup.WeaponModifiers(false, -1, -1, -1), cpickup.Position.SetVector(), Quaternion.Euler(new Vector3(0, 0, 0)));
                            var rpickup = gameObject.AddComponent<ReplayPickup>();
                            rpickup.uniqueId = cpickup.ItemID;
                            replayPickups.Add(cpickup.ItemID, rpickup);
                            break;
                        case RecordEvents.UpdatePlayer:
                            stream.Position = oldPos;
                            var uplayer = MessagePack.MessagePackSerializer.Deserialize<UpdatePlayerData>(stream);
                            if (!replayPlayers.ContainsKey(uplayer.PlayerID))
                                break;
                            Log.Info($"Update player");
                            replayPlayers[uplayer.PlayerID].UpdatePlayer(uplayer);
                            break;
                        case RecordEvents.UpdatePickup:
                            stream.Position = oldPos;
                            var upickup = MessagePack.MessagePackSerializer.Deserialize<UpdatePickupData>(stream);
                            if (!replayPickups.ContainsKey(upickup.ItemID))
                                break;
                            Log.Info($"Update pickup");
                            replayPickups[upickup.ItemID].UpdatePickup(upickup);
                            break;
                        case RecordEvents.PlayerLeave:
                            stream.Position = oldPos;
                            var lplayer = MessagePack.MessagePackSerializer.Deserialize<LeaveData>(stream);
                            if (!replayPlayers.ContainsKey(lplayer.PlayerID))
                                break;
                            Log.Info($"Remove player");
                            var rplayer = replayPlayers[lplayer.PlayerID];
                            NetworkServer.Destroy(rplayer.gameObject);
                            break;
                        case RecordEvents.RemovePickup:
                            stream.Position = oldPos;
                            var rppickup = MessagePack.MessagePackSerializer.Deserialize<RemovePickupData>(stream);
                            if (!replayPickups.ContainsKey(rppickup.ItemID))
                                break;
                            Log.Info($"Remove pickup");
                            var rrpickup = replayPickups[rppickup.ItemID];
                            NetworkServer.Destroy(rrpickup.gameObject);
                            break;
                        case RecordEvents.DoorState:
                            stream.Position = oldPos;
                            var ddata = MessagePack.MessagePackSerializer.Deserialize<DoorData>(stream);
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
                            Log.Info($"Use door");
                            break;
                        case RecordEvents.UpdateRole:
                            stream.Position = oldPos;
                            var urole = MessagePack.MessagePackSerializer.Deserialize<UpdateRoleData>(stream);
                            if (!replayPlayers.ContainsKey(urole.PlayerID))
                                break;
                            Log.Info($"Update role");
                            replayPlayers[urole.PlayerID].UpdateRole(urole);
                            break;
                        case RecordEvents.UseLift:
                            stream.Position = oldPos;
                            var ulift = MessagePack.MessagePackSerializer.Deserialize<LiftData>(stream);
                            foreach (var lift2 in Map.Lifts)
                                if (lift2.elevatorName == ulift.Elevatorname)
                                    lift2.UseLift();
                            Log.Info($"Use lift");
                            break;
                        case RecordEvents.ShotWeapon:
                            stream.Position = oldPos;
                            var sweapon = MessagePack.MessagePackSerializer.Deserialize<ShotWeaponData>(stream);
                            if (!replayPlayers.ContainsKey(sweapon.PlayerID))
                                break;
                            Log.Info($"Shot weapon");
                            replayPlayers[sweapon.PlayerID].ShotWeapon();
                            break;
                        case RecordEvents.ReloadWeapon:
                            stream.Position = oldPos;
                            var rweapon = MessagePack.MessagePackSerializer.Deserialize<ReloadWeaponData>(stream);
                            if (!replayPlayers.ContainsKey(rweapon.PlayerID))
                                break;
                            Log.Info($"Reload weapon");
                            replayPlayers[rweapon.PlayerID].ReloadWeapon();
                            break;
                        case RecordEvents.Delay:
                            stream.Position = oldPos;
                            var delay = MessagePack.MessagePackSerializer.Deserialize<DelayData>(stream);
                            Log.Info($"Waiting {delay.DelayTime}");
                            yield return Timing.WaitForSeconds(delay.DelayTime);
                            break;
                        case RecordEvents.RoundEnd:
                            Map.Broadcast(10, "PlayerRecord | ROUND ENDED");
                            break;
                    }
                }
            }
            Log.Info("Replay ended");
            yield break;
        }
    }
}
