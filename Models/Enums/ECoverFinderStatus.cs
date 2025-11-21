namespace SAIN.Models.Enums;

public enum ECoverFinderStatus
{
    None = 0,
    Idle = 1,
    SearchingColliders = 1,
    RecheckingPointsWithLimit = 2,
    RecheckingPoints = 3,
}