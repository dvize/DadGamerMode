using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace dvize.DadGamerMode.Patches
{

    internal class OnWeightUpdatedPatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {

            return AccessTools.Method(typeof(Physical), nameof(Physical.OnWeightUpdated));
        }

        [PatchPrefix]
        static bool Prefix(Physical __instance)
        {
            var iobserverToPlayerBridge_0 = AccessTools.FieldRefAccess<Physical, Physical.IObserverToPlayerBridge>(__instance, "iobserverToPlayerBridge_0");
            
            if (iobserverToPlayerBridge_0.iPlayer.IsYourPlayer)
            {
                var float_3 = AccessTools.FieldRefAccess<Physical, float>(__instance, "float_3");

                //modify the weight by our totalWeightReductionPercentage(int converted to percentage)
                float totalWeight = iobserverToPlayerBridge_0.TotalWeight * (dadGamerPlugin.totalWeightReductionPercentage.Value / 100f);

                BackendConfigSettingsClass.InertiaSettings inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
                __instance.Inertia = __instance.CalculateValue(__instance.BaseInertiaLimits, totalWeight);
                __instance.SprintAcceleration = inertia.SprintAccelerationLimits.InverseLerp(__instance.Inertia);
                __instance.PreSprintAcceleration = inertia.PreSprintAccelerationLimits.Evaluate(__instance.Inertia);
                float num = Mathf.Lerp(inertia.MinMovementAccelerationRangeRight.x, inertia.MaxMovementAccelerationRangeRight.x, __instance.Inertia);
                float num2 = Mathf.Lerp(inertia.MinMovementAccelerationRangeRight.y, inertia.MaxMovementAccelerationRangeRight.y, __instance.Inertia);
                EFTHardSettings.Instance.MovementAccelerationRange.MoveKey(1, new Keyframe(num, num2));
                __instance.Overweight = __instance.BaseOverweightLimits.InverseLerp(totalWeight);
                __instance.WalkOverweight = __instance.WalkOverweightLimits.InverseLerp(totalWeight);
                float_3 = __instance.SprintOverweightLimits.InverseLerp(totalWeight);
                __instance.WalkSpeedLimit = 1f - __instance.WalkSpeedOverweightLimits.InverseLerp(totalWeight);
                __instance.MoveSideInertia = inertia.SideTime.Evaluate(__instance.Inertia);
                __instance.MoveDiagonalInertia = inertia.DiagonalTime.Evaluate(__instance.Inertia);
                if (iobserverToPlayerBridge_0.iPlayer.IsAI)
                {
                    float_3 = 0f;
                }
                __instance.FallDamageMultiplier = Mathf.Lerp(1f, __instance.StaminaParameters.FallDamageMultiplier, __instance.Overweight);
                __instance.SoundRadius = __instance.StaminaParameters.SoundRadius.Evaluate(__instance.Overweight);
                __instance.MinStepSound.SetDirty();
                __instance.TransitionSpeed.SetDirty();
                __instance.method_3();
                __instance.method_7(totalWeight);

                return false;
            }

            return true;
        }
    }


}
