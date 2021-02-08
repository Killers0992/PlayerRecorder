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

        public Player player;
        public int uniqueId = 0;

        void Start()
        {
            PlayerManager.AddPlayer(transform.gameObject);
            Log.Info($"Player replay init for {this.hub.nicknameSync._firstNickname} ({this.hub.characterClassManager.UserId}) ({this.hub.queryProcessor.PlayerId})");
            RecorderCore.OnRegisterReplayPlayer(this);
        }

        public void UpdatePlayer(UpdatePlayerData e)
        {
            if (uniqueId == 0)
                return;
            Log.Info("set player pos to " + e.Position.SetVector().ToString());
            hub.inventory.Network_curItemSynced = (ItemType)e.HoldingItem;
            hub.animationController.NetworkcurAnim = e.CurrentAnim;
            hub.animationController.Networkspeed = e.Speed.SetVector();
            hub.animationController.Network_curMoveState = e.MoveState;
            hub.playerMovementSync.OverridePosition(e.Position.SetVector(), hub.transform.rotation.eulerAngles.y);
            hub.playerMovementSync.RotationSync = e.Rotation.SetVector();
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
            PlayerManager.RemovePlayer(transform.gameObject);
            RecorderCore.replayPlayers.Remove(uniqueId);
            Log.Info($"Player replay destroy for {this.hub.nicknameSync._firstNickname} ({this.hub.characterClassManager.UserId}) ({this.hub.queryProcessor.PlayerId})");
            RecorderCore.OnUnRegisterReplayPlayer(this);
        }
    }
}
