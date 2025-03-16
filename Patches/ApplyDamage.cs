using System;
using System.Reflection;
using SPT.Reflection.Patching;
using dvize.GodModeTest;
using EFT.HealthSystem;
using HarmonyLib;
using EFT;

namespace dvize.DadGamerMode.Patches
{
    internal class ApplyDamage : ModulePatch
    {
        private static ActiveHealthController healthController;
        private static EFT.HealthSystem.ValueStruct currentHealth;
        private static bool potentialHealthLowerThanMinimum;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthController), "ApplyDamage");
        }

        [PatchPrefix]
        private static bool Prefix(ActiveHealthController __instance, ref float damage, ref Player ___Player, EBodyPart bodyPart, DamageInfoStruct damageInfo)
        {
            try
            {
                
                if (___Player != null &&
                ___Player.IsYourPlayer)
                {
                    healthController = ___Player.ActiveHealthController;
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

                    //if there's a custom headshot damage value use that
                    if (bodyPart == EBodyPart.Head && dadGamerPlugin.PercentageHeadShotDamageOnly.Value)
                    {
                        //set damage early so we can use it in the keep1health check
                        damage = damage * ((float)dadGamerPlugin.CustomHeadDamageModeVal.Value / 100);
                    }

                    //if keep 1 health enabled, ensure health does not drop below 1
                    if (dadGamerPlugin.Keep1Health.Value)
                    {
                        potentialHealthLowerThanMinimum = (currentHealth.Current - damage) <= currentHealth.Minimum;

                        // Check if this damage would bring health below 3f
                        if (potentialHealthLowerThanMinimum)
                        {
                            if ((dadGamerPlugin.Keep1HealthSelection.Value == "Head And Thorax" && (bodyPart == EBodyPart.Head || bodyPart == EBodyPart.Chest)))
                            {
                                damage = 0f;
                                currentHealth.Current = 3f;
                                return false;
                            }

                            else if (dadGamerPlugin.Keep1HealthSelection.Value == "All")
                            {
                                damage = 0f;
                                currentHealth.Current = 3f;
                                return false;
                            }
                        }
                    }

                    //add check if bodypart at minimum for non-critical parts as it can still kill you and CODMode is not enabled
                    if (currentHealth.AtMinimum && 
                        (dadGamerPlugin.Keep1HealthSelection.Value == "Head And Thorax" || dadGamerPlugin.Keep1HealthSelection.Value == "All") &&
                        !dadGamerPlugin.CODModeToggle.Value
                        )
                    {
                        return false;
                    }
                }
                else
                {
                    //multiply damage by multiplier if a type of player
                    if (___Player != null)
                    {
                        damage = damage * dadGamerPlugin.enemyDamageMultiplier.Value;
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
