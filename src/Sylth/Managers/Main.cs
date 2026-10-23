using HarmonyLib;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using M = TribeCollection.Main;

namespace TribeCollection.Sylth;

public static class Main
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.AddGameLogicPlaceholders))]
    private static void Register()
    {
        if (
            !EnumCache<TribeAbility.Type>.TryGetType("incorporeal_tribeability", out Incorporeal)
            || !EnumCache<UnitAbility.Type>.TryGetType("veil_ability", out Veil)
            || !EnumCache<TileData.EffectType>.TryGetType("veiled_tileeffect", out Veiled)
        )
        {
            M.modLogger.LogError("cant get enum stuff for sylth");
            return;
        }
    }

    public static TribeAbility.Type Incorporeal;
    public static TileData.EffectType Veiled;
    public static UnitAbility.Type Veil;
}
