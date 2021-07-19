using PlayerRecorder.Enums;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    [ProtoInclude(1, typeof(CreatePickupData))]
    [ProtoInclude(2, typeof(DoorData))]
    [ProtoInclude(3, typeof(LeaveData))]
    [ProtoInclude(4, typeof(LiftData))]
    [ProtoInclude(5, typeof(PlayerInfoData))]
    [ProtoInclude(6, typeof(ReloadWeaponData))]
    [ProtoInclude(7, typeof(RemovePickupData))]
    [ProtoInclude(8, typeof(RoundEndData))]
    [ProtoInclude(9, typeof(SeedData))]
    [ProtoInclude(10, typeof(ShotWeaponData))]
    [ProtoInclude(11, typeof(UpdatePickupData))]
    [ProtoInclude(12, typeof(UpdatePlayerData))]
    [ProtoInclude(13, typeof(UpdateRoleData))]
    [ProtoInclude(14, typeof(UpdateHoldingItem))]
    [ProtoInclude(15, typeof(CreateRagdollData))]
    [ProtoInclude(16, typeof(GeneratorUpdateData))]
    [ProtoInclude(17, typeof(WarheadUpdateData))]
    public interface IEventType
    {
    }
}
