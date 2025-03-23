using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Search;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public class AimDownSightsController : BotBase, IBotClass
    {
        public AimDownSightsController(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public void UpdateADSstatus()
        {
            Enemy targetEnemy = Bot.CurrentTarget?.CurrentTargetEnemy;

            // If a bot is sneaky, don't change ADS if their enemy is close to avoid alerting them.
            if (Bot?.Info?.PersonalitySettings?.Search?.Sneaky == true &&
                targetEnemy != null &&
                !targetEnemy.IsVisible &&
                targetEnemy.KnownPlaces.EnemyDistanceFromLastKnown < 40f) {
                return;
            }

            bool shallADS = ShallAimDownSights(targetEnemy?.KnownPlaces.LastKnownPosition);
            SetADS(shallADS);
        }

        public bool ShallAimDownSights(Vector3? targetPosition = null)
        {
            bool result = false;
            EAimDownSightsStatus status = EAimDownSightsStatus.None;
            if (targetPosition != null) {
                status = GetADSStatus(targetPosition.Value);
            }
            float timeSinceChangeDecision = Bot.Decision.TimeSinceChangeDecision;
            switch (status) {
                case EAimDownSightsStatus.EnemyHeardRecent:
                case EAimDownSightsStatus.EnemySeenRecent:
                case EAimDownSightsStatus.EnemyVisible:
                    result = true;
                    break;

                case EAimDownSightsStatus.None:
                case EAimDownSightsStatus.Sprinting:
                case EAimDownSightsStatus.DogFight:
                case EAimDownSightsStatus.MovingToCover:
                    result = false;
                    break;

                case EAimDownSightsStatus.HoldInCover:
                    result = timeSinceChangeDecision > 3f;
                    break;

                case EAimDownSightsStatus.StandAndShoot:
                    result = Bot.Enemy != null && Bot.Enemy.RealDistance > 15f;
                    break;

                case EAimDownSightsStatus.Suppressing:
                    result = Bot.ManualShoot.Reason == EShootReason.SquadSuppressing;
                    break;

                default:
                    break;
            }

            LastADSstatus = CurrentADSstatus;
            CurrentADSstatus = status;
            return result;
        }

        public void SetADS(bool value)
        {
            var aim = BotAimingClass;
            if (aim != null) {
                aim.HardAim = value;
            }
            var shootController = BotOwner.WeaponManager.ShootController;
            if (shootController != null && shootController.IsAiming != value) {
                shootController.SetAim(value);
            }
        }

        public BotAimingClass BotAimingClass {
            get
            {
                if (_botAimingClass == null) {
                    var aimData = BotOwner.AimingManager.CurrentAiming;
                    if (aimData != null && aimData is BotAimingClass aimClass) {
                        _botAimingClass = aimClass;
                    }
                }
                return _botAimingClass;
            }
        }

        private BotAimingClass _botAimingClass;

        public EAimDownSightsStatus CurrentADSstatus { get; private set; }
        public EAimDownSightsStatus LastADSstatus { get; private set; }

        public EAimDownSightsStatus GetADSStatus(Vector3 targetPosition)
        {
            var enemy = Bot.Enemy;
            float sqrMagToTarget = (targetPosition - Bot.Position).sqrMagnitude;

            if (Bot.Player.IsSprintEnabled || Bot.Mover.SprintController.Running) {
                return EAimDownSightsStatus.Sprinting;
            }

            ECombatDecision currentDecision = Bot.Decision.CurrentCombatDecision;
            if (currentDecision == ECombatDecision.ShootDistantEnemy) {
                return EAimDownSightsStatus.StandAndShoot;
            }

            if (enemy != null) {
                if (enemy.CanShoot &&
                    enemy.IsVisible &&
                    enemy.RealDistance > 50f) {
                    return EAimDownSightsStatus.EnemyVisible;
                }
                if (enemy.Seen && enemy.TimeSinceSeen < 5) {
                    return EAimDownSightsStatus.EnemySeenRecent;
                }
                if (enemy.Heard && enemy.TimeSinceHeard < 5) {
                    return EAimDownSightsStatus.EnemyHeardRecent;
                }
            }

            if (Bot.Decision.CurrentSquadDecision == ESquadDecision.Suppress &&
                Bot.ManualShoot.Reason == EShootReason.SquadSuppressing) {
                return EAimDownSightsStatus.Suppressing;
            }

            switch (currentDecision) {
                case ECombatDecision.RunToCover:
                case ECombatDecision.MoveToCover:
                    return EAimDownSightsStatus.MovingToCover;

                case ECombatDecision.HoldInCover:
                    return EAimDownSightsStatus.HoldInCover;

                case ECombatDecision.StandAndShoot:
                    return EAimDownSightsStatus.StandAndShoot;

                case ECombatDecision.DogFight:
                    return EAimDownSightsStatus.DogFight;

                case ECombatDecision.Search:
                    return Bot.Search.CurrentState != ESearchMove.DirectMove ? EAimDownSightsStatus.SearchPeekWait : EAimDownSightsStatus.None;

                default:
                    return EAimDownSightsStatus.None;
            }
        }

        public enum EAimDownSightsStatus
        {
            None = 0,
            HoldInCover = 1,
            StandAndShoot = 2,
            EnemyVisible = 3,
            Sprinting = 4,
            MovingToCover = 5,
            Suppressing = 6,
            DogFight = 7,
            EnemySeenRecent = 8,
            EnemyHeardRecent = 9,
            SearchPeekWait = 10,
        }
    }
}