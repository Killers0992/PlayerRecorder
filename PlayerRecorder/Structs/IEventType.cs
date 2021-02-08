using MessagePack;
using PlayerRecorder.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [Union((int)RecordEvents.PlayerInfo, typeof(PlayerInfoData))]
    [Union((int)RecordEvents.PlayerLeave, typeof(LeaveData))]
    [Union((int)RecordEvents.UpdateRole, typeof(UpdateRoleData))]
    [Union((int)RecordEvents.UpdatePlayer, typeof(UpdatePlayerData))]
    [Union((int)RecordEvents.DoorState, typeof(DoorData))]
    [Union((int)RecordEvents.RoundEnd, typeof(RoundEndData))]
    [Union((int)RecordEvents.ReceiveSeed, typeof(SeedData))]
    [Union((int)RecordEvents.ReloadWeapon, typeof(ReloadWeaponData))]
    [Union((int)RecordEvents.ShotWeapon, typeof(ShotWeaponData))]
    [Union((int)RecordEvents.UpdatePickup, typeof(UpdatePickupData))]
    [Union((int)RecordEvents.RemovePickup, typeof(RemovePickupData))]
    [Union((int)RecordEvents.CreatePickup, typeof(CreatePickupData))]
    [Union((int)RecordEvents.UseLift, typeof(LiftData))]
    [Union((int)RecordEvents.Delay, typeof(DelayData))]
    public interface IEventType
    {
    }
}
