using BepInEx.Logging;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class MaxStaminaComponent : MonoBehaviour
    {
        private Player player;
        protected static ManualLogSource Logger
        {
            get; private set;
        }

        private MaxStaminaComponent()
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(MaxStaminaComponent));
            }
        }

        private void Start()
        {
            player = Singleton<GameWorld>.Instance.MainPlayer;
        }
        private void Update()
        {
            if (dadGamerPlugin.MaxStaminaToggle.Value)
            {
                player.Physical.Stamina.Current = player.Physical.Stamina.TotalCapacity.Value;
                player.Physical.HandsStamina.Current = player.Physical.HandsStamina.TotalCapacity.Value;
                player.Physical.Oxygen.Current = player.Physical.Oxygen.TotalCapacity.Value;
            }
        }

        public static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<MaxStaminaComponent>();

                Logger.LogDebug("DadGamerMode: Setting Max Stamina");
            }
        }
    }
}
