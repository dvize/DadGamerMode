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
    [BepInPlugin("com.dvize.DadGamerMode", "dvize.DadGamerMode", "1.3.1")]

    public class dadGamer : BaseUnityPlugin
    {
        public ConfigEntry<Boolean> Godmode
        {
            get; set;
        }
        public ConfigEntry<Boolean> NoFallingDamage
        {
            get; set;
        }
        public static ConfigEntry<int> CustomDamageModeVal
        {
            get; set;
        }
        public static ConfigEntry<Boolean> IgnoreHeadShotDamage
        {
            get; set;
        }
        public ConfigEntry<Boolean> InstaSearch
        {
            get; private set;
        }
        public ConfigEntry<Boolean> MaxStaminaToggle
        {
            get; set;
        }
        public ConfigEntry<Boolean> CODModeToggle
        {
            get; set;
        }
        public static ConfigEntry<float> CODModeHealRate
        {
            get; set;
        }
        public static ConfigEntry<float> CODModeHealWait
        {
            get; set;
        }
        internal void Awake()
        {
            Godmode = Config.Bind("Player | Health", "Godmode", false, "Invincible and No Fall Damage");
            NoFallingDamage = Config.Bind("Player | Health", "No Falling Damage", false, "No Falling Damage");
            MaxStaminaToggle = Config.Bind("Player | Skills", "Infinite Stamina", false, "Stamina Never Drains");
            InstaSearch = Config.Bind("Player | Skills", "Instant Search", false, "Allows you to instantly search containers.");
            CustomDamageModeVal = Config.Bind("Player | Health", "% Damage Received Value", 100);
            IgnoreHeadShotDamage = Config.Bind("Player | Health", "Ignore Headshot Damage", false);
            CODModeToggle = Config.Bind("Player | COD", "CODMode", false, "If you don't die, gradually heals you");
            CODModeHealRate = Config.Bind("Player | COD", "CODMode Heal Rate", 10f, "Sets how fast you heal");
            CODModeHealWait = Config.Bind("Player | COD", "CODMode Heal Wait", 10f, "Sets how long you wait to heal in seconds");

            new ApplyDamagePatch().Enable();
        }

        public static Player player;
        public static AbstractGame game;
        private float lastHitTime;
        private DamageInfo tempDmg;
        public static float newHealRate;
        public static bool runOnceAlready;
        void Update()
        {
            try
            {
                game = Singleton<AbstractGame>.Instance;

                if (game.InRaid && Camera.main.transform.position != null)
                {

                    player = Singleton<GameWorld>.Instance.MainPlayer;

                    //allow the assignment of this only once
                    if (runOnceAlready == false)
                    {
                        //Assign player.BeingHit event to OnPlayerTakeDamage method
                        player.BeingHitAction += Player_BeingHitAction;
                        
                        runOnceAlready = true;
                    }
                    
                    SetGodMode(Godmode.Value);
                    SetNoFallingDamage(NoFallingDamage.Value);
                    SetMaxStamina(MaxStaminaToggle.Value);
                    SetInstaSearch(InstaSearch.Value);
                    SetCODMode(CODModeToggle.Value);

                }
                else
                {
                    runOnceAlready = false;
                    lastHitTime = -1;
                }
            }
            catch { }

        }

        private void Player_BeingHitAction(DamageInfo arg1, EBodyPart arg2, float arg3)
        {
            lastHitTime = Time.time;
            //Logger.LogDebug("The PlayerTakeDamage Event was Invoked");
            //Logger.LogDebug("lastHitTime = " + lastHitTime);
        }

        //create array of all EbodyPart enums to search through in loops later
        EBodyPart[] parts = { EBodyPart.Stomach, EBodyPart.Chest, EBodyPart.Head, EBodyPart.RightLeg, 
            EBodyPart.LeftLeg, EBodyPart.LeftArm, EBodyPart.RightArm };
        void SetCODMode(bool value)
        {
            if (value)
            {
                foreach (EBodyPart limb in parts)
                {
                    player.ActiveHealthController.RemoveNegativeEffects(limb);
                }

                if (Time.time - lastHitTime >= dadGamer.CODModeHealWait.Value)
                {
                    newHealRate = dadGamer.CODModeHealRate.Value * Time.unscaledDeltaTime;
                    
                    //Logger.LogDebug($"Current Time.time: {Time.time} , LastHitTime: {lastHitTime}"); 
                    try
                    {
                        foreach (EBodyPart limb in parts)
                        {
                            player.ActiveHealthController.ChangeHealth(limb, newHealRate, tempDmg);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("DadGamerMode COD ChangeHealth: " + e);
                    }
                }
            }

        }

        void SetGodMode(bool value)
        {

            if (value)
            {
                player.PlayerHealthController.SetDamageCoeff(-1f);
                player.PlayerHealthController.FallSafeHeight = float.MaxValue;
                player.PlayerHealthController.RemoveNegativeEffects(EBodyPart.Common);
                player.PlayerHealthController.RestoreFullHealth();
            }
            else
            {
                player.PlayerHealthController.SetDamageCoeff(1f);
                player.PlayerHealthController.FallSafeHeight = 1.5f;
            }

        }

        void SetNoFallingDamage(bool value)
        {
            if (value)
            {
                player.PlayerHealthController.FallSafeHeight = 999999f;
            }
            else
            {
                player.PlayerHealthController.FallSafeHeight = 1.5f;
            }

        }

        void SetMaxStamina(bool value)
        {
            if (value)
            {
                player.Physical.Stamina.Current = player.Physical.Stamina.TotalCapacity.Value;
                player.Physical.HandsStamina.Current = player.Physical.HandsStamina.TotalCapacity.Value;
                player.Physical.Oxygen.Current = player.Physical.Oxygen.TotalCapacity.Value;
            }
        }

        void SetInstaSearch(bool value)
        {

            if (value)
            {
                player.Skills.AttentionEliteExtraLootExp.Value = true;
                player.Skills.AttentionEliteLuckySearch.Value = 100f;
                player.Skills.IntellectEliteContainerScope.Value = true;
            }

        }

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

