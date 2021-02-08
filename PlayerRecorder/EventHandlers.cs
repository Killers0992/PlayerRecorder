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
            Exiled.Events.Handlers.Server.RestartingRound += Server_RestartingRound1;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += Server_SendingRemoteAdminCommand;
            Exiled.Events.Handlers.Player.InteractingDoor += Player_InteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator += Player_InteractingElevator;
            Exiled.Events.Handlers.Player.Died += Player_Died;
            Exiled.Events.Handlers.Player.Shot += Player_Shot;
            Exiled.Events.Handlers.Player.ReloadingWeapon += Player_ReloadingWeapon;
            Exiled.Events.Handlers.Server.RoundEnded += Server_RoundEnded;
            Exiled.Events.Handlers.Player.Verified += Player_Verified;
            Exiled.Events.Handlers.Map.SpawnedItem += Map_SpawnedItem;
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            firstrun = true;
        }

        private void Server_RoundStarted()
        {
            waitingforplayers = false;
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (!RecorderCore.isRecording)
                return;
            ev.Player.GameObject.AddComponent<RecordPlayer>();
        }

        private void Map_SpawnedItem(Exiled.Events.EventArgs.SpawnedItemEventArgs ev)
        {
            if (!RecorderCore.isRecording)
                return;
            ev.Pickup.gameObject.AddComponent<RecordPickup>();
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            core.endInstance = true;
        }

        private void Player_ReloadingWeapon(Exiled.Events.EventArgs.ReloadingWeaponEventArgs ev)
        {
            if (!RecorderCore.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new ReloadWeaponData()
            {
                PlayerID = (sbyte)ev.Player.Id
            });
        }

        private void Player_Shot(Exiled.Events.EventArgs.ShotEventArgs ev)
        {
            if (!RecorderCore.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new ShotWeaponData()
            {
                PlayerID = (sbyte)ev.Shooter.Id
            });
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!RecorderCore.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new UpdateRoleData()
            {
                PlayerID = (sbyte)ev.Target.Id,
                RoleID = (sbyte)RoleType.Spectator
            });
        }

        private void Player_InteractingElevator(Exiled.Events.EventArgs.InteractingElevatorEventArgs ev)
        {
            if (!RecorderCore.isRecording)
                return;
            if (ev.Status == Lift.Status.Moving)
                return;
            RecorderCore.OnReceiveEvent(new LiftData()
            {
                Elevatorname = ev.Lift.elevatorName
            });
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (!RecorderCore.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new DoorData()
            {
                State = !ev.Door.TargetState,
                Position = ev.Door.transform.position.GetData()
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
                                if (File.Exists(Path.Combine(MainClass.pluginDir, "RecorderData", ev.Arguments[1],"Record_" + ev.Arguments[2] + ".rd")))
                                {
                                    Log.Info("Start prepare");
                                    replayHandler = Timing.RunCoroutine(core.Replay(Path.Combine(MainClass.pluginDir, "RecorderData", ev.Arguments[1], "Record_" + ev.Arguments[2] + ".rd")));
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
                            if (RecorderCore.isReplayReady && !RecorderCore.isReplaying)
                            {
                                RecorderCore.isReplaying = true;
                                ev.Sender.RemoteAdminMessage("Replay started.", true, "PlayerRecorder");
                            }
                            else
                            {
                                ev.Sender.RemoteAdminMessage("Replay not prepared.", true, "PlayerRecorder");
                            }
                            break;
                        case "PAUSE":
                            if (RecorderCore.isReplaying)
                            {
                                RecorderCore.isReplayPaused = !RecorderCore.isReplayPaused;
                                ev.Sender.RemoteAdminMessage($"Replay {(RecorderCore.isReplayPaused ? "paused" : "started")}.", true, "PlayerRecorder");
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
                            if (RecorderCore.isReplaying)
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
                            if (RecorderCore.isReplaying)
                            {
                                RecorderCore.isReplaying = false;
                                RecorderCore.isReplayReady = false;
                                core.SeedID = -1;
                                foreach (var item in RecorderCore.replayPickups)
                                {
                                    NetworkServer.Destroy(item.Value.gameObject);
                                }
                                RecorderCore.replayPickups.Clear();
                                foreach (var player in RecorderCore.replayPlayers)
                                {
                                    NetworkServer.Destroy(player.Value.gameObject);
                                }
                                RecorderCore.replayPlayers.Clear();
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

        public static bool waitingforplayers = false;
        private void Server_RestartingRound1()
        {
            waitingforplayers = false;
            RecorderCore.isRecording = false;
            core.endInstance = false;
            if (replayHandler != null)
                Timing.KillCoroutines(replayHandler);
            if (RecorderCore.singleton.recordingStream != null)
            {
                RecorderCore.singleton.recordingStream.Close();
                RecorderCore.singleton.recordingStream.Dispose();
                RecorderCore.singleton.recordingStream = null;
            }
            if (!Directory.Exists(Path.Combine(MainClass.pluginDir, "RecorderData")))
                Directory.CreateDirectory(Path.Combine(MainClass.pluginDir, "RecorderData"));
            if (!Directory.Exists(Path.Combine(MainClass.pluginDir, "RecorderData", Server.Port.ToString())))
                Directory.CreateDirectory(Path.Combine(MainClass.pluginDir, "RecorderData", Server.Port.ToString()));
            if (RecorderCore.isReplayReady)
                return;
            core.StartRecording();
            RecorderCore.isRecording = true;
            Log.Info("New recorder instance created.");
        }


        private void Player_ItemDropped(Exiled.Events.EventArgs.ItemDroppedEventArgs ev)
        {
            if (!RecorderCore.isRecording)
                return;
            ev.Pickup.gameObject.AddComponent<RecordPickup>();
        }


        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!RecorderCore.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new UpdateRoleData()
            {
                PlayerID = (sbyte)ev.Player.Id,
                RoleID = (sbyte)ev.NewRole
            });
        }

        private void WaitingForPlayers()
        {
            if (firstrun)
            {
                firstrun = false;
                Server_RestartingRound1();
            }
            if (RecorderCore.isReplayReady)
            {
                Log.Info("Start replay");
                foreach (var itm in UnityEngine.Object.FindObjectsOfType<Pickup>())
                {
                    NetworkServer.Destroy(itm.gameObject);
                }
                RoundSummary.RoundLock = true;
                CharacterClassManager.ForceRoundStart();
            }
            else if (RecorderCore.isRecording)
            {
                foreach (var pickup in UnityEngine.Object.FindObjectsOfType<Pickup>())
                {
                    pickup.gameObject.AddComponent<RecordPickup>();
                }
            }
            waitingforplayers = true;
        }


    }
}
