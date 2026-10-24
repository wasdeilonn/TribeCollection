using HarmonyLib;
using Polibrary;
using Polibrary.PolyScript;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using M = TribeCollection.Main;

namespace TribeCollection.Sylth;

public static class InvisibilityManager
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MoveAction), nameof(MoveAction.Execute))]
    private static void MoveVeil(GameState state, MoveAction __instance)
    {
        
    }
}
