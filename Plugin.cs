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
    [BepInPlugin("com.dvize.DadGamerMode", "dvize.DadGamerMode", "1.6.5")]

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

            Godmode = Config.Bind("Health", "Godmode", false, "Invincible but Fall Damage Separate");
            NoFallingDamage = Config.Bind("Health", "No Falling Damage", false, "No Falling Damage");
            MaxStaminaToggle = Config.Bind("Health", "Infinite Stamina", false, "Stamina Never Drains");
            Keep1Health = Config.Bind("Health", "Keep 1 Health", false, "Keeps your bodyparts from falling below 1 Health");

            CustomDamageModeVal = Config.Bind("Health", "% Damage Received Value", 100, "Set a Damage Threshold limit %");
            IgnoreHeadShotDamage = Config.Bind("Health", "Ignore Headshot Damage", false, "Ignore headshot");
            CODModeToggle = Config.Bind("COD", "CODMode", false, "If you don't die, gradually heals you and no blacked out limbs");
            CODModeHealRate = Config.Bind("COD", "CODMode Heal Rate", 10f, "Sets how fast you heal");
            CODModeHealWait = Config.Bind("COD", "CODMode Heal Wait", 10f, "Sets how long you wait to heal in seconds");
            CODBleedingDamageToggle = Config.Bind("COD", "CODMode Bleeding Damage", false, "You still get bleeding and fractures if enabled");


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

