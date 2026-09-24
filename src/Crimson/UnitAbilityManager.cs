using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Polytopia;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;
using DG.Tweening;
using Polibrary;

namespace TribeCollection.Crimson;

public static class UnitAbilityManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MoveAction), nameof(MoveAction.Execute))]
    private static void DoubleMove_Move(GameState state, MoveAction __instance)
    {
        if (!state.TryGetUnit(__instance.UnitId, out var unit))
        {
            M.modLogger.LogError("good one jackass");
            return;
        }

        if (unit.HasAbility(Main.DoubleMove) && unit.attacked == false)
        {
            unit.attacked = true;
            unit.moved = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PathFinder), nameof(PathFinder.GetMoveOptions))]
    private static void PathFinder_GetMoveOptions(this GameState gameState, WorldCoordinates start, int maxCost, UnitState unit, ref Il2Gen.List<WorldCoordinates> __result)
    {
        TileData origin = gameState.Map.GetTile(start);
        PathFinderSettings settings = PathFinderSettings.CreateForUnit(unit, gameState);
        if (unit.HasAbility(Main.Jump))
        {
            List<WorldCoordinates> list = new();
            foreach (TileData tile in gameState.Map.GetArea(start, unit.UnitData.movement, true))
            {
                if (PathFinder.IsTileAccessible(tile, origin, settings) && tile.unit == null)
                {
                    list.Add(tile.coordinates);
                }
            }
            __result = list.ToIl2List();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PathFinder), nameof(PathFinder.GetPath), typeof(GameState), typeof(WorldCoordinates), typeof(WorldCoordinates), typeof(int), typeof(UnitState))]
    private static void PathFinder_GetPath(this GameState gameState, WorldCoordinates start, WorldCoordinates destination, int maxCost, UnitState unit, ref Il2Gen.List<WorldCoordinates> __result)
    {
        TileData tile = gameState.Map.GetTile(destination);
        PathFinderSettings settings = PathFinderSettings.CreateForUnit(unit, gameState);
        if (unit.HasAbility(Main.Jump))
        {
            if (!PathFinder.IsAllowedToFinishOnTile(settings, tile))
            {
                __result = null;
            }
            List<WorldCoordinates> list = new()
            {
                start,
                destination
            };

            __result = list.ToIl2List();
        }
    }
}
