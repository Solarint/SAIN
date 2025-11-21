using EFT;
using SAIN.Components;
using SAIN.Helpers.Events;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System;
using EFT.UI;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision;

public class BotDecisionManager(SAINDecisionClass decisionClass) : BotSubClass<SAINDecisionClass>(decisionClass), IBotClass
{
    private const float DECISION_FREQUENCY = 1f / 10;

    public event Action<ECombatDecision, ESquadDecision, ESelfActionType, Enemy, BotComponent> OnDecisionMade;

    public ToggleEvent HasDecisionToggle { get; } = new ToggleEvent();

    public FSainBotDecision CurrentDecision { get; private set; } = new();

    public ECombatDecision CurrentCombatDecision { get; private set; }
    public ECombatDecision PreviousCombatDecision { get; private set; }
    public ESquadDecision CurrentSquadDecision { get; private set; }
    public ESquadDecision PreviousSquadDecision { get; private set; }
    public ESelfActionType CurrentSelfDecision { get; private set; }
    public ESelfActionType PreviousSelfDecision { get; private set; }

    public bool HasDecision => HasDecisionToggle.Value;
    public float ChangeDecisionTime { get; private set; }
    public float TimeSinceChangeDecision => Time.time - ChangeDecisionTime;

    public override void Init()
    {
        Bot.BotActivation.BotActiveToggle.OnToggle += resetDecisions;
        base.Init();
    }

    public override void ManualUpdate()
    {
        if (_nextGetDecisionTime < Time.time)
        {
            _nextGetDecisionTime = Time.time + DECISION_FREQUENCY;
            getDecision();
        }
    }

    public override void Dispose()
    {
        Bot.BotActivation.BotActiveToggle.OnToggle -= resetDecisions;
        base.Dispose();
    }

    private bool shallTagillaHammerAttack(Enemy enemy)
    {
        if (enemy == null)
        {
            return false;
        }
        bool alreadyAttacking = CurrentCombatDecision == ECombatDecision.MeleeAttack;
        ETagStatus status = Bot.Memory.Health.HealthStatus;

        if (!alreadyAttacking)
        {
            if (CurrentSelfDecision != ESelfActionType.None)
            {
                return false;
            }
            if (status != ETagStatus.Healthy && status != ETagStatus.Injured)
            {
                return false;
            }
            if (enemy.Path.PathToEnemyStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                return false;
            }
            if (enemy.RealDistance < 35 && enemy.Path.PathLength < 30 && enemy.Status.VulnerableAction != EEnemyAction.None)
            {
                enemy.BotOwner.WeaponManager.Melee.ShallEndRun = false;
                return true;
            }
            if (enemy.RealDistance < 20 && enemy.Path.PathLength < 15)
            {
                enemy.BotOwner.WeaponManager.Melee.ShallEndRun = false;
                return true;
            }
            return false;
        }
        if (enemy.BotOwner.WeaponManager.Melee.ShallEndRun)
        {
            return false;
        }
        if (status != ETagStatus.Dying && enemy.RealDistance < 40 && enemy.Path.PathLength < 35)
        {
            return true;
        }
        return false;
    }

    private void getDecision()
    {
        Enemy enemy = Bot.EnemyController.ChooseEnemy();
        if (enemy == null)
        {
            SetDecisions(ECombatDecision.None, ESquadDecision.None, ESelfActionType.None, enemy);
            return;
        }
        BaseClass.EnemyDecisions.DebugShallSearch = null;
        if (BaseClass.SelfActionDecisions.GetDecision(out ESelfActionType selfDecision, enemy))
        {
            SetDecisions(ECombatDecision.SeekCover, ESquadDecision.None, selfDecision, enemy);
            return;
        }

        // TODO: rework melee decisions
        if (Bot.Info.Profile.WildSpawnType is WildSpawnType.bossTagilla or WildSpawnType.bossTagillaAgro)
        {
            if (shallTagillaHammerAttack(enemy))
            {
                SetDecisions(ECombatDecision.MeleeAttack, ESquadDecision.None, ESelfActionType.None, enemy);
                return;
            }
            if (BotOwner.WeaponManager.IsMelee) BotOwner.WeaponManager.Selector.ChangeToMain();
        }

        if (enemy != null && enemy.IsZombie)
        {
            bool hasShooterContact = false;
            foreach (var knownEnemy in Bot.EnemyController.KnownEnemies)
                if (knownEnemy?.IsZombie != true)
                    hasShooterContact = true;
            if (!hasShooterContact)
            {
                BaseClass.SelfActionDecisions.GetDecision(out ESelfActionType zombieDecision, enemy);
                BaseClass.SquadDecisions.GetDecision(out ESquadDecision zombieSqdDecision, enemy);
                SetDecisions(ECombatDecision.FightZombies, zombieSqdDecision, zombieDecision, enemy);
                return;
            }
        }
        if (Bot.Decision.DogFightDecision.DogFightActive)
        {
            SetDecisions(ECombatDecision.DogFight, ESquadDecision.None, ESelfActionType.None, enemy);
            return;
        }
        if (BotOwner.WeaponManager.IsMelee)
        {
            SetDecisions(ECombatDecision.MeleeAttack, ESquadDecision.None, ESelfActionType.None, enemy);
            return;
        }
        if (ContinueMoveToCover())
        {
            SetDecisions(ECombatDecision.SeekCover, ESquadDecision.None, Bot.Decision.CurrentSelfDecision, enemy);
            return;
        }
        if (BaseClass.SquadDecisions.GetDecision(out ESquadDecision squadDecision, enemy))
        {
            SetDecisions(ECombatDecision.None, squadDecision, ESelfActionType.None, enemy);
            return;
        }
        if (BaseClass.EnemyDecisions.GetDecision(out ECombatDecision combatDecision, enemy, Bot.EnemyController.KnownEnemies))
        {
            SetDecisions(combatDecision, ESquadDecision.None, ESelfActionType.None, enemy);
            return;
        }
        SetDecisions(ECombatDecision.None, ESquadDecision.None, ESelfActionType.None, enemy);
    }

