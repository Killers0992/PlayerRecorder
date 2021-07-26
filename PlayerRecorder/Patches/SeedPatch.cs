using GameCore;
using HarmonyLib;
using MapGeneration;
using Mirror;
using PlayerRecorder;

[HarmonyPatch(typeof(SeedSynchronizer), nameof(SeedSynchronizer.Start))]
internal static class SeedPatch
{
    private static bool Prefix(SeedSynchronizer __instance)
    {
		if (MainClass.SeedID == -1)
			return true;

		__instance.Network_syncSeed = MainClass.SeedID;
		return false;
	}
}
