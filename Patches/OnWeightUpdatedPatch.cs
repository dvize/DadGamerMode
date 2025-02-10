using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
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

            return AccessTools.Method(typeof(InventoryEquipment), nameof(InventoryEquipment.smethod_1));
        }

        [PatchPrefix]
        internal static bool Prefix(InventoryEquipment __instance, ref float __result, IEnumerable<Slot> slots)
        {

            //original functionality
            __result = slots.Sum(new Func<Slot, float>(InventoryEquipment.Class2246.class2246_0.method_1));

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
