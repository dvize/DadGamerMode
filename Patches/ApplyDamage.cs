using System.Reflection;
using Aki.Reflection.Patching;
using Aki.SinglePlayer.Models.Healing;
using Comfort.Common;
using dvize.GodModeTest;
using HarmonyLib;
using UnityEngine;

namespace dvize.DadGamerMode.Patches
{
    internal class ApplyDamage : ModulePatch
    {
        private static PlayerHealth playerStats;
        private static MethodInfo currentHealthProp;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthControllerClass), "ApplyDamage");
        }

        [PatchPrefix]
        public static bool Prefix(ActiveHealthControllerClass __instance, ref float damage, EBodyPart bodyPart, DamageInfo damageInfo)
        {
            if (__instance.Player != null && __instance.Player.IsYourPlayer)
            {
                playerStats = Singleton<PlayerHealth>.Instance;

                //just set damage to 0 and not run apply damage for GodMode
                if (dadGamerPlugin.Godmode.Value)
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

                //if headshot damage ignore it
                if (bodyPart == EBodyPart.Head && dadGamerPlugin.IgnoreHeadShotDamage.Value)
                {
                    damage = 0f;
                    return false;
                }

                //if keep 1 health enabled, set damage to 0 
                if ((dadGamerPlugin.Keep1Health.Value &&
                    (playerStats.Health[bodyPart].Current - damage) < 1))
                {
                    //just never apply damage if keep1health is enabled and we are going to be below 1 health
                    damage = 0f;

                    return false;
                }
            }

            return true;
        }

    }
}
