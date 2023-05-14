using System;
using System.Reflection;
using Aki.Reflection.Patching;
using Aki.SinglePlayer.Models.Healing;
using Aki.SinglePlayer.Utils.Healing;
using Comfort.Common;
using dvize.GodModeTest;
using HarmonyLib;

namespace dvize.DadGamerMode.Patches
{
    internal class ApplyDamage : ModulePatch
    {
        private static ActiveHealthControllerClass healthController;
        private static EFT.HealthSystem.ValueStruct currentHealth;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthControllerClass), "ApplyDamage");
        }

        [PatchPrefix]
        private static bool Prefix(ActiveHealthControllerClass __instance, ref float damage, EBodyPart bodyPart, DamageInfo damageInfo)
        {
            try
            {
                if (__instance.Player != null &&
                __instance.Player.IsYourPlayer)
                {
                    healthController = __instance.Player.ActiveHealthController;
                    currentHealth = healthController.GetBodyPartHealth(bodyPart, false);

                    //just set damage to 0 and not run apply damage for GodMode
                    if (dadGamerPlugin.Godmode.Value)
                    {
                        damage = 0f;
                        return false;
                    }

                    //if headshot damage ignore it
                    if (bodyPart == EBodyPart.Head && dadGamerPlugin.IgnoreHeadShotDamage.Value)
                    {
                        damage = 0f;
                        return false;
                    }

                    //if there's a custom damage value use that
                    if (dadGamerPlugin.CustomDamageModeVal.Value != 100)
                    {
                        //set damage early so we can use it in the keep1health check
                        damage = damage * ((float)dadGamerPlugin.CustomDamageModeVal.Value / 100);
                    }

                    //if keep 1 health enabled, set damage to 0 
                    if (dadGamerPlugin.Keep1Health.Value &&
                        ((currentHealth.Current - damage) <= 0))
                    {
                        damage = 0f;
                        currentHealth.Current = 1f;
                        
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

            return true;
        }

    }
}
