using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using MEC;
using Mirror;
using PlayerRecorder.Core.Record;
using PlayerRecorder.Core.Replay;
using PlayerRecorder.Enums;
using PlayerRecorder.Structs;
using System;
using System.IO;

namespace PlayerRecorder
{
    public class EventHandlers
    {
        private RecorderCore core;
        private ReplayCore core2;
        public bool firstrun = false;
        public EventHandlers(RecorderCore core, ReplayCore core2)
        {
            this.core = core;
            this.core2 = core2;
            Exiled.Events.Handlers.Server.WaitingForPlayers += WaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound += Server_RestartingRound1;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += Server_SendingRemoteAdminCommand;
            Exiled.Events.Handlers.Player.InteractingElevator += Player_InteractingElevator;
            Exiled.Events.Handlers.Player.Shot += Player_Shot;
            Exiled.Events.Handlers.Player.ReloadingWeapon += Player_ReloadingWeapon;
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.SpawningRagdoll += Player_SpawningRagdoll;
            Exiled.Events.Handlers.Player.UnlockingGenerator += Player_UnlockingGenerator;
            Exiled.Events.Handlers.Player.OpeningGenerator += Player_OpeningGenerator;
            Exiled.Events.Handlers.Player.ClosingGenerator += Player_ClosingGenerator;
            Exiled.Events.Handlers.Scp914.ChangingKnobSetting += Scp914_ChangingKnobSetting;
            Exiled.Events.Handlers.Player.Verified += Player_Verified;
            firstrun = true;
        }

        private void Player_Verified(VerifiedEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            ev.Player.GameObject.AddComponent<RecordPlayer>();
        }

        private void Scp914_ChangingKnobSetting(Exiled.Events.EventArgs.ChangingKnobSettingEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new Change914KnobData()
            {
                KnobSetting = (sbyte)ev.KnobSetting
            });
        }

