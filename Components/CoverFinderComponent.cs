using EFT;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components.CoverFinder;

public class CoverFinderComponent : BotComponentBase
{
    private const float FIND_COVER_INTERVAL = 1f / 10f;

    public ECoverFinderStatus CurrentStatus { get; private set; }

    public BotComponent Bot { get; private set; }
    public List<CoverPoint> CoverPoints { get; } = [];
    private CoverAnalyzer CoverAnalyzer { get; set; }
    public List<SpottedCoverPoint> SpottedCoverPoints { get; private set; } = [];

    public void Init(BotComponent bot)
    {
        base.Init(bot.PlayerComponent, bot.BotOwner);
        Bot = bot;

        CoverAnalyzer = new CoverAnalyzer(bot, this);

        bot.BotActivation.BotActiveToggle.OnToggle += botEnabled;
        bot.BotActivation.BotStandByToggle.OnToggle += botInStandBy;
        bot.OnDispose += botDisposed;
    }

#if DEBUG
    public void Update()
    {
        if (SAINPlugin.LoadedPreset.GlobalSettings.General.Cover.DebugCoverFinder &&
            CoverPoints.Count > 0)
        {
            DebugGizmos.DrawLine(CoverPoints.PickRandom().Position, Bot.Transform.EyePosition, Color.yellow, 0.05f, 0.1f);
        }
    }
#endif

    public override void Dispose()
    {
        base.Dispose();
        StopLooking();
        StopAllCoroutines();
        if (Bot != null)
        {
            Bot.OnDispose -= botDisposed;
            Bot.BotActivation.BotActiveToggle.OnToggle -= botEnabled;
            Bot.BotActivation.BotStandByToggle.OnToggle -= botInStandBy;
        }
        Destroy(this);
    }

    private void botInStandBy(bool value)
    {
        if (value)
        {
            ToggleCoverFinder(false);
        }
    }

    private void botEnabled(bool value)
    {
        if (!value)
            ToggleCoverFinder(false);
    }

    public void ToggleCoverFinder(bool value)
    {
        switch (value)
        {
            case true:
                LookForCover();
                break;

            case false:
                StopLooking();
                break;
        }
    }

    public void LookForCover()
    {
        _findCoverPointsCoroutine ??= StartCoroutine(FindCoverLoop());
        //if (_recheckCoverPointsCoroutine == null)
        //{
        //    _recheckCoverPointsCoroutine = StartCoroutine(RecheckCoverLoop());
        //}
    }

    public void StopLooking()
    {
        if (_findCoverPointsCoroutine != null)
        {
            CurrentStatus = ECoverFinderStatus.None;
            StopCoroutine(_findCoverPointsCoroutine);
            _findCoverPointsCoroutine = null;
            CoverPoints.Clear();
        }
    }

    private float _nextGetCollidersTime;
    private readonly SainBotCoverData CoverData = new();

