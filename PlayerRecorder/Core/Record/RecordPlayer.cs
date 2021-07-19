using Exiled.API.Features;
using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Core.Record
{
    public class RecordPlayer : MonoBehaviour
    {
        public Vector3 currentPosition = new Vector3(0f, 0f, 0f);
        public Vector2 currentRotation = new Vector2(0f, 0f);
        public ItemType currentHoldingItem = ItemType.None;
        public RoleType currentRole = RoleType.None;

        ReferenceHub _hub;
        public ReferenceHub hub
        {
            get
            {
                if (_hub == null)
                    _hub = GetComponent<ReferenceHub>();
                return _hub;
            }
        }

        void Awake() 
        { 
            Log.Info($"Player record init for {this.hub.nicknameSync._firstNickname} ({this.hub.characterClassManager.UserId}) ({this.hub.queryProcessor.PlayerId})");
            RecorderCore.OnReceiveEvent(new PlayerInfoData()
            {
                PlayerID = (sbyte)this.hub.queryProcessor.PlayerId,
                UserID = this.hub.characterClassManager.UserId,
                UserName = this.hub.nicknameSync._firstNickname
            });
            //RecorderCore.OnRegisterRecordPlayer(this);
        }

        private void Update()
        {
            if (hub?.characterClassManager.NetworkCurClass == RoleType.Spectator || hub?.characterClassManager.NetworkCurClass == RoleType.None || !MainClass.isRecording)
                return;

            if (currentPosition != hub.transform.position || currentRotation != hub.playerMovementSync.Rotations)
            {

                currentPosition = hub.transform.position;
                currentRotation = hub.playerMovementSync.Rotations;
                RecorderCore.OnReceiveEvent(new UpdatePlayerData()
                {
                    PlayerID = (sbyte)hub.queryProcessor.NetworkPlayerId,
                    MoveState = (byte)hub.animationController.Network_curMoveState,
                    CurrentAnim = hub.animationController.NetworkcurAnim,
                    Speed = new Vector2Data() { x = hub.animationController.Networkspeed.x, y = hub.animationController.Networkspeed.y },
                    Position = new Vector3Data() { x = hub.transform.position.x, y = hub.transform.position.y, z = hub.transform.position.z },
                    Rotation = new Vector2Data() { x = hub.playerMovementSync.Rotations.x, y = hub.playerMovementSync.Rotations.y }
                });
            }

            if (currentHoldingItem != hub.inventory.Network_curItemSynced)
            {
                currentHoldingItem = hub.inventory.Network_curItemSynced;
                RecorderCore.OnReceiveEvent(new UpdateHoldingItem()
                {
                    PlayerID = (sbyte)hub.queryProcessor.NetworkPlayerId,
                    HoldingItem = (sbyte)hub.inventory.Network_curItemSynced
                });
            }

            if (currentRole != hub.characterClassManager.NetworkCurClass)
            {
                currentRole = hub.characterClassManager.NetworkCurClass;
                RecorderCore.OnReceiveEvent(new UpdateRoleData()
                {
                    PlayerID = (sbyte)hub.queryProcessor.NetworkPlayerId,
                    RoleID = (sbyte)currentRole
                });
            }
        }

        void OnDestroy()
        {
            Log.Info($"Player record destroy for {this.hub.nicknameSync._firstNickname} ({this.hub.characterClassManager.UserId}) ({this.hub.queryProcessor.PlayerId})");
            RecorderCore.OnReceiveEvent(new LeaveData()
            {
                PlayerID = (sbyte)this.hub.queryProcessor.PlayerId
            });
            //RecorderCore.OnRegisterRecordPlayer(this);
        }
    }
}
