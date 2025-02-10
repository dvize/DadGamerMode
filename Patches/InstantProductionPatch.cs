using System;
using System.Collections.Generic;
using System.Reflection;
using dvize.GodModeTest;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace dvize.DadGamerMode.Patches
{

    // Patch for the Update method
    internal class InstantUpdatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass2149), nameof(GClass2149.Update));
        }

        [PatchPrefix]
        private static bool Prefix(GClass2149 __instance, float deltaTime)
        {
            if (dadGamerPlugin.InstantProductionEnabled.Value)
            {
                if (__instance == null || __instance.ProducingItems == null)
                {
                    return false;
                }

                // Filter itemsToComplete by removing bitcoin farm
                List<KeyValuePair<string, GClass2156>> itemsToComplete = new List<KeyValuePair<string, GClass2156>>(__instance.ProducingItems);
                itemsToComplete.RemoveAll(x => x.Key == "5d5589c1f934db045e6c5492" || x.Key == "5d5c205bd582a50d042a3c0e"); //bitcoin and fuel?

                foreach (var kvp in itemsToComplete)
                {
                    if (__instance.Schemes != null && __instance.Schemes.TryGetValue(kvp.Key, out ProductionBuildAbstractClass scheme))
                    {
                        __instance.CompleteProduction(kvp.Value, scheme);
                    }
                }

                // Allow normal update processing for Bitcoin items
                return true;
            }

            return true;
        }
    }

    // Extension method to handle CompleteProduction
    internal static class GClass2152Extensions
    {
        private static readonly FieldInfo Class1782Field;
        private static readonly FieldInfo ProgressField;

        static GClass2152Extensions()
        {
            Class1782Field = AccessTools.Field(typeof(GClass2156), "class1782_0");
            ProgressField = AccessTools.Field(typeof(GClass2156.Class1782), "double_1");
        }

        public static void CompleteProduction(this GClass2149 __instance, GClass2156 producingItem, ProductionBuildAbstractClass scheme)
        {
            if (__instance == null || producingItem == null || scheme == null)
            {
                dadGamerPlugin.Logger.LogError("CompleteProduction: __instance, producingItem, or scheme is null.");
                return;
            }

            try
            {
                var class1782Instance = Class1782Field.GetValue(producingItem);
                if (class1782Instance == null)
                {
                    dadGamerPlugin.Logger.LogError("CompleteProduction: class1782Instance is null.");
                    return;
                }

                // Set the Progress field to 1.0 (complete)
                ProgressField.SetValue(class1782Instance, 1.0);

                Item item = __instance.CreateCompleteItem(scheme);
                if (item == null)
                {
                    dadGamerPlugin.Logger.LogError("CompleteProduction: item is null.");
                    return;
                }

                // Log the current state of the ProducingItems dictionary
                dadGamerPlugin.Logger.LogInfo("CompleteProduction: Current ProducingItems:");
                foreach (var kvp in __instance.ProducingItems)
                {
                    dadGamerPlugin.Logger.LogInfo($"Key: {kvp.Key}, Value SchemeId: {kvp.Value.SchemeId}");
                }

                // Check if the SchemeId exists in the dictionary before calling BeforeProductionComplete
                if (__instance.ProducingItems != null && __instance.ProducingItems.ContainsKey(producingItem.SchemeId))
                {
                    dadGamerPlugin.Logger.LogInfo($"CompleteProduction: Found SchemeId {producingItem.SchemeId} in ProducingItems.");

                    __instance.BeforeProductionComplete(producingItem.SchemeId);
                    __instance.CompleteItemsStorage.AddItem(scheme._id, item);
                    __instance.ProducingItems.Remove(producingItem.SchemeId);
                    __instance.SetDetailsData();
                }
                else
                {
                    dadGamerPlugin.Logger.LogError($"SchemeId {producingItem.SchemeId} not found in ProducingItems.");
                }
            }
            catch (Exception ex)
            {
                dadGamerPlugin.Logger.LogError($"Unexpected error during CompleteProduction: {ex.Message}");
            }
        }
    }
}
