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
    public class ReplayPlayer : MonoBehaviour
    {
        public int uniqueId = 0;

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
            Log.Info($"Player replay init for {this.hub.nicknameSync.Network_myNickSync} ({this.hub.characterClassManager.UserId}) ({this.hub.queryProcessor.PlayerId})");
            //RecorderCore.OnRegisterReplayPlayer(this);
        }

        public void UpdatePlayer(UpdatePlayerData e)
        {
            if (uniqueId == 0)
                return;
            hub.animationController.NetworkcurAnim = e.CurrentAnim;
            hub.animationController.Networkspeed = e.Speed.vector;
            hub.animationController.Network_curMoveState = e.MoveState;
            hub.playerMovementSync.RotationSync = e.Rotation.vector;

            try
            {
                hub.playerMovementSync.OverridePosition(e.Position.vector, 0f, true);
            }catch (NullReferenceException) { }
        }

        public void UpdateHoldingItem(UpdateHoldingItem e)
        {
            if (uniqueId == 0)
                return;
            hub.inventory.Network_curItemSynced = (ItemType)e.HoldingItem;
        }

        public void UpdateRole(UpdateRoleData e)
        {
            if (uniqueId == 0)
                return;
            hub.characterClassManager.NetworkCurClass = (RoleType)e.RoleID;
            Log.Info($"Changed fake player role ID: {e.PlayerID}, RoleType: {(RoleType)e.RoleID}.");
        }

        public void ShotWeapon()
        {
            if (uniqueId == 0)
                return;
            hub.weaponManager.RpcConfirmShot(false, (int)hub.weaponManager.curWeapon);
        }

        public void ReloadWeapon()
        {
            if (uniqueId == 0)
                return;
            hub.weaponManager.RpcReload(hub.weaponManager.curWeapon);
        }

        void OnDestroy()
        {
            ReplayCore.replayPlayers.Remove(uniqueId);
            Log.Info($"Player replay destroy for {this.hub.nicknameSync.Network_myNickSync} ({this.hub.characterClassManager.UserId}) ({this.hub.queryProcessor.PlayerId})");
            //RecorderCore.OnUnRegisterReplayPlayer(this);
        }
    }
}
