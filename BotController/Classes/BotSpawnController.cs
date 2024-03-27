using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.Preset;
using static EFT.SpeedTree.TreeWind;
using SAIN.Preset.GlobalSettings.Categories;

namespace SAIN.Components.BotController
{
    public class BotSpawnController : SAINControl
    {
        public static BotSpawnController Instance;
        public BotSpawnController() {
            Instance = this;
        }

        public Dictionary<string, SAINComponentClass> SAINBotDictionary = new Dictionary<string, SAINComponentClass>();
        private BotSpawner BotSpawner => BotController?.BotSpawner;

        private static readonly WildSpawnType[] ExclusionList =
        {
            WildSpawnType.bossZryachiy,
            WildSpawnType.followerZryachiy
        };

        public void Update()
        {
            if (BotSpawner != null)
            {
                if (!Subscribed && !GameEnding)
                {
                    BotSpawner.OnBotRemoved += RemoveBot;
                    Subscribed = true;
                }
                if (Subscribed)
                {
                    var status = BotController?.BotGame?.Status;
                    if (status == GameStatus.Stopping || status == GameStatus.Stopped || status == GameStatus.SoftStopping)
                    {
                        BotSpawner.OnBotRemoved -= RemoveBot;
                        Subscribed = false;
                        GameEnding = true;
                    }
                }
            }
        }

        private bool GameEnding = false;
        private bool Subscribed = false;

        private void SetBrainInfo(BotOwner botOwner)
        {
            if (!SAINPlugin.EditorDefaults.CollectBotLayerBrainInfo)
            {
                return;
            }

            WildSpawnType role = botOwner.Profile.Info.Settings.Role;
            string brain = botOwner.Brain.BaseBrain.ShortName();
            BotType botType = BotTypeDefinitions.GetBotType(role);
            if (botType.BaseBrain.IsNullOrEmpty())
            {
                botType.BaseBrain = brain;
                Logger.LogInfo($"Set {role} BaseBrain to {brain}");
                BotTypeDefinitions.ExportBotTypes();
            }
            else
            {
                Logger.LogError($"{role} BaseBrain is already set to {botType.BaseBrain}. Can't set it to {brain}");
            }
        }

        public void AddBot(BotOwner botOwner)
        {
            try
            {
                if (botOwner != null)
                {
                    var settings = botOwner.Profile?.Info?.Settings;
                    if (settings == null)
                    {
                        return;
                    }

                    botOwner.LeaveData.OnLeave += RemoveBot;
                    SetBrainInfo(botOwner);

                    if (ExclusionList.Contains(settings.Role))
                    {
                        AddNoBushESP(botOwner);
                        return;
                    }

                    if (!CheckIfSAINEnabled(botOwner))
                    {
                        AddNoBushESP(botOwner);
                        return;
                    }

                    if (SAINComponentClass.TryAddSAINToBot(botOwner, out SAINComponentClass component))
                    {
                        string profileId = component.ProfileId;
                        if (SAINBotDictionary.ContainsKey(profileId))
                        {
                            SAINBotDictionary.Remove(profileId);
                        }
                        SAINBotDictionary.Add(profileId, component);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Add Component Error: {ex}");
            }
        }

        public void AddNoBushESP(BotOwner botOwner)
        {
            botOwner.GetOrAddComponent<SAINNoBushESP>().Init(botOwner);
        }

        private bool CheckIfSAINEnabled(BotOwner botOwner)
        {
            Brain brain = BotBrains.Parse(botOwner.Brain.BaseBrain.ShortName());
            return SAINPlugin.LoadedPreset.GlobalSettings.General.EnabledBrains.Contains(brain);
        }

        public void RemoveBot(BotOwner botOwner)
        {
            try
            {
                if (botOwner != null)
                {
                    SAINBotDictionary.Remove(botOwner.ProfileId);
                    if (botOwner.TryGetComponent(out SAINComponentClass component))
                    {
                        component.Dispose();
                    }
                    if (botOwner.TryGetComponent(out SAINNoBushESP noBush))
                    {
                        UnityEngine.Object.Destroy(noBush);
                    }
                }
                else
                {
                    Logger.LogError("Bot is null, cannot dispose!");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Dispose Component Error: {ex}");
            }
        }
    }
}
