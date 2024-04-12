using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using UnityEngine;

namespace dvize.DadGamerMode.Patches
{

    internal class OnWeightUpdatedPatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {

            return AccessTools.Method(typeof(EquipmentClass), nameof(EquipmentClass.method_11));
        }

        [PatchPrefix]
        internal static bool Prefix(EquipmentClass __instance, ref float __result, IEnumerable<Slot> slots)
        {

            //original functionality
            __result = slots.Select(new Func<Slot, Item>(EquipmentClass.Class2073.class2073_0.method_1)).Sum(new Func<Item, float>(__instance.method_12));

            // Get the total weight reduction setting
            float totalWeightReduction = dadGamerPlugin.totalWeightReductionPercentage.Value;

            // Convert it into a reduction factor: 0% -> full reduction (factor = 0), 100% -> no reduction (factor = 1)
            float reductionFactor = totalWeightReduction / 100f;

            // Apply the reduction factor
            __result *= reductionFactor;

            return false; // false to skip original method after prefix
        }
    }


}
