using Exiled.API.Features.Items;
using PlayerRecorder.Structs;
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
            pickup.Position = e.Position.vector;
            pickup.Rotation = e.Rotation.quaternion;
            //if (pickup.Type != (ItemType)e.ItemType)
                //pickup.NetworkitemId = (ItemType)e.ItemType;
        }

        void OnDestroy()
        {
            MainClass.replayPickups.Remove(uniqueId);
        }
    }
}
