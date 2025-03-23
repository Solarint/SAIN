using EFT;
using HarmonyLib;
using SAIN.Plugin;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.WeaponFunction;
using System.Reflection;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class SteerPriorityClass : BotSubClass<SAINSteeringClass>
    {
        public ESteerPriority CurrentSteerPriority { get; private set; }
        public ESteerPriority LastSteerPriority { get; private set; }
        public PlaceForCheck LastHeardSound { get; private set; }
        public Enemy EnemyWhoLastShotMe { get; private set; }

        // How long a bot will look at where they last saw an enemy instead of something they hear
        private readonly float Steer_TimeSinceLocationKnown_Threshold = 3f;

        // How long a bot will look at where they last saw an enemy if they don't hear any other threats
        private readonly float Steer_TimeSinceSeen_Long = 60f;

        // How far a sound can be for them to react by looking toward it.
        private readonly float Steer_HeardSound_Dist = 50f;

        // How old a sound can be, in seconds, for them to react by looking toward it.
        private readonly float Steer_HeardSound_Age = 3f;

        public AimStatus AimStatus {
            get
            {
                object aimStatus = aimStatusField.GetValue(BotOwner.AimingManager.CurrentAiming);
                if (aimStatus == null) {
                    return AimStatus.NoTarget;
                }

                var status = (AimStatus)aimStatus;

                if (status != AimStatus.NoTarget &&
                    Bot.Enemy?.IsVisible == false &&
                    Bot.LastEnemy?.IsVisible == false) {
                    return AimStatus.NoTarget;
                }
                return (AimStatus)aimStatus;
            }
        }

        public SteerPriorityClass(SAINSteeringClass steering) : base(steering)
        {
        }

        public ESteerPriority GetCurrentSteerPriority(bool lookRandom, bool ignoreRunningPath)
        {
            var lastPriority = CurrentSteerPriority;
            CurrentSteerPriority = findSteerPriority(lookRandom, ignoreRunningPath);

            if (CurrentSteerPriority != lastPriority)
                LastSteerPriority = lastPriority;

            return CurrentSteerPriority;
        }

        private ESteerPriority findSteerPriority(bool lookRandom, bool ignoreRunningPath)
        {
            ESteerPriority result = strickChecks(ignoreRunningPath);

            if (result != ESteerPriority.None) {
                return result;
            }

            result = reactiveSteering();

            if (result != ESteerPriority.None) {
                return result;
            }

            result = senseSteering();

            if (result != ESteerPriority.None) {
                return result;
            }

            if (lookRandom) {
                return ESteerPriority.RandomLook;
            }
            return ESteerPriority.None;
        }

        private ESteerPriority strickChecks(bool ignoreRunningPath)
        {
            if (!ignoreRunningPath && Bot.Mover.SprintController.Running)
                return ESteerPriority.RunningPath;

            if (Player.IsSprintEnabled)
                return ESteerPriority.Sprinting;

            if (lookToAimTarget())
                return ESteerPriority.Aiming;

            if (Bot.ManualShoot.Reason != EShootReason.None
                && Bot.ManualShoot.ShootPosition != Vector3.zero)
                return ESteerPriority.ManualShooting;

            if (enemyVisible())
                return ESteerPriority.EnemyVisible;

            return ESteerPriority.None;
        }

        private ESteerPriority reactiveSteering()
        {
            if (enemyShotMe()) {
                return ESteerPriority.LastHit;
            }

            //if (BotOwner.Memory.IsUnderFire && !Bot.Memory.LastUnderFireEnemy.IsCurrentEnemy)
            if (BotOwner.Memory.IsUnderFire)
                return ESteerPriority.UnderFire;

            return ESteerPriority.None;
        }

        private ESteerPriority senseSteering()
        {
            EnemyPlace lastKnownPlace = Bot.Enemy?.KnownPlaces?.LastKnownPlace;

            if (lastKnownPlace != null && lastKnownPlace.TimeSincePositionUpdated < Steer_TimeSinceLocationKnown_Threshold)
                return ESteerPriority.EnemyLastKnown;

            if (heardThreat())
                return ESteerPriority.HeardThreat;

            if (lastKnownPlace != null && lastKnownPlace.TimeSincePositionUpdated < Steer_TimeSinceSeen_Long)
                return ESteerPriority.EnemyLastKnownLong;

            return ESteerPriority.None;
        }

        private bool heardThreat()
        {
            if (BaseClass.HeardSoundSteering.LastHeardVisibleDanger?.ShallLook == true) {
                return true;
            }
            if (Bot.Search.SearchActive) {
                return false;
            }
            if (BaseClass.HeardSoundSteering.LastHeardDanger?.ShallLook == true) {
                return true;
            }
            return false;
        }

        private bool heardThreat(out PlaceForCheck placeForCheck)
        {
            placeForCheck = BotOwner.BotsGroup.YoungestFastPlace(BotOwner, Steer_HeardSound_Dist, Steer_HeardSound_Age);
            if (placeForCheck != null) {
                Enemy enemy = Bot.Enemy;
                if (enemy == null) {
                    return true;
                }
                if (Bot.Squad.SquadInfo?.PlayerPlaceChecks.TryGetValue(enemy.EnemyProfileId, out PlaceForCheck enemyPlace) == true &&
                    enemyPlace != placeForCheck) {
                    return true;
                }
            }
            return false;
        }

        private bool enemyShotMe()
        {
            float timeSinceShot = Bot.Medical.TimeSinceShot;
            if (timeSinceShot > 3f || timeSinceShot < 0.2f) {
                EnemyWhoLastShotMe = null;
                return false;
            }

            Enemy enemy = Bot.Medical.HitByEnemy.EnemyWhoLastShotMe;
            if (enemy != null &&
                enemy.CheckValid() &&
                enemy.EnemyPerson.Active &&
                !enemy.IsCurrentEnemy) {
                EnemyWhoLastShotMe = enemy;
                return true;
            }
            EnemyWhoLastShotMe = null;
            return false;
        }

        private bool lookToAimTarget()
        {
            if (BotOwner.WeaponManager.Reload?.Reloading == true) {
                return false;
            }
            if (Bot.Aim.AimStatus == AimStatus.NoTarget) {
                return false;
            }
            return canSeeAndShoot(Bot.Enemy) || canSeeAndShoot(Bot.LastEnemy) || canSeeAndShoot(Bot.Shoot.LastShotEnemy);
        }

        private bool canSeeAndShoot(Enemy enemy)
        {
            return enemy != null && enemy.IsVisible && enemy.CanShoot;
        }

        private bool enemyVisible()
        {
            Enemy enemy = Bot.Enemy;

            if (enemy != null) {
                if (enemy.IsVisible) {
                    return true;
                }

                if (enemy.Seen &&
                    enemy.TimeSinceSeen < 0.5f) {
                    return true;
                }
            }
            return false;
        }

        static SteerPriorityClass()
        {
            aimStatusField = AccessTools.Field(Helpers.HelpersGClass.AimDataType, "aimStatus_0");
        }

        private void updateSettings()
        {
        }

        private static FieldInfo aimStatusField;
    }
}