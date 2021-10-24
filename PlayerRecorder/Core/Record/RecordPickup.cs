using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem.Items.Pickups;
using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Core.Record 
{
    public class RecordPickup : MonoBehaviour
    {
        public int uniqueId = 0;

        public Vector3 currentPosition = new Vector3(0f, 0f, 0f);
        public Quaternion currentRotation = new Quaternion(0f, 0f, 0f, 0f);

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

        void Awake()
        {
            for (int i = 1; i < int.MaxValue; i++)
            {
                if (MainClass.recordPickups.Keys.Any(p => p == i))
                    continue;

                uniqueId = i;
                break;
            }
            MainClass.recordPickups.Add(uniqueId, this);
            RecordCore.OnReceiveEvent(new CreatePickupData()
            {
                ItemID = uniqueId,
                ItemType = (int)pickup.Info.ItemId,
                Position = new Vector3Data() { x = pickup.transform.position.x, y = pickup.transform.position.y, z = pickup.transform.position.z },
                Rotation = new QuaternionData() {  x= pickup.transform.rotation.x, y=pickup.transform.rotation.y, z=pickup.transform.rotation.z,w=pickup.transform.rotation.w}
            });;
        }

        private void Update()
        {                           
            if (!MainClass.isRecording || pickup?.Info.ItemId == ItemType.None)
                return;
            if (currentPosition != pickup.transform.position || currentRotation != pickup.transform.rotation)
            {
                currentPosition = pickup.transform.position;
                currentRotation = pickup.transform.rotation;
                RecordCore.OnReceiveEvent(new UpdatePickupData()
                {
                    ItemID = uniqueId,
                    ItemType = (int)pickup.Info.ItemId,
                    Position = new Vector3Data() { x = pickup.transform.position.x, y = pickup.transform.position.y, z = pickup.transform.position.z },
                    Rotation = new QuaternionData() { x = pickup.transform.rotation.x, y = pickup.transform.rotation.y, z = pickup.transform.rotation.z, w = pickup.transform.rotation.w }
                });
            }
        }

        void OnDestroy()
        {
            MainClass.recordPickups.Remove(uniqueId);
            RecordCore.OnReceiveEvent(new RemovePickupData()
            {
                ItemID = uniqueId
            });
        }
    }
}
