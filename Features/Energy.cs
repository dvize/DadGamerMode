using System;
using BepInEx.Logging;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class EnergyComponent : MonoBehaviour
    {
        private Player player;
        protected static ManualLogSource Logger
        {
            get; private set;
        }

        private EnergyComponent()
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(EnergyComponent));
            }
        }

        private void Start()
        {
            player = Singleton<GameWorld>.Instance.MainPlayer;
            
            player.ActiveHealthController.EnergyChangedEvent += ActiveHealthController_EnergyChangedEvent;
        }

        private void ActiveHealthController_EnergyChangedEvent(float obj)
        {
            if (dadGamerPlugin.MaxEnergyToggle.Value)
            {
                player.ActiveHealthController.ChangeEnergy(player.ActiveHealthController.Energy.Maximum);
            }
        }

        public static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<EnergyComponent>();

                Logger.LogDebug("DadGamerMode: Setting Energy Component");
            }
        }
    }
}
