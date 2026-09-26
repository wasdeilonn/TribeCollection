using HarmonyLib;
using UnityEngine;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;

namespace TribeCollection.Crimson.Visual;

public static class Jump
{
    public const float DURATION = 0.5f;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MoveReaction), nameof(MoveReaction.Execute))]
    private static void Jump_Reaction(MoveReaction __instance)
    {
        if (!GameManager.GameState.TryGetUnit(__instance.action.UnitId, out var unit))
        {
            M.modLogger.LogError("good one jackass");
            return;
        }

        if (unit.HasAbility(Main.Jump))
        {
            Tile origin = GameManager.GameState.Map.GetTile(__instance.action.Path[__instance.action.Path.Count - 1]).GetInstance();
            Tile target = GameManager.GameState.Map.GetTile(__instance.action.Path[0]).GetInstance();
            origin.Sway();
            target.Unit.swayTween = JumpSway(target.Unit.spriteContainer);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Unit), nameof(Unit.AnimatePathMove))]
    private static bool Jump_AnimatePathMoveRedirect(Unit __instance, Il2Gen.List<WorldCoordinates> path, Il2CppSystem.Action onComplete = null)
    {
        if (__instance.unitData.HasAbility(Main.Jump))
        {
            __instance.AnimatePathMoveJump(path, DURATION, onComplete);
            return false;
        }
        return true;
    }

    public static Tween JumpSway(Transform target, float delay = 0f)
    {
        Vector3 targetPos = target.localPosition;
        return TweenSettingsExtensions.OnComplete
        (
            TweenSettingsExtensions.SetDelay
            (
                TweenSettingsExtensions.SetEase
                (
                    ShortcutExtensions.DOLocalMoveY
                    (
                        target, 
                        target.localPosition.y + 0.6f, 
                        0.155f
                    ),
                    Ease.OutQuad
                ), 
                delay
            ),
            (TweenCallback) delegate
            {
                TweenSettingsExtensions.SetEase
                (
                    ShortcutExtensions.DOLocalMoveY
                    (
                        target, 
                        targetPos.y, 
                        0.155f
                    ), 
                    Ease.InQuad
                );
            }
        );
    }

    public static void AnimatePathMoveJump(this Unit unit, Il2Gen.List<WorldCoordinates> path, float duration, Il2CppSystem.Action onComplete = null)
    {
        if (unit.swayTween != null)
        {
            TweenUtils.KillTween(unit.swayTween);
        }
        WorldCoordinates coordinates = path[path.Count - 1];
        unit.transform.position = coordinates.ToPosition();
        unit.PlayMoveSFX(path[0]);
        float num = duration;
        float progress = 0f;
        WorldCoordinates lastFrom = WorldCoordinates.NULL_COORDINATES;
        WorldCoordinates lastTo = WorldCoordinates.NULL_COORDINATES;
        unit.moveTween = TweenSettingsExtensions.OnComplete(DOTween.To((DOGetter<float>)(() => progress), (DOSetter<float>)delegate(float newProgress)
        {
            progress = newProgress;
            float num2 = GameManager.GetSharedAnimationCurves().moveUnitAnimationCurve.Evaluate(progress) * (path.Count - 1);
            int num3 = Mathf.FloorToInt(num2);
            int num4 = path.Count - 1 - num3;
            int index = Mathf.Max(num4 - 1, 0);
            float num5 = num2 - num3;
            Tile tileInstance = MapRenderer.Current.GetTileInstance(path[index]);
            if (tileInstance != null && !unit.GetVisible() && !tileInstance.IsHidden && !unit.IsInvisibleForLocalPlayer())
            {
                unit.SetVisible(true);
            }
            if (path[num4] != lastFrom || path[index] != lastTo)
            {
                if (path[num4] != path[index])
                {
                    unit.FlipUnit(path[num4], path[index]);
                }
                lastFrom = path[num4];
                lastTo = path[index];
            }
            unit.transform.position = Vector3.LerpUnclamped(path[num4].ToPosition(), path[index].ToPosition(), num5);
        }, 1f, num), (TweenCallback)delegate
        {
            if (unit.Tile != null)
            {
                unit.Tile.RenderUnit();
            }
            unit.OnMoveComplete();
        });
        if (unit.unitState.HasFollower() && onComplete != null)
        {
            GameManager.DelayCall(80, onComplete);
        }
        else
        {
            GameManager.DelayCall(DURATION * 0.6f, onComplete);
        }
    }
}
