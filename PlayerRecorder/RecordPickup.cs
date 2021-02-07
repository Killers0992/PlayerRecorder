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
        public Pickup pickup;
        public Vector3 currentPosition = new Vector3(0f, 0f, 0f);
        public Quaternion currentRotation = new Quaternion(0f, 0f,0f,0f);

        public int uniqueId = 0;

        void Start()
        {
            this.pickup = GetComponent<Pickup>();
            for (int i = 1; i < int.MaxValue; i++)
            {
                if (RecorderCore.singleton.itemData.Keys.Any(p => p == i))
                    continue;

                uniqueId = i;
                break;
            }
            RecorderCore.singleton.itemData.Add(uniqueId, this.pickup);
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
            RecorderCore.singleton.itemData.Remove(uniqueId);
            RecorderCore.OnUnRegisterRecordPickup(this);
            RecorderCore.OnReceiveEvent(new RemovePickupData()
            {
                ItemID = uniqueId
            });
        }
    }
}
