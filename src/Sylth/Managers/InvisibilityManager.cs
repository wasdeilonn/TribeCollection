using DG.Tweening;
using HarmonyLib;
using Polibrary;
using Polibrary.PolyScript;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using M = TribeCollection.Main;

namespace TribeCollection.Sylth;

public static class InvisibilityManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MoveCommand), nameof(MoveCommand.Execute))]
    private static void MoveVeil(GameState gameState, MoveCommand __instance)
    {
        if (gameState.TryGetUnit(__instance.UnitId, out var unit)) return;
        if (gameState.TryGetPlayer(unit.owner, out var player)) return;
        
        if (player.HasTribeAbility(Main.Incorporeal, gameState))
        {
            if (!unit.HasAbility(Main.Veil))
            {
                if (gameState.Map.GetTile(__instance.To).HasEffect(Main.Veiled))
                {
                    CAR.New(out ChangeVisibilityAction action);
                    action.PlayerId = player.Id;
                    action.Coordinates = unit.coordinates;
                    gameState.ActionStack.Add(action);
                }
                else
                {
                    CAR.New(out ChangeVisibilityAction action);
                    action.PlayerId = player.Id;
                    action.Coordinates = unit.coordinates;
                    action.MakeInvisible = false;
                    gameState.ActionStack.Add(action);
                }
            }
        }
    }
}
