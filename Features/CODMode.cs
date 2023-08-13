using System.Collections;
using BepInEx.Logging;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class CODModeComponent : MonoBehaviour
    {
        private static Player player;
        private static ActiveHealthController healthController;
        private static float timeSinceLastHit = 0f;
        private static bool isRegenerating = false;
        private static DamageInfo tmpDmg;

        private readonly EBodyPart[] bodyPartsDict = { EBodyPart.Stomach, EBodyPart.Chest, EBodyPart.Head, EBodyPart.RightLeg,
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
        internal static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<CODModeComponent>();

                Logger.LogDebug("DadGamerMode: CODModeComponent enabled");
            }
        }
        private void Start()
        {
            player = Singleton<GameWorld>.Instance.MainPlayer;
            healthController = player.ActiveHealthController;

            player.OnPlayerDeadOrUnspawn += Player_OnPlayerDeadOrUnspawn;
            player.BeingHitAction += Player_BeingHitAction;
        }

        void Update()
        {
            if (dadGamerPlugin.CODModeToggle.Value)
            {
                timeSinceLastHit += Time.unscaledDeltaTime;

                if (timeSinceLastHit >= dadGamerPlugin.CODModeHealWait.Value)
                {
                    if (!isRegenerating)
                    {
                        isRegenerating = true;
                        StartCoroutine(Heal());
                    }
                }
            }
        }


        private IEnumerator Heal()
        {
            while (isRegenerating && dadGamerPlugin.CODModeToggle.Value)
            {
                // Remove negative effects every frame unless bleeding damage is toggled
                if (!dadGamerPlugin.CODBleedingDamageToggle.Value)
                {
                    foreach (EBodyPart limb in bodyPartsDict)
                    {
                        // Remove negative effects only if bleeding damage is disabled.
                        healthController.RemoveNegativeEffects(limb);
                    }
                }

                // Heal player if time passed the CODModeHealWait value
                float newHealRate = dadGamerPlugin.CODModeHealRate.Value * Time.unscaledDeltaTime;

                foreach (EBodyPart limb in bodyPartsDict)
                {
                    healthController.ChangeHealth(limb, newHealRate, tmpDmg);
                }

                // Wait for the next frame before continuing
                yield return null;
            }
        }
        private void Disable()
        {
            if (player != null)
            {
                player.OnPlayerDeadOrUnspawn -= Player_OnPlayerDeadOrUnspawn;
                player.BeingHitAction -= Player_BeingHitAction;
            }
        }

        private void Player_BeingHitAction(DamageInfo arg1, EBodyPart arg2, float arg3)
        {
            //Logger.LogDebug("DadGamerMode: Player_BeingHitAction called");
            timeSinceLastHit = 0f;
            isRegenerating = false;
            StopCoroutine(Heal());
        }


        private void Player_OnPlayerDeadOrUnspawn(Player player)
        {
            Disable();
        }
    }
}
