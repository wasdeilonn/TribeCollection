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
using ClipperLib;

namespace TribeCollection.Crimson;

public static class UnitAbilityManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MoveAction), nameof(MoveAction.Execute))]
    private static void MovePatch(GameState state, MoveAction __instance)
    {
        if (!state.TryGetUnit(__instance.UnitId, out var unit))
        {
            M.modLogger.LogError("good one jackass");
            return;
        }

        if (__instance.subActions != null)
        {
            __instance.subActions.Clear();
        }

        if (__instance.Reason != MoveAction.MoveReason.Push && unit.HasAbility(Main.Impact) && unit.attacked == false)
        {
            ImpactAction impactAction = PolibActionManager.MakeIl2CppAction<ImpactAction>();
            impactAction.PlayerId = __instance.PlayerId;
            impactAction.Coordinates = __instance.Path[0];
            __instance.AddSubAction(impactAction);
        }

        if (__instance.Reason != MoveAction.MoveReason.Push && unit.HasAbility(Main.DoubleMove) && unit.attacked == false)
        {
            unit.attacked = true;
            unit.moved = false;
        }

        __instance.CommitSubActionsToStack(state.ActionStack);
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
                destination,
                start
            };

            __result = list.ToIl2List();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Execute))]
    private static bool InfectPatch(GameState state, AttackCommand __instance)
    {
        TileData origin = state.Map.GetTile(__instance.Origin);

        if (origin.unit.HasAbility(Main.Infect))
        {
            InfectAction infectAction = PolibActionManager.MakeIl2CppAction<InfectAction>();
            infectAction.Origin = __instance.Origin;
            infectAction.Target = __instance.Target;
            infectAction.PlayerId = __instance.PlayerId;
            state.ActionStack.Add(infectAction);
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CommandUtils), nameof(CommandUtils.GetUnitActions))]
    private static void AddCommands(ref Il2Gen.List<CommandBase> __result, GameState gameState, PlayerState player, TileData tile, bool includeUnavailable)
    {
        if (tile.unit != null && !tile.unit.attacked && !tile.unit.moved && tile.unit.HasEffect(Main.Infected))
        {
            TransformCommand command = PolibCommandManager.MakeIl2CppCommand<TransformCommand>();
            command.PlayerId = player.Id;
            command.Coordinates = tile.coordinates;
            CommandUtils.AddCommand(gameState, __result, command, includeUnavailable);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitDataExtensions), nameof(UnitDataExtensions.HasAbility), typeof(UnitState), typeof(UnitAbility.Type))]
    private static void DemotivateInfected(this UnitState unit, UnitAbility.Type ability, ref bool __result)
    {
        if (unit != null && unit.HasEffect(Main.Infected) && ability == EnumCache<UnitAbility.Type>.GetType("polib_demotivate"))
        {
            __result = true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(KillUnitAction), nameof(KillUnitAction.Execute))]
    private static void SpawnSeedOnInfectedKill(GameState gameState, KillUnitAction __instance)
    {
        TileData tile = gameState.Map.GetTile(__instance.Coordinates);
        UnitState unit = tile.unit;

        if (unit != null && unit.HasEffect(Main.Infected))
        {
            gameState.ActionStack.Add(new TrainAction(unit.owner, EnumCache<UnitData.Type>.GetType("chrymeaseed_unit"), tile.coordinates, 0));
        }
    }
}
