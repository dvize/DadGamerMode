using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using dvize.GodModeTest;
using HarmonyLib;
using UnityEngine;

namespace dvize.DadGamerMode.Patches
{

    internal class UpdateWeightLimitsPatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {

            return AccessTools.Method(typeof(GClass681), nameof(GClass681.UpdateWeightLimits));
        }

        [PatchPrefix]
        static bool Prefix(GClass681 __instance)
        {
            // Accessing a protected field
            var iobserverToPlayerBridge_0 = AccessTools.FieldRefAccess<GClass681, GClass681.IObserverToPlayerBridge>(__instance, "iobserverToPlayerBridge_0");
            var bool_7 = AccessTools.FieldRefAccess<GClass681, bool>(__instance, "bool_7");

            //use our modifier for weight
            if (!bool_7 && iobserverToPlayerBridge_0.iPlayer.IsYourPlayer)
            {
                BackendConfigSettingsClass.GClass1368 stamina = Singleton<BackendConfigSettingsClass>.Instance.Stamina;
                float num = iobserverToPlayerBridge_0.Skills.CarryingWeightRelativeModifier
                            * iobserverToPlayerBridge_0.iPlayer.HealthController.CarryingWeightRelativeModifier
                            * (1 + (dadGamerPlugin.totalWeightReductionPercentage.Value / 100f));

                Vector2 vector = new Vector2(iobserverToPlayerBridge_0.iPlayer.HealthController.CarryingWeightAbsoluteModifier, iobserverToPlayerBridge_0.iPlayer.HealthController.CarryingWeightAbsoluteModifier);
                BackendConfigSettingsClass.InertiaSettings inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
                Vector3 vector2 = new Vector3(inertia.InertiaLimitsStep * (float)iobserverToPlayerBridge_0.Skills.Strength.SummaryLevel, inertia.InertiaLimitsStep * (float)iobserverToPlayerBridge_0.Skills.Strength.SummaryLevel, 0f);
                __instance.BaseInertiaLimits = inertia.InertiaLimits + vector2;
                __instance.WalkOverweightLimits = stamina.WalkOverweightLimits * num + vector;
                __instance.BaseOverweightLimits = stamina.BaseOverweightLimits * num + vector;
                __instance.SprintOverweightLimits = stamina.SprintOverweightLimits * num + vector;
                __instance.WalkSpeedOverweightLimits = stamina.WalkSpeedOverweightLimits * num + vector;
                return false;
            }
            __instance.WalkOverweightLimits.Set(9000f, 10000f);
            __instance.BaseOverweightLimits.Set(9000f, 10000f);
            __instance.SprintOverweightLimits.Set(9000f, 10000f);
            __instance.WalkSpeedOverweightLimits.Set(9000f, 10000f);

            //don't run again
            return false;
        }
    }


}
