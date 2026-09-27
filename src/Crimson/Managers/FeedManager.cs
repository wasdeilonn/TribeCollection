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
using Il2CppSystem.Net.NetworkInformation;

namespace TribeCollection.Crimson;

public static class FeedManager
{
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
            !tribeData.HasAbility(Main.Feed)
        )
        {
            return;
        }

        __result.home = WorldCoordinates.NULL_COORDINATES;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ConvertAction), nameof(ConvertAction.Execute))]
    private static void ConvertAction_Execute(GameState state, ConvertAction __instance)
    {
        if (
            !state.TryGetPlayer(__instance.PlayerId, out var player) || 
            !state.GameLogicData.TryGetData(player.tribe, out var tribeData) || 
            !tribeData.HasAbility(Main.Feed)
        )
        {
            return;
        }

        if (state.Map.GetTile(__instance.Target).unit != null)
        {
            state.Map.GetTile(__instance.Target).unit.home = WorldCoordinates.NULL_COORDINATES;
        }
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ActionUtils), nameof(ActionUtils.TrainUnit))]
    private static void TrainUnit(GameState gameState, PlayerState playerState, TileData tile, UnitData unitData)
    {
        if (
            !gameState.TryGetPlayer(playerState.Id, out var player) || 
            !gameState.GameLogicData.TryGetData(player.tribe, out var tribeData) || 
            !tribeData.HasAbility(Main.Feed)
        )
        {
            return;
        }

        if (tile.unit != null)
        tile.unit.home = WorldCoordinates.NULL_COORDINATES;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CaptureCommand), nameof(CaptureCommand.IsValid))]
    private static bool CaptureValidation(CaptureCommand __instance, ref bool __result, GameState state, out string validationError)
    {
        if (
            !state.TryGetPlayer(__instance.PlayerId, out var player) || 
            !state.GameLogicData.TryGetData(player.tribe, out var tribeData) || 
            !tribeData.HasAbility(Main.Feed)
        )
        {
            validationError = "";
            return true;
        }

        if (state.Map.GetTile(player.GetCurrentCapitalCoordinates(state)).improvement.xp < Main.CapturePopulation)
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
        TribeData victimTribe = null;
        if (victim != null)
        {
            state.GameLogicData.TryGetData(victim.tribe, out victimTribe);
        }
        
        if 
        (
            !state.TryGetPlayer(__instance.PlayerId, out var player) || 
            !state.GameLogicData.TryGetData(player.tribe, out var tribeData)
        )
        {
            return true;
        }
        
        if (victim != null)
        {
            if (victimTribe.HasAbility(Main.Feed))
            {
                foreach (TileData cityTile in victim.GetCityTiles(state))
                {
                    __instance.AddSubAction(new CaptureCityAction(player.Id, cityTile.coordinates, victim.Id));
                }
            }
        }

        if (tribeData.HasAbility(Main.Feed) && (victim == null || victim.startTile != tile.coordinates))
        {
            player.cities++;
            UnfoundAction unfoundAction = PolibActionManager.MakeIl2CppAction<UnfoundAction>();
            unfoundAction.Coordinates = __instance.Coordinates;
            unfoundAction.PlayerId = __instance.PlayerId;
            __instance.AddSubAction(unfoundAction);
            __instance.AddSubAction(new DestroyImprovementAction(player.Id, tile.coordinates));
            __instance.AddSubAction(new DecreasePopulationAction(player.Id, player.GetCurrentCapitalCoordinates(state)));
            __instance.AddSubAction(new DecreasePopulationAction(player.Id, player.GetCurrentCapitalCoordinates(state), 20));
            __instance.AddSubAction(new DecreasePopulationAction(player.Id, player.GetCurrentCapitalCoordinates(state), 40));
            __instance.AddSubAction(new CityRewardAction(player.Id, Main.Grow, player.GetCurrentCapitalCoordinates(state)));
            __instance.CommitSubActionsToStack(state.ActionStack);
            return false;
        }

        __instance.CommitSubActionsToStack(state.ActionStack);
        return true;
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
            !tribeData.HasAbility(Main.Feed) ||
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
        #region TODO CAPTUREICON
        #endregion
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
