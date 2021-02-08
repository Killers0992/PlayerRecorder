using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Patches
{
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.OverridePosition))]
    [HarmonyPriority(Priority.First)]
    internal class ScalePositionFix
    {
        private static bool Prefix(PlayerMovementSync __instance, Vector3 pos, float rot, bool forceGround)
        {
            if (!RecorderCore.replayPlayers.Values.Any(p => p.gameObject == __instance.gameObject))
            {
                return true;
            }
            RaycastHit raycastHit;
            if (forceGround && Physics.Raycast(pos, Vector3.down, out raycastHit, 100f, __instance.CollidableSurfaces))
            {
                pos = raycastHit.point + Vector3.up;
                pos = new Vector3(pos.x, pos.y - (1f - __instance._hub.transform.localScale.y), pos.z);
            }
            __instance.ForcePosition(pos);
            __instance.PlayScp173SoundIfTeleported();
            return false;
        }
    }
}
