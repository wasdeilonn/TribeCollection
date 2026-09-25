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
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using NaughtyAttributes;

namespace TribeCollection.Crimson;

public static class Visual
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof(MapRenderContext))]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof(MapRenderContext), typeof(SkinVisualsTransientData))]
    private static void TintRed(Tile __instance)
    {
        if (__instance.data.climate == Main.Crimson)
        {
            if (
                __instance.data.terrain == Polytopia.Data.TerrainData.Type.Water ||
                __instance.data.terrain == Polytopia.Data.TerrainData.Type.Ice
                )
            {
                TerrainMaterialHelper.SetSpriteSaturated(__instance.terrainRenderer.SpriteRenderer, desaturated: true);
                TerrainMaterialHelper.SetSpriteTint(__instance.terrainRenderer.spriteRenderer, new UnityEngine.Color(1, 0.4f, 0.4f));
            }
            if (__instance.data.terrain == Polytopia.Data.TerrainData.Type.Ocean)
            {
                TerrainMaterialHelper.SetSpriteSaturated(__instance.terrainRenderer.SpriteRenderer, desaturated: true);
                TerrainMaterialHelper.SetSpriteTint(__instance.terrainRenderer.spriteRenderer, new UnityEngine.Color(0.8f, 0.2f, 0.2f));
            }
            __instance.shoreline.north.Sprite = PolyMod.Registry.GetSprite("shoreline");
            __instance.shoreline.north.Color = new Color(1, 0.2f, 0.2f);
            __instance.shoreline.south.Sprite = PolyMod.Registry.GetSprite("shoreline");
            __instance.shoreline.south.Color = new Color(1, 0.2f, 0.2f);
            __instance.shoreline.west.Sprite = PolyMod.Registry.GetSprite("shoreline");
            __instance.shoreline.west.Color = new Color(1, 0.2f, 0.2f);
            __instance.shoreline.east.Sprite = PolyMod.Registry.GetSprite("shoreline");
            __instance.shoreline.east.Color = new Color(1, 0.2f, 0.2f);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CityStatusDisplay), nameof(CityStatusDisplay.SetCity))]
    public static void CityStatusDisplay_SetCity(CityStatusDisplay __instance, City city)
    {
        if (city != null && city.CapitalOf != null && city.CapitalOf.tribe == Main.Crimson)
        {
            Color fillColorFeed = Color.yellow;
            Color fillColor = Color.coral;
            Color fullColor = Color.softYellow;
            Color baseColorFeed = __instance.progressBar.baseColor;

            baseColorFeed.r *= 0.5f;
            baseColorFeed.g *= 0.5f;
            baseColorFeed.b *= 0.5f;

            int i = 0;
            foreach (CityStatusProgressSegment segment in __instance.progressBar.segments)
            {
                bool filled = false;
                bool negativeFilled = false;
                bool full = false;
                if (i < __instance.progressBar.filledFields) filled = true;
                if (__instance.progressBar.filledFields < 0 && i < Math.Abs(__instance.progressBar.filledFields)) negativeFilled = true;
                if (__instance.progressBar.filledFields >= 3) full = true;
                
                
                if (negativeFilled)
                {
                    segment.SetColor(__instance.progressBar.negativeColor);
                }
                else if (filled)
                {
                    if (i < 3)
                    {
                        if (full)
                        {
                            segment.SetColor(fullColor);
                        }
                        else
                        {
                            segment.SetColor(fillColorFeed);
                        }
                    }
                    else
                    {
                        segment.SetColor(fillColor);
                    }
                }
                else
                {
                    if (i < 3)
                    {
                        segment.SetColor(baseColorFeed);
                    }
                    else
                    {
                        segment.SetColor(__instance.progressBar.baseColor);
                    }
                }
                
                i++;
            }
        }
    }

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
            __instance.AnimatePathMoveJump(path, 0.55f, onComplete);
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
            GameManager.DelayCall(0.6f, onComplete);
        }
    }
}
