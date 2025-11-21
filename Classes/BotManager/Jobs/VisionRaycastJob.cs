using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Models.Structs;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components;

public class VisionRaycastJob : BotManagerBase
{
    private const float VISION_UPDATE_INTERVAL = 1f / 30f;
    private const float VISION_JOB_INTERVAL = 1f / 30f;

    public VisionRaycastJob(BotManagerComponent botcontroller) : base(botcontroller)
    {
        botcontroller.StartCoroutine(EnemyVisionJob());
        botcontroller.StartCoroutine(UpdateEFTVision());
        //botcontroller.StartCoroutine(EnemyLoSChecker());
    }

    private readonly List<JobHandle> _handles = [];

    /// <summary>
    /// Test
    /// </summary>
    private IEnumerator EnemyLoSChecker()
    {
        WaitForSeconds waitDelay = new(1f / 20f);
        while (BotController != null)
        {
            yield return waitDelay;
            HashSet<BotComponent> bots = BotController?.BotSpawnController?.SAINBots;
            if (bots != null && bots.Count > 0)
            {
                foreach (BotComponent bot in bots)
                {
                    Vector3 eyePosition = bot.Transform.EyePosition;
                    var otherPlayers = bot.PlayerComponent.OtherPlayersData.DataList;
                    foreach (var otherPlayer in otherPlayers)
                    {
                        _handles.Add(otherPlayer.LoSData.ScheduleRaycasts(eyePosition, _losParams));
                    }
                }
                if (_handles.Count > 0)
                {
                    yield return null;
                    for (int i = 0; i < _handles.Count; i++)
                    {
                        _handles[i].Complete();
                    }
                    _handles.Clear();
                    foreach (BotComponent bot in bots)
                    {
                        var otherPlayers = bot.PlayerComponent.OtherPlayersData.DataList;
                        foreach (var otherPlayer in otherPlayers)
                        {
                            otherPlayer.LoSData.ReadLoSRaycastResults();
                        }
                    }
                }
            }
        }
    }

    private IEnumerator EnemyVisionJob()
    {
        //WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        WaitForSeconds wait = new(VISION_JOB_INTERVAL);
        yield return wait;
        while (BotController != null)
        {
            HashSet<BotComponent> bots = BotController.BotSpawnController?.SAINBots;
            if (bots != null && bots.Count > 0)
            {
                FindEnemies(bots, _enemies);
                int enemyCount = _enemies.Count;
                if (enemyCount > 0)
                {
                    int partCount = _enemies[0].Vision.EnemyParts.PartsArray.Length;
                    int totalRaycasts = enemyCount * partCount * RAYCAST_CHECKS;

                    _hits = new NativeArray<RaycastHit>(totalRaycasts, Allocator.TempJob);
                    _commands = new NativeArray<RaycastCommand>(totalRaycasts, Allocator.TempJob);

                    CreateCommands(_commands, enemyCount, partCount);
                    _handle = RaycastCommand.ScheduleBatch(_commands, _hits, 32);

                    yield return null;

                    var handle = _handle;
                    handle.Complete();
                    _handle = handle;

                    AnalyzeHits(_hits, enemyCount, partCount);
                    _commands.Dispose();
                    _hits.Dispose();
                }
            }

            yield return wait;
        }
    }

    private IEnumerator UpdateEFTVision()
    {
        WaitForSeconds wait = new(VISION_UPDATE_INTERVAL);
        WaitForFixedUpdate waitForFixedUpdate = new();
        yield return wait;
        while (BotController != null)
        {
            yield return waitForFixedUpdate;
            HashSet<BotComponent> botGroup1 = BotController.BotSpawnController.BotGroup1;
            if (botGroup1.Count > 0)
            {
                float currentTime = Time.time;
                foreach (var bot in botGroup1) bot.Vision.BotLook.UpdateLook(currentTime);
            }
            yield return null;

            yield return waitForFixedUpdate;
            HashSet<BotComponent> botGroup2 = BotController.BotSpawnController.BotGroup2;
            if (botGroup2.Count > 0)
            {
                float currentTime = Time.time;
                foreach (var bot in botGroup2) bot.Vision.BotLook.UpdateLook(currentTime);
            }
            yield return null;
        }
    }

    public void Dispose()
    {
        if (!_handle.IsCompleted) _handle.Complete();
        if (_commands.IsCreated) _commands.Dispose();
        if (_hits.IsCreated) _hits.Dispose();
    }

