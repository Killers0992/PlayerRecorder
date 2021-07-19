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
        public int uniqueId = 0;

        Pickup _pickup;
        public Pickup pickup
        {
            get
            {
                if (_pickup == null)
                    _pickup = GetComponent<Pickup>();
                return _pickup;
            }
        }


        void Awake()
        {
            Log.Info($"Pickup replay init for {pickup.ItemId}");
            //RecorderCore.OnRegisterReplayPickup(this);
        }

        public void UpdatePickup(UpdatePickupData e)
        {
            if (pickup == null || uniqueId == 0)
                return;
            pickup.Networkposition = e.Position.vector;
            pickup.Networkrotation = e.Rotation.quaternion;
            if (pickup.NetworkitemId != (ItemType)e.ItemType)
                pickup.NetworkitemId = (ItemType)e.ItemType;
        }

        void OnDestroy()
        {
            Log.Info($"Pickup replay destroy for {pickup.ItemId} ({uniqueId})");
            ReplayCore.replayPickups.Remove(uniqueId);
           // RecorderCore.OnUnRegisterReplayPickup(this);
        }
    }
}
