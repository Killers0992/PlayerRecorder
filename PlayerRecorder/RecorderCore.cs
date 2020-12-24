using Exiled.API.Features;
using Ionic.Zip;
using LiteDB;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PlayerRecorder.Enums;
using PlayerRecorder.Structs;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerRecorder
{
    public class RecorderCore
    {
        public static RecorderCore singleton;

        public bool recorderRunning = false;
        public bool replayRunning = false;
        public bool replayStarted = false;
        public bool replayPaused = false;

        public long replayId = -1;
        public int SeedID = -1;


        public Dictionary<int, Pickup> itemData = new Dictionary<int, Pickup>();
        public Dictionary<int, Vector3> savedItemPos = new Dictionary<int, Vector3>();

        public Dictionary<Pickup, int> itemsData = new Dictionary<Pickup, int>();
        public List<string> playerData = new List<string>();
        public Dictionary<string, GameObject> playerDb = new Dictionary<string, GameObject>();

        public RecorderCore()
        {
            singleton = this;
            Timing.RunCoroutine(Replay());
            Timing.RunCoroutine(Recorder());
        }

        public void CreateFakePlayer(int clientid, string name, string userId, RoleType RoleType)
        {
            try
            {
                GameObject obj =
                    UnityEngine.Object.Instantiate(
                        NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
                CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
                if (ccm == null)
                    return;
                ccm.CurClass = RoleType;
                obj.GetComponent<NicknameSync>().Network_myNickSync = string.IsNullOrEmpty(name) ? "[REC] Unknown name" :  $"[REC] {name}";
                obj.GetComponent<QueryProcessor>().NetworkPlayerId = clientid;
                obj.GetComponent<QueryProcessor>()._ipAddress = "127.0.0.WAN";
                obj.transform.position = new Vector3(0f, 0f, 0f);
                NetworkServer.Spawn(obj);
                PlayerManager.AddPlayer(obj);
                Player ply_obj = new Player(obj);
                Player.Dictionary.Add(obj, ply_obj);

                Player.IdsCache.Add(ply_obj.Id, ply_obj);
                if (!playerDb.ContainsKey(userId))
                    playerDb.Add(userId, obj);
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public Queue<object> dataQueue = new Queue<object>();
        public bool endInstance = false;

        public IEnumerator<float> RecorderInstance()
        {
            using (var stream = new FileStream(Path.Combine("RecorderData", $"{Server.Port}", $"Record_{DateTime.Now.Ticks}.rd"), FileMode.CreateNew))
            {
                while (!endInstance)
                {
                    try
                    {
                        if (dataQueue.Count != 0)
                        {
                            for (int p = 0; p < dataQueue.Count; p++)
                            {
                                var ev = dataQueue.Dequeue();
                                if (ev is PlayerInfoData data)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<PlayerInfoData>(stream, data);
                                }
                                else if (ev is LeaveData data1)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<LeaveData>(stream, data1);
                                }
                                else if (ev is UpdateRoleData data2)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<UpdateRoleData>(stream, data2);
                                }
                                else if (ev is DoorData data3)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<DoorData>(stream, data3);
                                }
                                else if (ev is LiftData data4)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<LiftData>(stream, data4);
                                }
                                else if (ev is CreatePickupData data5)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<CreatePickupData>(stream, data5);
                                }
                                else if (ev is ReloadWeaponData data6)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<ReloadWeaponData>(stream, data6);
                                }
                                else if (ev is ShotWeaponData data7)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<ShotWeaponData>(stream, data7);
                                }
                                else if (ev is RemovePickupData data8)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<RemovePickupData>(stream, data8);
                                }
                                else if (ev is RoundEndData data9)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<RoundEndData>(stream, data9);
                                }
                                else if (ev is SeedData data10)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<SeedData>(stream, data10);
                                }
                                else if (ev is UpdatePickupData data11)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<UpdatePickupData>(stream, data11);
                                }
                                else if (ev is UpdatePlayerData data12)
                                {
                                    MessagePack.MessagePackSerializer.Serialize<UpdatePlayerData>(stream, data12);
                                }
                            }
                        }
                        if (Round.IsStarted)
                            MessagePack.MessagePackSerializer.Serialize<WaitFrameData>(stream, new WaitFrameData());
                        stream.Flush();
                    }catch(Exception ex)
                    {
                        Log.Info(ex.ToString());
                    }
                    yield return Timing.WaitForOneFrame;

                }
                stream.Close();
                yield break;
            }
        }

        public IEnumerator<float> Replay()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(MainClass.singleton.Config.replayDelay);
                /*if (replayRunning && replayStarted && !replayPaused)
                {
                    try
                    {
                        if (replayEvents.ContainsKey(currentFrameId))
                        {
                            foreach (var p in replayEvents[currentFrameId])
                            {
                                if (p is PlayerData)
                                {
                                    var plr = (p as PlayerData);
                                    switch (p.Event)
                                    {
                                        case EventType.CreatePlayer:
                                            if (playerDb.ContainsKey(plr.UserID))
                                                continue;
                                            Log.Info($"Create new fake player: {plr.UserID}");
                                            CreateFakePlayer(plr.UserName, plr.UserID, plr.Role);
                                            break;
                                        case EventType.RemovePlayer:
                                            if (playerDb.ContainsKey(plr.UserID))
                                            {
                                                Log.Info($"Destroy fake player: {plr.UserID}");
                                                var id = Player.Get(playerDb[plr.UserID]).Id;
                                                Player.Dictionary.Remove(playerDb[plr.UserID]);
                                                Player.IdsCache.Remove(id);
                                                PlayerManager.RemovePlayer(playerDb[plr.UserID]);
                                                NetworkServer.Destroy(playerDb[plr.UserID]);
                                                playerDb.Remove(plr.UserID);
                                            }
                                            break;
                                        case EventType.UpdatePlayer:
                                            if (playerDb.ContainsKey(plr.UserID))
                                            {
                                                var hub = Player.Get(playerDb[plr.UserID]);
                                                hub.ReferenceHub.inventory.Network_curItemSynced = plr.HoldingItem;
                                                hub.Rotations = plr.Rotation.Vector;
                                                hub.Position = plr.Position.Vector;
                                                hub.ReferenceHub.animationController.Networkspeed = plr.Speed.Vector;
                                                hub.ReferenceHub.animationController.NetworkcurAnim = plr.CurAnim;
                                                hub.ReferenceHub.animationController.Network_curMoveState = (byte)plr.MoveState;
                                            }
                                            else
                                            {
                                                CreateFakePlayer(plr.UserName, plr.UserID, plr.Role);
                                            }
                                            break;
                                    }
                                }
                                else if (p is PickupData)
                                {
                                    switch (p.Event)
                                    {
                                        case EventType.CreateItem:
                                            var itm = (p as PickupData);
                                            if (itemData.ContainsKey(itm.ID))
                                                continue;
                                            Log.Info($"Create new fake item: {itm.ID} Item: {itm.Item}");
                                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ReferenceHub.HostHub.inventory.pickupPrefab);
                                            NetworkServer.Spawn(gameObject);
                                            gameObject.GetComponent<Pickup>().SetupPickup((ItemType)itm.Item, -1f, ReferenceHub.HostHub.gameObject, new Pickup.WeaponModifiers(false, -1, -1, -1), itm.Position.Vector, Quaternion.Euler(new Vector3(0, 0, 0)));
                                            itemData.Add(itm.ID, gameObject.GetComponent<Pickup>());
                                            continue;
                                        case EventType.RemoveItem:
                                            var itm2 = (p as PickupData);
                                            if (itemData.ContainsKey(itm2.ID))
                                            {
                                                Log.Info($"Destroy fake item: {itm2.ID} Item: {itm2.Item}");
                                                NetworkServer.Destroy(itemData[itm2.ID].gameObject);
                                                itemData.Remove(itm2.ID);
                                            }
                                            continue;
                                        case EventType.UpdateItem:
                                            var itm3 = (p as PickupData);
                                            if (itemData.ContainsKey(itm3.ID))
                                            {
                                                var pick = itemData[itm3.ID];
                                                pick.Networkposition = itm3.Position.Vector;
                                                pick.Networkrotation = itm3.Rotation.Rotation;
                                                if (pick.NetworkitemId != itm3.Item)
                                                    pick.NetworkitemId = itm3.Item;
                                            }
                                            continue;
                                    }
                                }
                                else if (p is DoorData)
                                {
                                    var door = (p as DoorData);
                                    var doorpos = door.Position.Vector;

                                    Door bestDoor = null;
                                    float bestDistance = 999f;
                                    foreach(var dor in Map.Doors)
                                    {
                                        float distance = Vector3.Distance(dor.transform.position, doorpos);
                                        if (distance < bestDistance)
                                        {
                                            bestDoor = dor;
                                            bestDistance = distance;
                                        }
                                    }
                                    if (bestDoor != null)
                                    {
                                        bestDoor.NetworkisOpen = door.state;
                                        bestDoor.RpcDoSound();
                                    }
                                }
                                else if (p is LiftData)
                                {
                                    var lift = (p as LiftData);
                                    foreach (var lift2 in Map.Lifts)
                                        if (lift2.elevatorName == lift.ElevatorName)
                                            lift2.UseLift();
                                }
                                else if (p is WeaponData)
                                {
                                    var weapon = (p as WeaponData);
                                    if (!playerDb.ContainsKey(weapon.UserID))
                                        continue;
                                    var plr = Player.Get(playerDb[weapon.UserID]);
                                    switch(weapon.Event)
                                    {
                                        case EventType.WeaponFire:
                                            plr.ReferenceHub.weaponManager.RpcConfirmShot(false, weapon.WeaponID);
                                            continue;
                                        case EventType.WeaponReload:
                                            plr.ReferenceHub.weaponManager.RpcReload(weapon.WeaponID);
                                            continue;
                                    }
                                }
                                else if (p is ThrowGrenadeData)
                                {
                                    var grenade = (p as ThrowGrenadeData);
                                    if (!playerDb.ContainsKey(grenade.UserID))
                                        continue;
                                    var plr = Player.Get(playerDb[grenade.UserID]);
                                    plr.GrenadeManager.CallCmdThrowGrenade(grenade.Grenadeid, grenade.SlowThrow, grenade.Fusetime);
                                }
                            }
                        }
                    }catch(Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                    currentFrameId++;
                }
            }
            */
            }
        }

        public IEnumerator<float> Recorder()
        {
            while (true)
            {
                if (recorderRunning)
                {
                    foreach (var plr in Player.List)
                    {
                        try
                        {
                            if (plr.Role == RoleType.Spectator)
                                continue;
                            dataQueue.Enqueue(new UpdatePlayerData()
                            {
                                PlayerID = (sbyte)plr.Id,
                                MoveState = (byte)plr.MoveState,
                                HoldingItem = (sbyte)plr.Inventory.Network_curItemSynced,
                                CurrentAnim = plr.ReferenceHub.animationController.NetworkcurAnim,
                                Speed = plr.ReferenceHub.animationController.Networkspeed.GetData(),
                                Position = plr.Position.GetData(),
                                Rotation = plr.ReferenceHub.playerMovementSync.Rotations.GetData()
                            });
                        }
                        catch (Exception) { }
                    }
                    foreach (var item in UnityEngine.Object.FindObjectsOfType<Pickup>())
                    {
                        try
                        {
                            if (!itemsData.ContainsKey(item))
                                continue;
                            int id = itemsData[item];
                            if (!savedItemPos.ContainsKey(id))
                                continue;
                            if (savedItemPos[id] == item.position)
                                continue;
                            savedItemPos[id] = item.position;
                            dataQueue.Enqueue(new UpdatePickupData()
                            {
                                ItemID = id,
                                ItemType = (int)item.itemId,
                                Position = item.position.GetData(),
                                Rotation = item.rotation.GetData()
                            });
                        }
                        catch (Exception) { }
                    }
                }
                yield return Timing.WaitForSeconds(MainClass.singleton.Config.recordDelay);
            }
        }

      
    }
}
