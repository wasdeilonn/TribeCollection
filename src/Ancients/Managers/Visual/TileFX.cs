using HarmonyLib;
using Polytopia.Data;
using Il2Gen = Il2CppSystem.Collections.Generic;
using Polibrary.PolyScript;
using M = TribeCollection.Main;
using UnityEngine;


namespace TribeCollection.Ancients.Visual;

public static class TileFX
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof(MapRenderContext))]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof(MapRenderContext), typeof(SkinVisualsTransientData))]
    private static void ResetTile(Tile __instance)
    {
        if (__instance.data.unit != null)
        {
            foreach (SkinVisualsReference.VisualPart visualPart in __instance.Unit.skinVisuals.visualParts)
            {
                if (visualPart != null)
                {
                    if (visualPart.renderer != null)
                    {
                        if (visualPart.renderer.spriteRenderer != null)
                        {
                            if (__instance.data.unit.HasEffect(Main.Hammered))
                            GlobalTint.Register(visualPart.renderer.spriteRenderer);
                            else
                            GlobalTint.Unregister(visualPart.renderer.spriteRenderer);
                        }
                    }
                }
            }
        }
    }
}