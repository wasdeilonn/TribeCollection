using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using TribeCollection.Ancients.Visual;
using Il2Gen = Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace TribeCollection;

public static class Main
{
    public static ManualLogSource modLogger;
    public static void Load(ManualLogSource logger)
    {
        modLogger = logger;
        Harmony.CreateAndPatchAll(typeof(Main));
        Harmony.CreateAndPatchAll(typeof(Crimson.Main));
        Harmony.CreateAndPatchAll(typeof(Crimson.UnitAbilityManager));
        Harmony.CreateAndPatchAll(typeof(Crimson.Visual.Misc));
        Harmony.CreateAndPatchAll(typeof(Crimson.Visual.Jump));
        Harmony.CreateAndPatchAll(typeof(Crimson.FeedManager));

        Harmony.CreateAndPatchAll(typeof(Sylth.Main));
        Harmony.CreateAndPatchAll(typeof(Sylth.VeilManager));
        Harmony.CreateAndPatchAll(typeof(Sylth.InvisibilityManager));
        Harmony.CreateAndPatchAll(typeof(Sylth.Visual.TileFX));

        Harmony.CreateAndPatchAll(typeof(Ancients.Main));
        Harmony.CreateAndPatchAll(typeof(Ancients.ChargeManager));
        Harmony.CreateAndPatchAll(typeof(Ancients.ConductionManager));
        Harmony.CreateAndPatchAll(typeof(Ancients.LightningManager));
        Harmony.CreateAndPatchAll(typeof(Ancients.PushManager));
        Harmony.CreateAndPatchAll(typeof(Ancients.RedirectionManager));
        Harmony.CreateAndPatchAll(typeof(Ancients.TechManager));
        Harmony.CreateAndPatchAll(typeof(Ancients.RewardManager));
        Harmony.CreateAndPatchAll(typeof(Ancients.Visual.Glow));
        PolyMod.Loader.AddPatchDataType("sfx", typeof(SFXTypes));
        PolyMod.Loader.AddPatchDataType("improvementEffect", typeof(ImprovementEffect));

        ClassInjector.RegisterTypeInIl2Cpp<GlobalTint>();
        var go = new GameObject("GlobalTint");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<GlobalTint>(); 
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof(MapRenderContext))]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof(MapRenderContext), typeof(SkinVisualsTransientData))]
    private static void ResetTile(Tile __instance)
    {
        TerrainMaterialHelper.SetSpriteTint(__instance.terrainRenderer.SpriteRenderer, new UnityEngine.Color(1f, 1f, 1f, 1f));

        if (__instance.Resource != null)
        {
            TerrainMaterialHelper.SetSpriteTint(__instance.Resource.SpriteRenderer, new UnityEngine.Color(1f, 1f, 1f, 1f));
        }

        //__instance.shoreline.north.Sprite = ; TODO: somehow get the shoreline sprites to reset
        TerrainMaterialHelper.SetSpriteTint(__instance.shoreline.north, new UnityEngine.Color(1f, 1f, 1f, 1f));
        //__instance.shoreline.south.Sprite = PolyMod.Registry.GetSprite("shoreline");
        TerrainMaterialHelper.SetSpriteTint(__instance.shoreline.south, new UnityEngine.Color(1f, 1f, 1f, 1f));
        //__instance.shoreline.west.Sprite = PolyMod.Registry.GetSprite("shoreline");
        TerrainMaterialHelper.SetSpriteTint(__instance.shoreline.west, new UnityEngine.Color(1f, 1f, 1f, 1f));
        //__instance.shoreline.east.Sprite = PolyMod.Registry.GetSprite("shoreline");
        TerrainMaterialHelper.SetSpriteTint(__instance.shoreline.east, new UnityEngine.Color(1f, 1f, 1f, 1f));
    }

    public static T Last<T>(this List<T> list)
    {
        return list[list.Count - 1];
    }
    public static T Last<T>(this Il2Gen.List<T> list)
    {
        return list[list.Count - 1];
    }
    public static T Last<T>(this T[] list)
    {
        return list[list.Length - 1];
    }
    public static TileData GetNeighbor(this TileData tile, GridDirection direction, GameState state)
    {
        WorldCoordinates toAdd = direction.ToCoordinates();

        return state.Map.GetTile(tile.coordinates + toAdd);
    }
}
