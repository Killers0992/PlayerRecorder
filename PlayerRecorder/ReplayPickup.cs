using Exiled.API.Features;
using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder
{
    public class ReplayPickup : MonoBehaviour
    {
        public Pickup pickup;

        public int uniqueId = 0;

        void Start()
        {
            this.pickup = GetComponent<Pickup>();
            Log.Info($"Pickup replay init for {pickup.ItemId} ({uniqueId})");
            RecorderCore.OnRegisterReplayPickup(this);
        }

        public void UpdatePickup(UpdatePickupData e)
        {
            if (pickup == null)
                return;
            pickup.Networkposition = e.Position.SetVector();
            pickup.Networkrotation = e.Rotation.SetQuaternion();
            if (pickup.NetworkitemId != (ItemType)e.ItemType)
                pickup.NetworkitemId = (ItemType)e.ItemType;
        }

        void OnDestroy()
        {
            Log.Info($"Pickup replay destroy for {pickup.ItemId} ({uniqueId})");
            RecorderCore.replayPickups.Remove(uniqueId);
            RecorderCore.OnUnRegisterReplayPickup(this);
        }
    }
}
