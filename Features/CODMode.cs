using System.Collections;
using System.Threading.Tasks;
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
        private Player player;
        private ActiveHealthController healthController;
        private float timeSinceLastHit = 0f;
        private bool isRegenerating = false;
        private float newHealRate;
        private DamageInfo tmpDmg;
        private EFT.HealthSystem.ValueStruct currentHealth;

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

        private async void Update()
        {
            if (dadGamerPlugin.CODModeToggle.Value)
            {
                timeSinceLastHit += Time.unscaledDeltaTime;

                if (timeSinceLastHit >= dadGamerPlugin.CODModeHealWait.Value)
                {
                    if (!isRegenerating)
                    {
                        isRegenerating = true;
                    }

                    StartHealingAsync();
                }
            }
        }

        private async Task StartHealingAsync()
        {
            if (isRegenerating && dadGamerPlugin.CODModeToggle.Value)
            {
                newHealRate = dadGamerPlugin.CODModeHealRate.Value * Time.unscaledDeltaTime;

                foreach (var limb in bodyPartsDict)
                {
                    currentHealth = healthController.GetBodyPartHealth(limb, false);

                    if (!dadGamerPlugin.CODBleedingDamageToggle.Value)
                    {
                        healthController.RemoveNegativeEffects(limb);
                    }

                    if (!currentHealth.AtMaximum)
                    {
                        healthController.ChangeHealth(limb, newHealRate, tmpDmg);
                    }
                }

                await Task.Yield();
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
        }


        private void Player_OnPlayerDeadOrUnspawn(Player player)
        {
            Disable();
        }
    }
}
