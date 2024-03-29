﻿using Exiled.API.Features;
using InventorySystem.Items.Firearms;
using JesusQC_Npcs.Features;
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
        public Dummy _dummy;

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

            hub.inventory.NetworkCurItem = new InventorySystem.Items.ItemIdentifier((ItemType)e.HoldingItem, 0);
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
            if (hub.inventory.CurInstance is Firearm fr)
            {
                //fr.
            }
        }

        public void ReloadWeapon()
        {
            if (uniqueId == 0)
                return;
            if (hub.inventory.CurInstance is Firearm fr)
            {
                //
            }
        }

        void OnDestroy()
        {
            MainClass.replayPlayers.Remove(uniqueId);
            if (_dummy != null)
            {
                Dummy.Dictionary.Remove(_dummy.PlayerWrapper.GameObject);
                PlayerManager.RemovePlayer(_dummy.PlayerWrapper.GameObject, CustomNetworkManager.slots);
            }
        }
    }
}
