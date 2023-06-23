using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class DecisionClass : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            SelfActionDecisions = new SelfActionDecisionClass(BotOwner);
            EnemyDecisions = new EnemyDecisionClass(BotOwner);
            GoalTargetDecisions = new TargetDecisionClass(BotOwner);
            SquadDecisions = new SquadDecisionClass(BotOwner);
        }

        public SAINEnemyPath EnemyDistance { get; private set; }

        public SAINSoloDecision MainDecision { get; private set; }
        public SAINSoloDecision OldMainDecision { get; private set; }

        public SAINSquadDecision CurrentSquadDecision { get; private set; }
        public SAINSquadDecision OldSquadDecision { get; private set; }

        public SAINSelfDecision CurrentSelfDecision { get; private set; }
        public SAINSelfDecision OldSelfDecision { get; private set; }

        public SelfActionDecisionClass SelfActionDecisions { get; private set; }
        public EnemyDecisionClass EnemyDecisions { get; private set; }
        public TargetDecisionClass GoalTargetDecisions { get; private set; }
        public SquadDecisionClass SquadDecisions { get; private set; }
        public List<SAINSoloDecision> RetreatDecisions { get; private set; } = new List<SAINSoloDecision> { SAINSoloDecision.Retreat };
        public float ChangeDecisionTime { get; private set; }
        public float TimeSinceChangeDecision => Time.time - ChangeDecisionTime;

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                MainDecision = SAINSoloDecision.None;
                CurrentSquadDecision = SAINSquadDecision.None;
                CurrentSelfDecision = SAINSelfDecision.None;
                return;
            }

            if (DecisionTimer < Time.time)
            {
                DecisionTimer = Time.time + 0.066f;

                if (UpdateEnemyTimer < Time.time)
                {
                    UpdateEnemyTimer = Time.time + 0.33f;
                    EnemyDistance = SAIN.HasEnemy ? SAIN.Enemy.CheckPathDistance() : SAINEnemyPath.NoEnemy;
                }

                GetDecision();
            }
        }

        public void ResetDecisions()
        {
            UpdateDecisionProperties(SAINSoloDecision.None, SAINSquadDecision.None, SAINSelfDecision.None);
        }

        private void GetDecision()
        {
            var soloDecision = SAINSoloDecision.None;
            var squadDecision = SAINSquadDecision.None;
            var selfDecision = SAINSelfDecision.None;

            if (SelfActionDecisions.GetDecision(out selfDecision))
            {
            }
            else if (CheckStuckDecision(out soloDecision))
            {
            }
            else if (SquadDecisions.GetDecision(out squadDecision))
            {
            }
            else if (EnemyDecisions.GetDecision(out soloDecision))
            {
            }
            else if (GoalTargetDecisions.GetDecision(out soloDecision))
            {
            }

            UpdateDecisionProperties(soloDecision, squadDecision, selfDecision);
        }

        private void UpdateDecisionProperties(SAINSoloDecision solo, SAINSquadDecision squad, SAINSelfDecision self)
        {
            if (self != SAINSelfDecision.None)
            {
                solo = SAINSoloDecision.Retreat;
            }

            OldMainDecision = MainDecision;
            MainDecision = solo;

            OldSquadDecision = CurrentSquadDecision;
            CurrentSquadDecision = squad;

            OldSelfDecision = CurrentSelfDecision;
            CurrentSelfDecision = self;

            if (MainDecision != OldMainDecision)
            {
                ChangeDecisionTime = Time.time;
            }
            if (CurrentSelfDecision != OldSelfDecision)
            {
                ChangeSelfDecisionTime = Time.time;
            }
            if (CurrentSquadDecision != OldSquadDecision)
            {
                ChangeSquadDecisionTime = Time.time;
            }
        }

        public float ChangeSelfDecisionTime { get; private set; }
        public float ChangeSquadDecisionTime { get; private set; }

        private bool CheckContinueRetreat()
        {
            if (SAIN.Enemy == null) return false;

            return MainDecision == SAINSoloDecision.Retreat && !SAIN.Cover.BotIsAtCoverPoint && TimeSinceChangeDecision < 3f && SAIN.BotHasStamina;
        }

        private bool CheckStuckDecision(out SAINSoloDecision Decision)
        {
            Decision = SAINSoloDecision.None;
            bool stuck = SAIN.BotStuck.BotIsStuck;

            if (!stuck && FinalBotUnstuckTimer != 0f)
            {
                FinalBotUnstuckTimer = 0f;
            }

            if (stuck && BotUnstuckTimerDecision < Time.time)
            {
                if (FinalBotUnstuckTimer == 0f)
                {
                    FinalBotUnstuckTimer = Time.time + 10f;
                }

                BotUnstuckTimerDecision = Time.time + 5f;

                var current = this.MainDecision;
                if (FinalBotUnstuckTimer < Time.time && SAIN.HasEnemy)
                {
                    Decision = SAINSoloDecision.UnstuckDogFight;
                    return true;
                }
                if (current == SAINSoloDecision.Search || current == SAINSoloDecision.UnstuckSearch)
                {
                    Decision = SAINSoloDecision.UnstuckMoveToCover;
                    return true;
                }
                if (current == SAINSoloDecision.WalkToCover || current == SAINSoloDecision.UnstuckMoveToCover)
                {
                    Decision = SAINSoloDecision.UnstuckSearch;
                    return true;
                }
            }
            return false;
        }

        private float UpdateEnemyTimer = 0f;
        private float BotUnstuckTimerDecision = 0f;
        private float FinalBotUnstuckTimer = 0f;
        private float DecisionTimer = 0f;
        public ManualLogSource Logger;
    }
}