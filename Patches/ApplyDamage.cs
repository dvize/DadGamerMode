using System;
using System.Reflection;
using Aki.Reflection.Patching;
using dvize.GodModeTest;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;

namespace dvize.DadGamerMode.Patches
{
    internal class ApplyDamage : ModulePatch
    {
        private static ActiveHealthController healthController;
        private static EFT.HealthSystem.ValueStruct currentHealth;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthController), "ApplyDamage");
        }

        [PatchPrefix]
        private static bool Prefix(ActiveHealthController __instance, ref float damage, EBodyPart bodyPart, DamageInfo damageInfo)
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
                        if (dadGamerPlugin.Keep1HealthSelection.Value == "Head And Thorax")
                        {
                            if (bodyPart == EBodyPart.Head || bodyPart == EBodyPart.Chest)
                            {
                                damage = currentHealth.Current - 2f;
                                currentHealth.Current = 2f;
                                return false;
                            }
                            else
                            {
                                if (currentHealth.AtMinimum)
                                {
                                    Logger.LogDebug("Destroyed body part: " + bodyPart.ToString());
                                    healthController.DestroyBodyPart(bodyPart, EDamageType.Bullet);

                                    return false;
                                }
                            }
                        }
                        //Check if they want to keep head and thorax at 1 health or all body parts
                        else if (dadGamerPlugin.Keep1HealthSelection.Value == "All")
                        {
                            damage = currentHealth.Current - 2f;
                            currentHealth.Current = 2f;

                            return false;
                        }

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
