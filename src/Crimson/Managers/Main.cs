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

namespace TribeCollection.Crimson;

public static class Main
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.AddGameLogicPlaceholders))]
    private static void Register()
    {
        if (
            !EnumCache<TribeAbility.Type>.TryGetType("feed_tribeability", out Feed)
            || !EnumCache<CityReward>.TryGetType("growth_reward", out Grow)
            || !EnumCache<TribeType>.TryGetType("crimson", out Crimson)
            || !EnumCache<UnitAbility.Type>.TryGetType("jump_ability", out Jump)
            || !EnumCache<UnitAbility.Type>.TryGetType("doublemove_ability", out DoubleMove)
            || !EnumCache<UnitAbility.Type>.TryGetType("impact_ability", out Impact)
            || !EnumCache<UnitAbility.Type>.TryGetType("infect_ability", out Infect)
            || !EnumCache<UnitEffect>.TryGetType("infected_effect", out Infected)
            || !EnumCache<ResourceData.Type>.TryGetType("residue_resource", out Residue)
        )
        {
            M.modLogger.LogError("cant get enum stuff for crimson");
            return;
        }
    }

    public static TribeAbility.Type Feed;
    public static CityReward Grow;
    public static TribeType Crimson;
    public static UnitAbility.Type Jump;
    public static UnitAbility.Type DoubleMove;
    public static UnitAbility.Type Impact;
    public static UnitAbility.Type Infect;
    public static UnitEffect Infected;
    public static ResourceData.Type Residue;
    public const int CapturePopulation = 3;
}
