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
}
