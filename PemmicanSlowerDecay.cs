using System;
using System.Reflection;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;

public class Main : MelonMod
{
    private HarmonyLib.Harmony harmony;

    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("PemmicanSlowerDecay loaded successfully!");

        Type gearDecayType = AccessTools.TypeByName("GearDecayModifier.Main");
        if (gearDecayType != null)
        {
            MethodInfo method = AccessTools.Method(gearDecayType, "ApplyDecayModifier", new Type[] { typeof(GearItem) });
            if (method != null)
            {
                harmony = new HarmonyLib.Harmony("com.yourname.pemmicanblock");
                harmony.Patch(
                    method,
                    prefix: new HarmonyMethod(typeof(GearDecayModifier_BlockPemmican).GetMethod(nameof(GearDecayModifier_BlockPemmican.Prefix)))
                );
                MelonLogger.Msg("[PemmicanSlowerDecay] GearDecayModifier patch applied.");
            }
        }
        else
        {
            MelonLogger.Msg("[PemmicanSlowerDecay] GearDecayModifier not found, skipping patch.");
        }
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

        float decayMultiplier = 0.0125f;
        hp *= decayMultiplier;
    }
}

// === GearDecayModifier Mod block patch ===
internal static class GearDecayModifier_BlockPemmican
{
    public static bool Prefix(GearItem gi, ref float __result)
    {
        try
        {
            if (gi == null)
                return true;

            if (gi.name.Equals("GEAR_CookedBarPemmican", StringComparison.OrdinalIgnoreCase))
            {
                __result = 1.0f;
                return false;
            }
        }
        catch (Exception e)
        {
            MelonLogger.Warning("[PemmicanSlowerDecay] Soft patch failed safely: " + e.Message);
        }

        return true;
    }
}