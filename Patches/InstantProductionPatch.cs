using System.Collections.Generic;
using System;
using System.Reflection;
using Aki.Reflection.Patching;
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
    //These patches related to production and not hideout upgrades
    // Patch for the StartProducing method
    internal class InstantStartProducingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1915), nameof(GClass1915.StartProducing));
        }

        [PatchPrefix]
        private static bool Prefix(GClass1915 __instance, GClass1922 scheme)
        {
            if (dadGamerPlugin.InstantProductionEnabled.Value)
            {
                GClass1921 producingItem = scheme.GetProducingItem(__instance.ProductionSpeedCoefficient, __instance.ReductionCoefficient);
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
            return AccessTools.Method(typeof(GClass1915), nameof(GClass1915.Update));
        }

        [PatchPrefix]
        private static bool Prefix(GClass1915 __instance, float deltaTime)
        {
            if (dadGamerPlugin.InstantProductionEnabled.Value)
            {
                List<KeyValuePair<string, GClass1921>> itemsToComplete = new List<KeyValuePair<string, GClass1921>>(__instance.ProducingItems);

                foreach (var kvp in itemsToComplete)
                {
                    if (__instance.Schemes.TryGetValue(kvp.Key, out GClass1922 scheme))
                    {
                        __instance.CompleteProduction(kvp.Value, scheme);
                    }
                }
                return false;
            }

            return true;
        }
    }

    // Extension method to handle CompleteProduction
    internal static class GClass1915Extensions
    {
        private static readonly FieldInfo Class1638Field;
        private static readonly FieldInfo ProgressField;

        static GClass1915Extensions()
        {
            Class1638Field = AccessTools.Field(typeof(GClass1921), "class1638_0");
            ProgressField = AccessTools.Field(typeof(GClass1921.Class1638), "double_1");
        }

        public static void CompleteProduction(this GClass1915 __instance, GClass1921 producingItem, GClass1922 scheme)
        {
            var class1638Instance = Class1638Field.GetValue(producingItem);

            //set the Progress field to 1.0 (complete)
            ProgressField.SetValue(class1638Instance, 1.0);

            Item item = __instance.CreateCompleteItem(scheme);
            
            __instance.BeforeProductionComplete(producingItem.SchemeId);
            __instance.CompleteItemsStorage.AddItem(scheme._id, item);
            __instance.ProducingItems.Remove(producingItem.SchemeId);
            __instance.SetDetailsData();
        }
    }



    
}
