using BepInEx.Logging;
using Comfort.Common;
using dvize.GodModeTest;
using EFT;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class MagReloadSpeed : MonoBehaviour
    {
        private Player player;

        private void Start()
        {
            player = Singleton<GameWorld>.Instance.MainPlayer;
        }
        private void Update()
        {
            if (dadGamerPlugin.ReloadSpeed.Value != Singleton<BackendConfigSettingsClass>.Instance.BaseLoadTime)
            {
                Singleton<BackendConfigSettingsClass>.Instance.BaseLoadTime = dadGamerPlugin.ReloadSpeed.Value;
                Singleton<BackendConfigSettingsClass>.Instance.BaseUnloadTime = dadGamerPlugin.ReloadSpeed.Value;
            }

        }

        public static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<MagReloadSpeed>();
            }
        }
    }
}

