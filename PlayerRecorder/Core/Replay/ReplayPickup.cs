using Exiled.API.Features;
using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Core.Replay
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
            MainClass.replayPickups.Remove(uniqueId);
        }
    }
}
