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

            float totalWeightReduction = dadGamerPlugin.totalWeightReductionPercentage.Value;
            float reductionFactor = totalWeightReduction / 100f;

            // only if the percentage is greater than 0
            if (totalWeightReduction > 0)
            {
                __result *= (1 - reductionFactor); 
            }
            else
            {
                __result = 0;
            }

            return false;
        }
    }


}
