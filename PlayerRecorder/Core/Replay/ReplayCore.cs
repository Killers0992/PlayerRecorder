using Exiled.API.Features;
using Exiled.API.Features.Items;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using JesusQC_Npcs.Features;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using PlayerRecorder.Interfaces;
using PlayerRecorder.Structs;
using ProtoBuf;
using Scp914;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PlayerRecorder.Core.Replay
{
    public class ReplayCore : MonoBehaviour
    {
        public static ReplayCore singleton;

        private RagdollManager _manager;


        public RagdollManager RagdollManager
        {
            get
            {
                if (_manager == null)
                    _manager = ReferenceHub.HostHub.gameObject.GetComponent<RagdollManager>();
                return _manager;
            }
        }

        private Scp914Controller _scp914Contoller;

        public Scp914Controller Scp914Controller
        {
            get
            {
                if (_scp914Contoller == null)
                    _scp914Contoller = UnityEngine.Object.FindObjectOfType<Scp914Controller>();
                return _scp914Contoller;
            }
        }

        AlphaWarheadOutsitePanel _panel;
        public AlphaWarheadOutsitePanel panel
        {
            get
            {
                if (_panel == null)
                    _panel = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
                return _panel;
            }
        }

        void Start()
        {
            singleton = this;
        }

        public IEnumerator<float> CreateFakePlayer(sbyte clientid, string name, string userId)
        {
            if (MainClass.replayPlayers.ContainsKey(clientid))
                yield break;

            var npc = new Dummy(Vector3.zero, Vector3.one, RoleType.Spectator, name, true);
            
            while(npc?.PlayerWrapper == null) 
            {
                yield return Timing.WaitForOneFrame;
            }

            var rplayer = npc.PlayerWrapper.GameObject.AddComponent<ReplayPlayer>();
            rplayer._dummy = npc;
            rplayer.uniqueId = clientid;
            MainClass.replayPlayers.Add(clientid, rplayer);
        }

        public Role GetRoleByName(string name)
        {
            var roles = ReferenceHub.HostHub.characterClassManager.Classes;
            foreach(var role in roles)
            {
                if (role.fullName == name)
                    return role;
            }
            return null;
        }


        public Scp079Generator GetBestGen(Vector3 position)
        {
            if (ReplayCache.GeneratorCache.TryGetValue(position, out Scp079Generator gen))
                return gen;

            Scp079Generator bestGen = null;
            float bestDistance = 999f;
            foreach (var gen2 in UnityEngine.Object.FindObjectsOfType<Scp079Generator>())
            {
                float distance = Vector3.Distance(gen2.transform.position, position);
                if (distance < bestDistance)
                {
                    bestGen = gen2;
                    bestDistance = distance;
                }
            }

            ReplayCache.GeneratorCache.Add(position, bestGen);
            return bestGen;
        }

        public DoorVariant GetBestDoor(Vector3 position)
        {
            if (ReplayCache.DoorCache.TryGetValue(position, out DoorVariant door))
                return door;

            DoorVariant bestDoor = null;
            float bestDistance = 999f;
            foreach (var dor in UnityEngine.Object.FindObjectsOfType<DoorVariant>())
            {
                float distance = Vector3.Distance(dor.transform.position, position);
                if (distance < bestDistance)
                {
                    bestDoor = dor;
                    bestDistance = distance;
                }
            }

            ReplayCache.DoorCache.Add(position, bestDoor);
            return bestDoor;
        }

        public Lift GetElevatorByName(string elevatorName)
        {
            if (ReplayCache.ElevatorCache.TryGetValue(elevatorName, out Lift elevator))
                return elevator;

            foreach (var lift in UnityEngine.Object.FindObjectsOfType<Lift>())
            {
                if (lift.elevatorName == elevatorName)
                {
                    ReplayCache.ElevatorCache.Add(elevatorName, lift);
                    return lift;
                }
            }

            return null;
        }

        private ItemPickupBase SpawnItem(ItemType type, Vector3 pos, Quaternion rotation)
        {
            if (!InventorySystem.InventoryItemLoader.AvailableItems.TryGetValue(type, out ItemBase itemBase))
                return null;

            InventorySystem.Items.Pickups.ItemPickupBase itemPickupBase = UnityEngine.Object.Instantiate<InventorySystem.Items.Pickups.ItemPickupBase>(itemBase.PickupDropModel, pos, rotation);
            itemPickupBase.Rb.isKinematic = true;
            NetworkServer.Spawn(itemPickupBase.gameObject);

            itemPickupBase.InfoReceived(
                default(InventorySystem.Items.Pickups.PickupSyncInfo),
                new InventorySystem.Items.Pickups.PickupSyncInfo()
                {
                    ItemId = type,
                    Serial = InventorySystem.Items.ItemSerialGenerator.GenerateNext(),
                    Weight = itemBase.Weight
                });
            return itemPickupBase;
        }

        public float lastDetonation = 0f;

        public IEnumerator<float> Replay(byte[] bytes, int forwardToFrame = -1, int targetUser = -1)
        {
            MainClass.replayEvents.Clear();
            MainClass.isRecording = false;
            MainClass.isReplaying = false;
            MainClass.isReplayEnded = false;
            MainClass.forceReplayStart = forwardToFrame != -1;
            MainClass.bringSpectatorToTarget = targetUser;
            using (var stream = new MemoryStream(bytes))
            {
                MainClass.replayEvents = Serializer.Deserialize<Dictionary<int, List<IEventType>>>(stream);
            }
            foreach (var seed in MainClass.replayEvents)
                foreach (var seed2 in seed.Value.Where(p => p is SeedData))
                    MainClass.SeedID = (seed2 as SeedData).Seed;
            ReferenceHub.HostHub.playerStats.Roundrestart();
            MainClass.framer = 0;
            MainClass.LastFrame = MainClass.replayEvents.Last().Key;
            MainClass.isReplayReady = true;
            while (true)
            {
                if (MainClass.isReplayPaused || (!MainClass.isReplaying && MainClass.isReplayReady) || !MainClass.isReplayReady)
                {
                    if (Warhead.IsInProgress)
                        Warhead.DetonationTimer = lastDetonation;
                    goto skipFor;
                }

                if (MainClass.replayEvents.TryGetValue(MainClass.framer, out List<IEventType> frames))
                {

                    MainClass.LastExecutedEvents = frames.Count;
                    foreach (var ev in frames)
                    {
                        try
                        {
                            switch (ev)
                            {
                                case PlayerInfoData pinfo:
                                    Timing.RunCoroutine(CreateFakePlayer(pinfo.PlayerID, pinfo.UserName, pinfo.UserID));
                                    continue;
                                case CreatePickupData cpickup:
                                    Log.Debug($"Create pickup {cpickup.ItemID}", MainClass.singleton.Config.debug);
                                    var p = SpawnItem((ItemType)cpickup.ItemType, cpickup.Position.vector, cpickup.Rotation.quaternion);
                                    var rpickup = p.gameObject.AddComponent<ReplayPickup>();

                                    rpickup.uniqueId = cpickup.ItemID;
                                    MainClass.replayPickups.Add(cpickup.ItemID, rpickup);
                                    continue;
                                case UpdatePlayerData uplayer:
                                    if (MainClass.replayPlayers.TryGetValue(uplayer.PlayerID, out ReplayPlayer updatePlayer))
                                        updatePlayer.UpdatePlayer(uplayer);
                                    continue;
                                case UpdatePickupData upickup:
                                    if (MainClass.replayPickups.TryGetValue(upickup.ItemID, out ReplayPickup updatePickup))
                                        updatePickup.UpdatePickup(upickup);
                                    continue;
                                case LeaveData lplayer:
                                    if (MainClass.replayPlayers.TryGetValue(lplayer.PlayerID, out ReplayPlayer removePlayer))
                                        NetworkServer.Destroy(removePlayer.gameObject);
                                    continue;
                                case RemovePickupData rppickup:
                                    if (MainClass.replayPickups.TryGetValue(rppickup.ItemID, out ReplayPickup removePickup))
                                        NetworkServer.Destroy(removePickup.gameObject);
                                    continue;
                                case DoorData ddata:
                                    DoorVariant door = GetBestDoor(ddata.Position.vector);
                                    if (door.NetworkTargetState != ddata.State)
                                        door.NetworkTargetState = ddata.State;

                                    if (door.NetworkActiveLocks != ddata.ActiveLocks)
                                        door.NetworkActiveLocks = ddata.ActiveLocks;
                                    continue;
                                case DoorDestroyData dddata:
                                    DoorVariant breakableDoor = GetBestDoor(dddata.Position.vector);
                                    if (breakableDoor is BreakableDoor brdoor)
                                    {
                                        if (!brdoor.Network_destroyed)
                                            brdoor.Network_destroyed = true;
                                    }
                                    continue;
                                case UpdateRoleData urole:
                                    if (MainClass.replayPlayers.TryGetValue(urole.PlayerID, out ReplayPlayer updatePlayerRole))
                                        updatePlayerRole.UpdateRole(urole);
                                    continue;
                                case LiftData ulift:
                                    Lift lift = GetElevatorByName(ulift.Elevatorname);
                                    if (lift.NetworkstatusID != ulift.StatusID)
                                        lift.NetworkstatusID = ulift.StatusID;

                                    if (lift.Network_locked != ulift.IsLocked)
                                        lift.Network_locked = ulift.IsLocked;
                                    continue;
                                case ShotWeaponData sweapon:
                                    if (MainClass.replayPlayers.TryGetValue(sweapon.PlayerID, out ReplayPlayer playerShot))
                                        playerShot.ShotWeapon();
                                    continue;
                                case ReloadWeaponData rweapon:
                                    if (MainClass.replayPlayers.TryGetValue(rweapon.PlayerID, out ReplayPlayer playerReload))
                                        playerReload.ReloadWeapon();
                                    continue;
                                case UpdateHoldingItem ehold:
                                    if (MainClass.replayPlayers.TryGetValue(ehold.PlayerID, out ReplayPlayer playerUpdateHold))
                                        playerUpdateHold.UpdateHoldingItem(ehold);
                                    continue;
                                case CreateRagdollData ragdoll:
                                    RagdollManager.SpawnRagdoll(ragdoll.Position.vector, ragdoll.Rotation.quaternion, new Vector3(0f, 0f, 0f), ragdoll.ClassID, new PlayerStats.HitInfo(0f, "", DamageTypes.FromIndex(ragdoll.ToolID), 0, true), false, ragdoll.OwnerID, ragdoll.OwnerNick, ragdoll.PlayerID);
                                    continue;
                                case GeneratorFlagsData genflags:
                                    Scp079Generator generator = GetBestGen(genflags.Position.vector);
                                    generator.Network_flags = genflags.Flags;
                                    continue;
                                case GeneratorTimeData gentime:
                                    Scp079Generator genDoor = GetBestGen(gentime.Position.vector);
                                    genDoor.Network_syncTime = gentime.Time;  
                                    continue;
                                case Change914KnobData knobData:
                                    Scp914Controller.Network_knobSetting = (Scp914KnobSetting)knobData.KnobSetting;
                                    continue;
                                case WarheadUpdateData warheadData:
                                    AlphaWarheadController.Host.NetworktimeToDetonation = warheadData.TimeToDetonation;
                                    AlphaWarheadController.Host.NetworksyncResumeScenario = warheadData.ResumeScenario;
                                    AlphaWarheadController.Host.NetworksyncStartScenario = warheadData.StartScenario;
                                    AlphaWarheadController.Host.NetworkinProgress = warheadData.InProgress;
                                    continue;
                                case ScpTerminationData scpterm:
                                    NineTailedFoxAnnouncer.AnnounceScpTermination(GetRoleByName(scpterm.RoleFullName), new PlayerStats.HitInfo(0f, "", DamageTypes.FromIndex(scpterm.ToolID), 0, true), scpterm.GroupID);
                                    continue;
                                case PlaceDecalData placeDecal:
                                    //WeaponManager.RpcPlaceDecal(placeDecal.IsBlood, placeDecal.Type, placeDecal.Position.vector, placeDecal.Rotation.quaternion);
                                    continue;
                                case NukeOutsideKeycardEnteredData keycardData:
                                    panel.NetworkkeycardEntered = keycardData.IsEntered;
                                    continue;
                                case NukesiteSwitchData switchData:
                                    AlphaWarheadOutsitePanel.nukeside.Networkenabled = switchData.IsEnabled;
                                    continue;
                                case RoundEndData end:
                                    MainClass.isReplayEnded = true;
                                    continue;
                            }
                        }
                        catch(Exception ex)
                        {
                            Log.Error($"Error while parsing frame {ev.ToString()} {ex}");
                        }
                        
                    }
                }
                if (MainClass.LastFrame < MainClass.framer)
                {
                    MainClass.isReplaying = false;
                    MainClass.isReplayEnded = true;
                    foreach (var item in MainClass.replayPickups)
                    {
                        NetworkServer.Destroy(item.Value.gameObject);
                    }
                    MainClass.replayPickups.Clear();
                    foreach (var player in MainClass.replayPlayers)
                    {
                        NetworkServer.Destroy(player.Value.gameObject);
                    }
                    MainClass.replayPlayers.Clear();
                    break;
                }

                if (forwardToFrame == MainClass.framer || (forwardToFrame < 0 && MainClass.forceReplayStart))
                {
                    MainClass.isReplayPaused = true;
                    MainClass.forceReplayStart = false;
                    forwardToFrame = -1;
                    IEnumerable<Player> players = Player.List.Where(p => !Dummy.Dictionary.ContainsKey(p.GameObject));
                    if (MainClass.replayPlayers.TryGetValue(MainClass.bringSpectatorToTarget, out ReplayPlayer plr))
                    {
                        if (plr.hub.characterClassManager.IsAlive)
                        {
                            foreach (var player in players)
                            {
                                player.Position = plr.hub.transform.position;
                            }
                        }
                    }
                    goto skipFor;
                }
                MainClass.framer++;
                skipFor:
                yield return Timing.WaitForSeconds(forwardToFrame != -1 ? 0.01f : MainClass.singleton.Config.replayDelay);
            }
            yield break;
        }
    }
}
