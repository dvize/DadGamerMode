using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace dvize.GodModeTest
{
    [BepInPlugin("com.dvize.DadGamerMode", "dvize.DadGamerMode", "1.3.0")]

    public class dadGamer : BaseUnityPlugin
    {
        public ConfigEntry<Boolean> Godmode { get; set; }
        public static ConfigEntry<Boolean> CustomDamageMode { get; set; }
        public static ConfigEntry<int> CustomDamageModeVal { get; set; }
        public static ConfigEntry<Boolean> IgnoreHeadShotDamage { get; set; }
        public ConfigEntry<Boolean> InstaSearch { get; private set; }
        public ConfigEntry<Boolean> MaxStaminaToggle { get; set; }
        //public static ConfigEntry<Boolean> CodHealthRestore { get; set; }
        //public static ConfigEntry<float> CodHealRate { get; set; }
        
        public static EFT.UI.SessionEnd.SessionResultExitStatus.GClass2755 sessionResultExitStatus;
        internal void Awake()
        {
            Godmode = Config.Bind("Player | Health", "Godmode", false, "Invincible");
            MaxStaminaToggle = Config.Bind("Player | Skills", "Infinite Stamina", false, "Stamina Never Drains");
            InstaSearch = Config.Bind("Player | Skills", "Instant Search", false);
            CustomDamageModeVal = Config.Bind("Player | Health", "% Damage Received Value", 100);
            IgnoreHeadShotDamage = Config.Bind("Player | Health", "Ignore Headshot Damage", false);
            //CodHealthRestore = Config.Bind("Player | Health", "COD Health Restore", false);
            //CodHealRate = Config.Bind("Player | Health", "COD Heal Rate", 10f);
            
            new ApplyDamagePatch().Enable();
        }

        public static Player player;
        public static AbstractGame game;
        public static float healrate;
        private bool eventAssigned = false;
        public static float lastHitTimer = 0f;
        public static bool readyToHeal = false;
        public static DamageInfo damageInfo = new DamageInfo
        {
            DamageType = EDamageType.Medicine,
            Damage = healrate,
            Player = player
        };
        void Update()
        {
            try
            {
                game = Singleton<AbstractGame>.Instance;
                lastHitTimer += Time.unscaledDeltaTime;
                
                if(lastHitTimer >= 10f)
                {
                    readyToHeal = true;
                }
                
                if (game.InRaid && Camera.main.transform.position != null)
                {
                    player = Singleton<GameWorld>.Instance.MainPlayer;

                    if (Godmode.Value == true)
                    {
                        try
                        {
                            player.PlayerHealthController.SetDamageCoeff(-1f);
                            player.PlayerHealthController.FallSafeHeight = 999999f;
                            //player.PlayerHealthController.ChangeHealth(EBodyPart.Head, 9999999, new DamageInfo());

                            player.PlayerHealthController.RemoveNegativeEffects(EBodyPart.Common);
                            player.PlayerHealthController.RestoreFullHealth();



                        }
                        catch
                        {
                            Logger.LogDebug("Error GodMode Not Running.");
                        }

                    }
                    else
                    {
                        try
                        {
                            player.PlayerHealthController.SetDamageCoeff(1f);
                            player.PlayerHealthController.FallSafeHeight = 1.5f;
                        }
                        catch
                        {
                            Logger.LogDebug("Error GodMode Off Functionality not working.");
                        }
                    }



                    if (MaxStaminaToggle.Value == true)
                    {
                        try
                        {
                            player.Physical.Stamina.Current = player.Physical.Stamina.TotalCapacity.Value;
                            player.Physical.HandsStamina.Current = player.Physical.HandsStamina.TotalCapacity.Value;
                            player.Physical.Oxygen.Current = player.Physical.Oxygen.TotalCapacity.Value;
                        }
                        catch
                        {
                            Logger.LogDebug("Error MaxStamina Invoke Failed.");
                        }
                    }

                    if (InstaSearch.Value == true)
                    {
                        try
                        {
                            player.Skills.AttentionEliteExtraLootExp.Value = true;
                            player.Skills.AttentionEliteLuckySearch.Value = 100f;
                            player.Skills.IntellectEliteContainerScope.Value = true;
                        }
                        catch
                        {
                            Logger.LogDebug("Error Instasearch Invoke Failed.");
                        }

                    }

                   /* if (CodHealthRestore.Value == true)
                    {
                        if (!eventAssigned)
                        {
                            player.BeingHitAction += Player_BeingHitAction;
                        }

                        try
                        {
                            if (readyToHeal)
                            {
                                //Logger.LogDebug("DadGamerMode: Timer is Finished");

                                if ((player.ActiveHealthController.GetBodyPartHealth(EBodyPart.Common, false).AtMaximum) == false)
                                {
                                    healrate = CodHealRate.Value * Time.unscaledDeltaTime;
                                    player.ActiveHealthController.ChangeHealth(EBodyPart.Common, healrate, damageInfo);

                                }
                            }
                        }
                        catch
                        {
                            
                        }

                    }*/

                    
                }

            }
            catch
            {
                
            }


        }

        /*private void Player_BeingHitAction(DamageInfo arg1, EBodyPart arg2, float arg3)
        {
            //check if neg effects and reset timer as well to 0.
            player.ActiveHealthController.RemoveNegativeEffects(arg2);

            if (player.ActiveHealthController.IsBodyPartDestroyed(arg2))
            {
                player.ActiveHealthController.RestoreBodyPart(arg2, 0f);
            }

            lastHitTimer = 0f;
            readyToHeal = false;
        }*/

    }

    
    public class ApplyDamagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            try
            {
                return typeof(ActiveHealthControllerClass).GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.Public);
            }
            catch(Exception e)
            {
                Logger.LogDebug("Error ApplyDamagePatch: " + e);
                return null;
            }

        }

        [PatchPrefix]
        public static bool Prefix(ActiveHealthControllerClass __instance, ref float damage, EBodyPart bodyPart, DamageInfo damageInfo)
        {
            if (__instance.Player.IsYourPlayer)   
            {
                if (bodyPart == EBodyPart.Head && dadGamer.IgnoreHeadShotDamage.Value == true)
                {
                    damage = 0f;
                    return false;
                }

                float damagePercent = (float)dadGamer.CustomDamageModeVal.Value / 100;
                damage = damage * damagePercent;
                
            }
            return true;
        }

    }

    

}

