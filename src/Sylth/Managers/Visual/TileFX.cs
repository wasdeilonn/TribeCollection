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

namespace TribeCollection.Sylth.Visual;

public static class TileFX
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof(MapRenderContext))]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof(MapRenderContext), typeof(SkinVisualsTransientData))]
    private static void Tint(Tile __instance)
    {
        if (__instance.data.HasEffect(Main.Veiled))
        {
            TerrainMaterialHelper.SetSpriteTint(__instance.terrainRenderer.SpriteRenderer, new Color(0.6f, 0f, 0.8f, 0.5f));
            TerrainMaterialHelper.SetSpriteTint(__instance.mountainRenderer, new Color(0.6f, 0f, 0.8f, 0.5f));
            TerrainMaterialHelper.SetSpriteTint(__instance.forestRenderer, new Color(0.6f, 0f, 0.8f, 0.5f));
            if (__instance.Resource != null)
            TerrainMaterialHelper.SetSpriteTint(__instance.Resource.SpriteRenderer, new Color(1f, 0f, 0.6f));
            if (__instance.Improvement != null)
            TerrainMaterialHelper.SetSpriteTint(__instance.Improvement.SpriteRenderer, new Color(1f, 0f, 0.6f));

            __instance.shoreline.north.Sprite = PolyMod.Registry.GetSprite("shoreline");
            __instance.shoreline.north.Color = new Color(1f, 0f, 0.6f);
            __instance.shoreline.south.Sprite = PolyMod.Registry.GetSprite("shoreline");
            __instance.shoreline.south.Color = new Color(1f, 0f, 0.6f);
            __instance.shoreline.west.Sprite = PolyMod.Registry.GetSprite("shoreline");
            __instance.shoreline.west.Color = new Color(1f, 0f, 0.6f);
            __instance.shoreline.east.Sprite = PolyMod.Registry.GetSprite("shoreline");
            __instance.shoreline.east.Color = new Color(1f, 0f, 0.6f);
        }
    }
}