    private IEnumerator FindCoverLoop()
    {
        WaitForSeconds wait = new(FIND_COVER_INTERVAL);
        while (Bot != null)
        {
            Enemy enemy = Bot.EnemyController.GoalEnemy;
            if (enemy != null)
            {
                int max = 5;
                CurrentStatus = ECoverFinderStatus.SearchingColliders;

                if (_nextGetCollidersTime < Time.time)
                {
                    _nextGetCollidersTime = Time.time + 4;
                    SainBotCoverData.BotColliderQueryParams queryParams = new() {
                        origin = Bot.NavMeshPosition + Vector3.up * 0.25f,
                        halfExtents = new Vector3(35, 5, 35),
                        mask = LayerMaskClass.HighPolyWithTerrainNoGrassMask,
                        minColliderSize = new(0.25f, GlobalSettingsClass.Instance.General.Cover.CoverMinHeight, 0.25f),
                        maxColliderSize = new(30f, 30f, 30f)
                    };
                    CoverData.OverlapBoxAndFilter(queryParams);
                }
                CoverData.HandleLists(Bot.NavMeshPosition);

                _totalChecked = 0;

                CoverPoints.Clear();
                CurrentStatus = ECoverFinderStatus.SearchingColliders;
                List<SainBotColliderData> validCollders = CoverData.ValidCollidersList;
                int validCount = validCollders.Count;
                for (int i = 0; i < validCount; i++)
                {
                    SainBotColliderData colliderData = validCollders[i];
                    Collider collider = colliderData.Collider;
                    if (collider == null)
                    {
#if DEBUG
                        Logger.LogDebug("collider null");
#endif
                        continue;
                    }
                    _totalChecked++;
                    enemy = Bot.EnemyController.GoalEnemy;
                    if (enemy == null || enemy.LastKnownPosition == null)
                    {
#if DEBUG
                        Logger.LogDebug("enemy null");
#endif
                        break;
                    }
                    Vector3 targetPosition = enemy.LastKnownPosition.Value;
                    Vector3 botPosition = Bot.NavMeshPosition;
                    Vector3 targetDirNormal = (targetPosition - botPosition).normalized;
                    if (colliderData.CoverPoint != null)
                    {
                        if (colliderData.CoverPoint.ShallUpdate(enemy.EnemyProfileId) &&
                            !CoverAnalyzer.RecheckCoverPoint(colliderData.CoverPoint, targetPosition, targetDirNormal, botPosition, out _))
                        {
                            colliderData.CoverPoint.CoverData.IsBad = true;
                        }
                        else
                        {
                            colliderData.CoverPoint.CoverData.IsBad = false;
                            CoverPoints.Add(colliderData.CoverPoint);
                        }
                    }
                    else if (CoverAnalyzer.CheckCreateNewCoverPoint(collider, targetPosition, botPosition, targetDirNormal, out CoverPoint newPoint, out _))
                    {
                        CoverPoints.Add(newPoint);
                        colliderData.CoverPoint = newPoint;
                        CoverData.ValidCollidersList[i] = colliderData;
                    }
                    if (CoverPoints.Count >= max) break;
                    yield return null;
                }
                sort(CoverPoints.Count, CoverPoints);
#if DEBUG
                log(CoverPoints.Count);
#endif
            }
            CurrentStatus = ECoverFinderStatus.None;
            yield return wait;
        }
    }

    private void sort(int coverCount, List<CoverPoint> points)
    {
        if (coverCount < 2) return;
        OrderPointsByPathDist(points);
    }

    private void log(int coverCount)
    {
        if (!DebugCoverFinder)
        {
            return;
        }

        if (_debugLogTimer < Time.time)
        {
            _debugLogTimer = Time.time + 1f;
            if (coverCount > 0)
                Logger.LogInfo($"[{BotOwner.name}] - Found [{coverCount}] CoverPoints. Colliders checked: [{_totalChecked}] Collider Array Size = [{CoverData.ValidCollidersList.Count}]");
            else
                Logger.LogWarning($"[{BotOwner.name}] - No Cover Found! Valid Colliders checked: [{_totalChecked}] Collider Array Size = [{CoverData.ValidCollidersList.Count}]");
        }
    }

    public static void OrderPointsByPathDist(List<CoverPoint> points)
    {
        points.Sort((x, y) => x.PathData.PathLength.CompareTo(y.PathData.PathLength));
    }

    public void OnDestroy()
    {
        StopLooking();
        StopAllCoroutines();
    }

    private void botDisposed()
    {
        Dispose();
    }

    private int _totalChecked;
    private float _debugLogTimer = 0f;
    private Coroutine _findCoverPointsCoroutine;

    public static bool PerformanceMode { get; private set; } = false;
    public static float CoverMinHeight { get; private set; } = 0.5f;
    public static float CoverMinEnemyDist { get; private set; } = 5f;
    public static float CoverMinEnemyDistSqr { get; private set; } = 25f;
    public static bool DebugCoverFinder { get; private set; } = false;

    static CoverFinderComponent()
    {
        PresetHandler.OnPresetUpdated += updateSettings;
        updateSettings(SAINPresetClass.Instance);
    }

    private static void updateSettings(SAINPresetClass preset)
    {
        PerformanceMode = SAINPlugin.LoadedPreset.GlobalSettings.General.Performance.PerformanceMode;
        CoverMinHeight = SAINPlugin.LoadedPreset.GlobalSettings.General.Cover.CoverMinHeight;
        CoverMinEnemyDist = SAINPlugin.LoadedPreset.GlobalSettings.General.Cover.CoverMinEnemyDistance;
        CoverMinEnemyDistSqr = CoverMinEnemyDist * CoverMinEnemyDist;
        DebugCoverFinder = SAINPlugin.LoadedPreset.GlobalSettings.General.Cover.DebugCoverFinder;
    }
}