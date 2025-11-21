using EFT;
using SAIN.Layers.Combat.Solo.Cover;
using SAIN.Models.Enums;

namespace SAIN.Layers.Combat.Solo;

internal class CombatSoloLayer(BotOwner bot, int priority) : SAINLayer(bot, priority, Name, ESAINLayer.Combat)
{
    public static readonly string Name = BuildLayerName("Combat Layer");

    public override Action GetNextAction()
    {
        _lastSelfDecision = _currentSelfDecision;
        _lastDecision = _currentDecision;

        if (_doSurgeryAction)
        {
            _doSurgeryAction = false;
            return new Action(typeof(DoSurgeryAction), $"Surgery");
        }

        switch (_lastDecision)
        {
            case ECombatDecision.MoveToEngage:
                return new Action(typeof(MoveToEngageAction), $"{_lastDecision}");

            case ECombatDecision.MeleeAttack:
                return new Action(typeof(MeleeAttackAction), $"{_lastDecision}");

            case ECombatDecision.FightZombies:
                return new Action(typeof(FightZombiesAction), $"{_lastDecision}");

            case ECombatDecision.RushEnemy:
                return new Action(typeof(RushEnemyAction), $"{_lastDecision}");

            case ECombatDecision.ThrowGrenade:
                return new Action(typeof(ThrowGrenadeAction), $"{_lastDecision}");

            case ECombatDecision.ShiftCover:
                return new Action(typeof(ShiftCoverAction), $"{_lastDecision}");

            case ECombatDecision.SeekCover:
            case ECombatDecision.Retreat:
                string label;
                if (_lastSelfDecision != ESelfActionType.None)
                    label = $"{_lastDecision} + {_lastSelfDecision}";
                else
                    label = $"{_lastDecision}";
                return new Action(typeof(SeekCoverAction), label);

            case ECombatDecision.ShootDistantEnemy:
            case ECombatDecision.StandAndShoot:
                return new Action(typeof(StandAndShootAction), $"{_lastDecision}");

            case ECombatDecision.Search:
                return new Action(typeof(SearchAction), $"{_lastDecision}");

            case ECombatDecision.Freeze:
                return new Action(typeof(FreezeAction), $"{_lastDecision}");

            default:
                return new Action(typeof(StandAndShootAction), $"DEFAULT! {_lastDecision}");
        }
    }

    public override bool IsActive()
    {
        bool active = GetBotComponent() && _currentDecision != ECombatDecision.None;
        CheckActiveChanged(active);
        return active;
    }

    public override bool IsCurrentActionEnding()
    {
        if (base.IsCurrentActionEnding())
        {
            return true;
        }

        // this is dumb im sorry
        if (!_doSurgeryAction
            && _currentSelfDecision == ESelfActionType.Surgery
            && Bot.Cover.CoverInUse != null)
        {
            _doSurgeryAction = true;
            return true;
        }

        if (_lastSelfDecision == ESelfActionType.Surgery &&
            _currentSelfDecision != ESelfActionType.Surgery)
        {
            return true;
        }
        return _currentDecision != _lastDecision;
    }

    private bool _doSurgeryAction;

    private ECombatDecision _lastDecision = ECombatDecision.None;
    private ESelfActionType _lastSelfDecision = ESelfActionType.None;
    public ECombatDecision _currentDecision => Bot.Decision.CurrentCombatDecision;
    public ESelfActionType _currentSelfDecision => Bot.Decision.CurrentSelfDecision;
}