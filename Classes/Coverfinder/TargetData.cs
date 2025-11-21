using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public struct TargetData(Enemy enemy)
{
    public Enemy TargetEnemy { get; } = enemy;

    public readonly Vector3 BotPosition => TargetEnemy.Bot.NavMeshPosition;

    public Vector3 TargetPosition {
        get
        {
            Vector3? lastKnown = TargetEnemy?.KnownPlaces?.LastKnownPosition;
            if (lastKnown != null)
            {
                _targetPosition = lastKnown.Value;
            }
            return _targetPosition;
        }
    }

    private Vector3 _targetPosition;

    public readonly Vector3 DirBotToTarget => TargetEnemy.EnemyDirection;
    public readonly Vector3 DirBotToTargetNormal => TargetEnemy.EnemyDirectionNormal;
    public readonly float TargetDistance => TargetEnemy.KnownPlaces.BotDistanceFromLastKnown;
}