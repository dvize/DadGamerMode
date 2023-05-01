using BepInEx.Logging;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class NoFallingDamageComponent : MonoBehaviour
    {
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
        public static void Enable()
        {
            try
            {
                if (Singleton<AbstractGame>.Instance.InRaid && Camera.main.transform.position != null)
                {
                    var gameWorld = Singleton<GameWorld>.Instance;
                    gameWorld.GetOrAddComponent<NoFallingDamageComponent>();

                    var player = gameWorld.MainPlayer;
                    Logger.LogDebug("DadGamerMode: Setting Falling Damage");

                    player.ActiveHealthController.FallSafeHeight = 999999f;

                }
            }
            catch { }
        }

    }
}
