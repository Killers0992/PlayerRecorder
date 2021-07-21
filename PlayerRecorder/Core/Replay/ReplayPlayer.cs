using Exiled.API.Features;
using PlayerRecorder.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder.Core.Replay
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

        public void UpdatePlayer(UpdatePlayerData e)
        {
            if (uniqueId == 0)
                return;

            try
            {
                if (hub.animationController.NetworkcurAnim != e.CurrentAnim)
                    hub.animationController.NetworkcurAnim = e.CurrentAnim;

                if (hub.animationController.Networkspeed != e.Speed.vector)
                    hub.animationController.Networkspeed = e.Speed.vector;

                if (hub.animationController.Network_curMoveState != e.MoveState)
                    hub.animationController.Network_curMoveState = e.MoveState;

                if (hub.characterClassManager.NetworkCurClass != (RoleType)e.RoleID)
                    hub.characterClassManager.NetworkCurClass = (RoleType)e.RoleID;

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }


            try
            {
                hub.playerMovementSync.OverridePosition(e.Position.vector, 0f, true);
            }
            catch (NullReferenceException) { }
            hub.playerMovementSync.RotationSync = e.Rotation.vector;
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
            MainClass.replayPlayers.Remove(uniqueId);
        }
    }
}
