using HarmonyLib;
using UnityEngine;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;

namespace TribeCollection.Crimson.Visual;

public static class JumpManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MoveReaction), nameof(MoveReaction.Execute))]
    private static void Jump_Reaction(MoveReaction __instance)
    {
        if (!GameManager.GameState.TryGetUnit(__instance.action.UnitId, out var unit))
        {
            return;
        }

        if (unit.HasAbility(Main.Jump))
        {
            Tile origin = GameManager.GameState.Map.GetTile(__instance.action.Path[__instance.action.Path.Count - 1]).GetInstance();
            Tile target = GameManager.GameState.Map.GetTile(__instance.action.Path[0]).GetInstance();

            if (origin != null && !origin.IsHidden)
            {
                origin.Sway();
            }

            target.Unit.swayTween = Jump.JumpSway(target.Unit.spriteContainer);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Unit), nameof(Unit.AnimatePathMove))]
    private static bool Jump_AnimatePathMoveRedirect(Unit __instance, Il2Gen.List<WorldCoordinates> path, Il2CppSystem.Action onComplete = null)
    {
        if (__instance.unitData.HasAbility(Main.Jump))
        {
            __instance.AnimatePathMoveJump(path, Jump.DURATION, onComplete);
            return false;
        }
        return true;
    }
}
