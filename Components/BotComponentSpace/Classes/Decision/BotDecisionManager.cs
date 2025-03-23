using EFT;
using SAIN.Helpers.Events;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision
{
    public class BotDecisionManager : BotSubClass<SAINDecisionClass>, IBotClass
    {
        private const float DECISION_FREQUENCY = 1f / 30;
        private const float DECISION_FREQUENCY_PEACE = 1f / 10;

        public event Action<ECombatDecision, ESquadDecision, ESelfDecision, BotComponent> OnDecisionMade;

        public ToggleEvent HasDecisionToggle { get; } = new ToggleEvent();

        public ECombatDecision CurrentCombatDecision { get; private set; }
        public ECombatDecision PreviousCombatDecision { get; private set; }
        public ESquadDecision CurrentSquadDecision { get; private set; }
        public ESquadDecision PreviousSquadDecision { get; private set; }
        public ESelfDecision CurrentSelfDecision { get; private set; }
        public ESelfDecision PreviousSelfDecision { get; private set; }

        public bool HasDecision => HasDecisionToggle.Value;
        public float ChangeDecisionTime { get; private set; }
        public float TimeSinceChangeDecision => Time.time - ChangeDecisionTime;

        public BotDecisionManager(SAINDecisionClass decisionClass) : base(decisionClass)
        {
        }

        public void Init()
        {
            Bot.BotActivation.BotActiveToggle.OnToggle += resetDecisions;
        }

        public void Update()
        {
            updateDecision();
        }

        public void Dispose()
        {
            Bot.BotActivation.BotActiveToggle.OnToggle -= resetDecisions;
        }

        private void updateDecision()
        {
            if (_nextGetDecisionTime < Time.time) {
                getDecision();
                float delay = HasDecision ? DECISION_FREQUENCY : DECISION_FREQUENCY_PEACE;
                _nextGetDecisionTime = Time.time + delay;
            }
        }

        private bool shallTagillaHammerAttack()
        {
            if (CurrentSelfDecision != ESelfDecision.None) {
                return false;
            }
            var status = Bot.Memory.Health.HealthStatus;
            if (status != ETagStatus.Healthy && status != ETagStatus.Injured) {
                return false;
            }

            var enemy = Bot.Enemy;
            if (enemy == null) {
                return false;
            }
            if (enemy.RealDistance > 50f) {
                return false;
            }
            bool alreadyAttacking = CurrentCombatDecision == ECombatDecision.TagillaMelee;
            if (!alreadyAttacking && enemy.Path.PathDistance < 20 && enemy.Status.VulnerableAction != EnemyClasses.EEnemyAction.None) {
                return true;
            }
            if (alreadyAttacking && enemy.Path.PathDistance < 25) {
                return true;
            }
            return false;
        }

        private void getDecision()
        {
            BaseClass.EnemyDecisions.DebugShallSearch = null;
            if (Bot.Info.Profile.WildSpawnType == WildSpawnType.bossTagilla) {
                if (shallTagillaHammerAttack()) {
                    if (!BotOwner.WeaponManager.IsMelee) BotOwner.WeaponManager.Selector.ChangeToMelee();
                    SetDecisions(ECombatDecision.TagillaMelee, ESquadDecision.None, ESelfDecision.None);
                    return;
                }
                if (BotOwner.WeaponManager.IsMelee) BotOwner.WeaponManager.Selector.ChangeToMain();
            }
            if (BaseClass.DogFightDecision.ShallDogFight()) {
                SetDecisions(ECombatDecision.DogFight, ESquadDecision.None, ESelfDecision.None);
                return;
            }
            if (BotOwner.WeaponManager.IsMelee) {
                SetDecisions(ECombatDecision.MeleeAttack, ESquadDecision.None, ESelfDecision.None);
                return;
            }
            if (BaseClass.SelfActionDecisions.GetDecision(out ESelfDecision selfDecision)) {
                var selfCombat = Bot.Cover.InCover ? ECombatDecision.HoldInCover : ECombatDecision.Retreat;
                SetDecisions(selfCombat, ESquadDecision.None, selfDecision);
                return;
            }
            if (CheckContinueRetreat()) {
                SetDecisions(ECombatDecision.Retreat, ESquadDecision.None, ESelfDecision.None);
                return;
            }
            if (BaseClass.SquadDecisions.GetDecision(out ESquadDecision squadDecision)) {
                SetDecisions(ECombatDecision.None, squadDecision, ESelfDecision.None);
                return;
            }
            if (BaseClass.EnemyDecisions.GetDecision(out ECombatDecision combatDecision)) {
                SetDecisions(combatDecision, ESquadDecision.None, ESelfDecision.None);
                return;
            }
            SetDecisions(ECombatDecision.None, ESquadDecision.None, ESelfDecision.None);
        }

        private void SetDecisions(ECombatDecision solo, ESquadDecision squad, ESelfDecision self)
        {
            if (SAINPlugin.DebugMode) {
                if (SAINPlugin.ForceSoloDecision != ECombatDecision.None) {
                    solo = SAINPlugin.ForceSoloDecision;
                }
                if (SAINPlugin.ForceSquadDecision != ESquadDecision.None) {
                    squad = SAINPlugin.ForceSquadDecision;
                }
                if (SAINPlugin.ForceSelfDecision != ESelfDecision.None) {
                    self = SAINPlugin.ForceSelfDecision;
                }
            }

            if (checkForNewDecision(solo, squad, self)) {
                bool hasDecision =
                    solo != ECombatDecision.None ||
                    self != ESelfDecision.None ||
                    squad != ESquadDecision.None;

                HasDecisionToggle.CheckToggle(hasDecision);

                ChangeDecisionTime = Time.time;
                OnDecisionMade?.Invoke(solo, squad, self, Bot);
            }
        }

        private bool checkForNewDecision(ECombatDecision newSoloDecision, ESquadDecision newSquadDecision, ESelfDecision newSelfDecision)
        {
            bool newDecision = false;

            if (newSoloDecision != CurrentCombatDecision) {
                PreviousCombatDecision = CurrentCombatDecision;
                CurrentCombatDecision = newSoloDecision;
                newDecision = true;
            }

            if (newSquadDecision != CurrentSquadDecision) {
                PreviousSquadDecision = CurrentSquadDecision;
                CurrentSquadDecision = newSquadDecision;
                newDecision = true;
            }

            if (newSelfDecision != CurrentSelfDecision) {
                PreviousSelfDecision = CurrentSelfDecision;
                CurrentSelfDecision = newSelfDecision;
                newDecision = true;
            }

            return newDecision;
        }

        public void ResetDecisions(bool active)
        {
            bool hasDecision = HasDecision;
            resetDecisions(false);
            if (active && hasDecision) {
                BotOwner.CalcGoal();
            }
        }

        private void resetDecisions(bool value)
        {
            if (!value) {
                SetDecisions(ECombatDecision.None, ESquadDecision.None, ESelfDecision.None);
            }
        }

        private bool CheckContinueRetreat()
        {
            bool runningToCover = CurrentCombatDecision == ECombatDecision.Retreat || CurrentCombatDecision == ECombatDecision.RunToCover;
            if (!runningToCover) {
                return false;
            }
            if (!Bot.Mover.SprintController.Running) {
                return false;
            }
            if (Bot.Cover.InCover) {
                return false;
            }

            float timeChangeDec = Bot.Decision.TimeSinceChangeDecision;
            if (timeChangeDec < 0.5f) {
                return true;
            }

            if (timeChangeDec > 3 &&
                !Bot.BotStuck.BotHasChangedPosition) {
                return false;
            }

            CoverPoint coverInUse = Bot.Cover.CoverInUse;
            if (coverInUse == null) {
                return false;
            }

            switch (coverInUse.PathDistanceStatus) {
                case CoverStatus.InCover:
                    return false;

                case CoverStatus.CloseToCover:
                    return true;

                default:
                    return !coverInUse.CoverData.IsBad;
            }
        }

        private float _nextGetDecisionTime;
    }
}