using HarmonyLib;
using Polibrary;
using Polibrary.PolyScript;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using M = TribeCollection.Main;

namespace TribeCollection.Sylth;

public static class VeilManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartMatchAction), nameof(StartMatchAction.Execute))]
    private static void AddVeilToStartingArea(GameState gameState, StartMatchAction __instance)
    {
        if (!gameState.TryGetPlayer(__instance.PlayerId, out var player) || !gameState.GameLogicData.TryGetData(player.tribe, out var tribeData))
        {
            M.modLogger.LogError("how the fuck");
            return;
        }
        
        foreach (TileData tile in gameState.Map.Tiles)
        {
            if (tile.owner == player.Id && tribeData.HasAbility(Main.Incorporeal))
            {
                tile.AddEffect(Main.Veiled);
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MoveAction), nameof(MoveAction.Execute))]
    private static void MoveVeil(GameState state, MoveAction __instance)
    {
        TileData target = state.Map.GetTile(__instance.Path[0]);
        TileData origin = state.Map.GetTile(__instance.Path.Last());

        if (origin.unit == null || !origin.unit.HasAbility(Main.Veil)) return;

        List<TileData> toVeil = state.Map.GetArea(target.coordinates, 1, true).ToSysList();
        List<TileData> toUnveil = state.Map.GetArea(origin.coordinates, 1, true).ToSysList();

        foreach (TileData tile in toVeil)
        {
            CAR.New(out VeilTileAction action);
            action.PlayerId = __instance.PlayerId;
            action.Coordinates = tile.coordinates;
            state.ActionStack.Add(action);
        }

        foreach (TileData tile in toUnveil)
        {
            bool contFlag = false;

            foreach (TileData neighbor in state.Map.GetTileNeighbors(tile.coordinates))
            {
                if (neighbor.unit != null && neighbor.unit.HasAbility(Main.Veil) && neighbor.unit.id != origin.unit.id) contFlag = true;
            }

            if (contFlag) continue;

            if (
                tile.rulingCityCoordinates != WorldCoordinates.NULL_COORDINATES 
                && state.TryGetPlayer(tile.owner, out var neighborPlayer)
                && neighborPlayer.HasTribeAbility(Main.Incorporeal, state)
            )
            {
                continue;
            }

            CAR.New(out UnveilTileAction action);
            action.PlayerId = __instance.PlayerId;
            action.Coordinates = tile.coordinates;
            state.ActionStack.Add(action);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CaptureCityAction), nameof(CaptureCityAction.Execute))]
    private static void ManageCaptureVeil(GameState state, CaptureCityAction __instance)
    {
        if (!state.TryGetPlayer(__instance.PlayerId, out var player) || !state.GameLogicData.TryGetData(player.tribe, out var tribeData))
        {
            M.modLogger.LogError("kys");
            return;
        }

        TileData origin = state.Map.GetTile(__instance.Coordinates);

        List<TileData> area = ActionUtils.GetCityArea(state, origin).ToSysList();

        foreach (TileData tile in area)
        {
            if (tribeData.HasAbility(Main.Incorporeal))
            {
                CAR.New(out VeilTileAction action);
                action.PlayerId = __instance.PlayerId;
                action.Coordinates = tile.coordinates;
                state.ActionStack.Add(action);
            }
            else if (tile.HasEffect(Main.Veiled))
            {
                bool contFlag = false;

                foreach (TileData neighbor in state.Map.GetTileNeighbors(tile.coordinates))
                {
                    if (neighbor.unit != null && neighbor.unit.HasAbility(Main.Veil)) contFlag = true;
                }

                if (contFlag) continue;

                CAR.New(out UnveilTileAction action);
                action.PlayerId = __instance.PlayerId;
                action.Coordinates = tile.coordinates;
                state.ActionStack.Add(action);
            }    
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExpandCityAction), nameof(ExpandCityAction.Execute))]
    private static void ManageBorderGrowthVeil(GameState state, ExpandCityAction __instance)
    {
        if (!state.TryGetPlayer(__instance.PlayerId, out var player) || !state.GameLogicData.TryGetData(player.tribe, out var tribeData))
        {
            M.modLogger.LogError("uuu shi");
            return;
        }

        TileData origin = state.Map.GetTile(__instance.Coordinates);

        List<TileData> area = ActionUtils.GetCityArea(state, origin).ToSysList();

        foreach (TileData tile in area)
        {
            if (tribeData.HasAbility(Main.Incorporeal))
            {
                CAR.New(out VeilTileAction action);
                action.PlayerId = __instance.PlayerId;
                action.Coordinates = tile.coordinates;
                state.ActionStack.Add(action);
            }
            else if (tile.HasEffect(Main.Veiled))
            {
                bool contFlag = false;

                foreach (TileData neighbor in state.Map.GetTileNeighbors(tile.coordinates))
                {
                    if (neighbor.unit != null && neighbor.unit.HasAbility(Main.Veil)) contFlag = true;
                }

                if (contFlag) continue;

                CAR.New(out UnveilTileAction action);
                action.PlayerId = __instance.PlayerId;
                action.Coordinates = tile.coordinates;
                state.ActionStack.Add(action);
            }    
        }
    }
}
