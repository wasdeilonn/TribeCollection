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

namespace TribeCollection.Crimson;

public static class UnitAbilityManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartTurnAction), nameof(StartTurnAction.Execute))]
    private static void DoubleMove_StartTurn(GameState state, StartTurnAction __instance)
    {
        foreach (TileData tile in state.Map.tiles)
        {
            if (tile.unit == null || !tile.unit.HasAbility(Main.DoubleMove)) continue;

            tile.unit.AddEffect(Main.MoveCharged);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MoveAction), nameof(MoveAction.Execute))]
    private static void DoubleMove_Move(GameState state, MoveAction __instance)
    {
        if (!state.TryGetUnit(__instance.UnitId, out var unit))
        {
            M.modLogger.LogError("good one jackass");
            return;
        }

        if (unit.HasEffect(Main.MoveCharged))
        {
            unit.RemoveEffect(Main.MoveCharged);
            unit.moved = false;
        }
    }
}
