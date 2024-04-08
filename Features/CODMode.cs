using BepInEx.Logging;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

using AbstractIEffect = EFT.HealthSystem.ActiveHealthController.GClass2415;

namespace dvize.DadGamerMode.Features
{
    internal class CODModeComponent : MonoBehaviour
    {
        private static Player player;
        private static ActiveHealthController healthController;
        private static float timeSinceLastHit = 0f;
        private static bool isRegenerating = false;
        private static float newHealRate;
        private static DamageInfo tmpDmg;
        private static HealthValue currentHealth;
        private static int frameCount = 0;

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
            isRegenerating = false;
            timeSinceLastHit = 0f;
            newHealRate = 0f;
            tmpDmg = new DamageInfo();
            currentHealth = null;
            frameCount = 0;

            player.OnPlayerDeadOrUnspawn += Player_OnPlayerDeadOrUnspawn;
            player.BeingHitAction += Player_BeingHitAction;
            healthController.EffectAddedEvent += HealthController_EffectAddedEvent;
        }

        private void HealthController_EffectAddedEvent(IEffect effect)
        {

#if DEBUG
            Logger.LogWarning("Effect added is of type: " + effect.Type);
            Logger.LogWarning("The Effect state is: " + effect.State);
            Logger.LogWarning("The BodyPart is: " + effect.BodyPart);
            Logger.LogWarning("The Effect Strength is: " + effect.Strength);
#endif
            //if (effect.Type == typeof(GInterface245) || effect.Type == typeof(GInterface259) || effect.Type == typeof(GInterface244))

            //grabbed this from remove negative effects method
            if (dadGamerPlugin.CODModeToggle.Value)
            {
                if (!(effect is GInterface237) && !(effect is GInterface238))
                {
                    //GInterface244 is bleeding
                    //GInterface245 is fracture
                    //GInterface259 is pain

                    healthController.RemoveEffectFromList((AbstractIEffect)effect);
#if DEBUG
                Logger.LogDebug("Effect is a Fracture, Bleeding, or Pain and has been removed");
#endif
                }
            }
        }

        private void Update()
        {
            if (dadGamerPlugin.CODModeToggle.Value)
            {
                frameCount++;
                timeSinceLastHit += Time.unscaledDeltaTime;

                if (frameCount >= 60) // Check every 60 frames instead
                {
                    frameCount = 0;

                    if (timeSinceLastHit >= dadGamerPlugin.CODModeHealWait.Value)
                    {
                        if (!isRegenerating)
                        {
                            isRegenerating = true;
                        }

                        StartHealing();
                    }
                }
            }
        }

        private void StartHealing()
        {
            if (isRegenerating && dadGamerPlugin.CODModeToggle.Value)
            {
                newHealRate = dadGamerPlugin.CODModeHealRate.Value;

                foreach (var limb in bodyPartsDict)
                {
                    currentHealth = healthController.Dictionary_0[limb].Health;

                    if (!currentHealth.AtMaximum)
                    {
                        currentHealth.Current += newHealRate;
                    }
                }
            }
        }
        private void Disable()
        {
            if (player != null)
            {
                player.OnPlayerDeadOrUnspawn -= Player_OnPlayerDeadOrUnspawn;
                player.BeingHitAction -= Player_BeingHitAction;
                healthController.EffectAddedEvent -= HealthController_EffectAddedEvent;
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
