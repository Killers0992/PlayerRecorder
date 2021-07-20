using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mirror;
using PlayerRecorder.Core.Record;
using PlayerRecorder.Core.Replay;
using PlayerRecorder.Structs;
using System;
using System.IO;

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
            Exiled.Events.Handlers.Map.PlacingDecal += Map_PlacingDecal;
            Exiled.Events.Handlers.Map.PlacingBlood += Map_PlacingBlood;
            Exiled.Events.Handlers.Map.AnnouncingScpTermination += Map_AnnouncingScpTermination;
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

        private void Map_PlacingDecal(PlacingDecalEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecordCore.OnReceiveEvent(new PlaceDecalData()
            {
                IsBlood = false,
                Type = (sbyte)ev.Type,
                Position = new Vector3Data() { x = ev.Position.x, y = ev.Position.y, z = ev.Position.z},
                Rotation = new QuaternionData() {  x = ev.Rotation.x, y = ev.Rotation.y, z = ev.Rotation.z, w = ev.Rotation.w}
            });
        }

        private void Map_AnnouncingScpTermination(AnnouncingScpTerminationEventArgs ev)
        {
            if (!MainClass.isRecording)
                return;
            RecordCore.OnReceiveEvent(new ScpTerminationData()
            {
                RoleFullName = ev.Role.fullName,
                ToolID = ev.HitInfo.Tool,
                GroupID = ev.TerminationCause
            });
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
                ToolID = ev.HitInformations.Tool,
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
            Timing.RunCoroutine(RecordCore.Process(MainClass.currentRoundID));
            MainClass.currentRoundID++;
            Log.Info($"Round restart with new round id: {MainClass.currentRoundID}");
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
                MainClass.RoundTimestamp = DateTime.Now;
                core.StartRecording();
                MainClass.recordPickups.Clear();
                MainClass.framer = 0;
                Log.Info("New recorder instance created.");
                MainClass.isRecording = true;
                foreach (var gen in UnityEngine.Object.FindObjectsOfType<Generator079>())
                {
                    gen.gameObject.AddComponent<GeneratorRecord>();
                }
                foreach (var door in UnityEngine.Object.FindObjectsOfType<DoorVariant>())
                {
                    door.gameObject.AddComponent<DoorRecord>();
                }
                foreach(var lift in UnityEngine.Object.FindObjectsOfType<Lift>())
                {
                    lift.gameObject.AddComponent<LiftRecord>();
                }
                AlphaWarheadController.Host.gameObject.AddComponent<WarheadRecord>();
            }
        }
    }
}
