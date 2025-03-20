using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Search;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAIN.Layers.Combat.Solo
{
    internal class SearchAction : CombatAction, ISAINAction
    {
        private Enemy _searchTarget;
        private bool _subscribed;
        private float _nextCheckWeaponTime;
        private float _nextUpdateSearchTime;
        private bool _haveTalked = false;
        private bool _sprintEnabled = false;
        private float _sprintTimer = 0f;
        private SAINSearchClass Search => Bot.Search;

        public override void Start()
        {
            subscribeToBotEvents();
            setSearchTarget(Bot.Enemy);
            Toggle(true);
        }

        public override void Stop()
        {
            clearSearchTarget();
            Toggle(false);
            BotOwner.Mover?.MovementResume();
            _haveTalked = false;
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            setTargetEnemy();
            var enemy = _searchTarget;
            if (enemy == null) return;

            bool isBeingStealthy = enemy.Hearing.EnemyHeardFromPeace;
            if (isBeingStealthy) {
                _sprintEnabled = false;
            }
            else {
                checkShouldSprint();
                talk();
            }

            steer();

            if (_nextUpdateSearchTime < Time.time) {
                _nextUpdateSearchTime = Time.time + 0.1f;
                Search.Search(_sprintEnabled, enemy);
            }

            if (!_sprintEnabled) {
                Shoot.CheckAimAndFire();
                if (!isBeingStealthy)
                    checkWeapon();
            }
        }

        private void checkClearEnemy(string profileId, Enemy enemy)
        {
            if (_searchTarget == null) {
                return;
            }
            if (_searchTarget.EnemyProfileId == profileId) {
                clearSearchTarget();
            }
        }

        private void enemyChanged(Enemy enemy, Enemy lastEnemy)
        {
            if (_searchTarget == null) {
                return;
            }
            clearSearchTarget();
            if (enemy != null) setSearchTarget(enemy);
        }

        private void clearSearchTarget()
        {
            Search.ToggleSearch(false, _searchTarget);
            _searchTarget = null;
        }

        private void setTargetEnemy()
        {
            if (_searchTarget != null &&
                (!_searchTarget.EnemyKnown ||
                !_searchTarget.Person.Active ||
                !_searchTarget.CheckValid())) {
                clearSearchTarget();
            }
            var activeEnemy = Bot.Enemy;
            if (_searchTarget == null) {
                if (activeEnemy == null) return;
                setSearchTarget(activeEnemy);
            }
        }

        private void setSearchTarget(Enemy enemy)
        {
            Search.ToggleSearch(true, enemy);
            _searchTarget = enemy;
            _nextUpdateSearchTime = 0f;
        }

        private void talk()
        {
            if (Search.FinalDestination == null) {
                return;
            }

            // Scavs will speak out and be more vocal
            if (!_haveTalked &&
                Bot.Info.Profile.IsScav &&
                (BotOwner.Position - Search.FinalDestination.Value).sqrMagnitude < 50f * 50f) {
                _haveTalked = true;
                if (EFTMath.RandomBool(40)) {
                    Bot.Talk.Say(EPhraseTrigger.OnMutter, ETagStatus.Aware, true);
                }
            }
        }

        private void checkWeapon()
        {
            if (_nextCheckWeaponTime < Time.time) {
                _nextCheckWeaponTime = Time.time + 180f * Random.Range(0.5f, 1.5f);
                if (_searchTarget.TimeSinceLastKnownUpdated > 30f) {
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
            if (Search.CurrentState == ESearchMove.MoveToEndPeek || Search.CurrentState == ESearchMove.Wait) {
                _sprintEnabled = false;
                return;
            }

            //  || Bot.Enemy?.InLineOfSight == true
            if (_searchTarget?.IsVisible == true) {
                _sprintEnabled = false;
                return;
            }

            if (Bot.Decision.CurrentSquadDecision == ESquadDecision.Help) {
                _sprintEnabled = true;
                return;
            }

            var persSettings = Bot.Info.PersonalitySettings;
            float chance = persSettings.Search.SprintWhileSearchChance;
            if (_sprintTimer < Time.time && chance > 0) {
                float myPower = Bot.Info.Profile.PowerLevel;
                if (Bot.Enemy?.EnemyPlayer != null && Bot.Enemy.EnemyPlayer.AIData.PowerOfEquipment < myPower * 0.5f) {
                    chance = 100f;
                }

                _sprintEnabled = EFTMath.RandomBool(chance);
                float timeAdd;
                if (_sprintEnabled) {
                    timeAdd = 4f * Random.Range(0.5f, 2.00f);
                }
                else {
                    timeAdd = 4f * Random.Range(0.5f, 1.5f);
                }
                _sprintTimer = Time.time + timeAdd;
            }
        }

        private void steer()
        {
            if (!Bot.Steering.SteerByPriority(_searchTarget, false)) {
                Bot.Steering.LookToMovingDirection();
            }
        }

        private void subscribeToBotEvents()
        {
            if (!_subscribed) {
                Bot.EnemyController.Events.OnEnemyRemoved += checkClearEnemy;
                Bot.EnemyController.Events.OnEnemyChanged += enemyChanged;
                _subscribed = true;
            }
        }

        public SearchAction(BotOwner bot) : base(bot, "Search")
        {
        }
    }
}