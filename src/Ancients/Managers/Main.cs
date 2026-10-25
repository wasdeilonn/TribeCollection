using BepInEx.Logging;
using HarmonyLib;
using Polytopia.Data;
using Polibrary.PolyScript;
using Il2Gen = Il2CppSystem.Collections.Generic;
using PolytopiaBackendBase.Common;
using Polibrary;
using M = TribeCollection.Main;

namespace TribeCollection.Ancients;

public static class Main
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.AddGameLogicPlaceholders))]
    public static void Register(Newtonsoft.Json.Linq.JObject rootObject)
    {
        M.modLogger.LogInfo(EnumCache<UnitAbility.Type>.TryGetType("charge_ability", out Charge));
        M.modLogger.LogInfo(EnumCache<UnitAbility.Type>.TryGetType("discharge_ability", out Discharge));
        M.modLogger.LogInfo(EnumCache<UnitAbility.Type>.TryGetType("capacitor_ability", out Capacitor));
        M.modLogger.LogInfo(EnumCache<UnitAbility.Type>.TryGetType("shock_ability", out Shock));
        M.modLogger.LogInfo(EnumCache<UnitAbility.Type>.TryGetType("protect_ability", out Protect));
        M.modLogger.LogInfo(EnumCache<UnitAbility.Type>.TryGetType("push_ability", out Push));
        M.modLogger.LogInfo(EnumCache<UnitAbility.Type>.TryGetType("lightning_ability", out LightningExplosion));
        M.modLogger.LogInfo(EnumCache<UnitEffect>.TryGetType("conductive_effect", out Conductive)); //
        M.modLogger.LogInfo(EnumCache<UnitEffect>.TryGetType("charge_effect", out Charged)); //
        M.modLogger.LogInfo(EnumCache<ImprovementAbility.Type>.TryGetType("lightning_improvementability", out LightningImprovement));
        M.modLogger.LogInfo(EnumCache<ImprovementAbility.Type>.TryGetType("powerstorage_improvementability", out PowerStorage));
        M.modLogger.LogInfo(EnumCache<TribeType>.TryGetType("ancients", out Ancients));
        M.modLogger.LogInfo(EnumCache<TileData.EffectType>.TryGetType("powerstored", out PowerStored));
        if (
            !EnumCache<UnitAbility.Type>.TryGetType("charge_ability", out Charge) 
            || !EnumCache<UnitAbility.Type>.TryGetType("discharge_ability", out Discharge)
            || !EnumCache<UnitAbility.Type>.TryGetType("capacitor_ability", out Capacitor)
            || !EnumCache<UnitAbility.Type>.TryGetType("shock_ability", out Shock) 
            || !EnumCache<UnitAbility.Type>.TryGetType("protect_ability", out Protect) 
            || !EnumCache<UnitAbility.Type>.TryGetType("push_ability", out Push) 
            || !EnumCache<UnitAbility.Type>.TryGetType("lightning_ability", out LightningExplosion) 

            || !EnumCache<UnitEffect>.TryGetType("conductive_effect", out Conductive)
            || !EnumCache<UnitEffect>.TryGetType("charge_effect", out Charged)

            || !EnumCache<ImprovementAbility.Type>.TryGetType("lightning_improvementability", out LightningImprovement)
            || !EnumCache<ImprovementAbility.Type>.TryGetType("powerstorage_improvementability", out PowerStorage)

            || !EnumCache<TribeType>.TryGetType("ancients", out Ancients)

            || !EnumCache<TileData.EffectType>.TryGetType("powerstored", out PowerStored)
            )
		{
			M.modLogger.LogFatal("Couldnt find EnumCache for ancients!");
			return;
		}
        
        PolibUtils.ParsePerEach<UnitData.Type, int>(rootObject, "unitData", "maxCharge", MaxCharge);
        PolibUtils.ParsePerEach<UnitData.Type, int>(rootObject, "unitData", "chargeConsumptionAmount", ChargeConsumptionAmount);
        PolibUtils.ParsePerEach<ImprovementData.Type, int>(rootObject, "improvementData", "lightningStars", LightningStars);
        PolibUtils.ParsePerEach<ImprovementData.Type, int>(rootObject, "improvementData", "lightningPop", LightningPop);
        PolibUtils.ParseListPerEach<UnitData.Type, string>(rootObject, "unitData", "chargeConsumptionEvent", ChargeConsumptionEvent);
        PolibUtils.ParseListPerEach<UnitData.Type, string>(rootObject, "unitData", "chargeBuff", ChargeBuff);
    }

	public static Dictionary<UnitData.Type, int> MaxCharge = new Dictionary<UnitData.Type, int>();
    public static Dictionary<UnitData.Type, int> ChargeConsumptionAmount = new Dictionary<UnitData.Type, int>();
    public static Dictionary<UnitData.Type, List<string>> ChargeConsumptionEvent = new Dictionary<UnitData.Type, List<string>>();
    public static Dictionary<UnitData.Type, List<string>> ChargeBuff = new Dictionary<UnitData.Type, List<string>>();
    public static Dictionary<ImprovementData.Type, int> LightningStars = new();
    public static Dictionary<ImprovementData.Type, int> LightningPop = new();
    public static TribeType Ancients;
    public static UnitAbility.Type Charge;
    public static UnitAbility.Type Discharge;
    public static UnitAbility.Type Capacitor;
    public static UnitAbility.Type Shock;
    public static UnitAbility.Type Protect;
    public static UnitAbility.Type Push;
    public static UnitAbility.Type LightningExplosion;
    public static UnitEffect Charged;
    public static UnitEffect Conductive;
    public static TileData.EffectType PowerStored;

    public static ImprovementAbility.Type LightningImprovement;
    public static ImprovementAbility.Type PowerStorage;


    [HarmonyPostfix]
    [HarmonyPatch(typeof(CommandUtils), nameof(CommandUtils.GetUnitActions))]
    private static void AddCommands(ref Il2Gen.List<CommandBase> __result, GameState gameState, PlayerState player, TileData tile, bool includeUnavailable)
    {
        if (tile.unit == null || tile.unit.owner != player.Id) return;

        if (tile.unit.HasAbility(Discharge) && tile.unit.HasAbility(Capacitor) && !tile.unit.moved && !tile.unit.attacked && ChargeManager.GetChargeCount(tile.unit) > 0)
        {
            CAR.New<DischargeCommand>(out var command);
            command.Coordinates = tile.coordinates;
            command.PlayerId = player.Id;
            command.Level = ChargeManager.GetChargeCount(tile.unit) - 1; //subtract 1 cause its 0 based
            CommandUtils.AddCommand(gameState, __result, command, includeUnavailable);
        }

        if (tile.unit.HasAbility(LightningExplosion) && ((!tile.unit.attacked && !tile.unit.moved) || tile.unit.HasAbility(UnitAbility.Type.Dash)))
        {
            bool flag = false;
            if (tile.unit.HasAbility(Capacitor))
            {
                if (ChargeManager.GetChargeCount(tile.unit) > 0)
                {
                    flag = true;
                }
            }
            else
            {
                flag = true;
            }

            if (flag)
            {
                CAR.New(out LightningExplosionCommand command);
                command.Coordinates = tile.coordinates;
                command.PlayerId = player.Id;
                CommandUtils.AddCommand(gameState, __result, command, includeUnavailable);
            }
        }
    }

    

	[HarmonyPostfix]
    [HarmonyPatch(typeof(UnitDataExtensions), nameof(UnitDataExtensions.GetAttackOptionsAtPosition))]
    private static void UnitDataExtensions_GetAttackOptionsAtPosition(ref Il2Gen.List<WorldCoordinates> __result, GameState gameState, byte playerId, WorldCoordinates position, int range, bool includeHiddenTiles = false, UnitState customUnitState = null, bool ignoreDiplomacyRelation = false)
	{
        if (!gameState.TryGetPlayer(playerId, out var player)) return;

        UnitState unit = gameState.Map.GetTile(position).unit;
        if (unit == null) return;

        if (unit.HasAbility(UnitAbility.Type.Consumed) && unit.UnitData.attack == 0 && !unit.HasAbility(UnitAbility.Type.Convert))
        {
            Il2Gen.List<TileData> area = gameState.Map.GetArea(position, range, allowDiagonal: true, includeCenter: false);
            foreach (TileData tileData in area)
            {
                UnitState unit1 = tileData.GetUnit(gameState, playerId, includeHiddenTiles);
                if (unit1 != null && (tileData.GetExplored(playerId) || includeHiddenTiles) && unit1.owner != playerId && !player.HasPeaceWith(unit1.owner))
                {
                    __result.Add(tileData.coordinates);
                }
            }
        }
	}
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ActionUtils), nameof(ActionUtils.KillUnit))]
    private static bool ActionUtils_KillUnit_NullCheck(GameState gameState, TileData tile) //why does this not have a nullcheck ingame??
    {
        if (tile.unit == null) return false;

        return true;
    }
}
