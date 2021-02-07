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
    public class RecordPlayer : MonoBehaviour
    {
        public ReferenceHub hub;
        public Vector3 currentPosition = new Vector3(0f,0f,0f);
        public Vector2 currentRotation = new Vector2(0f,0f);

        void Start() 
        { 
            this.hub = GetComponent<ReferenceHub>();
            Log.Info($"Player record init for {this.hub.nicknameSync._firstNickname} ({this.hub.characterClassManager.UserId}) ({this.hub.queryProcessor.PlayerId})");
            RecorderCore.OnReceiveEvent(new PlayerInfoData()
            {
                PlayerID = (sbyte)this.hub.queryProcessor.PlayerId,
                UserID = this.hub.characterClassManager.UserId,
                UserName = this.hub.nicknameSync._firstNickname
            });
            RecorderCore.OnRegisterRecordPlayer(this);
        }
        
        public void Update()
        {
            if (hub?.characterClassManager.NetworkCurClass == RoleType.Spectator || hub?.characterClassManager.NetworkCurClass == RoleType.None || !RecorderCore.isRecording)
                return;
            if (currentPosition != hub.transform.position || currentRotation != hub.playerMovementSync.Rotations)
            {
                currentPosition = hub.transform.position;
                currentRotation = hub.playerMovementSync.Rotations;
                RecorderCore.OnReceiveEvent(new UpdatePlayerData()
                {
                    PlayerID = (sbyte)hub.queryProcessor.NetworkPlayerId,
                    MoveState = (byte)hub.animationController.Network_curMoveState,
                    HoldingItem = (sbyte)hub.inventory.Network_curItemSynced,
                    CurrentAnim = hub.animationController.NetworkcurAnim,
                    Speed = hub.animationController.Networkspeed.GetData(),
                    Position = hub.transform.position.GetData(),
                    Rotation = hub.playerMovementSync.Rotations.GetData()
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
            RecorderCore.OnRegisterRecordPlayer(this);
        }
    }
}
