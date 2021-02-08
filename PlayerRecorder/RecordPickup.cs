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
                if (RecorderCore.recordPickups.Keys.Any(p => p == i))
                    continue;

                uniqueId = i;
                break;
            }
            RecorderCore.recordPickups.Add(uniqueId, this);
            RecorderCore.OnReceiveEvent(new CreatePickupData()
            {
                ItemID = uniqueId,
                ItemType = (int)pickup.ItemId,
                Position = pickup.position.GetData(),
                Rotation = pickup.rotation.GetData()
            });
            Log.Info($"Pickup record init for {pickup.ItemId} ({uniqueId})");
            RecorderCore.OnRegisterRecordPickup(this);
        }

        public void Update()
        {
            if (!RecorderCore.isRecording || pickup?.ItemId == ItemType.None)
                return;
            if (currentPosition != transform.position || currentRotation != transform.rotation)
            {
                currentPosition = transform.position;
                currentRotation = transform.rotation;
                RecorderCore.OnReceiveEvent(new UpdatePickupData()
                {
                    ItemID = uniqueId,
                    ItemType = (int)pickup.itemId,
                    Position = transform.position.GetData(),
                    Rotation = transform.rotation.GetData()
                });
            }
        }

        void OnDestroy()
        {
            Log.Info($"Pickup record destroy for {pickup.ItemId} ({uniqueId})");
            RecorderCore.recordPickups.Remove(uniqueId);
            RecorderCore.OnReceiveEvent(new RemovePickupData()
            {
                ItemID = uniqueId
            });
            RecorderCore.OnUnRegisterRecordPickup(this);
        }
    }
}
