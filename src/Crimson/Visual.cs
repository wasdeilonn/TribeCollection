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
}
