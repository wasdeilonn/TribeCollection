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
}
