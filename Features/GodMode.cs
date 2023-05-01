using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using Aki.SinglePlayer.Models.Healing;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace dvize.DadGamerMode.Features
{
    internal class GodModeComponent : MonoBehaviour
    {
        protected static ManualLogSource Logger
        {
            get; private set;
        }

        private GodModeComponent()
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(GodModeComponent));
            }
        }
        public static void Enable()
        {
            try
            {
                if (Singleton<AbstractGame>.Instance.InRaid && Camera.main.transform.position != null)
                {
                    var gameWorld = Singleton<GameWorld>.Instance;
                    gameWorld.GetOrAddComponent<GodModeComponent>();

                    var player = gameWorld.MainPlayer;
                    Logger.LogDebug("DadGamerMode: Attaching GodMode Events");

                    player.OnPlayerDeadOrUnspawn += Player_OnPlayerDeadOrUnspawn;
                }
            }
            catch { }
        }

        public static void Disable()
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;

            //unattach events
            player.OnPlayerDeadOrUnspawn -= Player_OnPlayerDeadOrUnspawn;
        }

        private static void Player_OnPlayerDeadOrUnspawn(Player player)
        {
            //unattach events
            player.OnPlayerDeadOrUnspawn -= Player_OnPlayerDeadOrUnspawn;
        }
        
    }
    
}
