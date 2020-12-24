using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Enums
{
    public enum EventType
    {
        CreatePlayer,
        UpdatePlayer,
        RemovePlayer,
        CreateItem,
        UpdateItem,
        RemoveItem,
        ApplySeed,
        DoorState,
        LiftState,
        WeaponFire,
        WeaponReload,
        FragExplode,
        FlashExplode,
        ThrowGrenade
    }
}
