#nullable disable
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

[HarmonyPatch(typeof(GearItem), "DecayOverTODHours")]
internal static class GearItem_DecayOverTODHours_Patch
{
    private const float k_InsideDecay = 0.025f;
    private const float k_OutsideDecay = 0.0025f;

    private static bool IsPemmican(GearItem gi)
    {
        string nm = gi?.name;
        return !string.IsNullOrEmpty(nm) &&
               nm.Contains("GEAR_CookedBarPemmican", StringComparison.OrdinalIgnoreCase);
    }

    private static void Prefix(GearItem __instance)
    {
        if (__instance == null || !IsPemmican(__instance)) return;

        FoodItem foodItem = __instance.m_FoodItem;
        if (foodItem == null) return;

        bool physicallyIndoors = IsPhysicallyIndoors(__instance);
        float target = physicallyIndoors ? k_InsideDecay : k_OutsideDecay;

        foodItem.m_DailyHPDecayInside = target;
        foodItem.m_DailyHPDecayOutside = target;
    }

    // For GearInfo to display both the correct decay values
    private static void Postfix(GearItem __instance)
    {
        if (__instance == null || !IsPemmican(__instance)) return;

        FoodItem foodItem = __instance.m_FoodItem;
        if (foodItem == null) return;

        foodItem.m_DailyHPDecayInside = k_InsideDecay;
        foodItem.m_DailyHPDecayOutside = k_OutsideDecay;
    }

    private static bool IsPhysicallyIndoors(GearItem gi)
    {
        Weather weather = GameManager.GetWeatherComponent();
        if (weather != null && weather.IsIndoorScene())
            return true;

        Collider itemCollider = gi.GetComponent<Collider>();
        if (itemCollider == null)
            return false;

        Collider[] nearby = Physics.OverlapSphere(
            itemCollider.bounds.center,
            itemCollider.bounds.extents.magnitude);

        for (int i = 0; i < nearby.Length; i++)
        {
            Collider other = nearby[i];
            if (other == itemCollider) continue;

            IndoorSpaceTrigger trigger = other.GetComponent<IndoorSpaceTrigger>();
            if (trigger == null) continue;
            if (trigger.m_DontCountAsInterior) continue;

            return true;
        }
        return false;
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