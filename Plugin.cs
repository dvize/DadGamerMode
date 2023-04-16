using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using UnityEngine;
using VersionChecker;

namespace dvize.GodModeTest
{
    [BepInPlugin("com.dvize.DadGamerMode", "dvize.DadGamerMode", "1.5.1")]

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
        public ConfigEntry<Boolean> CODBleedingDamageToggle
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
            CODModeToggle = Config.Bind("Player | COD", "CODMode", false, "If you don't die, gradually heals you and no blacked out limbs");
            CODModeHealRate = Config.Bind("Player | COD", "CODMode Heal Rate", 10f, "Sets how fast you heal");
            CODModeHealWait = Config.Bind("Player | COD", "CODMode Heal Wait", 10f, "Sets how long you wait to heal in seconds");
            CODBleedingDamageToggle = Config.Bind("Player | COD", "CODMode Bleeding Damage", false, "You still get bleeding and fractures if enabled");

            CheckEftVersion();
            
            new ApplyDamagePatch().Enable();
            new DestroyBodyPartPatch().Enable();

        }

        public AbstractGame game;
        private DamageInfo tempDmg;
        public float newHealRate;
        public static bool runOnceAlready = false;
        public static bool newGame = true;
        void Update()
        {
            try
            {
                game = Singleton<AbstractGame>.Instance;

                if (game.InRaid && Camera.main.transform.position != null)
                {
                    if (newGame)
                    {
                        var player = Singleton<GameWorld>.Instance.MainPlayer;

                        if (!runOnceAlready && game.Status == GameStatus.Started)
                        {
                            Logger.LogDebug("DadGamerMode: Attaching events");
                            player.BeingHitAction += Player_BeingHitAction;
                            player.OnPlayerDeadOrUnspawn += Player_OnPlayerDeadOrUnspawn;
                            player.ActiveHealthController.BodyPartDestroyedEvent += ActiveHealthController_BodyPartDestroyedEvent;
                            runOnceAlready = true;
                        }

                        SetGodMode(Godmode.Value, player);
                        SetNoFallingDamage(NoFallingDamage.Value, player);
                        SetMaxStamina(MaxStaminaToggle.Value, player);
                        SetInstaSearch(InstaSearch.Value, player);
                        SetCODMode(CODModeToggle.Value, player);
                    }
                }
            }
            catch { }
        }

        private void ActiveHealthController_BodyPartDestroyedEvent(EBodyPart arg1, EDamageType arg2)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;
            Logger.LogDebug("player Active Health Controller BodyPartDestroyed Event: " + arg1 + " " + arg2);

            player.ActiveHealthController.RestoreBodyPart(arg1, 0f);
        }

        float timeSinceLastHit = 0f;
        void Player_BeingHitAction(DamageInfo dmgInfo, EBodyPart bodyPart, float hitEffectId) => timeSinceLastHit = 0f;

        void Player_OnPlayerDeadOrUnspawn(Player player)
        {
            Logger.LogDebug("DadGamerMode: Undo all events");
            player.BeingHitAction -= Player_BeingHitAction;
            player.OnPlayerDeadOrUnspawn -= Player_OnPlayerDeadOrUnspawn;
            player.ActiveHealthController.BodyPartDestroyedEvent -= ActiveHealthController_BodyPartDestroyedEvent;
            runOnceAlready = false;
            newGame = false;

            Task.Delay(TimeSpan.FromSeconds(15)).ContinueWith(_ =>
            {
                // Set newGame = true after the timer is finished so it doesn't execute the events right away
                newGame = true;
            });
        }

        //create array of all EbodyPart enums to search through in loops later
        readonly EBodyPart[] parts = { EBodyPart.Stomach, EBodyPart.Chest, EBodyPart.Head, EBodyPart.RightLeg,
            EBodyPart.LeftLeg, EBodyPart.LeftArm, EBodyPart.RightArm };
        void SetCODMode(bool value, Player player)
        {
            if (value)
            {
                //track timeSinceLastHit since its enabled
                timeSinceLastHit += Time.unscaledDeltaTime;

                foreach (EBodyPart limb in parts)
                {
                    //player.ActiveHealthController.RestoreBodyPart(limb, 0f);

                    // Remove negative effects only if bleeding damage toggled.
                    if (!CODBleedingDamageToggle.Value)
                    {
                        player.ActiveHealthController.RemoveNegativeEffects(limb);
                    }
                }

                //heal player if time passed the CODModeHealWait value
                if (timeSinceLastHit >= dadGamer.CODModeHealWait.Value)
                {
                    newHealRate = dadGamer.CODModeHealRate.Value * Time.unscaledDeltaTime;

                    //Logger.LogDebug($"timeSinceLastHit: {timeSinceLastHit}");
                    try
                    {
                        foreach (EBodyPart limb in parts)
                        {
                            player.ActiveHealthController.ChangeHealth(limb, newHealRate, tempDmg);
                        }
                    }
                    catch
                    {
                        //Logger.LogError("DadGamerMode COD ChangeHealth: " + e);
                    }
                }
            }
        }

        void SetGodMode(bool value, Player player)
        {

            if (value)
            {
                player.PlayerHealthController.SetDamageCoeff(-1f);
                player.PlayerHealthController.FallSafeHeight = 999999;
                player.PlayerHealthController.RemoveNegativeEffects(EBodyPart.Common);
                player.PlayerHealthController.RestoreFullHealth();
            }
            else
            {
                player.PlayerHealthController.SetDamageCoeff(1f);
                player.PlayerHealthController.FallSafeHeight = 1.8f;
            }

        }

        void SetNoFallingDamage(bool value, Player player)
        {
            if (value)
            {
                player.PlayerHealthController.FallSafeHeight = 999999f;
            }
            else
            {
                player.PlayerHealthController.FallSafeHeight = 1.8f;
            }

        }

        void SetMaxStamina(bool value, Player player)
        {
            if (value)
            {
                player.Physical.Stamina.Current = player.Physical.Stamina.TotalCapacity.Value;
                player.Physical.HandsStamina.Current = player.Physical.HandsStamina.TotalCapacity.Value;
                player.Physical.Oxygen.Current = player.Physical.Oxygen.TotalCapacity.Value;
            }
        }

        void SetInstaSearch(bool value, Player player)
        {

            if (value)
            {
                player.Skills.AttentionEliteExtraLootExp.Value = true;
                player.Skills.AttentionEliteLuckySearch.Value = 100f;
                player.Skills.IntellectEliteContainerScope.Value = true;
            }

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

    }

    public class ApplyDamagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            try
            {
                return typeof(ActiveHealthControllerClass).GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.Public);
            }
            catch (Exception e)
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

    public class DestroyBodyPartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            try
            {
                return typeof(ActiveHealthControllerClass).GetMethod("DestroyBodyPart", BindingFlags.Instance | BindingFlags.Public);
            }
            catch (Exception e)
            {
                Logger.LogDebug("Error DestroyBodyPartPatch: " + e);
                return null;
            }

        }

        [PatchPrefix]
        static bool Prefix(ActiveHealthControllerClass __instance, EBodyPart bodyPart, EDamageType damageType)
        {
            if (__instance.Player.IsYourPlayer && dadGamer.CODModeToggle.Value)
            {
                return false; //skip orig method
            }

            return true;
        }

    }

}

