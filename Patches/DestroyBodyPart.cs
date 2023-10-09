using System;
using System.Reflection;
using Aki.Reflection.Patching;
using dvize.GodModeTest;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;

namespace dvize.DadGamerMode.Patches
{
    internal class DestroyBodyPartPatch : ModulePatch
    {
        private static readonly EBodyPart[] critBodyParts = { EBodyPart.Stomach, EBodyPart.Head, EBodyPart.Chest };
        private static DamageInfo tmpDmg;
        private static ActiveHealthController healthController;
        private static EFT.HealthSystem.ValueStruct currentHealth;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthController), "DestroyBodyPart");
        }

        [PatchPrefix]
        private static bool Prefix(ActiveHealthController __instance, EBodyPart bodyPart, EDamageType damageType)
        {
            try
            {
                //only care about your player
                if (__instance.Player != null
                    && __instance.Player.IsYourPlayer)
                {
                    //get the component HealthListener
                    healthController = __instance.Player.ActiveHealthController;
                    currentHealth = healthController.GetBodyPartHealth(bodyPart, false);

                    //if CODMode is enabled and bleeding damage is disabled
                    if (dadGamerPlugin.CODModeToggle.Value &&
                        !dadGamerPlugin.CODBleedingDamageToggle.Value)
                    {
                        return false;
                    }

                    //if bleeding damage is enabled, we don't want to destroy critical body parts
                    if (dadGamerPlugin.CODModeToggle.Value &&
                        dadGamerPlugin.CODBleedingDamageToggle.Value)
                    {
                        //if we are a critical body part return false
                        if (Array.Exists(critBodyParts, element => element == bodyPart))
                        {
                            return false;
                        }

                    }

                    //if keep1Health is enabled
                    if (dadGamerPlugin.Keep1Health.Value)
                    {
                        if (dadGamerPlugin.Keep1HealthSelection.Value == "Head And Thorax")
                        {
                            if (bodyPart == EBodyPart.Head || bodyPart == EBodyPart.Chest)
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else if (dadGamerPlugin.Keep1HealthSelection.Value == "All")
                        {
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
