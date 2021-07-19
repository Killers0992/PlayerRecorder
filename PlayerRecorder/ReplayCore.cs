using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using Mirror;
using NPCS;
using PlayerRecorder.Structs;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder
{
    public class ReplayCore : MonoBehaviour
    {
        public static ReplayCore singleton;

        public static int currentRoundID = 0;

        public static bool isRecording = false;
        public static bool isReplaying = false;

        public static bool isReplayReady = false;
        public static bool isReplayPaused = false;

        public int SeedID = -1;

        public static event EventHandler<ReplayPlayer> RegisterReplayPlayer;
        public static event EventHandler<ReplayPlayer> UnRegisterReplayPlayer;

        public static event EventHandler<ReplayPickup> RegisterReplayPickup;
        public static event EventHandler<ReplayPickup> UnRegisterReplayPickup;

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

        void Start()
        {
            singleton = this;
        }

        public static int framer;

        void Update()
        {
            if (isRecording)
                framer++;
        }

        private RagdollManager _manager;

        public RagdollManager RagdollManager
        {
            get
            {
                if (_manager == null)
                    _manager = UnityEngine.Object.FindObjectsOfType<RagdollManager>()[0];
                return _manager;
            }
        }

        public IEnumerator<float> CreateFakePlayer(sbyte clientid, string name, string userId, RoleType RoleType)
        {
            var npc = Methods.CreateNPC(new Vector3(0f, 0f, 0f), Vector2.zero, Vector3.one, RoleType, ItemType.None, name);
            yield return Timing.WaitForSeconds(0.5f);
            var rplayer = npc.NPCPlayer.GameObject.AddComponent<ReplayPlayer>();
            rplayer.uniqueId = clientid;
            replayPlayers.Add(clientid, rplayer);
        }

        public static Dictionary<int, ReplayPlayer> replayPlayers = new Dictionary<int, ReplayPlayer>();
        public static Dictionary<int, ReplayPickup> replayPickups = new Dictionary<int, ReplayPickup>();


        public void LogData(string str, bool output = true)
        {
            if (MainClass.singleton.Config.debug)
                Log.Info(str);
        }

        public Dictionary<int, List<IEventType>> replayEvents = new Dictionary<int, List<IEventType>>();

        public IEnumerator<float> Replay(string path)
        {
            replayEvents.Clear();
            isRecording = false;
            isReplaying = false;
            using (var stream = new MemoryStream(File.ReadAllBytes(path)))
            {
                replayEvents = Serializer.Deserialize<Dictionary<int, List<IEventType>>>(stream);
            }
            isReplayReady = true;
            foreach (var seed in replayEvents)
                foreach (var seed2 in seed.Value.Where(p => p is SeedData))
                    RecorderCore.singleton.SeedID = (seed2 as SeedData).Seed;
            ReferenceHub.HostHub.playerStats.Roundrestart();
            int lastFrame = replayEvents.Last().Key;
            framer = 0;
            while (true)
            {
                if (isReplayPaused || (!isReplaying && isReplayReady))
                {
                    yield return Timing.WaitForOneFrame;
                    continue;
                }
                if (replayEvents.TryGetValue(framer, out List<IEventType> frames))
                {
                    string last = "";
                    try
                    {
                        foreach (var ev in frames)
                        {
                            last = ev.GetType().Name;
                            switch (ev)
                            {
                                case PlayerInfoData pinfo:
                                    LogData($"Player joined {pinfo.UserName} ({pinfo.UserID}) ({pinfo.PlayerID})");
                                    Timing.RunCoroutine(CreateFakePlayer(pinfo.PlayerID, pinfo.UserName, pinfo.UserID, RoleType.Spectator));
                                    continue;
                                case CreatePickupData cpickup:
                                    LogData($"Create pickup {(ItemType)cpickup.ItemType}");
                                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ReferenceHub.HostHub.inventory.pickupPrefab);
                                    NetworkServer.Spawn(gameObject);
                                    gameObject.GetComponent<Pickup>().SetupPickup((ItemType)cpickup.ItemType, -1f, ReferenceHub.HostHub.gameObject, new Pickup.WeaponModifiers(false, -1, -1, -1), cpickup.Position.vector, Quaternion.Euler(new Vector3(0, 0, 0)));
                                    var rpickup = gameObject.AddComponent<ReplayPickup>();
                                    rpickup.uniqueId = cpickup.ItemID;
                                    replayPickups.Add(cpickup.ItemID, rpickup);
                                    continue;
                                case UpdatePlayerData uplayer:
                                    if (!replayPlayers.ContainsKey(uplayer.PlayerID))
                                        continue;
                                    replayPlayers[uplayer.PlayerID].UpdatePlayer(uplayer);
                                    continue;
                                case UpdatePickupData upickup:
                                    if (!replayPickups.ContainsKey(upickup.ItemID))
                                        continue;
                                    replayPickups[upickup.ItemID].UpdatePickup(upickup);
                                    continue;
                                case LeaveData lplayer:
                                    LogData($"Player leave {lplayer.PlayerID}");
                                    if (!replayPlayers.ContainsKey(lplayer.PlayerID))
                                        continue;
                                    var rplayer = replayPlayers[lplayer.PlayerID];
                                    NetworkServer.Destroy(rplayer.gameObject);
                                    continue;
                                case RemovePickupData rppickup:
                                    LogData($"Pickup remove {rppickup.ItemID}");
                                    if (!replayPickups.ContainsKey(rppickup.ItemID))
                                        continue;
                                    var rrpickup = replayPickups[rppickup.ItemID];
                                    NetworkServer.Destroy(rrpickup.gameObject);
                                    continue;
                                case DoorData ddata:
                                    DoorVariant bestDoor = null;
                                    float bestDistance = 999f;
                                    foreach (var dor in UnityEngine.Object.FindObjectsOfType<DoorVariant>())
                                    {
                                        float distance = Vector3.Distance(dor.transform.position, ddata.Position.vector);
                                        if (distance < bestDistance)
                                        {
                                            bestDoor = dor;
                                            bestDistance = distance;
                                        }
                                    }
                                    if (bestDoor != null)
                                    {
                                        bestDoor.NetworkTargetState = ddata.State;
                                    }
                                    continue;
                                case UpdateRoleData urole:
                                    LogData($"Update role {(RoleType)urole.RoleID} for player {urole.PlayerID}");
                                    if (!replayPlayers.ContainsKey(urole.PlayerID))
                                        continue;
                                    replayPlayers[urole.PlayerID].UpdateRole(urole);
                                    continue;
                                case LiftData ulift:
                                    LogData($"Use lift");
                                    foreach (var lift2 in Map.Lifts)
                                        if (lift2.elevatorName == ulift.Elevatorname && lift2.status != Lift.Status.Moving)
                                            lift2.UseLift();
                                    continue;
                                case ShotWeaponData sweapon:
                                    LogData($"Shot weapon {sweapon.PlayerID}");
                                    if (!replayPlayers.ContainsKey(sweapon.PlayerID))
                                        continue;
                                    replayPlayers[sweapon.PlayerID].ShotWeapon();
                                    continue;
                                case ReloadWeaponData rweapon:
                                    LogData($"Reload weapon {rweapon.PlayerID}");
                                    if (!replayPlayers.ContainsKey(rweapon.PlayerID))
                                        continue;
                                    replayPlayers[rweapon.PlayerID].ReloadWeapon();
                                    continue;
                                case UpdateHoldingItem ehold:
                                    LogData($"Change holding item {ehold.PlayerID} {ehold.HoldingItem}");
                                    if (!replayPlayers.ContainsKey(ehold.PlayerID))
                                        continue;
                                    replayPlayers[ehold.PlayerID].UpdateHoldingItem(ehold);
                                    continue;
                                case CreateRagdollData ragdoll:
                                    RagdollManager.SpawnRagdoll(ragdoll.Position.vector, ragdoll.Rotation.quaternion, ragdoll.Velocity.vector, ragdoll.ClassID, new PlayerStats.HitInfo(0f, "", DamageTypes.FromIndex(ragdoll.ToolID), 0), false, ragdoll.OwnerID, ragdoll.OwnerNick, ragdoll.PlayerID);
                                    continue;
                                case GeneratorUpdateData genupdate:
                                    Generator079 bestGen = null;
                                    float bestDist = 999f;
                                    foreach (var gen in UnityEngine.Object.FindObjectsOfType<Generator079>())
                                    {
                                        float distance = Vector3.Distance(gen.transform.position, genupdate.Position.vector);
                                        if (distance < bestDist)
                                        {
                                            bestGen = gen;
                                            bestDist = distance;
                                        }
                                    }
                                    if (bestGen != null)
                                    {
                                        bestGen.NetworkisTabletConnected = genupdate.TabletConnected;
                                        bestGen.NetworktotalVoltage = genupdate.TotalVoltage;
                                    }
                                    continue;
                                case UnlockGeneratorData genunlock:
                                    Generator079 bestGen2 = null;
                                    float bestDist2 = 999f;
                                    foreach (var gen in UnityEngine.Object.FindObjectsOfType<Generator079>())
                                    {
                                        float distance = Vector3.Distance(gen.transform.position, genunlock.Position.vector);
                                        if (distance < bestDist2)
                                        {
                                            bestGen2 = gen;
                                            bestDist2 = distance;
                                        }
                                    }
                                    if (bestGen2 != null)
                                    {
                                        bestGen2.NetworkisDoorUnlocked = true;
                                    }      
                                    continue;
                                case OpenCloseGeneratorData genoc:
                                    Generator079 bestGen3 = null;
                                    float bestDist3 = 999f;
                                    foreach (var gen in UnityEngine.Object.FindObjectsOfType<Generator079>())
                                    {
                                        float distance = Vector3.Distance(gen.transform.position, genoc.Position.vector);
                                        if (distance < bestDist3)
                                        {
                                            bestGen3 = gen;
                                            bestDist3 = distance;
                                        }
                                    }
                                    if (bestGen3 != null)
                                    {
                                        bestGen3.NetworkisDoorOpen = genoc.IsOpen;
                                    }
                                    continue;
                                case Change914KnobData knobData:
                                    Scp914.Scp914Machine.singleton.NetworkknobState = (Scp914.Scp914Knob)knobData.KnobSetting;
                                    continue;
                                case WarheadUpdateData warheadData:
                                    AlphaWarheadController.Host.NetworktimeToDetonation = warheadData.TimeToDetonation;
                                    AlphaWarheadController.Host.NetworksyncResumeScenario = warheadData.ResumeScenario;
                                    AlphaWarheadController.Host.NetworksyncStartScenario = warheadData.StartScenario;
                                    AlphaWarheadController.Host.NetworkinProgress = warheadData.InProgress;
                                    continue;
                                case RoundEndData end:
                                    LogData($"Round ended");
                                    continue;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Info(e.ToString() + " failed on  " + last);
                    }
                }
                if (lastFrame == framer)
                    break;
                framer++;
                yield return Timing.WaitForOneFrame;
            }
            replayEvents.Clear();
            Log.Info("Replay ended");
            yield break;
        }
    }
}
