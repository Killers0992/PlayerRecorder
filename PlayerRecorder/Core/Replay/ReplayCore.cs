using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using Mirror;
using NPCS;
using PlayerRecorder.Interfaces;
using PlayerRecorder.Structs;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private WeaponManager _wmanager;

        public WeaponManager WeaponManager
        {
            get
            {
                if (_wmanager == null)
                    _wmanager = ReferenceHub.HostHub.weaponManager;
                return _wmanager;
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
            Npc npc = Methods.CreateNPC(new Vector3(0f, 0f, 0f), Vector2.zero, Vector3.one, RoleType.Spectator, ItemType.None, name);
            while(npc?.NPCPlayer == null) 
            {
                yield return Timing.WaitForOneFrame;
            }

            npc.NPCPlayer.IsGodModeEnabled = true;
            var rplayer = npc.NPCPlayer.ReferenceHub.gameObject.AddComponent<ReplayPlayer>();
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

        public Generator079 GetBestGenerator(Vector3 position)
        {
            if (ReplayCache.GeneratorCache.TryGetValue(position, out Generator079 generator))
                return generator;

            Generator079 bestGen = null;
            float bestDist = 999f;
            foreach (var gen in UnityEngine.Object.FindObjectsOfType<Generator079>())
            {
                float distance = Vector3.Distance(gen.transform.position, position);
                if (distance < bestDist)
                {
                    bestGen = gen;
                    bestDist = distance;
                }
            }

            ReplayCache.GeneratorCache.Add(position, bestGen);
            return bestGen;
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
                    goto skipFor;
                }

                if (MainClass.replayEvents.TryGetValue(MainClass.framer, out List<IEventType> frames))
                {

                    MainClass.LastExecutedEvents = frames.Count;
                    foreach (var ev in frames)
                    {
                        switch (ev)
                        {
                            case PlayerInfoData pinfo:
                                var cor = Timing.RunCoroutine(CreateFakePlayer(pinfo.PlayerID, pinfo.UserName, pinfo.UserID));
                                if (cor == null)
                                    continue;
                                while (cor.IsRunning)
                                {
                                    yield return Timing.WaitForOneFrame;
                                }
                                continue;
                            case CreatePickupData cpickup:
                                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ReferenceHub.HostHub.inventory.pickupPrefab);
                                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                                NetworkServer.Spawn(gameObject);
                                gameObject.GetComponent<Pickup>().SetupPickup((ItemType)cpickup.ItemType, -1f, ReferenceHub.HostHub.gameObject, new Pickup.WeaponModifiers(false, -1, -1, -1), cpickup.Position.vector, Quaternion.Euler(new Vector3(0, 0, 0)));
                                var rpickup = gameObject.AddComponent<ReplayPickup>();
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
                                RagdollManager.SpawnRagdoll(ragdoll.Position.vector, ragdoll.Rotation.quaternion, new Vector3(0f,0f,0f), ragdoll.ClassID, new PlayerStats.HitInfo(0f, "", DamageTypes.FromIndex(ragdoll.ToolID), 0), false, ragdoll.OwnerID, ragdoll.OwnerNick, ragdoll.PlayerID);
                                continue;
                            case GeneratorUpdateData genupdate:
                                Generator079 generator = GetBestGenerator(genupdate.Position.vector);
                                generator.NetworkisTabletConnected = genupdate.TabletConnected;
                                generator.NetworktotalVoltage = genupdate.TotalVoltage;
                                generator.NetworkremainingPowerup = genupdate.RemainingPowerup;
                                continue;
                            case UnlockGeneratorData genunlock:
                                Generator079 genDoor = GetBestGenerator(genunlock.Position.vector);
                                genDoor.NetworkisDoorUnlocked = true;
                                continue;
                            case OpenCloseGeneratorData genoc:
                                Generator079 genDoor2 = GetBestGenerator(genoc.Position.vector);
                                genDoor2.NetworkisDoorOpen = genoc.IsOpen;
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
                            case ScpTerminationData scpterm:
                                NineTailedFoxAnnouncer.AnnounceScpTermination(GetRoleByName(scpterm.RoleFullName), new PlayerStats.HitInfo(0f, "", DamageTypes.FromIndex(scpterm.ToolID), 0), scpterm.GroupID);
                                continue;
                            case PlaceDecalData placeDecal:
                                WeaponManager.RpcPlaceDecal(placeDecal.IsBlood, placeDecal.Type, placeDecal.Position.vector, placeDecal.Rotation.quaternion);
                                continue;
                            case RoundEndData end:
                                MainClass.isReplayEnded = true;
                                continue;
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
                    IEnumerable<Player> players = Player.List.Where(p => !p.IsNPC());
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
