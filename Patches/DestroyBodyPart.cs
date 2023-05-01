﻿using System.Reflection;
using Aki.Reflection.Patching;
using Aki.SinglePlayer.Models.Healing;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using HarmonyLib;

namespace dvize.DadGamerMode.Patches
{
    internal class DestroyBodyPartPatch : ModulePatch
    {
        private static readonly EBodyPart[] critBodyParts = { EBodyPart.Stomach, EBodyPart.Head, EBodyPart.Chest };
        private static DamageInfo tmpDmg;
        private static PlayerHealth playerStats;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthControllerClass), "DestroyBodyPart");
        }

        [PatchPrefix]
        static bool Prefix(ActiveHealthControllerClass __instance, EBodyPart bodyPart, EDamageType damageType)
        {
            //only care about your player
            if (__instance.Player != null && __instance.Player.IsYourPlayer)
            {
                playerStats = Singleton<PlayerHealth>.Instance;

                //if CODMode is enabled and bleeding damage is disabled
                if (dadGamerPlugin.CODModeToggle.Value && 
                    !dadGamerPlugin.CODBleedingDamageToggle.Value)
                {
                    //we don't want to destroy body parts if we are bleeding
                    return false;
                }

                //if keep1Health is enabled
                if (dadGamerPlugin.Keep1Health.Value)
                {
                    //any part should have 1 health.. lets just set the current health to 1
                    var change = 1 - playerStats.Health[bodyPart].Current;
                    playerStats.Health[bodyPart].ChangeHealth(1f);

                    return false;
                }
            }

            return true;
        }

    }
}
