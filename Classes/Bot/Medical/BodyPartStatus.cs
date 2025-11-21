using EFT.HealthSystem;

namespace SAIN.SAINComponent.Classes;

public class BodyPartStatus
{
    public BodyPartStatus(EBodyPart part, SAINBotHitReaction hitReactionClass)
    {
        _bodyPart = part;
        _hitReaction = hitReactionClass;
    }

    private IHealthController _healthController => _hitReaction.HealthController;
    private readonly EBodyPart _bodyPart;
    private readonly SAINBotHitReaction _hitReaction;

    public EInjurySeverity InjurySeverity
    {
        get
        {
            float health = PartHealthNormalized;
            if (health > 0.75f)
            {
                return EInjurySeverity.None;
            }
            if (health > 0.4f)
            {
                return EInjurySeverity.Injury;
            }
            if (health > 0.01f)
            {
                return EInjurySeverity.HeavyInjury;
            }
            return EInjurySeverity.Destroyed;
        }
    }

    public float PartHealth => _healthController.GetBodyPartHealth(_bodyPart, false).Current;
    public float PartHealthNormalized => _healthController.GetBodyPartHealth(_bodyPart, false).Normalized;
    public bool PartDestoyed => _healthController.IsBodyPartDestroyed(_bodyPart);

}