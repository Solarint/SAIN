using SAIN.Preset.GlobalSettings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public enum VisiblePathNodeState
{
    NotSet,
    NotChecked,
    NotVisible,
    Visible,
    VisibleAndCanShoot,
}

public readonly struct PathVisibilityConfig
{
    public PathVisibilityConfig(SteeringSettings settings)
    {
        characterHeight = settings.characterHeight;
        startHeight = settings.startHeight;
        maxPathLength = settings.MaxPathLengthPathVision;
        distanceBetweenPoints = settings.DistanceBetweenPoints;
        stackHeight = Mathf.RoundToInt(settings.GeneratePointStackHeight);

        height = characterHeight - startHeight;
        startHeightOffset = Vector3.up * startHeight;
        spacing = height / stackHeight;
        heightStep = Vector3.up * spacing;
    }

    public readonly float characterHeight;
    public readonly float startHeight;
    public readonly Vector3 startHeightOffset;
    public readonly float maxPathLength;
    public readonly int stackHeight;
    public readonly float height;
    public readonly float spacing;
    public readonly Vector3 heightStep;
    public readonly float distanceBetweenPoints;
}

public struct BotVisiblePathNode
{
    public BotVisiblePathNode(Vector3 point, int cornerStartIndex, int cornerEndIndex)
    {
        Point = point;
        CornerStartIndex = cornerStartIndex;
        CornerEndIndex = cornerEndIndex;
    }

    public BotVisiblePathNode()
    {
        Point = default;
    }

    public readonly Vector3 Point;
    public bool Visible = false;
    public readonly int CornerStartIndex;
    public readonly int CornerEndIndex;
}

public class SAINEnemyPath(EnemyData enemyData) : EnemyBase(enemyData, enemyData.Enemy.Bot)
{
    public EPathDistance EPathDistance {
        get
        {
            float distance = PathLength;
            if (distance <= ENEMY_DISTANCE_VERYCLOSE)
            {
                return EPathDistance.VeryClose;
            }
            if (distance <= ENEMY_DISTANCE_CLOSE)
            {
                return EPathDistance.Close;
            }
            if (distance <= ENEMY_DISTANCE_MID)
            {
                return EPathDistance.Mid;
            }
            if (distance <= ENEMY_DISTANCE_FAR)
            {
                return EPathDistance.Far;
            }
            return EPathDistance.VeryFar;
        }
    }

    private const float ENEMY_DISTANCE_VERYCLOSE = 8f;
    private const float ENEMY_DISTANCE_CLOSE = 16f;
    private const float ENEMY_DISTANCE_MID = 32f;
    private const float ENEMY_DISTANCE_FAR = 64f;

    public float PathLength { get; private set; } = float.MaxValue;

    public float DistanceToEnemyPositionFromLastCorner { get; private set; }

    public NavMeshPath PathToEnemy { get; } = new NavMeshPath();
    public NavMeshPathStatus PathToEnemyStatus => PathToEnemy.status;
    public Vector3[] PathCorners => PathToEnemy.corners;

    public BotVisiblePathNode[] AllPathNodes { get; } = enemyData.EnemyPlayer.IsAI ? new BotVisiblePathNode[512] : new BotVisiblePathNode[1024];
    public List<BotVisiblePathNode> VisibleNodes { get; } = [];

    public int AllPathNodeCount { get; private set; } = 0;

    protected override void OnEnemyKnownChanged(bool known, Enemy enemy)
    {
        if (!known)
        {
            Clear();
        }
    }

    public override void Dispose()
    {
        Clear();
        base.Dispose();
    }

    public void CheckCalcPath(PathVisibilityConfig pathVisibilityConfig, float currentTime)
    {
        if (ShallCalcNewPath(currentTime))
        {
            Vector3 enemyPosition = Enemy.KnownPlaces.LastKnownPosition.Value;
            PathToEnemy.ClearCorners();
            NavMesh.CalculatePath(Bot.NavMeshPosition, enemyPosition, NavMesh.AllAreas, PathToEnemy);
            _newPath = true;
            int max = AllPathNodes.Length;
            PathLength = CalcPathLengthCreateVisionNodes(pathVisibilityConfig, AllPathNodes, PathCorners, out int nodeCount, max);
            AllPathNodeCount = nodeCount;
            if (PathCorners.Length > 0)
                DistanceToEnemyPositionFromLastCorner = (Enemy.LastKnownPosition.Value - PathCorners[PathCorners.Length - 1]).magnitude;
            else
                DistanceToEnemyPositionFromLastCorner = 0;
        }
    }

