using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;
using UnityEngine;

public class Main : MelonMod
{
    private HarmonyLib.Harmony harmony;

    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("[PemmicanSlowerDecay] Mod initialized, ready to patch pemmican decay.");

        // --- GearDecayModifier mod block ---
        try
        {
            Type gearDecayType = AccessTools.TypeByName("GearDecayModifier.Main");
            if (gearDecayType != null)
            {
                MethodInfo method = AccessTools.Method(gearDecayType, "ApplyDecayModifier", new Type[] { typeof(GearItem) });
                if (method != null)
                {
                    harmony = new HarmonyLib.Harmony("com.pemmican.blocker");
                    harmony.Patch(
                        method,
                        prefix: new HarmonyMethod(typeof(GearDecayModifier_BlockPemmican)
                            .GetMethod(nameof(GearDecayModifier_BlockPemmican.Prefix)))
                    );
                    MelonLogger.Msg("[PemmicanSlowerDecay] GearDecayModifier patch successfully applied.");
                }
            }
            else
            {
                MelonLogger.Msg("[PemmicanSlowerDecay] GearDecayModifier not found, skipping blocker.");
            }
        }
        catch (Exception e)
        {
            MelonLogger.Warning("[PemmicanSlowerDecay] Error while patching GearDecayModifier: " + e);
        }
    }
}

[HarmonyPatch(typeof(GearItem), nameof(GearItem.Awake))]
internal static class GearItem_Awake_Patch
{
    private static void Postfix(GearItem __instance)
    {
        if (__instance == null) return;

        string nm = __instance.name ?? "<null>";

        if (!nm.Contains("GEAR_CookedBarPemmican", StringComparison.OrdinalIgnoreCase))
            return;

        MelonLogger.Msg($"[PemmicanSlowerDecay] Awake detected for {nm} (id={__instance.m_InstanceID})");

        MelonCoroutines.Start(DelayedDecayPatch(__instance));
    }

    private static IEnumerator DelayedDecayPatch(GearItem gi)
    {
        for (int i = 0; i < 5; i++)
            yield return null;

        if (gi == null)
            yield break;

        if (gi.m_FoodItem == null)
        {
            MelonLogger.Msg($"[PemmicanSlowerDecay] {gi.name} has no FoodItem even after delay — skipping.");
            yield break;
        }

        float oldInside = gi.m_FoodItem.m_DailyHPDecayInside;
        float oldOutside = gi.m_FoodItem.m_DailyHPDecayOutside;

        gi.m_FoodItem.m_DailyHPDecayInside = 0.025f;
        gi.m_FoodItem.m_DailyHPDecayOutside = 0.0025f;

        MelonLogger.Msg($"[PemmicanSlowerDecay] Patched {gi.name} decay: inside {oldInside} --> {gi.m_FoodItem.m_DailyHPDecayInside}, outside {oldOutside} --> {gi.m_FoodItem.m_DailyHPDecayOutside}");
    }
}

// === GearDecayModifier mod block ===
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