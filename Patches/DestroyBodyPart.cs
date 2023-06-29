using System;
using System.Reflection;
using Aki.Reflection.Patching;
using dvize.GodModeTest;
using EFT;
using HarmonyLib;

namespace dvize.DadGamerMode.Patches
{
    internal class DestroyBodyPartPatch : ModulePatch
    {
        private static readonly EBodyPart[] critBodyParts = { EBodyPart.Stomach, EBodyPart.Head, EBodyPart.Chest };
        private static DamageInfo tmpDmg;
        private static ActiveHealthControllerClass healthController;
        private static EFT.HealthSystem.ValueStruct currentHealth;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthControllerClass), "DestroyBodyPart");
        }

        [PatchPrefix]
        private static bool Prefix(ActiveHealthControllerClass __instance, EBodyPart bodyPart, EDamageType damageType)
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
                        //we don't want to destroy body parts if we are bleeding
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
