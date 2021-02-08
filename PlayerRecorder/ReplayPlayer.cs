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
        public ReferenceHub hub;
        public int uniqueId = 0;

        void Start()
        {
            this.hub = GetComponent<ReferenceHub>();
            PlayerManager.AddPlayer(gameObject);
            Log.Info($"Player replay init for {this.hub.nicknameSync._firstNickname} ({this.hub.characterClassManager.UserId}) ({this.hub.queryProcessor.PlayerId})");
            RecorderCore.OnRegisterReplayPlayer(this);
        }

        public void UpdatePlayer(UpdatePlayerData e)
        {
            hub.inventory.Network_curItemSynced = (ItemType)e.HoldingItem;
            hub.animationController.NetworkcurAnim = e.CurrentAnim;
            hub.animationController.Networkspeed = e.Speed.SetVector();
            hub.animationController.Network_curMoveState = e.MoveState;
            hub.playerMovementSync.OverridePosition(e.Position.SetVector(), hub.transform.rotation.eulerAngles.y);
            hub.playerMovementSync.RotationSync = e.Rotation.SetVector();
        }

        public void UpdateRole(UpdateRoleData e)
        {
            hub.characterClassManager.NetworkCurClass = (RoleType)e.RoleID;
            Log.Info($"Changed fake player role ID: {e.PlayerID}, RoleType: {(RoleType)e.RoleID}.");
        }

        public void ShotWeapon()
        {
            hub.weaponManager.RpcConfirmShot(false, (int)hub.weaponManager.curWeapon);
        }

        public void ReloadWeapon()
        {
            hub.weaponManager.RpcReload(hub.weaponManager.curWeapon);
        }

        void OnDestroy()
        {
            Player.Dictionary.Remove(gameObject);
            PlayerManager.RemovePlayer(gameObject);
            Player.IdsCache.Remove(hub.queryProcessor.NetworkPlayerId);
            RecorderCore.replayPlayers.Remove(uniqueId);
            Log.Info($"Player replay destroy for {this.hub.nicknameSync._firstNickname} ({this.hub.characterClassManager.UserId}) ({this.hub.queryProcessor.PlayerId})");
            RecorderCore.OnUnRegisterReplayPlayer(this);
        }
    }
}
