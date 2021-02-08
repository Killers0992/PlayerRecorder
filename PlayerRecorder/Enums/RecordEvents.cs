using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Enums
{
    public enum RecordEvents : int
    {
        PlayerInfo,
        PlayerLeave,
        UpdateRole,
        UpdatePlayer,
        DoorState,
        RoundEnd,
        ReceiveSeed,
        ReloadWeapon,
        ShotWeapon,
        UpdatePickup,
        RemovePickup,
        CreatePickup,
        UseLift,
        Delay
    }
}
