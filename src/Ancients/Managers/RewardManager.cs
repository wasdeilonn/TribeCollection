using HarmonyLib;
using Polytopia.Data;
using Polibrary.PolyScript;
using M = TribeCollection.Main;
using System.Data;

namespace TribeCollection.Ancients;

public static class RewardManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CityRewardAction), nameof(CityRewardAction.Execute))]
    private static void CityRewardAction_Execute(CityRewardAction __instance, GameState state)
    {
        TileData tile = state.Map.GetTile(__instance.Coordinates);
        if(__instance.Reward == EnumCache<CityReward>.GetType("ancfreetech"))
        {
            if (tile == null || tile.improvement == null
                || !state.TryGetPlayer(__instance.PlayerId, out PlayerState playerState))
                return;

            if(tile.improvement.rewards == null)
                tile.improvement.rewards = new();

            tile.improvement.rewards.Remove(__instance.Reward);

            TileData capital = state.Map.GetTile(playerState.startTile);
            if(capital.improvement == null || !capital.HasImprovement(ImprovementData.Type.City))
            {
                M.modLogger.LogInfo("Capital city wasnt found. The fuck?");
                return;
            }

            if(capital.improvement.rewards == null)
                capital.improvement.rewards = new();

            capital.improvement.rewards.Add(EnumCache<CityReward>.GetType("ancinternal_tech"));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MoveAction), nameof(MoveAction.Execute))]
    private static void MoveAction_Execute(MoveAction __instance, GameState state)
    {
        TileData targetTile = state.Map.GetTile(__instance.Path[0]);

        if (targetTile.rulingCityCoordinates == WorldCoordinates.NULL_COORDINATES) return;
        
        TileData cityTile = state.Map.GetTile(targetTile.rulingCityCoordinates);

        if(!cityTile.HasImprovement(ImprovementData.Type.City))
            return;

        if(!cityTile.improvement.HasReward(Main.TeslaTower))
            return;

        if(!state.TryGetUnit(__instance.UnitId, out UnitState unit))
            return;

        if(targetTile.owner == unit.owner)
            return;

		state.ActionStack.Add(new AttackAction(targetTile.owner, targetTile.coordinates, targetTile.coordinates, 40, shouldMoveToTarget: false, AttackAction.AnimationType.Passive, 100));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ActionUtils), nameof(ActionUtils.TrainUnit))]
    private static void ActionUtils_TrainUnit(ref UnitState __result, GameState gameState, 
        PlayerState playerState, TileData tile, UnitData unitData)
    {
        if(!tile.HasImprovement(ImprovementData.Type.City))
            return;

        CityReward hammer = EnumCache<CityReward>.GetType("anchammer");
        if(!tile.improvement.HasReward(hammer))
            return;

        __result.AddEffect(Main.Hammered);

        if(tile.improvement.rewards == null)
            tile.improvement.rewards = new();

        tile.improvement.rewards.Remove(hammer);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitDataExtensions), nameof(UnitDataExtensions.GetAttack), new System.Type[] { typeof(UnitState), typeof(GameState)})]
	public static void GetAttack(ref int __result, UnitState unitState, GameState gameState)
	{
		if(unitState.HasEffect(Main.Hammered))
            __result += 200;
	}

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitDataExtensions), nameof(UnitDataExtensions.GetDefenceBonus))]
	public static void GetDefenceBonus(ref int __result, UnitState unit, GameState gameState)
	{
		if(unit.HasEffect(Main.Hammered))
            __result += 10;
	}

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CityRewardReaction), nameof(CityRewardReaction.Execute))]
    private static void CityRewardReaction_Execute(CityRewardReaction __instance, Il2CppSystem.Action onComplete)
    {
        if (__instance.action.Reward == Main.TeslaTower)
        {
            TileData tileData = GameManager.GameState.Map.GetTile(__instance.action.Coordinates);


            foreach (TileData areaTileData in ActionUtils.GetCityArea(GameManager.GameState, tileData))
            {
                Tile tile = areaTileData.GetInstance();

                tile.Render();
            }
        }

        if(__instance.action.Reward == EnumCache<CityReward>.GetType("anchammer"))
        {
            TileData tileData = GameManager.GameState.Map.GetTile(__instance.action.Coordinates);
            Tile tile = tileData.GetInstance();
            GameManager.DelayCall(100, (Il2CppSystem.Action)(() =>
            {
                tile.Sway();
                tile.SpawnShine();
                AudioManager.PlaySFXAtTile(SFXTypes.Promote, tileData.coordinates);
            }));
        }
    }
}
