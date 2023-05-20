using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Layers;
using SAIN.Layers.Logic;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINComponent : MonoBehaviour
    {
        public SAINLogicDecision CurrentDecision => Decisions.CurrentDecision;

        public bool InCover = false;
        public float CoverRatio = 0f;

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();

            Logger = BepInEx.Logging.Logger.CreateLogSource($"[{GetType().Name}]" + $" - [{BotOwner.name}]");

            Core = BotOwner.GetComponent<SAINCoreComponent>();
            LeanComponent = BotOwner.GetOrAddComponent<LeanComponent>();

            Init();
        }

        private void Update()
        {
            Cover.ManualUpdate();

            Decisions.ManualUpdate();

            DoSelfAction();

            if (DebugTimer < Time.time)
            {
                DebugTimer = Time.time + 5f;
                Logger.LogWarning($"Current Bot Decision = [{CurrentDecision}]");
            }
        }

        private float DebugTimer = 0f;

        private void DoSelfAction()
        {
            switch (CurrentDecision)
            {
                case SAINLogicDecision.Reload:
                    BotReload();
                    break;

                case SAINLogicDecision.Heal:
                    BotHeal();
                    break;

                case SAINLogicDecision.CombatHeal:
                    BotHeal();
                    break;

                case SAINLogicDecision.Stims:
                    BotUseStims();
                    break;

                default:
                    break;
            }
        }

        private void Init()
        {
            Settings = new SettingsClass(BotOwner);
            Cover = new CoverClass(BotOwner);
            Movement = new MovementClass(BotOwner);
            Dodge = new DodgeClass(BotOwner);
            Decisions = new DecisionClass(BotOwner);
            Steering = new SteeringClass(BotOwner);
            Grenade = new GrenadeClass(BotOwner);

            DebugDrawList = new DebugGizmos.DrawLists(Core.BotColor, Core.BotColor);
        }

        public void BotHeal()
        {
            if (!BotOwner.Medecine.Using)
            {
                Logger.LogDebug($"I healed!");

                BotOwner.Medecine.FirstAid.TryApplyToCurrentPart(null, null);
            }
        }

        public void BotUseStims()
        {
            if (!BotOwner.Medecine.Stimulators.Using)
            {
                Logger.LogDebug($"I'm Popping Stims");

                BotOwner.Medecine.Stimulators.TryApply(true, null, null);
            }
        }

        public void BotReload()
        {
            if (!BotOwner.WeaponManager.Reload.Reloading)
            {
                Logger.LogDebug($"Reloading!");

                BotOwner.WeaponManager.Reload.Reload();
            }
        }

        public void BotCancelReload()
        {
            if (BotOwner.WeaponManager.Reload.Reloading)
            {
                Logger.LogDebug($"I need to stop reloading!");

                BotOwner.WeaponManager.Reload.TryStopReload();
            }
        }

        public void Dispose()
        {
            Cover.Component.Dispose();
            LeanComponent.Dispose();
            Destroy(this);
        }

        public GrenadeClass Grenade { get; private set; }
        public MovementClass Movement { get; private set; }
        public DodgeClass Dodge { get; private set; }
        public DecisionClass Decisions { get; private set; }
        public SteeringClass Steering { get; private set; }
        public DebugGizmos.DrawLists DebugDrawList { get; private set; }
        public CoverClass Cover { get; private set; }
        public LeanComponent LeanComponent { get; private set; }
        public SAINCoreComponent Core { get; private set; }
        public SettingsClass Settings { get; private set; }
        public BotOwner BotOwner { get; private set; }

        protected ManualLogSource Logger;
    }

    public class SettingsClass : SAINBotExt
    {
        public SettingsClass(BotOwner bot) : base(bot)
        {
            DifficultyModifier = CalculateDifficulty(bot);
        }

        public float DifficultyModifier { get; private set; }

        public readonly float FightIn = 60f;
        public readonly float FightOut = 70f;

        public readonly float DogFightIn = 10f;
        public readonly float DogFightOut = 15f;

        public readonly float LowAmmoThresh0to1 = 0.3f;

        private float CalculateDifficulty(BotOwner bot)
        {
            var settings = bot.Profile.Info.Settings;

            if (settings != null)
            {
                return GetDifficultyMod(settings.Role, settings.BotDifficulty, SAIN.Core.Info.IsBoss, SAIN.Core.Info.IsFollower);
            }

            return 1f;
        }

        private static float GetDifficultyMod(WildSpawnType bottype, BotDifficulty difficulty, bool isBoss, bool isFollower)
        {
            float modifier = 1f;
            if (isBoss)
            {
                modifier = 0.85f;
            }
            else if (isFollower)
            {
                modifier = 0.95f;
            }
            else
            {
                switch (bottype)
                {
                    case WildSpawnType.assault:
                        modifier *= 1.25f;
                        break;

                    case WildSpawnType.pmcBot:
                        modifier *= 1.1f;
                        break;

                    case WildSpawnType.cursedAssault:
                        modifier *= 1.35f;
                        break;

                    case WildSpawnType.exUsec:
                        modifier *= 1.1f;
                        break;

                    default:
                        modifier *= 0.75f;
                        break;
                }
            }

            switch (difficulty)
            {
                case BotDifficulty.easy:
                    modifier *= 1.25f;
                    break;

                case BotDifficulty.normal:
                    modifier *= 1.0f;
                    break;

                case BotDifficulty.hard:
                    modifier *= 0.85f;
                    break;

                case BotDifficulty.impossible:
                    modifier *= 0.75f;
                    break;

                default:
                    modifier *= 1f;
                    break;
            }

            return modifier;
        }
    }
}