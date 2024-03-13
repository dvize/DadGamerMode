using System;
using BepInEx.Logging;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class HydrationComponent : MonoBehaviour
    {
        private Player player;
        protected static ManualLogSource Logger
        {
            get; private set;
        }

        private HydrationComponent()
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(HydrationComponent));
            }
        }

        private void Start()
        {
            player = Singleton<GameWorld>.Instance.MainPlayer;

            //setup event so it doesn't have to check every frame
            player.ActiveHealthController.HydrationChangedEvent += ActiveHealthController_HydrationChangedEvent;
            
        }

        private void ActiveHealthController_HydrationChangedEvent(float obj)
        {
            if (dadGamerPlugin.MaxHydrationToggle.Value)
            {

                player.ActiveHealthController.ChangeHydration(player.ActiveHealthController.Hydration.Maximum);
                
            }
        }


        public static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<HydrationComponent>();

                Logger.LogDebug("DadGamerMode: Setting Max Hydration");
            }
        }
    }
}
