using Exiled.API.Features;
using Exiled.API.Features.Items;
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
                ItemType = (int)pickup.Type,
                Position = new Vector3Data() { x = pickup.Position.x, y = pickup.Position.y, z = pickup.Position.z },
                Rotation = new QuaternionData() {  x= pickup.Rotation.x, y=pickup.Rotation.y, z=pickup.Rotation.z,w=pickup.Rotation.w}
            });;
        }

        private void Update()
        {                           
            if (!MainClass.isRecording || pickup?.Type == ItemType.None)
                return;
            if (currentPosition != transform.position || currentRotation != transform.rotation)
            {
                currentPosition = transform.position;
                currentRotation = transform.rotation;
                RecordCore.OnReceiveEvent(new UpdatePickupData()
                {
                    ItemID = uniqueId,
                    ItemType = (int)pickup.Type,
                    Position = new Vector3Data() { x = pickup.Position.x, y = pickup.Position.y, z = pickup.Position.z },
                    Rotation = new QuaternionData() { x = pickup.Rotation.x, y = pickup.Rotation.y, z = pickup.Rotation.z, w = pickup.Rotation.w }
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
