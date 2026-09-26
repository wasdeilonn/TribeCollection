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
        )
        {
            M.modLogger.LogError("cant get enum stuff for crimson");
            return;
        }

        PolibActionManager.RegisterAction<UnfoundAction>("unfoundaction");
        PolibReactionManager.AssignReaction<UnfoundReaction>("unfoundaction");

        PolibActionManager.RegisterAction<ImpactAction>("impactaction");
        PolibReactionManager.AssignReaction<ImpactReaction>("impactaction");
    }

    public static TribeAbility.Type Feed;
    public static CityReward Grow;
    public static TribeType Crimson;
    public static UnitAbility.Type Jump;
    public static UnitAbility.Type DoubleMove;
    public static UnitAbility.Type Impact;
    public const int CapturePopulation = 3;
}
