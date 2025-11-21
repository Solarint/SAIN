using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class AimDownSightsController : BotComponentClassBase
{
    private const float ADS_UPDATE_COOLDOWN = 0.2f;

    public AimDownSightsController(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyBotInCombat;
    }

    public override void ManualUpdate()
    {
        if (Bot.Mover.Running)
        {
            AimingDownSights = false;
        }
        SetADS(AimingDownSights);
        base.ManualUpdate();
    }

    public void UpdateADSstatus(Enemy Enemy)
    {
        if (_timeLastCheckedStatus + 0.1f > Time.time)
            return;

        // If a bot is sneaky, don't change ADS if their enemy is close to avoid alerting them.
        if (Bot?.Info?.PersonalitySettings?.Search?.Sneaky == true &&
            Enemy != null &&
            !Enemy.IsVisible &&
            Enemy.KnownPlaces.EnemyDistanceFromLastKnown < 40f)
        {
            return;
        }

        bool wasAiming = AimingDownSights;
        AimingDownSights = ShallAimDownSights(Enemy?.KnownPlaces.LastKnownPosition, Enemy);
        if (AimingDownSights != wasAiming)
        {
            _timeLastCheckedStatus = Time.time;
        }
    }

    private float _timeLastCheckedStatus;

    public bool AimingDownSights { get; private set; }

    public bool ShallAimDownSights(Vector3? targetPosition = null, Enemy enemy = null)
    {
        bool result = false;
        if (BotOwner.WeaponManager?.Reload.Reloading == true)
        {
            return false; // Don't aim down sights while reloading
        }
        EAimDownSightsStatus status = EAimDownSightsStatus.None;
        if (targetPosition != null)
        {
            status = GetADSStatus(targetPosition.Value, enemy);
        }
        float timeSinceChangeDecision = Bot.Decision.TimeSinceChangeDecision;
        switch (status)
        {
            case EAimDownSightsStatus.EnemyHeardRecent:
            case EAimDownSightsStatus.EnemySeenRecent:
            case EAimDownSightsStatus.EnemyVisible:
            case EAimDownSightsStatus.DogFight:
            case EAimDownSightsStatus.MovingToCover:
                result = enemy != null && enemy.KnownPlaces.BotDistanceFromLastKnown > (AimingDownSights ? 5f : 10f);
                break;
                
            //case EAimDownSightsStatus.DogFight:
            //    result = Bot.Mover.DogFight.Status == Mover.EDogFightStatus.Shooting;
            //    break;

            case EAimDownSightsStatus.None:
            case EAimDownSightsStatus.Sprinting:
                result = false;
                break;

            case EAimDownSightsStatus.HoldInCover:
                result = timeSinceChangeDecision > 3f &&
                    (EFTMath.RandomBool(60) || enemy != null && enemy.KnownPlaces.BotDistanceFromLastKnown > (AimingDownSights ? 5f : 10f));
                break;

            case EAimDownSightsStatus.StandAndShoot:
            case EAimDownSightsStatus.Suppressing:
                //result = enemy != null && enemy.RealDistance > (AimingDownSights ? 10f : 15f);
                result = true;
                break;

            //case EAimDownSightsStatus.Suppressing:
            //    result = enemy != null && enemy.KnownPlaces.BotDistanceFromLastKnown > (AimingDownSights ? 10f : 15f);
            //    break;

            default:
                result = enemy != null && enemy.KnownPlaces.BotDistanceFromLastKnown > (AimingDownSights ? 5f : 10f);
                break;
        }

        LastADSstatus = CurrentADSstatus;
        CurrentADSstatus = status;
        return result;
    }

    public void SetADS(bool value, bool force = false)
    {
        if (!force && Time.time - _timeLastADSUpdate < ADS_UPDATE_COOLDOWN)
        {
            return; // Avoid rapid toggling of ADS
        }
        if (BotOwner.AimingManager.CurrentAiming is BotAimingClass aimingClass)
        {
            aimingClass.HardAim = value;
        }
        var shootController = BotOwner.WeaponManager.ShootController;
        if (shootController != null && shootController.IsAiming != value)
        {
            shootController.SetAim(value);
            _timeLastADSUpdate = Time.time;
        }
        AimingDownSights = value;
    }

    private float _timeLastADSUpdate;

    public EAimDownSightsStatus CurrentADSstatus { get; private set; }
    public EAimDownSightsStatus LastADSstatus { get; private set; }

    public EAimDownSightsStatus GetADSStatus(Vector3 targetPosition, Enemy enemy)
    {
        if (Bot.Mover.ActivePath?.Status == Mover.EBotMoveStatus.DoorInteraction)
        {
            return EAimDownSightsStatus.None;
        }
        if (Bot.Player.IsSprintEnabled || Bot.Mover.Running)
        {
            return EAimDownSightsStatus.Sprinting;
        }

        ECombatDecision currentDecision = Bot.Decision.CurrentCombatDecision;
        if (currentDecision == ECombatDecision.ShootDistantEnemy)
        {
            return EAimDownSightsStatus.StandAndShoot;
        }

        if (enemy != null)
        {
            if (enemy.CanShoot &&
                enemy.IsVisible)
            {
                return EAimDownSightsStatus.EnemyVisible;
            }
            if (enemy.Seen && enemy.TimeSinceSeen < 5)
            {
                return EAimDownSightsStatus.EnemySeenRecent;
            }
            if (enemy.Heard && enemy.TimeSinceHeard < 5)
            {
                return EAimDownSightsStatus.EnemyHeardRecent;
            }
        }

        if (Bot.Decision.CurrentSquadDecision == ESquadDecision.Suppress &&
            Bot.ManualShoot.Reason == EShootReason.SquadSuppressing)
        {
            return EAimDownSightsStatus.Suppressing;
        }

        return currentDecision switch {
            ECombatDecision.SeekCover => Bot.Cover.CoverInUse == null ? EAimDownSightsStatus.MovingToCover : EAimDownSightsStatus.HoldInCover,
            ECombatDecision.StandAndShoot => EAimDownSightsStatus.StandAndShoot,
            ECombatDecision.DogFight => EAimDownSightsStatus.DogFight,
            ECombatDecision.Search => Bot.Search.CurrentState != ESearchMove.DirectMove ? EAimDownSightsStatus.SearchPeekWait : EAimDownSightsStatus.None,
            _ => EAimDownSightsStatus.None,
        };
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