    private NativeArray<RaycastHit> _hits;
    private NativeArray<RaycastCommand> _commands;
    private JobHandle _handle;

    private void CreateCommands(NativeArray<RaycastCommand> raycastCommands, int enemyCount, int partCount)
    {
        _colliderTypes.Clear();
        _castPoints.Clear();

        int commands = 0;
        for (int i = 0; i < enemyCount; i++)
        {
            var enemy = _enemies[i];
            var botTransform = enemy.Bot.Transform;
            Vector3 eyePosition = botTransform.EyePosition;
            Vector3 weaponFirePort = botTransform.WeaponData.FirePort;
            var parts = enemy.Vision.EnemyParts.PartsArray;

            for (int j = 0; j < partCount; j++)
            {
                var part = parts[j];

                SAINBodyPartRaycast raycastData = part.GetRaycast();
                Vector3 castPoint = raycastData.CastPoint;

                _colliderTypes.Add(raycastData.ColliderType);
                _castPoints.Add(castPoint);

                Vector3 weaponDir = castPoint - weaponFirePort; // we should normalize this, however, setting the magnitude to 1f allows us to skip that
                Vector3 eyeDir = castPoint - eyePosition;

                raycastCommands[commands] = new RaycastCommand(eyePosition, eyeDir, _losParams, 1f);
                commands++;

                raycastCommands[commands] = new RaycastCommand(eyePosition, eyeDir, _visParams, 1f);
                commands++;

                raycastCommands[commands] = new RaycastCommand(weaponFirePort, weaponDir, _shootParams, 1f);
                commands++;
            }
        }
    }

    private static readonly QueryParameters _losParams = new(LayerMaskClass.HighPolyWithTerrainNoGrassMask);

    private static readonly QueryParameters _visParams = new(LayerMaskClass.AI);

    private static readonly QueryParameters _shootParams = new(LayerMaskClass.HighPolyWithTerrainNoGrassMask);

    private void AnalyzeHits(NativeArray<RaycastHit> raycastHits, int enemyCount, int partCount)
    {
        float time = Time.time;
        int hits = 0;
        int colliderTypeCount = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            var enemy = _enemies[i];
            var parts = enemy.Vision.EnemyParts.PartsArray;
            for (int j = 0; j < partCount; j++)
            {
                var part = parts[j];
                EBodyPartColliderType colliderType = _colliderTypes[colliderTypeCount];
                Vector3 castPoint = _castPoints[colliderTypeCount];
                colliderTypeCount++;

                part.SetLineOfSight(castPoint, colliderType, raycastHits[hits], ERaycastCheck.LineofSight, time);
                hits++;
                part.SetLineOfSight(castPoint, colliderType, raycastHits[hits], ERaycastCheck.Vision, time);
                hits++;
                part.SetLineOfSight(castPoint, colliderType, raycastHits[hits], ERaycastCheck.Shoot, time);
                hits++;
            }
        }
#if DEBUG
        if (DebugSettings.Instance.Gizmos.DrawLineOfSightGizmos)
        {
            hits = 0;

            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = _enemies[i];
                var parts = enemy.Vision.EnemyParts.PartsArray;

                for (int j = 0; j < partCount; j++)
                {
                    var part = parts[j];
                    EBodyPartColliderType colliderType = _colliderTypes[colliderTypeCount];
                    Vector3 castPoint = _castPoints[colliderTypeCount];
                    colliderTypeCount++;

                    if (raycastHits[hits].collider == null)
                    {
                        DebugGizmos.DrawLine(_commands[hits].from, _commands[hits].from + _commands[hits].direction, Color.red, 0.025f, 0.02f);
                    }
                    hits++;
                    //if (raycastHits[hits].collider == null)
                    //{
                    //}
                    hits++;
                    //if (raycastHits[hits].collider == null)
                    //{
                    //}
                    hits++;
                }
            }
        }
#endif
    }

    private const int RAYCAST_CHECKS = 3;

    private readonly List<EBodyPartColliderType> _colliderTypes = [];
    private readonly List<Vector3> _castPoints = [];

    private static void FindEnemies(HashSet<BotComponent> bots, List<Enemy> enemies)
    {
        float currentTime = Time.time;
        enemies.Clear();
        foreach (BotComponent bot in bots)
            if (bot != null)
                foreach (Enemy enemy in bot.EnemyController.EnemiesArray)
                    if (enemy.ShallCheckLoS(currentTime))
                        enemies.Add(enemy);
    }

    private readonly List<Enemy> _enemies = [];
}