    public bool ShallCheckPathVision(float currentTime, Vector3 botWeaponRoot)
    {
        if (!Enemy.WasValid || !Enemy.EnemyKnown) return false;
        if (!_newPath && _nextPathVisionCheck > currentTime) return false;
        float interval;
        if (Enemy.IsAI)
        {
            interval = Enemy.IsCurrentEnemy ? 1f / 12f : 1f / 2f;
        }
        else
        {
            interval = Enemy.IsCurrentEnemy ? 1f / 20f : 1f / 4f;
        }
        _nextPathVisionCheck = currentTime + (interval * UnityEngine.Random.Range(0.85f, 1.15f));

        if (!Enemy.IsEnemyActive(Enemy))
        {
            return false;
        }
        if (_newPath)
        {
            _newPath = false;
            _lastBotHeadPos = botWeaponRoot;
            return true;
        }
        if ((botWeaponRoot - _lastBotHeadPos).sqrMagnitude < 0.01f)
        {
            return false;
        }
        _lastBotHeadPos = botWeaponRoot;
        return true;
    }

    private bool _newPath = false;
    private Vector3 _lastBotHeadPos;
    private float _nextPathVisionCheck;

    public bool ShallCalcNewPath(float currentTime)
    {
        if (Enemy?.EnemyKnown == false)
        {
            return false;
        }

        Vector3? LastKnown = Enemy.KnownPlaces.LastKnownPosition;
        if (LastKnown == null)
        {
            return false;
        }

        bool isCurrentEnemy = Enemy.IsCurrentEnemy;
        if (!isCurrentEnemy && !isEnemyInRange())
        {
            return false;
        }

        // Did we already check the current enemy position and has the bot not moved? dont recalc path then
        if (!checkPositionsChanged(Bot.Position, LastKnown.Value))
        {
            return false;
        }

        if (_calcPathTime + calcDelayOnDistance() > currentTime)
        {
            return false;
        }
        _calcPathTime = currentTime;
        return true;
    }

    private static float CalcPathLengthCreateVisionNodes(PathVisibilityConfig pathVisibilityConfig, BotVisiblePathNode[] allPathPoints, Vector3[] pathCorners, out int nodeCount, int max)
    {
        float maxPathLength = pathVisibilityConfig.maxPathLength;
        int stackHeight = pathVisibilityConfig.stackHeight;
        Vector3 startHeightOffset = pathVisibilityConfig.startHeightOffset;
        float spacing = pathVisibilityConfig.spacing;
        Vector3 heightStep = pathVisibilityConfig.heightStep;
        float distanceBetweenPoints = pathVisibilityConfig.distanceBetweenPoints;
        float minGenerationMagnitude = distanceBetweenPoints * 2f;

        float pathLength = 0f;
        nodeCount = 0;
        bool full = false;
        for (int i = 0; i < pathCorners.Length - 1; i++)
        {
            Vector3 cornerA = pathCorners[i];
            Vector3 cornerB = pathCorners[i + 1];
            Vector3 direction = cornerB - cornerA;
            float magnitude = direction.magnitude;
            pathLength += magnitude;

            if (i > 0 && !full)
            {
                if (pathLength < maxPathLength)
                {
                    GeneratePoints(allPathPoints, cornerA, stackHeight, startHeightOffset, heightStep, ref nodeCount, ref full, i, i + 1, max);
                    if (full) continue;
                    // Create Equal dist points along the line between two corners.
                    if (magnitude > minGenerationMagnitude)
                    {
                        Vector3 lengthStep = direction.normalized * spacing;
                        int pointCount = Mathf.FloorToInt(magnitude / spacing);
                        for (int j = 0; j < pointCount; j++)
                        {
                            Vector3 point = cornerA + lengthStep * j;
                            GeneratePoints(allPathPoints, point, stackHeight, startHeightOffset, heightStep, ref nodeCount, ref full, i, i + 1, max);
                            if (full) break;
                        }
                    }
                    else if (magnitude > distanceBetweenPoints)
                    {
                        GeneratePoints(allPathPoints, cornerA + direction * 0.5f, stackHeight, startHeightOffset, heightStep, ref nodeCount, ref full, i, i + 1, max);
                    }
                }
                else if (i == pathCorners.Length - 2)
                {
                    GeneratePoints(allPathPoints, cornerB, stackHeight, startHeightOffset, heightStep, ref nodeCount, ref full, i + 1, i + 1, max);
                }
            }
        }
        for (int i = nodeCount + 1; i < allPathPoints.Length; i++) allPathPoints[i] = default;
        return pathLength;
    }

    private static void GeneratePoints(
        BotVisiblePathNode[] allPathPoints,
        Vector3 checkPoint,
        int stackHeight,
        Vector3 startHeightOffset,
        Vector3 step,
        ref int currentIndex,
        ref bool full,
        int cornerAindex,
        int cornerBindex,
        int max
        )
    {
        Vector3 point = checkPoint + startHeightOffset;
        for (int i = 0; i < stackHeight; i++)
        {
            if (currentIndex == max)
            {
                full = true;
                break;
            }
            Vector3 nodePosition = point + step * i;
            allPathPoints[currentIndex] = new BotVisiblePathNode(nodePosition, cornerAindex, cornerBindex);
            currentIndex++;
        }
    }

