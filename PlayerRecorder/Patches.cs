using GameCore;
using HarmonyLib;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder
{
    public static class Patches
    {
        [HarmonyPatch(typeof(RandomSeedSync), nameof(RandomSeedSync.Start))]
        internal static class SeedPatch
        {
            private static bool Prefix(RandomSeedSync __instance)
            {
				if (!__instance.isLocalPlayer || !NetworkServer.active)
				{
					return false;
				}
				foreach (WorkStation workStation in UnityEngine.Object.FindObjectsOfType<WorkStation>())
				{
					workStation.Networkposition = new Offset
					{
						position = workStation.transform.localPosition,
						rotation = workStation.transform.localRotation.eulerAngles,
						scale = Vector3.one
					};
				}
				__instance.Networkseed = RecorderCore.singleton.SeedID;
				while (NetworkServer.active && __instance.Networkseed == -1)
				{
					__instance.Networkseed = UnityEngine.Random.Range(-999999999, 999999999);
				}
				return false;
            }
        }
    }
}
