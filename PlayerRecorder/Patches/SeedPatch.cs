using GameCore;
using HarmonyLib;
using MapGeneration;
using Mirror;
using PlayerRecorder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[HarmonyPatch(typeof(SeedSynchronizer), nameof(SeedSynchronizer.Start))]
internal static class SeedPatch
{
    private static bool Prefix(SeedSynchronizer __instance)
    {
		if (!NetworkServer.active)
		{
			return false;
		}
		int num = ConfigFile.ServerConfig.GetInt("map_seed", -1);
		if (num < 1)
		{
			num = UnityEngine.Random.Range(1, int.MaxValue);
			SeedSynchronizer.DebugInfo("Server has successfully generated a random seed: " + num, MessageImportance.Normal, false);
		}
		else
		{
			SeedSynchronizer.DebugInfo("Server has successfully loaded a seed from config: " + num, MessageImportance.Normal, false);
		}
		__instance.Network_syncSeed = RecorderCore.singleton.SeedID != -1 ? RecorderCore.singleton.SeedID : Mathf.Clamp(num, 1, int.MaxValue);
		return false;
	}
}
