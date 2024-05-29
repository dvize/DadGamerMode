using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using Comfort.Common;
using dvize.GodModeTest;
using EFT.Hideout;
using HarmonyLib;

namespace dvize.DadGamerMode.Patches
{
    //hideout upgrades

    // Patch for the method_0 (construction and upgrade timing)
    internal class InstantConstructionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AreaData), "method_0", new[] { typeof(int) });
        }

        [PatchPrefix]
        private static bool Prefix(ref Task __result, AreaData __instance, int timestamp)
        {
            if (dadGamerPlugin.InstantConstructionEnabled.Value)
            {
                __result = InstantCompleteConstruction(__instance, timestamp);
                return false;
            }
            return true;
        }

        private static async Task InstantCompleteConstruction(AreaData __instance, int timestamp)
        {
            await Task.Yield();
            Stage currentStage = __instance.CurrentStage;
            currentStage.Waiting = false;
            __instance.Status = (__instance.CurrentLevel > 0) ? EAreaStatus.ReadyToInstallUpgrade : EAreaStatus.ReadyToInstallConstruct;
            currentStage.ActionGoing = false;
            currentStage.ActionReady = true;
        }
    }

    

    
}
