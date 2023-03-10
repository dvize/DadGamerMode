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
        public static ConfigEntry<Boolean> CustomDamageMode
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

        internal void Awake()
        {
            Godmode = Config.Bind("Player | Health", "Godmode", false, "Invincible");
            NoFallingDamage = Config.Bind("Player | Health", "No Falling Damage", false, "No Falling Damage");
            MaxStaminaToggle = Config.Bind("Player | Skills", "Infinite Stamina", false, "Stamina Never Drains");
            InstaSearch = Config.Bind("Player | Skills", "Instant Search", false);
            CustomDamageModeVal = Config.Bind("Player | Health", "% Damage Received Value", 100);
            IgnoreHeadShotDamage = Config.Bind("Player | Health", "Ignore Headshot Damage", false);

            new ApplyDamagePatch().Enable();
        }

        public static Player player;
        public static AbstractGame game;
        public static float healrate;
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
            game = Singleton<AbstractGame>.Instance;

            if (game.InRaid && Camera.main.transform.position != null)
            {
                player = Singleton<GameWorld>.Instance.MainPlayer;

                SetGodMode(Godmode.Value);
                SetNoFallingDamage(NoFallingDamage.Value);
                SetMaxStamina(MaxStaminaToggle.Value);
                SetInstaSearch(InstaSearch.Value);
            }
        }

        void SetGodMode(bool value)
        {
            try
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
            catch { }
        }

        void SetNoFallingDamage(bool value)
        {
            try
            {
                if (value)
                    player.PlayerHealthController.FallSafeHeight = 999999f;
                else
                    player.PlayerHealthController.FallSafeHeight = 1.5f;
            }
            catch { }
        }

        void SetMaxStamina(bool value)
        {
            try
            {
                player.Physical.Stamina.Current = player.Physical.Stamina.TotalCapacity.Value;
                player.Physical.HandsStamina.Current = player.Physical.HandsStamina.TotalCapacity.Value;
                player.Physical.Oxygen.Current = player.Physical.Oxygen.TotalCapacity.Value;
            }
            catch
            {
            }
        }

        void SetInstaSearch(bool value)
        {
            try
            {
                player.Skills.AttentionEliteExtraLootExp.Value = true;
                player.Skills.AttentionEliteLuckySearch.Value = 100f;
                player.Skills.IntellectEliteContainerScope.Value = true;
            }
            catch
            {
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

