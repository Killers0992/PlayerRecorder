using HarmonyLib;
using InventorySystem.Items.Pickups;
using PlayerRecorder;
using PlayerRecorder.Core.Record;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[HarmonyPatch(typeof(ItemPickupBase), nameof(ItemPickupBase.InfoReceived))]
internal static class InfoReceivedPatch
{
	private static bool Prefix(ItemPickupBase __instance, PickupSyncInfo oldInfo, PickupSyncInfo newInfo)
	{
		if (!MainClass.isRecording)
			return true;
		if (!__instance.gameObject.TryGetComponent<RecordPickup>(out _))
			__instance.gameObject.AddComponent<RecordPickup>();
		return true;
	}
}
