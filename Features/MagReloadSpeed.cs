using System;
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
            // Subscribe to configuration change events
            dadGamerPlugin.ReloadSpeed.SettingChanged += OnReloadSpeedChanged;
            dadGamerPlugin.UnloadSpeed.SettingChanged += OnUnloadSpeedChanged;
            dadGamerPlugin.ToggleReloadUnloadSpeed.SettingChanged += OnToggleReloadUnloadSpeedChanged;

            if (dadGamerPlugin.ToggleReloadUnloadSpeed.Value)
            {
                Singleton<BackendConfigSettingsClass>.Instance.BaseLoadTime = dadGamerPlugin.ReloadSpeed.Value;
                Singleton<BackendConfigSettingsClass>.Instance.BaseUnloadTime = dadGamerPlugin.UnloadSpeed.Value;
            } 
        }
        private void OnReloadSpeedChanged(object sender, EventArgs e)
        {
            if (dadGamerPlugin.ToggleReloadUnloadSpeed.Value)
            {
                Singleton<BackendConfigSettingsClass>.Instance.BaseLoadTime = dadGamerPlugin.ReloadSpeed.Value;
            }
        }

        private void OnUnloadSpeedChanged(object sender, EventArgs e)
        {
            if (dadGamerPlugin.ToggleReloadUnloadSpeed.Value)
            {
                Singleton<BackendConfigSettingsClass>.Instance.BaseUnloadTime = dadGamerPlugin.UnloadSpeed.Value;
            }
        }

        private void OnToggleReloadUnloadSpeedChanged(object sender, EventArgs e)
        {
            if (dadGamerPlugin.ToggleReloadUnloadSpeed.Value)
            {
                Singleton<BackendConfigSettingsClass>.Instance.BaseLoadTime = dadGamerPlugin.ReloadSpeed.Value;
                Singleton<BackendConfigSettingsClass>.Instance.BaseUnloadTime = dadGamerPlugin.UnloadSpeed.Value;
            }
            else
            {
                // Reset to default values
                Singleton<BackendConfigSettingsClass>.Instance.BaseLoadTime = 0.85f;
                Singleton<BackendConfigSettingsClass>.Instance.BaseUnloadTime = 0.3f;
                dadGamerPlugin.ReloadSpeed.Value = 0.85f;
                dadGamerPlugin.UnloadSpeed.Value = 0.3f;
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
        private void OnDestroy()
        {
            // Unsubscribe from configuration change events
            dadGamerPlugin.ReloadSpeed.SettingChanged -= OnReloadSpeedChanged;
            dadGamerPlugin.UnloadSpeed.SettingChanged -= OnUnloadSpeedChanged;
            dadGamerPlugin.ToggleReloadUnloadSpeed.SettingChanged -= OnToggleReloadUnloadSpeedChanged;
        }
    }
}

