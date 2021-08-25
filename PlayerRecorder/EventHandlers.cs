using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Mirror;
using NPCS;
using PlayerRecorder.Core.Record;
using PlayerRecorder.Core.Replay;
using PlayerRecorder.Structs;
using System;
using System.IO;
using Utf8Json;

namespace PlayerRecorder
{
    public class EventHandlers
    {
        private RecordCore core;
        private ReplayCore core2;
        public bool firstrun = true;
        public EventHandlers(RecordCore core, ReplayCore core2)
        {
            this.core = core;
            this.core2 = core2;
            Exiled.Events.Handlers.Server.WaitingForPlayers += WaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound += Server_RestartingRound1;
            Exiled.Events.Handlers.Player.Shot += Player_Shot;
            Exiled.Events.Handlers.Player.ReloadingWeapon += Player_ReloadingWeapon;
            Exiled.Events.Handlers.Player.SpawningRagdoll += Player_SpawningRagdoll;
            Exiled.Events.Handlers.Scp914.ChangingKnobSetting += Scp914_ChangingKnobSetting;
            Exiled.Events.Handlers.Player.Verified += Player_Verified;
            Exiled.Events.Handlers.Map.PlacingBlood += Map_PlacingBlood;
            Exiled.Events.Handlers.Map.AnnouncingScpTermination += Map_AnnouncingScpTermination;
            Exiled.Events.Handlers.Server.LocalReporting += Server_LocalReporting;
            Exiled.Events.Handlers.Player.Spawning += Player_Spawning;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= WaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound -= Server_RestartingRound1;
            Exiled.Events.Handlers.Player.Shot -= Player_Shot;
            Exiled.Events.Handlers.Player.ReloadingWeapon -= Player_ReloadingWeapon;
            Exiled.Events.Handlers.Player.SpawningRagdoll -= Player_SpawningRagdoll;
            Exiled.Events.Handlers.Scp914.ChangingKnobSetting -= Scp914_ChangingKnobSetting;
            Exiled.Events.Handlers.Player.Verified -= Player_Verified;
            Exiled.Events.Handlers.Map.PlacingBlood -= Map_PlacingBlood;
            Exiled.Events.Handlers.Map.AnnouncingScpTermination -= Map_AnnouncingScpTermination;
            Exiled.Events.Handlers.Server.LocalReporting -= Server_LocalReporting;
            Exiled.Events.Handlers.Player.Spawning -= Player_Spawning;
        }

        private void Player_Spawning(SpawningEventArgs ev)
        {
            if (ev.Player.IsNPC())
                return;
            if (MainClass.isReplayReady && MainClass.bringSpectatorToTarget != -1)
            {
                if (MainClass.replayPlayers.TryGetValue(MainClass.bringSpectatorToTarget, out ReplayPlayer plr))
                {
                    if (plr.hub.characterClassManager.IsAlive)
                    {
                        ev.Position = plr.transform.position;
                    }
                }
            }
        }

        private void Server_LocalReporting(LocalReportingEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            HttpQuery.Post(MainClass.singleton.Config.webhookUrl, "payload_json=" + JsonSerializer.ToJsonString<DiscordWebhook>(new DiscordWebhook(string.Empty, "Player Recorder", "https://cdn.discordapp.com/attachments/742563439918055510/867318607826386954/recording-icon-15.png", false, new DiscordEmbed[]
            {
                new DiscordEmbed("Round report", "rich", $"New report on server ``{Server.IpAddress}:{Server.Port}``", CheaterReport.WebhookColor, new DiscordEmbedField[]
                {
                    new DiscordEmbedField("Issuer" , $"{ev.Issuer.Nickname} (||{ev.Issuer.UserId}||)", false),
                    new DiscordEmbedField("Reported" , $"{ev.Target.Nickname} (||{ev.Target.UserId}||)", false),
                    new DiscordEmbedField("Reason" , $"{ev.Reason}", false),
                    new DiscordEmbedField("Info", $"If you want to replay that wait to round end beacuse record is not ready !!", false),
                    new DiscordEmbedField("Command", $"||replay prepare {Server.Port} {MainClass.RoundTimestamp.Ticks} {MainClass.framer - ((MainClass.singleton.Config.secondsBeforeReport/MainClass.singleton.Config.recordDelay)*2)} {ev.Target.Id}||", false)
                })
            })));
        }

