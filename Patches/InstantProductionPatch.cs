using System.Collections.Generic;
using System;
using System.Reflection;
using SPT.Reflection.Patching;
using dvize.GodModeTest;
using EFT.InventoryLogic;
using HarmonyLib;
using EFT.Hideout;
using System.Threading.Tasks;
using System.Threading;
using Comfort.Common;

using static dvize.GodModeTest.dadGamerPlugin;

namespace dvize.DadGamerMode.Patches
{
    //These patches relate to production and not hideout upgrades
    // Patch for the StartProducing method
    internal class InstantStartProducingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1933), nameof(GClass1933.StartProducing));
        }

        [PatchPrefix]
        private static bool Prefix(GClass1933 __instance, ProductionBuildAbstractClass scheme)
        {
            if (dadGamerPlugin.InstantProductionEnabled.Value)
            {
                if (__instance == null || scheme == null)
                {
                    //dadGamerPlugin.Logger.LogError("InstantStartProducingPatch: __instance or scheme is null.");
                    return false;
                }

                GClass1937 producingItem = scheme.GetProducingItem(__instance.ProductionSpeedCoefficient, __instance.ReductionCoefficient);
                if (producingItem == null)
                {
                    //dadGamerPlugin.Logger.LogError("InstantStartProducingPatch: producingItem is null.");
                    return false;
                }

                __instance.AddProducingItem(producingItem);
                __instance.CompleteProduction(producingItem, scheme);
                return false;
            }
            return true;
        }
    }

    // Patch for the Update method
    internal class InstantUpdatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1931), nameof(GClass1931.Update));
        }

        [PatchPrefix]
        private static bool Prefix(GClass1931 __instance, float deltaTime)
        {
            if (dadGamerPlugin.InstantProductionEnabled.Value)
            {
                if (__instance == null || __instance.ProducingItems == null)
                {
                    //dadGamerPlugin.Logger.LogError("InstantUpdatePatch: __instance or ProducingItems is null.");
                    return false;
                }

                List<KeyValuePair<string, GClass1937>> itemsToComplete = new List<KeyValuePair<string, GClass1937>>(__instance.ProducingItems);

                foreach (var kvp in itemsToComplete)
                {
                    if (__instance.Schemes != null && __instance.Schemes.TryGetValue(kvp.Key, out ProductionBuildAbstractClass scheme))
                    {
                        __instance.CompleteProduction(kvp.Value, scheme);
                    }
                    else
                    {
                        //dadGamerPlugin.Logger.LogError($"InstantUpdatePatch: Scheme for key {kvp.Key} not found or __instance.Schemes is null.");
                    }
                }
                return false;
            }

            return true;
        }
    }

    // Extension method to handle CompleteProduction
    internal static class GClass1933Extensions
    {
        private static readonly FieldInfo Class1666Field;
        private static readonly FieldInfo ProgressField;

        static GClass1933Extensions()
        {
            Class1666Field = AccessTools.Field(typeof(GClass1937), "class1666_0");
            ProgressField = AccessTools.Field(typeof(GClass1937.Class1666), "double_1");
        }

        public static void CompleteProduction(this GClass1931 __instance, GClass1937 producingItem, ProductionBuildAbstractClass scheme)
        {
            if (__instance == null || producingItem == null || scheme == null)
            {
                //dadGamerPlugin.Logger.LogError("CompleteProduction: __instance, producingItem, or scheme is null.");
                return;
            }

            try
            {
                var class1666Instance = Class1666Field.GetValue(producingItem);
                if (class1666Instance == null)
                {
                    //dadGamerPlugin.Logger.LogError("CompleteProduction: class1666Instance is null.");
                    return;
                }

                // Set the Progress field to 1.0 (complete)
                ProgressField.SetValue(class1666Instance, 1.0);

                Item item = __instance.CreateCompleteItem(scheme);
                if (item == null)
                {
                    //dadGamerPlugin.Logger.LogError("CompleteProduction: item is null.");
                    return;
                }

                // Check if the SchemeId exists in the dictionary before calling BeforeProductionComplete
                if (__instance.ProducingItems != null && __instance.ProducingItems.ContainsKey(producingItem.SchemeId))
                {
                    __instance.BeforeProductionComplete(producingItem.SchemeId);
                    __instance.CompleteItemsStorage.AddItem(scheme._id, item);
                    __instance.ProducingItems.Remove(producingItem.SchemeId);
                    __instance.SetDetailsData();
                }
                else
                {
                    //dadGamerPlugin.Logger.LogError($"SchemeId {producingItem.SchemeId} not found in ProducingItems for {item.LocalizedName()}");
                }
            }
            catch (KeyNotFoundException ex)
            {
                dadGamerPlugin.Logger.LogError($"KeyNotFoundException: The given key {producingItem.SchemeId} was not present in the dictionary. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                dadGamerPlugin.Logger.LogError($"Unexpected error during CompleteProduction: {ex.Message}");
            }
        }
    }
}
