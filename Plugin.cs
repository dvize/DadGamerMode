using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using System;

namespace dvize.AILimit
{
    [BepInPlugin("com.dvize.GodModeTest", "dvize.GodModeTest", "1.1.0")]

    public class Plugin : BaseUnityPlugin
    {
        public ConfigEntry<Boolean> Godmode { get; set; }
        public ConfigEntry<Boolean> CustomDamageMode { get; set; }
        public ConfigEntry<int> CustomDamageModeVal { get; set; }
        public ConfigEntry<Boolean> InstaSearch { get; private set; }
        public ConfigEntry<Boolean> MaxStaminaToggle { get; set; }
        internal void Awake()
        {
            Godmode = Config.Bind("Player | Health", "Godmode", false, "Invincible");
            MaxStaminaToggle = Config.Bind("Player | Skills", "Infinite Stamina", false, "Stamina Never Drains");
            InstaSearch = Config.Bind("Player | Skills", "Instant Search", false);
            CustomDamageMode = Config.Bind("Player | Health", "% Damage Received Enabled", false);
            CustomDamageModeVal = Config.Bind("Player | Health", "% Damage Received Value", 100);
        }

        public static Player player;
        void Update()
        {
            if (GClass1748.InRaid)
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
                        Logger.LogInfo("Error GodMode On.");
                    }

                }
                else {
                    try
                    {
                        player.PlayerHealthController.SetDamageCoeff(1f);
                        player.PlayerHealthController.FallSafeHeight = 1.5f;
                    }
                    catch
                    {
                        Logger.LogInfo("Error GodMode Off.");
                    }
                }

                if (CustomDamageMode.Value == true && Godmode.Value != true)
                {
                    try
                    {
                        player.PlayerHealthController.SetDamageCoeff((float)(CustomDamageModeVal.Value/100));
                    }
                    catch
                    {
                        Logger.LogInfo("Error GodMode Off.");
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
                        Logger.LogInfo("Error MaxStamina");
                    }
                }

                if(InstaSearch.Value == true)
                {
                    try
                    {
                        player.Skills.AttentionEliteExtraLootExp.Value = true;
                        player.Skills.AttentionEliteLuckySearch.Value = 100f;
                        player.Skills.IntellectEliteContainerScope.Value = true;
                    }
                    catch
                    {
                        Logger.LogInfo("Error Instasearch");
                    }
                    
                }


            }
            
        }
        

    }
}

