using SAIN.Models.Structs;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Search;

public enum EPathCalcFailReason
{
    None,
    NullDestination,
    NoTarget,
    NullPlace,
    TooClose,
    SampleStart,
    SampleEnd,
    CalcPath,
    LastCorner,
}

public class SearchPathFinder : BotSubClass<SAINSearchClass>
{
    public EnemyPlace TargetPlace { get; private set; }
    public BotPeekPlan? PeekPoints { get; private set; }
    public bool SearchedTargetPosition => TargetPlace == null || TargetPlace.HasArrivedPersonal || TargetPlace.HasArrivedSquad;
    public bool FinishedPeeking { get; set; }

    public SearchPathFinder(SAINSearchClass searchClass) : base(searchClass)
    {
        CanEverTick = false;
    }

    public bool HasPathToSearchTarget(Enemy enemy, out string failReason)
    {
        return CheckEnemyPath(enemy, out failReason);
    }

    public void UpdateSearchDestination(Enemy enemy)
    {
        if (!SearchedTargetPosition)
        {
            checkFinishedSearch(enemy);
        }

        if (_nextCheckPosTime < Time.time || SearchedTargetPosition || TargetPlace == null)
        {
            _nextCheckPosTime = Time.time + 4f;
            if (!CheckEnemyPath(enemy, out string failReason))
            {
                //Logger.LogDebug($"Failed to calc path during search for reason: [{failReason}]");
            }
        }
    }

    private void checkFinishedSearch(Enemy enemy)
    {
        if (SearchedTargetPosition)
        {
            return;
        }
        var lastKnown = enemy.KnownPlaces.LastKnownPlace;
        if (lastKnown == null)
        {
            Reset();
            return;
        }
        if (lastKnown.HasArrivedPersonal || lastKnown.HasArrivedSquad)
        {
            Reset();
            return;
        }
        EnemyPlace targetPlace = TargetPlace;
        if (targetPlace == null || targetPlace != lastKnown)
        {
            Reset();
            return;
        }

        var pathToEnemy = enemy.Path.PathToEnemy;
        if (pathToEnemy.corners.Length > 2)
        {
            return;
        }

        float destinationDistance = targetPlace.DistanceToBot;
        if (destinationDistance > 2f)
        {
            return;
        }
        enemy.KnownPlaces.SetPlaceAsSearched(targetPlace);
        Reset();
    }

    public void Reset()
    {
        PeekPoints = null;
        TargetPlace = null;
        FinishedPeeking = false;
    }

    public bool CheckEnemyPath(Enemy enemy, out string failReason)
    {
        EnemyPlace lastKnownPlace = enemy.KnownPlaces.LastKnownPlace;
        if (lastKnownPlace == null)
        {
            failReason = "lastKnown null";
            return false;
        }
        NavMeshPath path = enemy.Path.PathToEnemy;
        if (path == null || path.status == NavMeshPathStatus.PathInvalid)
        {
            failReason = "path Invalid";
            return false;
        }
        int length = path.corners.Length;
        if (length < 2)
        {
            failReason = "path Invalid corner length";
            return false;
        }
        Vector3 destination = path.corners[length - 1];
        if ((destination - Bot.Position).sqrMagnitude <= 0.33f)
        {
            failReason = "tooClose";
            return false;
        }

        if ((destination - lastKnownPlace.Position).sqrMagnitude > 0.5f &&
            Physics.SphereCast(destination + Vector3.up, 0.1f, lastKnownPlace.Position - destination, out RaycastHit hit, 1f))
        {
            failReason = "path not complete";
            return false;
        }

        BaseClass.Reset();
        PeekPoints = findPeekPosition(enemy);
        TargetPlace = lastKnownPlace;
        failReason = string.Empty;
        return true;
    }

    private BotPeekPlan? findPeekPosition(Enemy enemy)
    {
        //const float MIN_ANGLE_TO_PEEK = 5f;
        //const float CORNER_PEEK_DIST = 3f;
        //if (enemy.VisiblePathPoint == null)
        //{
        //    return null;
        //}
        //
        // Need to rework this because the "blindcorner" is no longer created
        //Vector3[] pathCorners = enemy.Path.PathToEnemy.corners;
        //int count = pathCorners.Length;
        //int blindCornerIndex = blindCorner.PathIndex;
        //Vector3 blindCornerPosition = blindCorner.GroundPosition;
        //Vector3 botPosition = Bot.Position;
        //Vector3 blindCornerDir = blindCornerPosition - botPosition;
        //Vector3 blindCornerDirNormal = blindCornerDir.normalized;
        //
        //Vector3 startPeekPosition = blindCornerPosition - (blindCornerDirNormal * CORNER_PEEK_DIST);
        //
        //for (int i = blindCornerIndex; i < count; i++)
        //{
        //    Vector3 corner = pathCorners[i];
        //    Vector3 dir = corner - blindCornerPosition;
        //    Vector3 dirNormal = dir.normalized;
        //    float signedAngle = findHorizSignedAngle(blindCornerDirNormal, dirNormal);
        //    if (Mathf.Abs(signedAngle) < MIN_ANGLE_TO_PEEK)
        //    {
        //        continue;
        //    }
        //    Vector3 oppositePoint = blindCornerPosition - (dirNormal * CORNER_PEEK_DIST);
        //    if (NavMesh.Raycast(blindCornerPosition, oppositePoint, out NavMeshHit hit, -1))
        //    {
        //        oppositePoint = hit.position;
        //    }
        //
        //    return new BotPeekPlan(startPeekPosition, oppositePoint, corner);
        //}
        return null;
    }

    private float findHorizSignedAngle(Vector3 dirA, Vector3 dirB)
    {
        dirA.y = 0;
        dirB.y = 0;
        float signedAngle = Vector3.SignedAngle(dirA, dirB, Vector3.up);
        return signedAngle;
    }

    private float _nextCheckPosTime;
}