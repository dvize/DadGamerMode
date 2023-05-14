using BepInEx.Logging;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class NoFallingDamageComponent : MonoBehaviour
    {
        private Player player;
        protected static ManualLogSource Logger
        {
            get; private set;
        }

        private NoFallingDamageComponent()
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(NoFallingDamageComponent));
            }
        }

        private void Start()
        {
            player = Singleton<GameWorld>.Instance.MainPlayer;
            Logger.LogDebug("DadGamerMode: Setting No Falling Damage");
        }
        private void Update()
        {
            player.ActiveHealthController.FallSafeHeight = dadGamerPlugin.NoFallingDamage.Value ? 999999f : 1.8f;
        }

        internal static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<NoFallingDamageComponent>();
            }
        }

        private static void Disable()
        {
            if (!dadGamerPlugin.NoFallingDamage.Value)
            {
                var gameWorld = Singleton<GameWorld>.Instance;

                var player = gameWorld.MainPlayer;
                Logger.LogDebug("DadGamerMode: Setting Falling Damage To Normal");

                player.ActiveHealthController.FallSafeHeight = 1.8f;

            }
        }

    }
}
