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
using Unity.Mathematics;

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
    [HarmonyPatch(typeof(CityStatusProgressBar), nameof(CityStatusProgressBar.UpdateFields))]
    public static void CityStatusProgressBar_UpdateFields(CityStatusProgressBar __instance)
    {
        if (GameManager.LocalPlayer.tribe == Main.Crimson)
        {
            Color fillColorFeed = Color.yellow;
            Color fillColor = Color.coral;
            Color fullColor = Color.softYellow;
            Color baseColorFeed = __instance.baseColor;

            baseColorFeed.r *= 0.5f;
            baseColorFeed.g *= 0.5f;
            baseColorFeed.b *= 0.5f;

            int i = 0;
            foreach (CityStatusProgressSegment segment in __instance.segments)
            {
                bool filled = false;
                bool negativeFilled = false;
                bool full = false;
                if (i < __instance.filledFields) filled = true;
                if (__instance.filledFields < 0 && i < Math.Abs(__instance.filledFields)) negativeFilled = true;
                if (__instance.filledFields >= 3) full = true;
                
                
                if (negativeFilled)
                {
                    segment.SetColor(__instance.negativeColor);
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
                        segment.SetColor(__instance.baseColor);
                    }
                }
                
                i++;
            }
        }
    }
}
