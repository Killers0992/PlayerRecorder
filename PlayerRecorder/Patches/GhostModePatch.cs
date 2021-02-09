﻿using CustomPlayerEffects;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using PlayerRecorder;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Scp096 = PlayableScps.Scp096;
[HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.TransmitData))]
[HarmonyPriority(Priority.First)]
internal class GhostModePatch
{
    private static readonly Vector3 GhostPos = Vector3.up * 6000f;

    private static bool Prefix(PlayerPositionManager __instance)
    {
        try
        {
            if (++__instance._frame != __instance._syncFrequency)
                return false;

            __instance._frame = 0;

            List<GameObject> players = PlayerManager.players;
            __instance._usedData = players.Count;

            if (__instance._receivedData == null
                || __instance._receivedData.Length < __instance._usedData)
            {
                __instance._receivedData = new PlayerPositionData[__instance._usedData * 2];
            }

            for (int index = 0; index < __instance._usedData; ++index)
                __instance._receivedData[index] = new PlayerPositionData(ReferenceHub.GetHub(players[index]));

            if (__instance._transmitBuffer == null
                || __instance._transmitBuffer.Length < __instance._usedData)
            {
                __instance._transmitBuffer = new PlayerPositionData[__instance._usedData * 2];
            }

            foreach (GameObject gameObject in players)
            {
                if (gameObject.GetComponent<ReplayPlayer>() != null)
                {
                    continue;
                }

                Player player = Player.Get(gameObject);
                Array.Copy(__instance._receivedData, __instance._transmitBuffer, __instance._usedData);

                if (player.Role.Is939())
                {
                    for (int index = 0; index < __instance._usedData; ++index)
                    {
                        if (__instance._transmitBuffer[index].position.y < 800f)
                        {
                            ReferenceHub hub2 = ReferenceHub.GetHub(__instance._transmitBuffer[index].playerID);

                            if (hub2.characterClassManager.CurRole.team != Team.SCP
                                && hub2.characterClassManager.CurRole.team != Team.RIP
                                && !hub2
                                    .GetComponent<Scp939_VisionController>()
                                    .CanSee(player.ReferenceHub.characterClassManager.Scp939))
                            {
                                MakeGhost(index, __instance._transmitBuffer);
                            }
                        }
                    }
                }
                else if (player.Role != RoleType.Spectator && player.Role != RoleType.Scp079)
                {
                    for (int index = 0; index < __instance._usedData; ++index)
                    {
                        PlayerPositionData ppd = __instance._transmitBuffer[index];
                        Player currentTarget = Player.Get(ppd.playerID);
                        Scp096 scp096 = player.ReferenceHub.scpsController.CurrentScp as Scp096;

                        if (currentTarget?.ReferenceHub == null)
                            continue;

                        Vector3 vector3 = ppd.position - player.ReferenceHub.playerMovementSync.RealModelPosition;
                        if (Math.Abs(vector3.y) > 35f)
                        {
                            MakeGhost(index, __instance._transmitBuffer);
                        }
                        else
                        {
                            float sqrMagnitude = vector3.sqrMagnitude;
                            if (player.ReferenceHub.playerMovementSync.RealModelPosition.y < 800f)
                            {
                                if (sqrMagnitude >= 1764f)
                                {
                                    if (!(sqrMagnitude < 4225f))
                                    {
                                        MakeGhost(index, __instance._transmitBuffer);
                                        continue;
                                    }
                                    if (!(currentTarget.ReferenceHub.scpsController.CurrentScp is Scp096 scp) || !scp.EnragedOrEnraging)
                                    {
                                        MakeGhost(index, __instance._transmitBuffer);
                                        continue;
                                    }
                                }
                            }
                            else if (sqrMagnitude >= 7225f)
                            {
                                MakeGhost(index, __instance._transmitBuffer);
                                continue; // As the target is already ghosted
                            }

                            // The code below doesn't have to follow a ELSE statement!
                            // Otherwise Scp268 won't be processed

                            if (scp096 != null
                                && scp096.EnragedOrEnraging
                                && !scp096.HasTarget(currentTarget.ReferenceHub)
                                && currentTarget.Team != Team.SCP)
                            {
#if DEBUG
                                Log.Debug($"[Scp096@GhostModePatch] {player.UserId} can't see {currentTarget.UserId}");
#endif
                                MakeGhost(index, __instance._transmitBuffer);
                            }
                            else if (currentTarget.ReferenceHub.playerEffectsController.GetEffect<Scp268>().Enabled)
                            {
                                bool flag2 = false;
                                if (scp096 != null)
                                    flag2 = scp096.HasTarget(currentTarget.ReferenceHub);

                                if (player.Role != RoleType.Scp079
                                    && player.Role != RoleType.Spectator
                                    && !flag2)
                                {
                                    MakeGhost(index, __instance._transmitBuffer);
                                }
                            }
                        }
                    }
                }

                // We do another FOR for the ghost things
                // because it's hard to do it without
                // whole code changes in the game code
                for (var z = 0; z < __instance._usedData; z++)
                {
                    var ppd = __instance._transmitBuffer[z];

                    // Do you remember the bug
                    // when you can't pick up any item?
                    // - Me too;
                    // It was because for a reason
                    // we made the source player
                    // invisible to themself
                    if (player.Id == ppd.playerID)
                        continue;

                    // If it's already has the ghost position
                    if (ppd.position == GhostPos)
                        continue;

                    if (!ReferenceHub.TryGetHub(ppd.playerID, out var targetHub))
                        continue;

                    var target = GetPlayerOrServer(targetHub.gameObject);
                    // If for some reason the player/their ref hub is null
                    if (target?.ReferenceHub == null)
                        continue;


                    if (target.IsInvisible || PlayerCannotSee(player, target.Id))
                    {
                        MakeGhost(z, __instance._transmitBuffer);
                    }
                    // Rotate the player because
                    // those movement checks are
                    // in client-side
                    else if (player.Role == RoleType.Scp173
                        && ((!Exiled.Events.Events.Instance.Config.CanTutorialBlockScp173
                                && target.Role == RoleType.Tutorial)
                            || Scp173.TurnedPlayers.Contains(target)))
                    {
                        RotatePlayer(z, __instance._transmitBuffer, FindLookRotation(player.Position, target.Position));
                    }
                }

                NetworkConnection networkConnection = player.ReferenceHub.characterClassManager.netIdentity.isLocalPlayer
                    ? NetworkServer.localConnection
                    : player.ReferenceHub.characterClassManager.netIdentity.connectionToClient;
                if (__instance._usedData <= 20)
                {
                    networkConnection.Send(
                        new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, (byte)__instance._usedData, 0), 1);
                }
                else
                {
                    byte part;
                    for (part = 0; part < __instance._usedData / 20; ++part)
                        networkConnection.Send(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, 20, part), 1);
                    byte count = (byte)(__instance._usedData % (part * 20));
                    if (count > 0)
                        networkConnection.Send(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, count, part), 1);
                }
            }

            return false;
        }
        catch (Exception exception)
        {
            Log.Error($"GhostMode error: {exception}");
            return true;
        }
    }

    private static Vector3 FindLookRotation(Vector3 player, Vector3 target) => (target - player).normalized;

    // It's called when the player checks to see a player,
    // as the method called that the player CANNOT see another player
    // so an execution result will be:
    // true -> the player can't see another player
    // false -> the player can see another player
    private static bool PlayerCannotSee(Player source, int playerId) => source.TargetGhostsHashSet.Contains(playerId) || source.TargetGhosts.Contains(playerId);

    private static void MakeGhost(int index, PlayerPositionData[] buff) => buff[index] = new PlayerPositionData(GhostPos, buff[index].rotation, buff[index].playerID);

    private static void RotatePlayer(int index, PlayerPositionData[] buff, Vector3 rotation) => buff[index]
        = new PlayerPositionData(buff[index].position, Quaternion.LookRotation(rotation).eulerAngles.y, buff[index].playerID);

    private static Player GetPlayerOrServer(GameObject gameObject)
    {
        if (gameObject == null)
            return null;

        var refHub = ReferenceHub.GetHub(gameObject);

        // The only reason is that the server is also a player,
        // and we've seen a lot of NullRef exceptions at the place
        // where we call this method
        return refHub.isLocalPlayer ? Server.Host : Player.Get(gameObject);
    }
}