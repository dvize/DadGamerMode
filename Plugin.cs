using System;
using System.Reflection;
using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using dvize.DadGamerMode.Features;
using EFT;

namespace dvize.GodModeTest
{
    [BepInPlugin("com.dvize.DadGamerMode", "dvize.DadGamerMode", "1.8.0")]
    //[BepInDependency("com.spt-aki.core", "3.8.0")]
    public class dadGamerPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<Boolean> Godmode
        {
            get; set;
        }
        public static ConfigEntry<Boolean> Keep1Health
        {
            get; set;
        }
        public static ConfigEntry<string> Keep1HealthSelection
        {
            get; set;
        }
        //list of values for Keep1HealthSelection
        public string[] Keep1HealthSelectionList = new string[] { "All", "Head And Thorax" };
        public static ConfigEntry<Boolean> NoFallingDamage
        {
            get; set;
        }
        public static ConfigEntry<int> CustomHeadDamageModeVal
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
        public static ConfigEntry<Boolean> PercentageHeadShotDamageOnly
        {
            get; set;
        }
        public static ConfigEntry<Boolean> MaxStaminaToggle
        {
            get; set;
        }

        public static ConfigEntry<Boolean> MaxHydrationToggle
        {
            get; set;
        }

        public static ConfigEntry<Boolean> MaxEnergyToggle
        {
            get; set;
        }

        public static ConfigEntry<Boolean> CODModeToggle
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
        public static ConfigEntry<Boolean> CODBleedingDamageToggle
        {
            get; set;
        }
        public static ConfigEntry<float> ReloadSpeed
        {
            get; set;
        }

        public static ConfigEntry<int> enemyDamageMultiplier
        {
            get; set;
        }

        public static ConfigEntry<int> totalWeightReductionPercentage
        {
            get; set;
        }

        internal void Awake()
        {

            Godmode = Config.Bind("1. Health", "Godmode", false, new ConfigDescription("Makes You Invincible Except for Fall Damage",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 9 }));

            Keep1Health = Config.Bind("1. Health", "Keep 1 Health", false, new ConfigDescription("Enable To Keep Body Parts Above 1 Health",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 8 }));

            Keep1HealthSelection = Config.Bind("1. Health", "Keep 1 Health Selection", "All", new ConfigDescription("Select which body parts to keep above 1 health",
                new AcceptableValueList<string>(Keep1HealthSelectionList), new ConfigurationManagerAttributes { IsAdvanced = false, Order = 7 }));

            IgnoreHeadShotDamage = Config.Bind("1. Health", "Ignore Headshot Damage", false, new ConfigDescription("Ignore Headshot Damage",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 6 }));

            PercentageHeadShotDamageOnly = Config.Bind("1. Health", "Percentage Headshot Damage", false, new ConfigDescription("Toggle Percentage Headshot Damage, will override all body percentage",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            CustomHeadDamageModeVal = Config.Bind("1. Health", "% Damage Received Value (Only Head)", 100, new ConfigDescription("Set a Damage Threshold Limith and needs Percentage Headshot Damage toggled",
                new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = true, Order = 4 }));

            CustomDamageModeVal = Config.Bind("1. Health", "% Damage Received Value (All Body)", 100, new ConfigDescription("Set a Damage Threshold Limit",
                new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = true, Order = 3 }));

            CODModeToggle = Config.Bind("2. COD", "CODMode", false, new ConfigDescription("Gradually heals all your damage over time including bleeds and fractures",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            CODModeHealRate = Config.Bind("2. COD", "CODMode Heal Rate", 10f, new ConfigDescription("Sets How Fast You Heal",
                new AcceptableValueRange<float>(0f, 100f), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 3 }));

            CODModeHealWait = Config.Bind("2. COD", "CODMode Heal Wait", 10f, new ConfigDescription("Sets How Long You Have to Wait in Seconds with no damage before healing starts",
                new AcceptableValueRange<float>(0f, 600f), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 2 }));

            CODBleedingDamageToggle = Config.Bind("2. COD", "CODMode Bleeding Damage", false, new ConfigDescription("You still get bleeding and fractures for COD Mode",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));

            NoFallingDamage = Config.Bind("3. QOL", "No Falling Damage", false, new ConfigDescription("No Falling Damage",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 7 }));

            MaxStaminaToggle = Config.Bind("3. QOL", "Infinite Stamina", false, new ConfigDescription("Stamina Never Drains",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 6 }));

            MaxEnergyToggle = Config.Bind("3. QOL", "Infinite Energy", false, new ConfigDescription("Energy Never Drains so no eating",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            MaxHydrationToggle = Config.Bind("3. QOL", "Infinite Hydration", false, new ConfigDescription("Hydration never drains so no drinking",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4}));

            ReloadSpeed = Config.Bind("3. QOL", "ReloadSpeed", 0.85f, new ConfigDescription("Magazine Reload Speed Multiplier (smaller is faster)",
                new AcceptableValueRange<float>(0f, 0.85f), new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            enemyDamageMultiplier = Config.Bind("3. QOL", "Enemy Damage Multiplier", 1, new ConfigDescription("Multiply Damage Given to Enemies (Default is 1)",
                new AcceptableValueRange<int>(1, 20), new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));

            totalWeightReductionPercentage = Config.Bind("3. QOL", "Total Weight % Reduction", 0, new ConfigDescription("Percentage to reduce your characters total weight",
                new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));

            new NewGamePatch().Enable();
            new DadGamerMode.Patches.ApplyDamage().Enable();
            new DadGamerMode.Patches.DestroyBodyPartPatch().Enable();
            new DadGamerMode.Patches.OnWeightUpdatedPatch().Enable();
        }

        internal class NewGamePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));

            [PatchPrefix]
            private static void PatchPrefix()
            {
                CODModeComponent.Enable();
                MaxStaminaComponent.Enable();
                HydrationComponent.Enable();
                EnergyComponent.Enable();
                NoFallingDamageComponent.Enable();
                MagReloadSpeed.Enable();
            }
        }
    }



}

