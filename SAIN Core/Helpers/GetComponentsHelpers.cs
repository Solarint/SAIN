using Comfort.Common;
using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Helpers
{
    public class MainPlayerComponentSingle
    {
        public MainPlayerComponentSingle()
        { }

        public void AddSingleComponent<T>() where T : Component
        {
            if (GetComponentHelpers.GetMainPlayer(out var player))
            {
                if (!ComponentAdded)
                {
                    player.GetOrAddComponent<T>();
                    //GetComponentHelpers.GetOrAddComponentForPlayer<T>(player);
                    ComponentAdded = true;
                }
            }
            else
            {
                ComponentAdded = false;
            }
        }

        private bool ComponentAdded = false;
    }

    public static class GetComponentHelpers
    {
        public static List<T> GetBotComponents<T>(List<IAIDetails> list) where T : Component
        {
            List<T> result = new List<T>();
            foreach (var person in list)
            {
                if (person.AIData.IsAI && person.HealthController.IsAlive)
                {
                    var component = person.AIData.BotOwner.GetComponent<T>();
                    result.Add(component);
                }
            }
            return result;
        }

        public static List<T> GetBotComponents<T>(List<BotOwner> list) where T : Component
        {
            List<T> result = new List<T>();
            foreach (var person in list)
            {
                if (person.HealthController.IsAlive)
                {
                    var component = person.GetComponent<T>();
                    result.Add(component);
                }
            }
            return result;
        }

        public static List<T> GetBotComponents<T>(List<Player> list) where T : Component
        {
            List<T> result = new List<T>();
            foreach (var person in list)
            {
                if (person.AIData.IsAI && person.HealthController.IsAlive)
                {
                    var component = person.AIData.BotOwner.GetComponent<T>();
                    result.Add(component);
                }
            }
            return result;
        }

        public static bool GetMainPlayer(out Player player)
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld?.MainPlayer == null)
            {
                player = null;
                return false;
            }

            player = MainPlayer;
            return true;
        }

        public static Component GetOrAddComponentForPlayer<T>(Player player) where T : Component
        {
            var component = player.GetComponent<T>() ?? player.gameObject.AddComponent<T>();
            System.Console.WriteLine($"Added [{component.name}] to [{player.Profile.Nickname}]");
            return component;
        }

        public static Player MainPlayer => Singleton<GameWorld>.Instance.MainPlayer;
    }
}