    public void Clear()
    {
        _calcPathTime = 0;
        PathToEnemy.ClearCorners();
        PathLength = float.MaxValue;
        DistanceToEnemyPositionFromLastCorner = 0;
    }

    private float calcDelayOnDistance()
    {
        bool performanceMode = SAINPlugin.LoadedPreset.GlobalSettings.General.Performance.PerformanceMode;
        bool currentEnemy = Enemy.IsCurrentEnemy;
        bool isAI = Enemy.IsAI;
        //bool searchingForEnemy = Enemy.Events.OnSearch.Value;
        float distance = Enemy.RealDistance;

        float maxDelay = isAI ? MAX_FREQ_CALCPATH_AI : MAX_FREQ_CALCPATH;
        if (currentEnemy)
            maxDelay *= CURRENTENEMY_COEF;
        if (performanceMode)
            maxDelay *= PERFORMANCE_MODE_COEF;
        //if (searchingForEnemy)
        //    maxDelay *= ACTIVE_SEARCH_COEF;

        if (distance > MAX_FREQ_CALCPATH_DISTANCE)
        {
            return maxDelay;
        }

        float minDelay = isAI ? MIN_FREQ_CALCPATH_AI : MIN_FREQ_CALCPATH;
        if (currentEnemy)
            minDelay *= CURRENTENEMY_COEF;
        if (performanceMode)
            minDelay *= PERFORMANCE_MODE_COEF;
        //if (searchingForEnemy)
        //    minDelay *= ACTIVE_SEARCH_COEF;

        if (distance < MIN_FREQ_CALCPATH_DISTANCE)
        {
            return minDelay;
        }

        float difference = distance - MIN_FREQ_CALCPATH_DISTANCE;
        float distanceRatio = difference / DISTANCE_DIFFERENCE;
        float delayDifference = maxDelay - minDelay;

        float result = distanceRatio * delayDifference + minDelay;
        float clampedResult = Mathf.Clamp(result, minDelay, maxDelay);

        //if (_nextLogTime < Time.time)
        //{
        //    _nextLogTime = Time.time + 10f;
        //    //Logger.LogDebug($"{BotOwner.name} calcPathFreqResults for [{Enemy.EnemyPerson.Nickname}] Result: [{result}] preClamped: [[{result}] [{distanceRatio} * {delayDifference} + {minDelay}]] : Distance: [{distance}] : IsAI? [{isAI}] : Current Enemy? [{currentEnemy}] : MinDelay [{minDelay}] : MaxDelay [{maxDelay}]");
        //}

        return clampedResult;
    }

    private const float MAX_FREQ_CALCPATH = 3f;
    private const float MAX_FREQ_CALCPATH_AI = 6f;
    private const float MAX_FREQ_CALCPATH_DISTANCE = 250f;

    private const float MIN_FREQ_CALCPATH = 1f;
    private const float MIN_FREQ_CALCPATH_AI = 2f;
    private const float MIN_FREQ_CALCPATH_DISTANCE = 25f;

    private const float DISTANCE_DIFFERENCE = MAX_FREQ_CALCPATH_DISTANCE - MIN_FREQ_CALCPATH_DISTANCE;
    private const float PERFORMANCE_MODE_COEF = 1.5f;
    private const float CURRENTENEMY_COEF = 0.25f;

    private bool isEnemyInRange()
    {
        return Enemy.IsAI && Enemy.RealDistance <= MAX_CALCPATH_RANGE_AI ||
            !Enemy.IsAI && Enemy.RealDistance <= MAX_CALCPATH_RANGE;
    }

    private const float MAX_CALCPATH_RANGE = 500f;
    private const float MAX_CALCPATH_RANGE_AI = 300f;

    private bool checkPositionsChanged(Vector3 botPosition, Vector3 enemyPosition)
    {
        // Did we already check the current enemy position and has the bot not moved? dont recalc path then
        if (_enemyLastPosChecked != null
            && (_enemyLastPosChecked.Value - enemyPosition).sqrMagnitude < 0.1f
            && (_botLastPosChecked - botPosition).sqrMagnitude < 0.1f)
        {
            return false;
        }

        // cache the positions we are currently checking
        _enemyLastPosChecked = enemyPosition;
        _botLastPosChecked = botPosition;
        return true;
    }

    private Vector3? _enemyLastPosChecked;
    private Vector3 _botLastPosChecked;
    private float _calcPathTime = 0f;
}