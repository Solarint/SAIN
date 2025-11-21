namespace SAIN.Models.Enums;

public enum EEnemySteerDir
{
    None,
    LastSeenPos,
    LastHeardPos,
    VisibleEnemyPos,
    BlindCornerPos,
    LastCornerPos,
    LastKnownPos,
    PathNode,
    VisibleLastKnownPos,
    NullLastKnown_ERROR,
    NullEnemy_ERROR,
}