using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Polytopia;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using Polibrary.PolyScript;
using Il2Gen = Il2CppSystem.Collections.Generic;
using DG.Tweening;

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
            || !EnumCache<TribeType>.TryGetType("crimson", out Crimson)
        )
        {
            Main.modLogger.LogError("cant get enum stuff for crimson");
            return;
        }
    }

    public static TribeAbility.Type Feed;
    public static CityReward Grow;
    public static TribeType Crimson;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof (MapRenderContext))]
    private static void TintRed(Tile __instance)
    {
        if (__instance.data.climate == Crimson)
        {
            if (
                __instance.data.terrain == Polytopia.Data.TerrainData.Type.Water ||
                __instance.data.terrain == Polytopia.Data.TerrainData.Type.Ocean ||
                __instance.data.terrain == Polytopia.Data.TerrainData.Type.Ice
                )
            {
                TerrainMaterialHelper.SetSpriteSaturated(__instance.terrainRenderer.SpriteRenderer, desaturated: true);
                TerrainMaterialHelper.SetSpriteTint(__instance.terrainRenderer.spriteRenderer, new UnityEngine.Color(1, 0.4f, 0.4f));
            }
            
        }
    }

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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitState), nameof(UnitState.Create), typeof(GameState), typeof(byte), typeof(uint), typeof(UnitData), typeof(WorldCoordinates), typeof(WorldCoordinates))]
    private static void UnitState_Create(ref UnitState __result, GameState gameState, byte playerId, uint currentTurn, UnitData unitData, WorldCoordinates coordinates, WorldCoordinates home)
    {
        if (
            !gameState.TryGetPlayer(playerId, out var player) || 
            !gameState.GameLogicData.TryGetData(player.tribe, out var tribeData) || 
            !tribeData.HasAbility(Feed)
        )
        {
            return;
        }

        __result.home = WorldCoordinates.NULL_COORDINATES;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CaptureCommand), nameof(CaptureCommand.IsValid))]
    private static bool CaptureValidation(CaptureCommand __instance, ref bool __result, GameState state, out string validationError)
    {
        if (
            !state.TryGetPlayer(__instance.PlayerId, out var player) || 
            !state.GameLogicData.TryGetData(player.tribe, out var tribeData) || 
            !tribeData.HasAbility(Feed)
            
        )
        {
            validationError = "";
            return true;
        }

        if (state.Map.GetTile(player.GetCurrentCapitalCoordinates(state)).improvement.xp < 2)
        {
            validationError = CommandBase.VALIDATION_ERROR_CANT_CAPTURE;
            return false;
        }

        validationError = "";
        return true;
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

        player.cities++;
        Unfound(state, __instance.PlayerId, __instance.Coordinates);
        __instance.AddSubAction(new DestroyImprovementAction(player.Id, tile.coordinates));
        __instance.AddSubAction(new DecreasePopulationAction(player.Id, player.GetCurrentCapitalCoordinates(state)));
        __instance.AddSubAction(new DecreasePopulationAction(player.Id, player.GetCurrentCapitalCoordinates(state), 20));
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
        instance.StopFire();
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

    private static void Unfound(GameState state, byte playerId, WorldCoordinates wcoords)
    {
        TileData tile = state.Map.GetTile(wcoords);
        byte previousOwnerId = tile.owner;
        if (previousOwnerId == 0) return;

        state.TryGetPlayer(previousOwnerId, out var playerState);

        if (playerState != null)
        {
            playerState.cities--;
        }

        for (int i = 0; i < state.Map.Tiles.Length; i++)
        {
            TileData tileData = state.Map.Tiles[i];
            if (tileData.unit != null && tileData.unit.home == wcoords)
            {
                tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
            }
        }

        Il2Gen.List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
        List<ActionBase> scoreActions = new List<ActionBase>();

        for (int j = 0; j < cityAreaSorted.Count; j++)
        {
            TileData territoryTile = cityAreaSorted[j];

            if (territoryTile.owner == previousOwnerId)
            {
                int scoreValue = ScoreSheet.tileValue;
                if (territoryTile.improvement != null && territoryTile.coordinates != tile.coordinates)
                {
                    scoreValue += state.CalculateImprovementScore(territoryTile);
                }

                scoreActions.Add(new DecreaseScoreAction(previousOwnerId, scoreValue));

                territoryTile.owner = 0;
                territoryTile.rulingCityCoordinates = WorldCoordinates.NULL_COORDINATES;
            }
        }

        int cityCenterScore = state.CalculateImprovementScore(tile);
        ActionUtils.AddScore(state, playerState, -cityCenterScore);
        foreach (ActionBase ab in scoreActions)
        {
            state.ActionStack.Add(ab);
        }

        Polibrary.PolibUtils.RemoveEmbassies(state, tile);

        tile.owner = 0;
        
        tile.improvement.level = 1;
        
        if (playerState != null && !playerState.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
        {
            state.ActionStack.Add(new WipePlayerAction(playerId, previousOwnerId));
        }
    }
}
