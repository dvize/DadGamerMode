using System;
using System.Diagnostics;
using System.Reflection;
using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using dvize.DadGamerMode.Features;
using EFT;
using VersionChecker;

namespace dvize.GodModeTest
{
    [BepInPlugin("com.dvize.DadGamerMode", "dvize.DadGamerMode", "1.6.7")]

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
        public static ConfigEntry<int> CustomDamageModeVal
        {
            get; set;
        }
        public static ConfigEntry<Boolean> IgnoreHeadShotDamage
        {
            get; set;
        }
        public static ConfigEntry<Boolean> MaxStaminaToggle
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

        internal void Awake()
        {
            CheckEftVersion();

            Godmode = Config.Bind("1. Health", "Godmode", false, new ConfigDescription("Makes You Invincible Except for Fall Damage",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 7 }));

            Keep1Health = Config.Bind("1. Health", "Keep 1 Health", false, new ConfigDescription("Enable To Keep Body Parts Above 1 Health",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 6 }));

            Keep1HealthSelection = Config.Bind("1. Health", "Keep 1 Health Selection", "All", new ConfigDescription("Select which body parts to keep above 1 health",
                new AcceptableValueList<string>(Keep1HealthSelectionList), new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            IgnoreHeadShotDamage = Config.Bind("1. Health", "Ignore Headshot Damage", false, new ConfigDescription("Ignore Headshot Damage",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            CustomDamageModeVal = Config.Bind("1. Health", "% Damage Received Value", 100, new ConfigDescription("Set a Damage Threshold Limit",
                new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = true, Order = 3 }));

            NoFallingDamage = Config.Bind("1. Health", "No Falling Damage", false, new ConfigDescription("No Falling Damage",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));

            MaxStaminaToggle = Config.Bind("1. Health", "Infinite Stamina", false, new ConfigDescription("Stamina Never Drains",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));

            CODModeToggle = Config.Bind("2. COD", "CODMode", false, new ConfigDescription("Gradually heals all your damage over time including bleeds and fractures",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            CODModeHealRate = Config.Bind("2. COD", "CODMode Heal Rate", 10f, new ConfigDescription("Sets How Fast You Heal",
                new AcceptableValueRange<float>(0f, 100f), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 3 }));

            CODModeHealWait = Config.Bind("2. COD", "CODMode Heal Wait", 10f, new ConfigDescription("Sets How Long You Have to Wait in Seconds with no damage before healing starts",
                new AcceptableValueRange<float>(0f, 600f), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 2 }));

            CODBleedingDamageToggle = Config.Bind("2. COD", "CODMode Bleeding Damage", false, new ConfigDescription("You still get bleeding and fractures for COD Mode",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));


            new NewGamePatch().Enable();
            new DadGamerMode.Patches.ApplyDamage().Enable();
            new DadGamerMode.Patches.DestroyBodyPartPatch().Enable();
        }

        private void CheckEftVersion()
        {
            // Make sure the version of EFT being run is the correct version
            int currentVersion = FileVersionInfo.GetVersionInfo(BepInEx.Paths.ExecutablePath).FilePrivatePart;
            int buildVersion = TarkovVersion.BuildVersion;
            if (currentVersion != buildVersion)
            {
                Logger.LogError($"ERROR: This version of {Info.Metadata.Name} v{Info.Metadata.Version} was built for Tarkov {buildVersion}, but you are running {currentVersion}. Please download the correct plugin version.");
                EFT.UI.ConsoleScreen.LogError($"ERROR: This version of {Info.Metadata.Name} v{Info.Metadata.Version} was built for Tarkov {buildVersion}, but you are running {currentVersion}. Please download the correct plugin version.");
                throw new Exception($"Invalid EFT Version ({currentVersion} != {buildVersion})");
            }
        }
        internal class NewGamePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));

            [PatchPrefix]
            private static void PatchPrefix()
            {
                CODModeComponent.Enable();
                MaxStaminaComponent.Enable();
                NoFallingDamageComponent.Enable();

            }
        }
    }



}

