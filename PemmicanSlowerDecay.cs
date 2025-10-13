using System;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;

public class Main : MelonMod
{
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("PemmicanSlowerDecay loaded successfully!");
    }
}

[HarmonyPatch(typeof(GearItem), nameof(GearItem.Degrade), new Type[] { typeof(float) })]
internal static class GearItem_Degrade_Pemmican
{
    private static void Prefix(GearItem __instance, ref float hp)
    {
        if (__instance == null || string.IsNullOrEmpty(__instance.name))
            return;

        if (!__instance.name.Equals("GEAR_CookedBarPemmican", StringComparison.OrdinalIgnoreCase))
            return;

        // Same as CuredMeat outdoors
        float decayMultiplier = 0.0125f;

        hp *= decayMultiplier;
    }
}