        private void Player_ClosingGenerator(Exiled.Events.EventArgs.ClosingGeneratorEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new OpenCloseGeneratorData()
            {
                IsOpen = false,
                Position = new Vector3Data() { x = ev.Generator.transform.position.x, y = ev.Generator.transform.position.y, z = ev.Generator.transform.position.z }
            });
        }

        private void Player_OpeningGenerator(Exiled.Events.EventArgs.OpeningGeneratorEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new OpenCloseGeneratorData()
            {
                IsOpen = true,
                Position = new Vector3Data() { x = ev.Generator.transform.position.x, y = ev.Generator.transform.position.y, z = ev.Generator.transform.position.z }
            });
        }

        private void Player_UnlockingGenerator(Exiled.Events.EventArgs.UnlockingGeneratorEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new UnlockGeneratorData()
            {
                Position = new Vector3Data() { x= ev.Generator.transform.position.x, y = ev.Generator.transform.position.y, z = ev.Generator.transform.position.z}
            });
        }

        private void Player_SpawningRagdoll(Exiled.Events.EventArgs.SpawningRagdollEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new CreateRagdollData()
            {
                ClassID = (int)ev.RoleType,
                OwnerID = ev.DissonanceId,
                PlayerID = ev.Owner.Id,
                ToolID = ev.HitInformations.Tool,
                OwnerNick = ev.Owner.DisplayNickname,
                Position = new Vector3Data() {  x= ev.Position.x, y= ev.Position.z, z =ev.Position.z},
                Rotation = new QuaternionData() {  x = ev.Rotation.x, y= ev.Rotation.y, z = ev.Rotation.z, w = ev.Rotation.w},
                Velocity = new Vector3Data() { x = ev.Velocity.x, y = ev.Velocity.z, z = ev.Velocity.z }
            });
        }

        private void Server_RoundStarted()
        {
            waitingforplayers = false;
        }


        private void Player_ReloadingWeapon(Exiled.Events.EventArgs.ReloadingWeaponEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new ReloadWeaponData()
            {
                PlayerID = (sbyte)ev.Player.Id
            });
        }

        private void Player_Shot(Exiled.Events.EventArgs.ShotEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecorderCore.OnReceiveEvent(new ShotWeaponData()
            {
                PlayerID = (sbyte)ev.Shooter.Id
            });
        }

        private void Player_InteractingElevator(Exiled.Events.EventArgs.InteractingElevatorEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            if (ev.Status == Lift.Status.Moving)
                return;
            RecorderCore.OnReceiveEvent(new LiftData()
            {
                Elevatorname = ev.Lift.elevatorName
            });
        }

        public CoroutineHandle replayHandler;
        private void Server_SendingRemoteAdminCommand(Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs ev)
        {
            Log.Info("Command " + ev.Name);
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
                                    replayHandler = Timing.RunCoroutine(core2.Replay(Path.Combine(MainClass.pluginDir, "RecorderData", ev.Arguments[1], "Record_" + ev.Arguments[2] + ".rd")));
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
                            if (MainClass.isReplayReady && !MainClass.isReplaying)
                            {
                                MainClass.isReplaying = true;
                                ev.Sender.RemoteAdminMessage("Replay started.", true, "PlayerRecorder");
                            }
                            else
                            {
                                ev.Sender.RemoteAdminMessage("Replay not prepared.", true, "PlayerRecorder");
                            }
                            break;
                        case "PAUSE":
                            if (MainClass.isReplaying)
                            {
                                MainClass.isReplayPaused = !MainClass.isReplayPaused;
                                ev.Sender.RemoteAdminMessage($"Replay {(MainClass.isReplayPaused ? "paused" : "started")}.", true, "PlayerRecorder");
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
                            if (MainClass.isReplaying)
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
                            if (MainClass.isReplaying)
                            {
                                MainClass.isReplaying = false;
                                MainClass.isReplayReady = false;
                                MainClass.SeedID = -1;
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
            MainClass.isRecording = false;
            Timing.RunCoroutine(RecorderCore.Process(MainClass.currentRoundID));
            MainClass.currentRoundID++;
            Log.Info($"Round restart with new round id: {MainClass.currentRoundID}");
            if (replayHandler != null && !MainClass.isReplayReady)
                Timing.KillCoroutines(replayHandler);
            if (!Directory.Exists(Path.Combine(MainClass.pluginDir, "RecorderData")))
                Directory.CreateDirectory(Path.Combine(MainClass.pluginDir, "RecorderData"));
            if (!Directory.Exists(Path.Combine(MainClass.pluginDir, "RecorderData", Server.Port.ToString())))
                Directory.CreateDirectory(Path.Combine(MainClass.pluginDir, "RecorderData", Server.Port.ToString()));
        }

        private void WaitingForPlayers()
        {
            if (firstrun)
            {
                firstrun = false;
                if (!Directory.Exists(Path.Combine(MainClass.pluginDir, "RecorderData")))
                    Directory.CreateDirectory(Path.Combine(MainClass.pluginDir, "RecorderData"));
                if (!Directory.Exists(Path.Combine(MainClass.pluginDir, "RecorderData", Server.Port.ToString())))
                    Directory.CreateDirectory(Path.Combine(MainClass.pluginDir, "RecorderData", Server.Port.ToString()));
            }
            if (MainClass.isReplayReady)
            {
                Log.Info("Start replay");
                foreach (var itm in UnityEngine.Object.FindObjectsOfType<Pickup>())
                {
                    NetworkServer.Destroy(itm.gameObject);
                }
                RoundSummary.RoundLock = true;
                CharacterClassManager.ForceRoundStart();
                MainClass.replayPickups.Clear();
                MainClass.replayPlayers.Clear();
            }
            else
            {
                core.StartRecording();
                MainClass.framer = 0;
                Log.Info("New recorder instance created.");
                MainClass.isRecording = true;
                foreach (var gen in UnityEngine.Object.FindObjectsOfType<Generator079>())
                {
                    gen.gameObject.AddComponent<GeneratorRecord>();
                }
                AlphaWarheadController.Host.gameObject.AddComponent<WarheadRecord>();
            }
            waitingforplayers = true;
        }
    }
}
