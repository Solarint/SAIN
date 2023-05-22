using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Layers;
using SAIN.UserSettings;
using SAIN.Layers.Logic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Comfort.Common;

namespace SAIN.Components
{
    public class SAINComponent : MonoBehaviour
    {
        public SAINLogicDecision CurrentDecision => Decisions.CurrentDecision;

        public static List<SAINLogicDecision> HealDecisions = new List<SAINLogicDecision> { SAINLogicDecision.Heal, SAINLogicDecision.CombatHeal, SAINLogicDecision.Stims };
        public static List<SAINLogicDecision> AggressiveActions = new List<SAINLogicDecision> { SAINLogicDecision.DogFight, SAINLogicDecision.Fight, SAINLogicDecision.Search, SAINLogicDecision.Suppress, SAINLogicDecision.Skirmish };
        public static List<SAINLogicDecision> DefensiveActions = new List<SAINLogicDecision> { SAINLogicDecision.Reload, SAINLogicDecision.RunForCover, SAINLogicDecision.HoldInCover, SAINLogicDecision.RunAway, SAINLogicDecision.WalkToCover, SAINLogicDecision.RunAwayGrenade };

        public bool InCover { get; private set; }
        public float CoverRatio { get; private set; }
        public bool HasEnemyAndCanShoot => BotOwner.Memory.GoalEnemy != null && BotOwner.Memory.GoalEnemy.CanShoot && BotOwner.Memory.GoalEnemy.IsVisible;

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            Logger = BepInEx.Logging.Logger.CreateLogSource($"[{GetType().Name}]" + $" - [{BotOwner.name}]");
            Core = BotOwner.GetComponent<SAINCoreComponent>();

            Init();
        }

        private void Update()
        {
            if (BotOwner.IsDead || BotOwner.BotState != EBotState.Active)
            {
                return;
            }

            if (CoverConfig.AllBotsMoveToPlayer.Value)
            {
                var playerPos = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
                if (Vector3.Distance(BotOwner.Destination, playerPos) > 10f)
                {
                    NavMeshPath Path = new NavMeshPath();
                    if (NavMesh.CalculatePath(BotOwner.Transform.position, playerPos, -1, Path))
                    {
                        if (Path.status == NavMeshPathStatus.PathComplete)
                        {
                            BotOwner.GoToPoint(playerPos, true, 20f, false, false);
                        }
                        if (Path.status == NavMeshPathStatus.PathPartial)
                        {
                            BotOwner.GoToPoint(Path.corners[Path.corners.Length - 1], true, 20f, false, false);
                        }
                    }
                }
            }

            Lean.ManualUpdate();
            Cover.ManualUpdate();
            Decisions.ManualUpdate();

            DoSelfAction();
        }

        private void DoSelfAction()
        {
            switch (CurrentDecision)
            {
                case SAINLogicDecision.Reload:
                    DoReload();
                    break;

                case SAINLogicDecision.Heal:
                    DoFullHeal();
                    break;

                case SAINLogicDecision.CombatHeal:
                    DoCombatHeal();
                    break;

                case SAINLogicDecision.Stims:
                    DoStims();
                    break;

                default:
                    break;
            }
        }

        private void Init()
        {
            Settings = new SettingsClass(BotOwner);
            Lean = new LeanClass(BotOwner);
            Cover = new CoverClass(BotOwner);
            Movement = new MovementClass(BotOwner);
            Dodge = new DodgeClass(BotOwner);
            Decisions = new DecisionClass(BotOwner);
            Steering = new SteeringClass(BotOwner);
            Grenade = new GrenadeClass(BotOwner);

            DebugDrawList = new DebugGizmos.DrawLists(Core.BotColor, Core.BotColor);
        }

        public void DoCombatHeal()
        {
            var heal = BotOwner.Medecine.FirstAid;
            if (heal.ShallStartUse() && HealTimer < Time.time)
            {
                HealTimer = Time.time + 5f;

                Logger.LogDebug($"I healed!");

                heal.TryApplyToCurrentPart(null, null);
            }
        }

        public void DoFullHeal()
        {
            var surgery = BotOwner.Medecine.SurgicalKit;
            if (surgery.ShallStartUse() && HealTimer < Time.time)
            {
                HealTimer = Time.time + 5f;

                Logger.LogDebug($"Used Surgery");

                surgery.ApplyToCurrentPart();
            }
        }

        public void DoStims()
        {
            var stims = BotOwner.Medecine.Stimulators;
            if (stims.CanUseNow())
            {
                Logger.LogDebug($"I'm Popping Stims");

                stims.TryApply(true, null, null);
            }
        }

        public void DoReload()
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
            Destroy(this);
        }

        public LeanClass Lean { get; private set; }
        public GrenadeClass Grenade { get; private set; }
        public MovementClass Movement { get; private set; }
        public DodgeClass Dodge { get; private set; }
        public DecisionClass Decisions { get; private set; }
        public SteeringClass Steering { get; private set; }
        public DebugGizmos.DrawLists DebugDrawList { get; private set; }
        public CoverClass Cover { get; private set; }
        public SAINCoreComponent Core { get; private set; }
        public SettingsClass Settings { get; private set; }
        public BotOwner BotOwner { get; private set; }

        protected ManualLogSource Logger;
        private float HealTimer = 0f;
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