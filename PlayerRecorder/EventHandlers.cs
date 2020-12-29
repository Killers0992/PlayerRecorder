using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using MessagePack;
using Mirror;
using PlayerRecorder.Enums;
using PlayerRecorder.Structs;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder
{
    public class EventHandlers
    {
        private RecorderCore core;
        public bool firstrun = false;
        public EventHandlers(RecorderCore core)
        {
            this.core = core;
            Exiled.Events.Handlers.Server.WaitingForPlayers += WaitingForPlayers;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            Exiled.Events.Handlers.Player.ItemDropped += Player_ItemDropped;
            Exiled.Events.Handlers.Player.PickingUpItem += Player_PickingUpItem;
            Exiled.Events.Handlers.Server.RestartingRound += Server_RestartingRound1;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += Server_SendingRemoteAdminCommand;
            Exiled.Events.Handlers.Player.InteractingDoor += Player_InteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator += Player_InteractingElevator;
            Exiled.Events.Handlers.Player.Died += Player_Died;
            Exiled.Events.Handlers.Player.Shot += Player_Shot;
            Exiled.Events.Handlers.Player.ReloadingWeapon += Player_ReloadingWeapon;
            Exiled.Events.Handlers.Server.RoundEnded += Server_RoundEnded;
            Exiled.Events.Handlers.Player.Joined += Player_Joined;
            Exiled.Events.Handlers.Player.Left += Player_Left;
            firstrun = true;
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            core.dataQueue.Enqueue(new LeaveData()
            {
                PlayerID = (sbyte)ev.Player.Id,
            });
        }

        private void Player_Joined(Exiled.Events.EventArgs.JoinedEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            core.dataQueue.Enqueue(new PlayerInfoData() 
            {
                PlayerID = (sbyte)ev.Player.Id,
                UserID = ev.Player.UserId,
                UserName = ev.Player.Nickname
            });
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            core.endInstance = true;
            if (!core.recorderRunning)
                return;
            core.dataQueue.Enqueue(new RoundEndData());
        }

        private void Player_ReloadingWeapon(Exiled.Events.EventArgs.ReloadingWeaponEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            core.dataQueue.Enqueue(new ReloadWeaponData()
            {
                PlayerID = (sbyte)ev.Player.Id
            });
        }

        private void Player_Shot(Exiled.Events.EventArgs.ShotEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            core.dataQueue.Enqueue(new ShotWeaponData()
            {
                PlayerID = (sbyte)ev.Shooter.Id,
            });
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            core.dataQueue.Enqueue(new UpdateRoleData()
            {
                PlayerID = (sbyte)ev.Target.Id,
                RoleID = (sbyte)RoleType.Spectator
            });
        }

        private void Player_InteractingElevator(Exiled.Events.EventArgs.InteractingElevatorEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            if (ev.Status == Lift.Status.Moving)
                return;
            core.dataQueue.Enqueue(new LiftData()
            {
                Elevatorname = ev.Lift.elevatorName
            });
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            if (ev.Door.moving.moving)
                return;
            core.dataQueue.Enqueue(new DoorData()
            {
                State = !ev.Door.NetworkisOpen,
                Position = ev.Door.transform.position.GetData(),
            });
        }
        public CoroutineHandle replayHandler;
        private void Server_SendingRemoteAdminCommand(Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs ev)
        {
            switch(ev.Name.ToUpper())
            {
                case "REPLAY":
                    ev.IsAllowed = false;
                    /*if (!ev.Sender.CheckPermission("playerrecorder.replay"))
                    {
                        ev.Sender.RemoteAdminMessage("No permission...", true, "PlayerRecorder");
                        break;
                    }*/
                    if (ev.Arguments.Count == 0)
                    {
                        ev.Sender.RemoteAdminMessage(string.Concat("Commands:",
                            Environment.NewLine,
                            "- replay prepare <port> <recordId>",
                            Environment.NewLine,
                            "- replay start",
                            Environment.NewLine,
                            "- replay pause",
                            Environment.NewLine,
                            "- replay setspeed <value> ( default is: 0.1 )"), true, "PlayerRecorder");
                        return;
                    }
                    switch(ev.Arguments[0].ToUpper())
                    {
                        case "PREPARE":
                            if (ev.Arguments.Count == 3)
                            {
                                if (File.Exists("./RecorderData/" + ev.Arguments[1] + "/Record_" + ev.Arguments[2] + ".rd"))
                                {
                                    Log.Info("Start prepare");
                                    replayHandler = Timing.RunCoroutine(core.Replay("RecorderData/" + ev.Arguments[1] + "/Record_" + ev.Arguments[2] + ".rd"));
                                }
                                else
                                {
                                    ev.Sender.RemoteAdminMessage("File not found.", true, "PlayerRecorder");
                                }
                            }
                            else
                            {
                                ev.Sender.RemoteAdminMessage("Syntax: PREPARE <port> <recordId>", true, "PlayerRecorder");
                            }
                            break;
                        case "START":
                            if (core.replayRunning)
                            {
                                core.replayStarted = true;
                                ev.Sender.RemoteAdminMessage("Replay started.", true, "PlayerRecorder");
                            }
                            else
                            {
                                ev.Sender.RemoteAdminMessage("Replay not prepared.", true, "PlayerRecorder");
                            }
                            break;
                        case "PAUSE":
                            if (core.replayRunning)
                            {
                                core.replayPaused = !core.replayPaused;
                                ev.Sender.RemoteAdminMessage($"Replay {(core.replayPaused ? "paused" : "started")}.", true, "PlayerRecorder");
                            }
                            else
                            {
                                ev.Sender.RemoteAdminMessage("Replay not prepared.", true, "PlayerRecorder");
                            }
                            break;
                        case "SETSPEED":
                            if (ev.Arguments.Count != 2)
                            {
                                ev.Sender.RemoteAdminMessage("Syntax: PAUSE <value>", true, "PlayerRecorder");
                                return;
                            }
                            if (core.replayRunning)
                            {
                                if (float.TryParse(ev.Arguments[1], out float speed))
                                {
                                    MainClass.singleton.Config.replayDelay = speed;
                                    ev.Sender.RemoteAdminMessage($"Replay speed set to {speed}.", true, "PlayerRecorder");
                                }
                                else
                                {
                                    ev.Sender.RemoteAdminMessage("Syntax: PAUSE <value>", true, "PlayerRecorder");
                                }
                            }
                            else
                            {
                                ev.Sender.RemoteAdminMessage("Replay not prepared.", true, "PlayerRecorder");
                            }
                            break;
                        case "END":
                            if (core.replayRunning)
                            {
                                core.replayRunning = false;
                                core.replayStarted = false;
                                core.SeedID = -1;
                                foreach (var dummy in core.playerDb)
                                {
                                    var id = Player.Get(dummy.Value).Id;
                                    Player.Dictionary.Remove(dummy.Value);
                                    Player.IdsCache.Remove(id);
                                    PlayerManager.RemovePlayer(dummy.Value);
                                    NetworkServer.Destroy(dummy.Value);
                                    core.playerDb.Remove(dummy.Key);
                                    NetworkServer.Destroy(dummy.Value);
                                }
                                foreach (var dummyItem in core.itemData)
                                {
                                    NetworkServer.Destroy(dummyItem.Value.gameObject);
                                }
                                core.playerDb = new Dictionary<string, GameObject>();
                                core.itemData = new Dictionary<int, Pickup>();
                                ev.Sender.RemoteAdminMessage("Replay ended.", true, "PlayerRecorder");

                            }
                            else
                            {
                                ev.Sender.RemoteAdminMessage("Replay not prepared.", true, "PlayerRecorder");
                            }
                            break;
                    }
                    break;
            }
        }

        public bool waitingforplayers = false;
        private void Server_RestartingRound1()
        {
            waitingforplayers = false;
            core.recorderRunning = false;
            core.endInstance = false;
            if (record != null)
                Timing.KillCoroutines(record);
            if (replayHandler != null)
                Timing.KillCoroutines(replayHandler);
            if (!core.replayRunning)
                record = Timing.RunCoroutine(core.RecorderInstance());
            if (!Directory.Exists("RecorderData/" + Server.Port))
                Directory.CreateDirectory("RecorderData/" + Server.Port);
            core.playerData = new List<string>();
            core.itemsData = new Dictionary<Pickup, int>();
            core.itemData = new Dictionary<int, Pickup>();
            core.playerDb = new Dictionary<string, GameObject>();
            if (core.replayRunning)
                return;
            core.recorderRunning = true;
            Log.Info("New recorder instance created.");
        }

        private void Player_PickingUpItem(Exiled.Events.EventArgs.PickingUpItemEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            if (ev.Pickup == null)
                return;
            if (!core.itemsData.ContainsKey(ev.Pickup))
                return;
            int id = core.itemsData[ev.Pickup];
            if (!core.savedItemPos.ContainsKey(id))
                return;
            core.savedItemPos.Remove(id);
            core.itemsData.Remove(ev.Pickup);
            core.dataQueue.Enqueue(new CreatePickupData()
            {
                ItemID = id,
                ItemType = (int)ev.Pickup.ItemId,
                Position = ev.Pickup.position.GetData(),
                Rotation = ev.Pickup.rotation.GetData()
            });
        }

        private void Player_ItemDropped(Exiled.Events.EventArgs.ItemDroppedEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            int generatedID = -1;
            for (int i = 1; i < int.MaxValue; i++)
            {
                if (core.itemsData.Values.Any(p => p == i))
                    continue;

                generatedID = i;
                break;
            }
            if (core.itemsData.ContainsKey(ev.Pickup))
                return;
            core.itemsData.Add(ev.Pickup, generatedID);
            if (core.savedItemPos.ContainsKey(generatedID))
                return;
            core.savedItemPos.Add(generatedID, new Vector3(0f, 0f, 0f));
            core.dataQueue.Enqueue(new CreatePickupData()
            {
                ItemID = generatedID,
                ItemType = (int)ev.Pickup.ItemId,
                Position = ev.Pickup.position.GetData(),
                Rotation = ev.Pickup.rotation.GetData()
            });
        }


        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!core.recorderRunning)
                return;
            core.dataQueue.Enqueue(new UpdateRoleData()
            {
                PlayerID = (sbyte)ev.Player.Id,
                RoleID = (sbyte)ev.NewRole
            });
        }

        public CoroutineHandle record;
        private void WaitingForPlayers()
        {
            if (firstrun)
            {
                firstrun = false;
                Server_RestartingRound1();
            }
            if (core.replayRunning)
            {
                Log.Info("Start replay");
                foreach (var itm in UnityEngine.Object.FindObjectsOfType<Pickup>())
                {
                    NetworkServer.Destroy(itm.gameObject);
                }
                RoundSummary.RoundLock = true;
                CharacterClassManager.ForceRoundStart();
            }
            waitingforplayers = true;
        }

        private void Server_RoundEnded()
        {
            throw new NotImplementedException();
        }
    }
}
