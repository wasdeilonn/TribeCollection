using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Polytopia;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using Polibrary.PolyScript;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection;

public static class CrimsonMain
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.AddGameLogicPlaceholders))]
    private static void Register()
    {
        if (
            !EnumCache<TribeAbility.Type>.TryGetType("feed_tribeability", out Feed)
            || !EnumCache<CityReward>.TryGetType("growth_reward", out Grow)
        )
        {
            Main.modLogger.LogError("cant get enum stuff for crimson");
            return;
        }
    }

    public static TribeAbility.Type Feed;
    public static CityReward Grow;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ActionUtils), nameof(ActionUtils.GetCityArea))]
    [HarmonyPatch(typeof(ActionUtils), nameof(ActionUtils.GetCityAreaSorted))] //ty sol
    private static void BgIntoVillageCatch(GameState gameState, TileData cityTile, ref Il2Gen.List<TileData> __result)
    {
        if (gameState == null || cityTile == null || __result == null) return;
        Il2Gen.List<TileData> area = new Il2Gen.List<TileData>();
        
        foreach (TileData tileData in __result)
        {
            if (tileData != cityTile && tileData.HasImprovement(ImprovementData.Type.City)) area.Add(tileData);
        }
        
        foreach (TileData tileData1 in area)
        {
            __result.Remove(tileData1);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CaptureCityAction), nameof(CaptureCityAction.Execute))]
    private static bool FeedPatch(GameState state, CaptureCityAction __instance)
    {
        TileData tile = state.Map.GetTile(__instance.Coordinates);
        state.TryGetPlayer(tile.owner, out var victim);

        if (
            !state.TryGetPlayer(__instance.PlayerId, out var player) || 
            !state.GameLogicData.TryGetData(player.tribe, out var tribeData) || 
            !tribeData.HasAbility(Feed) ||
            (victim != null && victim.GetCurrentCapitalCoordinates(state) == tile.coordinates)
        )
        {
            return true;
        }

        Main.modLogger.LogInfo("ye");
        player.cities++;
        __instance.AddSubAction(new DestroyImprovementAction(player.Id, tile.coordinates));
        __instance.AddSubAction(new CityRewardAction(player.Id, Grow, player.GetCurrentCapitalCoordinates(state)));
        __instance.CommitSubActionsToStack(state.ActionStack);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CaptureCityReaction), nameof(CaptureCityReaction.Execute))]
    private static bool FeedReactionPatch(Il2CppSystem.Action onComplete, CaptureCityReaction __instance)
    {
        TileData tile = GameManager.GameState.Map.GetTile(__instance.action.Coordinates);
        GameManager.GameState.TryGetPlayer(tile.owner, out var victim);

        if (
            !GameManager.GameState.TryGetPlayer(__instance.action.PlayerId, out var player) || 
            !GameManager.GameState.GameLogicData.TryGetData(player.tribe, out var tribeData) || 
            !tribeData.HasAbility(Feed) ||
            (victim != null && victim.GetCurrentCapitalCoordinates(GameManager.GameState) == tile.coordinates)
        )
        {
            return true;
        }

        Tile instance = MapRenderer.Current.GetTileInstance(__instance.action.Coordinates);

        if (instance == null || instance.IsHidden == true)
        {
            onComplete.Invoke();
            return false;
        }
        instance.SpawnDarkPuff();
        instance.SpawnEmbers();
        instance.Sway();
        AudioManager.PlaySFXAtTile(SFXTypes.Parasite, instance.Coordinates, player.skinType);
        AudioManager.PlaySFXAtTile(SFXTypes.Explode, instance.Coordinates, player.skinType);

        string style = "";
        //if (player.skinType == ) IMPLEMENT SKIN LATER
        NotificationManager.Notify(
            Localization.GetSkinned(player.skinType, player.tribe, "crimson.popup.capture.info"),
            Localization.GetSkinned(player.skinType, player.tribe, "crimson.popup.capture.header"),
            PolyMod.Registry.GetSprite("crimsoncapture", style, Math.Clamp(player.cities, 0, 5)),
            player
        );

        GameManager.DelayCall(0.5f, onComplete);
        return false;
    }
}
