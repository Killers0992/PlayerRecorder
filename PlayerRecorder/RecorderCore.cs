using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
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
    public class RecorderCore : MonoBehaviour
    {
        public static RecorderCore singleton;

        public static bool isRecording = false;
        public static bool isReplaying = false;

        public static bool isReplayReady = false;
        public static bool isReplayPaused = false;

        public int SeedID = -1;

        public static event EventHandler<RecordPlayer> RegisterRecordPlayer;
        public static event EventHandler<RecordPlayer> UnRegisterRecordPlayer;

        public static event EventHandler<RecordPickup> RegisterRecordPickup;
        public static event EventHandler<RecordPickup> UnRegisterRecordPickup;

        public static event EventHandler<object> ReceiveEvent;

        public static void OnRegisterRecordPlayer(RecordPlayer recordplayer)
        {
            RegisterRecordPlayer.Invoke(null, recordplayer);
        }

        public static void OnUnRegisterRecordPlayer(RecordPlayer recordplayer)
        {
            UnRegisterRecordPlayer.Invoke(null, recordplayer);
        }

        public static void OnRegisterRecordPickup(RecordPickup recordpickup)
        {
            RegisterRecordPickup.Invoke(null, recordpickup);
        }

        public static void OnUnRegisterRecordPickup(RecordPickup recordpickup)
        {
            UnRegisterRecordPickup.Invoke(null, recordpickup);
        }

        public static void OnReceiveEvent(object eventObject)
        {
            ReceiveEvent.Invoke(null, eventObject);
        }

        public Dictionary<int, Pickup> itemData = new Dictionary<int, Pickup>();

        public List<string> playerData = new List<string>();
        public Dictionary<string, GameObject> playerDb = new Dictionary<string, GameObject>();
        public EventHandlers handler;

        public float timeElapsed = 0f;

        void Start()
        {
            singleton = this;
            ReceiveEvent += EventReceived;
        }

        private void EventReceived(object sender, object ev)
        {
            MessagePack.MessagePackSerializer.Serialize<DelayData>(recordingStream, new DelayData() { DelayTime = timeElapsed });
            timeElapsed = 0;
            if (ev is PlayerInfoData data)
            {
                MessagePack.MessagePackSerializer.Serialize<PlayerInfoData>(recordingStream, data);
            }
            else if (ev is LeaveData data1)
            {
                MessagePack.MessagePackSerializer.Serialize<LeaveData>(recordingStream, data1);
            }
            else if (ev is UpdateRoleData data2)
            {
                MessagePack.MessagePackSerializer.Serialize<UpdateRoleData>(recordingStream, data2);
            }
            else if (ev is DoorData data3)
            {
                MessagePack.MessagePackSerializer.Serialize<DoorData>(recordingStream, data3);
            }
            else if (ev is LiftData data4)
            {
                MessagePack.MessagePackSerializer.Serialize<LiftData>(recordingStream, data4);
            }
            else if (ev is CreatePickupData data5)
            {
                MessagePack.MessagePackSerializer.Serialize<CreatePickupData>(recordingStream, data5);
            }
            else if (ev is ReloadWeaponData data6)
            {
                MessagePack.MessagePackSerializer.Serialize<ReloadWeaponData>(recordingStream, data6);
            }
            else if (ev is ShotWeaponData data7)
            {
                MessagePack.MessagePackSerializer.Serialize<ShotWeaponData>(recordingStream, data7);
            }
            else if (ev is RemovePickupData data8)
            {
                MessagePack.MessagePackSerializer.Serialize<RemovePickupData>(recordingStream, data8);
            }
            else if (ev is RoundEndData data9)
            {
                MessagePack.MessagePackSerializer.Serialize<RoundEndData>(recordingStream, data9);
            }
            else if (ev is SeedData data10)
            {
                MessagePack.MessagePackSerializer.Serialize<SeedData>(recordingStream, data10);
            }
            else if (ev is UpdatePickupData data11)
            {
                MessagePack.MessagePackSerializer.Serialize<UpdatePickupData>(recordingStream, data11);
            }
            else if (ev is UpdatePlayerData data12)
            {
                MessagePack.MessagePackSerializer.Serialize<UpdatePlayerData>(recordingStream, data12);
            }
            else if (ev is DelayData data123)
            {
                MessagePack.MessagePackSerializer.Serialize<DelayData>(recordingStream, data123);
            }
            recordingStream.Flush();
        }

        void Update()
        {
            if (isRecording && !EventHandlers.waitingforplayers)
                timeElapsed += Time.deltaTime;
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
                obj.GetComponent<QueryProcessor>().NetworkPlayerId = QueryProcessor._idIterator++;
                idDB.Add(clientid, obj.GetComponent<QueryProcessor>().NetworkPlayerId);
                obj.GetComponent<QueryProcessor>()._ipAddress = "127.0.0.WAN";
                obj.transform.position = new Vector3(0f, 0f, 0f);
                NetworkServer.Spawn(obj);
                PlayerManager.AddPlayer(obj);
                hubs.Add(obj.GetComponent<QueryProcessor>().NetworkPlayerId, ReferenceHub.GetHub(obj));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public Dictionary<int, int> idDB = new Dictionary<int, int>();
        public bool endInstance = false;

        public FileStream recordingStream;

        public void StartRecording()
        {   
            recordingStream = new FileStream(Path.Combine(MainClass.pluginDir, "RecorderData", $"{Server.Port}", $"Record_{DateTime.Now.Ticks}.rd"), FileMode.CreateNew);
            OnReceiveEvent(new SeedData()
            {
                Seed = UnityEngine.Object.FindObjectsOfType<SeedSynchronizer>()[0].Network_syncSeed
            });
        }



        public Dictionary<int, ReferenceHub> hubs = new Dictionary<int, ReferenceHub>();


        public IEnumerator<float> Replay(string path)
        {
            idDB = new Dictionary<int, int>();
            using (var stream = new FileStream(path, FileMode.Open))
            {

                while (true)
                {
                    if (isReplayPaused || (!isReplaying && isReplayReady))
                    {
                        isReplaying = true;
                        yield return Timing.WaitForOneFrame;
                        continue;
                    }
                    var oldPos = stream.Position;
                    var data = (MessagePack.MessagePackSerializer.Deserialize<object[]>(stream));


                    switch ((RecordEvents)data[0])
                    {
                        case RecordEvents.ReceiveSeed:
                            stream.Position = oldPos;
                            var seed = MessagePack.MessagePackSerializer.Deserialize<SeedData>(stream);
                            RecorderCore.singleton.SeedID = seed.Seed;
                            isReplayReady = true;
                            ReferenceHub.HostHub.playerStats.Roundrestart();
                            Log.Info($"Received seed, {seed.Seed}");
                            break;
                        case RecordEvents.PlayerInfo:
                            stream.Position = oldPos;
                            var pinfo = MessagePack.MessagePackSerializer.Deserialize<PlayerInfoData>(stream);
                            if (!idDB.ContainsKey(pinfo.PlayerID))
                            {
                                CreateFakePlayer(pinfo.PlayerID, pinfo.UserName, pinfo.UserID, RoleType.Spectator);
                                Log.Info($"New fake player created ID: {pinfo.PlayerID}, Nickname: {pinfo.UserName}, UserID: {pinfo.UserID}.");
                            }
                            break;
                        case RecordEvents.UpdatePlayer:
                            stream.Position = oldPos;
                            var uplayer = MessagePack.MessagePackSerializer.Deserialize<UpdatePlayerData>(stream);
                            var phub2 = Player.Get(idDB[uplayer.PlayerID]);
                            if (phub2 != null)
                            {
                                phub2.ReferenceHub.inventory.Network_curItemSynced = (ItemType)uplayer.HoldingItem;
                                phub2.ReferenceHub.animationController.NetworkcurAnim = uplayer.CurrentAnim;
                                phub2.ReferenceHub.animationController.Networkspeed = uplayer.Speed.SetVector();
                                phub2.ReferenceHub.animationController.Network_curMoveState = uplayer.MoveState;
                                phub2.Position = uplayer.Position.SetVector();
                                phub2.Rotations = uplayer.Rotation.SetVector();
                            }
                            break;
                        case RecordEvents.PlayerLeave:
                            stream.Position = oldPos;
                            var lplayer = MessagePack.MessagePackSerializer.Deserialize<LeaveData>(stream);
                            var phub3 = Player.Get(idDB[lplayer.PlayerID]);
                            if (phub3 != null)
                            {
                                var obj = phub3.GameObject;
                                Player.Dictionary.Remove(obj);
                                Player.IdsCache.Remove(idDB[lplayer.PlayerID]);
                                PlayerManager.RemovePlayer(obj);
                                NetworkServer.Destroy(obj);
                                Log.Info($"Fake player removed ID: {lplayer.PlayerID}.");
                            }
                            break;
                        case RecordEvents.CreatePickup:
                            stream.Position = oldPos;
                            var cpickup = MessagePack.MessagePackSerializer.Deserialize<CreatePickupData>(stream);
                            if (itemData.ContainsKey(cpickup.ItemID))
                                break;
                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ReferenceHub.HostHub.inventory.pickupPrefab);
                            NetworkServer.Spawn(gameObject);
                            gameObject.GetComponent<Pickup>().SetupPickup((ItemType)cpickup.ItemType, -1f, ReferenceHub.HostHub.gameObject, new Pickup.WeaponModifiers(false, -1, -1, -1), cpickup.Position.SetVector(), Quaternion.Euler(new Vector3(0, 0, 0)));
                            itemData.Add(cpickup.ItemID, gameObject.GetComponent<Pickup>());
                            Log.Info($"New fake item created ID: {cpickup.ItemID}, ItemType: {(ItemType)cpickup.ItemType}.");
                            break;
                        case RecordEvents.UpdatePickup:
                            stream.Position = oldPos;
                            var upickup = MessagePack.MessagePackSerializer.Deserialize<UpdatePickupData>(stream);
                            if (itemData.ContainsKey(upickup.ItemID))
                            {
                                var pick = itemData[upickup.ItemID];
                                pick.Networkposition = upickup.Position.SetVector();
                                pick.Networkrotation = upickup.Rotation.SetQuaternion();
                                if (pick.NetworkitemId != (ItemType)upickup.ItemType)
                                    pick.NetworkitemId = (ItemType)upickup.ItemType;
                            }
                            break;
                        case RecordEvents.RemovePickup:
                            stream.Position = oldPos;
                            var rpickup = MessagePack.MessagePackSerializer.Deserialize<RemovePickupData>(stream);
                            if (itemData.ContainsKey(rpickup.ItemID))
                            {
                                NetworkServer.Destroy(itemData[rpickup.ItemID].gameObject);
                                itemData.Remove(rpickup.ItemID);
                                Log.Info($"Fake item removed ID: {rpickup.ItemID}.");
                            }
                            break;
                        case RecordEvents.DoorState:
                            stream.Position = oldPos;
                            var ddata = MessagePack.MessagePackSerializer.Deserialize<DoorData>(stream);
                            var doorpos = ddata.Position.SetVector();

                            DoorVariant bestDoor = null;
                            float bestDistance = 999f;
                            foreach (var dor in Map.Doors)
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
                                bestDoor.TargetState = ddata.State;
                            }
                            break;
                        case RecordEvents.UpdateRole:
                            stream.Position = oldPos;
                            var urole = MessagePack.MessagePackSerializer.Deserialize<UpdateRoleData>(stream);
                            var phub4 = Player.Get(idDB[urole.PlayerID]);
                            if (phub4 != null)
                            {
                                phub4.ReferenceHub.characterClassManager.NetworkCurClass = (RoleType)urole.RoleID;
                                Log.Info($"Changed fake player role ID: {urole.PlayerID}, RoleType: {(RoleType)urole.RoleID}.");
                            }
                            break;
                        case RecordEvents.UseLift:
                            stream.Position = oldPos;
                            var ulift = MessagePack.MessagePackSerializer.Deserialize<LiftData>(stream);
                            foreach (var lift2 in Map.Lifts)
                                if (lift2.elevatorName == ulift.Elevatorname)
                                    lift2.UseLift();
                            break;
                        case RecordEvents.ShotWeapon:
                            stream.Position = oldPos;
                            var sweapon = MessagePack.MessagePackSerializer.Deserialize<ShotWeaponData>(stream);
                            var phub5 = Player.Get(sweapon.PlayerID + 30);
                            if (phub5 != null)
                                phub5.ReferenceHub.weaponManager.RpcConfirmShot(false, (int)phub5.ReferenceHub.weaponManager.curWeapon);
                            break;
                        case RecordEvents.ReloadWeapon:
                            stream.Position = oldPos;
                            var rweapon = MessagePack.MessagePackSerializer.Deserialize<ReloadWeaponData>(stream);
                            var phub6 = Player.Get(rweapon.PlayerID + 30);
                            if (phub6 != null)
                                phub6.ReferenceHub.weaponManager.RpcReload(phub6.ReferenceHub.weaponManager.curWeapon);
                            break;
                        case RecordEvents.RoundEnd:
                            Map.Broadcast(10, "PlayerRecord | ROUND ENDED");
                            break;
                        case RecordEvents.Delay:
                            stream.Position = oldPos;
                            var delay = MessagePack.MessagePackSerializer.Deserialize<DelayData>(stream);
                            Log.Info($"Waiting {delay.DelayTime} seconds.");
                            break;
                    }
                    if (stream.Length == 0)
                        break;
                }
            }
            Log.Info("Replay ended");
            yield break;
        }
    }
}
