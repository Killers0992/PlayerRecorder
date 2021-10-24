using Exiled.API.Features.Items;
using InventorySystem.Items.Pickups;
using PlayerRecorder.Structs;
using UnityEngine;

namespace PlayerRecorder.Core.Replay
{
    public class ReplayPickup : MonoBehaviour
    {
        public int uniqueId = 0;

        ItemPickupBase _pickup;
        public ItemPickupBase pickup
        {
            get
            {
                if (_pickup == null)
                    _pickup = GetComponent<ItemPickupBase>();
                return _pickup;
            }
        }

        public void UpdatePickup(UpdatePickupData e)
        {
            if (pickup == null || uniqueId == 0)
                return;
            pickup.transform.position = e.Position.vector;
            pickup.transform.rotation = e.Rotation.quaternion;
        }

        void OnDestroy()
        {
            MainClass.replayPickups.Remove(uniqueId);
        }
    }
}
