using System.Reflection;
using Aki.Reflection.Patching;
using dvize.GodModeTest;
using EFT;
using HarmonyLib;

namespace dvize.DadGamerMode.Patches
{

    internal class OnWeightUpdatedPatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {

            return AccessTools.Method(typeof(GClass681), "OnWeightUpdated");
        }

        [PatchPrefix]
        static bool Prefix(ref float ___totalWeight, GClass681 __instance)
        {
            float weightReductionMultiplier = 1f - (dadGamerPlugin.totalWeightReductionPercentage.Value / 100f);

            ___totalWeight *= weightReductionMultiplier;

            return true; 
        }
    }


}