        private void Map_PlacingBlood(PlacingBloodEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecordCore.OnReceiveEvent(new PlaceDecalData()
            {
                IsBlood = true,
                Type = (sbyte)ev.Type,
                Position = new Vector3Data() { x = ev.Position.x, y = ev.Position.y, z = ev.Position.z },
                Rotation = new QuaternionData() { x = 0f, y = 0f, z = 0f, w = 0f }
            });
        }

        private void Map_AnnouncingScpTermination(AnnouncingScpTerminationEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecordCore.OnReceiveEvent(new ScpTerminationData()
            {
                RoleFullName = ev.Role.fullName,
                ToolID = (int)ev.HitInfo.Tool.Weapon,
                GroupID = ev.TerminationCause
            });
        }

        private void Player_Verified(VerifiedEventArgs ev)
        {
            if (ev.Player.IsNPC())
                return;
            if (MainClass.isReplayReady)
            {
                ev.Player.Role = RoleType.Tutorial;
                ev.Player.NoClipEnabled = true;
                ev.Player.IsGodModeEnabled = true;
            }
            if (!MainClass.isRecording)
                return;
            ev.Player.GameObject.AddComponent<RecordPlayer>();
        }

        private void Scp914_ChangingKnobSetting(Exiled.Events.EventArgs.ChangingKnobSettingEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecordCore.OnReceiveEvent(new Change914KnobData()
            {
                KnobSetting = (sbyte)ev.KnobSetting
            });
        }

        private void Player_SpawningRagdoll(Exiled.Events.EventArgs.SpawningRagdollEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecordCore.OnReceiveEvent(new CreateRagdollData()
            {
                ClassID = (int)ev.RoleType,
                OwnerID = ev.DissonanceId,
                PlayerID = ev.Owner.Id,
                ToolID = (int)ev.HitInformations.Tool.Weapon,
                OwnerNick = ev.Owner.DisplayNickname,
                Position = new Vector3Data() {  x= ev.Position.x, y= ev.Position.z, z =ev.Position.z},
                Rotation = new QuaternionData() {  x = ev.Rotation.x, y= ev.Rotation.y, z = ev.Rotation.z, w = ev.Rotation.w},
                Velocity = new Vector3Data() { x = ev.Velocity.x, y = ev.Velocity.z, z = ev.Velocity.z }
            });
        }

        private void Player_ReloadingWeapon(Exiled.Events.EventArgs.ReloadingWeaponEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecordCore.OnReceiveEvent(new ReloadWeaponData()
            {
                PlayerID = (sbyte)ev.Player.Id
            });
        }

        private void Player_Shot(Exiled.Events.EventArgs.ShotEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecordCore.OnReceiveEvent(new ShotWeaponData()
            {
                PlayerID = (sbyte)ev.Shooter.Id
            });
        }

        private void Server_RestartingRound1()
        {
            ReplayCache.ClearCache();
            MainClass.isRecording = false;
            Timing.RunCoroutine(RecordCore.Process(MainClass.currentRoundID, MainClass.RoundTimestamp));
            MainClass.currentRoundID++;
            Log.Debug($"Round restart with new round id: {MainClass.currentRoundID}");
            if (MainClass.replayHandler != null && !MainClass.isReplayReady)
                Timing.KillCoroutines(MainClass.replayHandler);
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
                Log.Debug("Start replay");
                RoundSummary.RoundLock = true;
                CharacterClassManager.ForceRoundStart();
                MainClass.replayPickups.Clear();
                MainClass.replayPlayers.Clear();
                if (MainClass.forceReplayStart)
                    MainClass.isReplaying = true;
            }
            else
            {
                MainClass.RoundTimestamp = DateTime.Now;
                MainClass.recordPickups.Clear();
                MainClass.framer = 0;
                core.StartRecording();
                Log.Debug("New recorder instance created.");
                MainClass.isRecording = true;

                foreach (var door in Map.Doors)
                {
                    door.Base.gameObject.AddComponent<DoorRecord>();
                }
                foreach(var lift in Map.Lifts)
                {
                    lift.gameObject.AddComponent<LiftRecord>();
                }
                AlphaWarheadController.Host.gameObject.AddComponent<WarheadRecord>();
            }
        }
    }
}
