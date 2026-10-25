using BepInEx.Logging;
using HarmonyLib;
using Polytopia.Data;
using Polibrary.PolyScript;

namespace TribeCollection.Ancients;

public static class ConductionManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.ExecuteDefault))]
    private static void AttackCommand_Execute(GameState gameState, AttackCommand __instance)
	{
		UnitState attacker = gameState.Map.GetTile(__instance.Origin).unit;
		UnitState defender = gameState.Map.GetTile(__instance.Target).unit;

        if (attacker.HasAbility(Main.Shock))
        {
            CAR.New(out ApplyConductionAction action);
            action.PlayerId = attacker.owner;
            action.Coordinates = defender.coordinates;
            action.Origin = attacker.coordinates;
            gameState.ActionStack.Add(action);

            if (attacker.HasAbility(UnitAbility.Type.Splash))
            {
                gameState.TryGetPlayer(__instance.PlayerId, out var player);
                foreach (TileData tile in gameState.Map.GetArea(__instance.Target, 1, true, false))
                {
                    if (tile.unit != null && !player.HasPeaceWith(tile.unit.owner) && tile.unit.owner != __instance.PlayerId)
                    {
                        CAR.New(out ApplyConductionAction action1);
                        action1.PlayerId = attacker.owner;
                        action1.Coordinates = tile.coordinates;
                        action1.Origin = attacker.coordinates;
                        gameState.ActionStack.Add(action1);
                    }
                }
            }
        }
	}

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ActionUtils), nameof(ActionUtils.KillUnit))]
    private static void ActionUtils_KillUnit(GameState gameState, TileData tile)
	{
        if (tile.unit == null || !gameState.TryGetPlayer(tile.unit.owner, out var player))
        {
            return;
        }
        if (tile.unit.HasEffect(Main.Conductive))
        {
            foreach (TileData tile1 in gameState.Map.GetArea(tile.coordinates, 1, true, false))
            {
                if (tile1.unit != null && tile1.unit.owner == tile.unit.owner)
                {
                    gameState.ActionStack.Add(new AttackAction(tile.unit.owner, tile1.coordinates, tile1.coordinates, 50, false, AttackAction.AnimationType.Splash, 20));
                    CAR.New(out ApplyConductionAction action);
                    action.PlayerId = tile.unit.owner;
                    action.Coordinates = tile1.coordinates;
                    action.Origin = tile.coordinates;
                    gameState.ActionStack.Add(action);
                }
            }
        }
	}
}