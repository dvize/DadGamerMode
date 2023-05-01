using Aki.SinglePlayer.Models.Healing;
using BepInEx.Logging;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class CODModeComponent : MonoBehaviour
    {
        private static float timeSinceLastHit = 0f;
        private static float newHealRate = 0f;
        private static Player player;
        private static PlayerHealth playerStats;

        private static readonly EBodyPart[] bodyPartsDict = { EBodyPart.Stomach, EBodyPart.Chest, EBodyPart.Head, EBodyPart.RightLeg,
            EBodyPart.LeftLeg, EBodyPart.LeftArm, EBodyPart.RightArm };
        protected static ManualLogSource Logger
        {
            get; private set;
        }

        private CODModeComponent()
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(CODModeComponent));
            }
        }

        private void Update()
        {
            //since its enabled
            if (dadGamerPlugin.CODModeToggle.Value)
            {
                //track timeSinceLastHit since its enabled
                timeSinceLastHit += Time.unscaledDeltaTime;

                runCODMode();
            }

        }

        void runCODMode()
        {
            playerStats = Singleton<PlayerHealth>.Instance;

            //remove negative effects every frame unless bleeding damage toggled
            foreach (EBodyPart limb in bodyPartsDict)
            {
                // Remove negative effects only if bleeding damage is enabled.
                if (dadGamerPlugin.CODBleedingDamageToggle.Value && 
                    playerStats != null)
                {
                    playerStats.Health[limb].RemoveAllEffects();
                }
            }

            //heal player if time passed the CODModeHealWait value
            if (timeSinceLastHit >= dadGamerPlugin.CODModeHealWait.Value &&
                playerStats != null)
            {
                newHealRate = dadGamerPlugin.CODModeHealRate.Value * Time.unscaledDeltaTime;

                //Logger.LogDebug($"timeSinceLastHit: {timeSinceLastHit}");
                try
                {
                    foreach (EBodyPart limb in bodyPartsDict)
                    {
                        playerStats.Health[limb].ChangeHealth(newHealRate);
                    }
                }
                catch
                {
                }
            }

        }

        public static void Enable()
        {
            try
            {
                if (Singleton<AbstractGame>.Instance.InRaid && Camera.main.transform.position != null)
                {
                    var gameWorld = Singleton<GameWorld>.Instance;
                    gameWorld.GetOrAddComponent<CODModeComponent>();

                    var player = gameWorld.MainPlayer;
                    Logger.LogDebug("DadGamerMode: Attaching CODMode Events");

                    player.OnPlayerDeadOrUnspawn += Player_OnPlayerDeadOrUnspawn;
                    player.BeingHitAction += Player_BeingHitAction;

                }
            }
            catch { }
        }

        private static void Player_BeingHitAction(DamageInfo arg1, EBodyPart arg2, float arg3)
        {
            timeSinceLastHit = 0f;
        }

        public static void Disable()
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;

            //unattach events
            player.OnPlayerDeadOrUnspawn -= Player_OnPlayerDeadOrUnspawn;
            player.BeingHitAction -= Player_BeingHitAction;
        }

        private static void Player_OnPlayerDeadOrUnspawn(Player player)
        {
            //unattach events
            player.OnPlayerDeadOrUnspawn -= Player_OnPlayerDeadOrUnspawn;
            player.BeingHitAction -= Player_BeingHitAction;
        }
    }
}
