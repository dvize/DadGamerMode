using BepInEx.Logging;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class MaxStaminaComponent : MonoBehaviour
    {
        Player player;
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

        private void Update()
        {
            player = Singleton<GameWorld>.Instance.MainPlayer;
            player.Physical.Stamina.Current = player.Physical.Stamina.TotalCapacity.Value;
            player.Physical.HandsStamina.Current = player.Physical.HandsStamina.TotalCapacity.Value;
            player.Physical.Oxygen.Current = player.Physical.Oxygen.TotalCapacity.Value;
        }

        public static void Enable()
        {
            try
            {
                if (Singleton<AbstractGame>.Instance.InRaid && Camera.main.transform.position != null)
                {
                    var gameWorld = Singleton<GameWorld>.Instance;
                    gameWorld.GetOrAddComponent<MaxStaminaComponent>();

                    var player = gameWorld.MainPlayer;
                    Logger.LogDebug("DadGamerMode: Setting Max Stamina");
                }
            }
            catch { }
        }
    }
}