    private void SetDecisions(ECombatDecision solo, ESquadDecision squad, ESelfActionType self, Enemy enemy)
    {
#if DEBUG
        if (SAINPlugin.DebugMode)
        {
            if (SAINPlugin.ForceSoloDecision != ECombatDecision.None)
            {
                solo = SAINPlugin.ForceSoloDecision;
            }
            if (SAINPlugin.ForceSquadDecision != ESquadDecision.None)
            {
                squad = SAINPlugin.ForceSquadDecision;
            }
            if (SAINPlugin.ForceSelfDecision != ESelfActionType.None)
            {
                self = SAINPlugin.ForceSelfDecision;
            }
        }
#endif

        if (checkForNewDecision(solo, squad, self, enemy))
        {
            bool hasDecision =
                solo != ECombatDecision.None ||
                self != ESelfActionType.None ||
                squad != ESquadDecision.None;

            if (hasDecision)
            {
                BotOwner.PatrollingData.Pause();
            }

            ChangeDecisionTime = Time.time;
            HasDecisionToggle.CheckToggle(hasDecision, ChangeDecisionTime);
            OnDecisionMade?.Invoke(solo, squad, self, enemy, Bot);
        }
    }
    
    private void SetDecision(FSainBotDecision decision)
    {
#if DEBUG
        if (SAINPlugin.DebugMode)
        {
            if (SAINPlugin.ForceSoloDecision != ECombatDecision.None)
            {
                decision.CombatDecision = SAINPlugin.ForceSoloDecision;
            }
            if (SAINPlugin.ForceSquadDecision != ESquadDecision.None)
            {
                decision.SquadDecision = SAINPlugin.ForceSquadDecision;
            }
            if (SAINPlugin.ForceSelfDecision != ESelfActionType.None)
            {
                decision.SelfAction = SAINPlugin.ForceSelfDecision;
            }
        }
#endif

        if (CurrentDecision != decision)
        {
            bool hasDecision = decision.CombatDecision != ECombatDecision.None || decision.SelfAction != ESelfActionType.None || decision.SquadDecision != ESquadDecision.None;

            ChangeDecisionTime = Time.time;
            HasDecisionToggle.CheckToggle(hasDecision, ChangeDecisionTime);
            CurrentDecision = decision;
            OnDecisionMade?.Invoke(decision.CombatDecision, decision.SquadDecision, decision.SelfAction, decision.Enemy, Bot);
        }
    }

    private bool checkForNewDecision(ECombatDecision newSoloDecision, ESquadDecision newSquadDecision, ESelfActionType newSelfDecision, Enemy enemy)
    {
        bool newDecision = false;

        if (_lastDecisionEnemy != enemy)
        {
            _lastDecisionEnemy = enemy;
            newDecision = true;
        }

        if (newSoloDecision != CurrentCombatDecision)
        {
            PreviousCombatDecision = CurrentCombatDecision;
            CurrentCombatDecision = newSoloDecision;
            newDecision = true;
        }

        if (newSquadDecision != CurrentSquadDecision)
        {
            PreviousSquadDecision = CurrentSquadDecision;
            CurrentSquadDecision = newSquadDecision;
            newDecision = true;
        }

        if (newSelfDecision != CurrentSelfDecision)
        {
            PreviousSelfDecision = CurrentSelfDecision;
            CurrentSelfDecision = newSelfDecision;
            newDecision = true;
        }

        return newDecision;
    }

    private Enemy _lastDecisionEnemy;

    public void ResetDecisions(bool active)
    {
        bool hasDecision = HasDecision;
        resetDecisions(false);
        if (active && hasDecision)
        {
            //BotOwner.CalcGoal();
        }
    }

    private void resetDecisions(bool value)
    {
        if (!value)
        {
            SetDecisions(ECombatDecision.None, ESquadDecision.None, ESelfActionType.None, null);
        }
    }

    private bool ContinueMoveToCover()
    {
        bool runningToCover = Bot.Decision.RunningToCover;
        if (!runningToCover) return false;
        if (!Bot.Mover.Moving) return false;
        if (Bot.Cover.HasCover) return false;

        float timeChangeDec = Bot.Decision.TimeSinceChangeDecision;
        if (timeChangeDec < 0.5f) return true;

        //if (timeChangeDec > 3 &&
        //    !Bot.BotStuck.BotHasChangedPosition)
        //{
        //    return false;
        //}

        CoverPoint coverMovingTo = Bot.Cover.CoverPoint_MovingTo;
        return coverMovingTo != null && coverMovingTo.PathDistanceStatus switch {
            CoverStatus.InCover => false,
            CoverStatus.CloseToCover => true,
            _ => !coverMovingTo.CoverData.IsBad,
        };
    }

    private float _nextGetDecisionTime;
}