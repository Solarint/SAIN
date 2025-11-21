using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Search;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAIN.Layers.Combat.Solo;

internal class SearchAction(BotOwner bot) : BotAction(bot, "Search"), IBotAction
{
    private Enemy _searchTarget => Search?.SearchTarget;
    private bool _subscribed;
    private float _nextCheckWeaponTime;
    private bool _haveTalked = false;
    private bool _sprintEnabled = false;
    private float _sprintTimer = 0f;
    private SAINSearchClass Search => Bot.Search;

    public override void Start()
    {
        base.Start();
        subscribeToBotEvents();
        setSearchTarget(Bot.GoalEnemy);
    }

    public override void Stop()
    {
        base.Stop();
        clearSearchTarget();
        _haveTalked = false;
    }

    public override void Update(CustomLayer.ActionData data)
    {
        setTargetEnemy();
        if (_searchTarget != null)
        {
            if (_searchTarget.IsVisible)
            {
                Bot.Mover.DogFight.DogFightMove(true, _searchTarget);
                return;
            }
            bool isBeingStealthy = _searchTarget.Hearing.EnemyHeardFromPeace;
            if (isBeingStealthy)
            {
                _sprintEnabled = false;
            }
            else
            {
                checkShouldSprint();
                talk();
            }
            Search.Search(_sprintEnabled, _searchTarget);
        }
        
    }

    public override void OnSteeringTicked()
    {
        if (Shoot.ShootAnyVisibleEnemies(_searchTarget))
        {
            Bot.Steering.SteerByPriority(_searchTarget, false);
            return;
        }
        if (_searchTarget != null)
        {
            bool isBeingStealthy = _searchTarget.Hearing.EnemyHeardFromPeace;
            if (isBeingStealthy)
            {
                _sprintEnabled = false;
            }

            if (!isBeingStealthy &&
                !Bot.Suppression.TrySuppressAnyEnemy(_searchTarget, KnownEnemies))
            {
                checkWeapon();
                Bot.Steering.LookToLastKnownEnemyPosition(_searchTarget);
            }
            else
            {
                Bot.Steering.SteerByPriority(_searchTarget, false);
            }
        }
    }

    private void checkClearEnemy(string profileId, Enemy enemy)
    {
        if (_searchTarget == null)
        {
            return;
        }
        if (_searchTarget.EnemyProfileId == profileId)
        {
            clearSearchTarget();
        }
    }

    private void enemyChanged(Enemy enemy, Enemy lastEnemy)
    {
        if (_searchTarget == null)
        {
            return;
        }
        clearSearchTarget();
        if (enemy != null) setSearchTarget(enemy);
    }

    private void clearSearchTarget()
    {
        Search.ToggleSearch(false, _searchTarget);
    }

    private void setTargetEnemy()
    {
        Enemy searchTarget = _searchTarget;
        if (searchTarget != null &&
            (!searchTarget.EnemyKnown ||
            !Enemy.IsEnemyActive(searchTarget) ||
            !searchTarget.CheckValid()))
        {
            clearSearchTarget();
        }
        if (_searchTarget == null)
        {
            var activeEnemy = Bot.GoalEnemy;
            if (activeEnemy == null) return;
            setSearchTarget(activeEnemy);
        }
    }

    private void setSearchTarget(Enemy enemy)
    {
        Search.ToggleSearch(true, enemy);
    }

    private void talk()
    {
        EnemyPlace targetPlace = Search.PathFinder.TargetPlace;
        if (targetPlace == null)
        {
            return;
        }

        // Scavs will speak out and be more vocal
        if (!_haveTalked &&
            Bot.Info.Profile.IsScav &&
            targetPlace.DistanceToBot < 50f)
        {
            _haveTalked = true;
            if (EFTMath.RandomBool(40))
            {
                Bot.Talk.Say(EPhraseTrigger.OnMutter, ETagStatus.Aware, true);
            }
        }
    }

    private void checkWeapon()
    {
        if (_nextCheckWeaponTime < Time.time)
        {
            _nextCheckWeaponTime = Time.time + 180f * Random.Range(0.5f, 1.5f);
            if (_searchTarget.TimeSinceLastKnownUpdated > 30f)
            {
                if (EFTMath.RandomBool())
                    Bot.Player.HandsController.FirearmsAnimator.CheckAmmo();
                else
                    Bot.Player.HandsController.FirearmsAnimator.CheckChamber();
            }
        }
    }

    private void checkShouldSprint()
    {
        //  || Search.CurrentState == ESearchMove.MoveToDangerPoint
        if (Search.CurrentState == ESearchMove.Wait)
        {
            _sprintEnabled = false;
            return;
        }

        //  || Bot.Enemy?.InLineOfSight == true
        if (_searchTarget?.IsVisible == true)
        {
            _sprintEnabled = false;
            return;
        }

        if (Bot.Decision.CurrentSquadDecision == ESquadDecision.Help)
        {
            _sprintEnabled = true;
            return;
        }

        if (_searchTarget.IsSniper && Bot.Info.PersonalitySettings.General.ENEMYSNIPER_ALWAYS_SPRINT_SEARCH)
        {
            _sprintEnabled = true;
            return;
        }

        var persSettings = Bot.Info.PersonalitySettings;
        float chance = persSettings.Search.SprintWhileSearchChance;
        if (_sprintTimer < Time.time && chance > 0)
        {
            float myPower = Bot.Info.Profile.PowerLevel;
            if (_searchTarget?.EnemyPlayer != null && _searchTarget.EnemyPlayer.AIData.PowerOfEquipment < myPower * 0.5f)
            {
                chance = 100f;
            }

            _sprintEnabled = EFTMath.RandomBool(chance);
            float timeAdd;
            if (_sprintEnabled)
            {
                timeAdd = 4f * Random.Range(0.5f, 2.00f);
            }
            else
            {
                timeAdd = 4f * Random.Range(0.5f, 1.5f);
            }
            _sprintTimer = Time.time + timeAdd;
        }
    }

    private void subscribeToBotEvents()
    {
        if (!_subscribed)
        {
            Bot.EnemyController.Events.OnEnemyRemoved += checkClearEnemy;
            Bot.EnemyController.Events.OnEnemyChanged += enemyChanged;
            _subscribed = true;
        }
    }

    public override void BuildDebugText(StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine($"Search Target {_searchTarget?.EnemyName}");
        base.BuildDebugText(stringBuilder);
    }
}