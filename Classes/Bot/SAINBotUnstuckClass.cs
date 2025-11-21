using Comfort.Common;
using EFT;
using SAIN.Components;
using SAIN.Preset.GlobalSettings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Debug;

public class SAINBotUnstuckClass : BotComponentClassBase
{
    public SAINBotUnstuckClass(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyBotActive;
    }

    public override void Init()
    {
        PathController = BotOwner.Mover.ActualPathController;
        DontUnstuckMe = DontUnstuckTheseTypes.Contains(Bot.Info.Profile.WildSpawnType);
        base.Init();
    }

    private void CheckIfPositionChanged()
    {
        if (_checkPositionTimer < Time.time)
        {
            _checkPositionTimer = Time.time + 0.5f;

            bool botChangedPositionLast = BotHasChangedPosition;

            const float DistThreshold = 0.1f;
            BotHasChangedPosition = (_lastPos - Bot.Position).sqrMagnitude > DistThreshold * DistThreshold;

            if (botChangedPositionLast && !BotHasChangedPosition)
            {
                TimeStartedChangingPosition = Time.time;
            }
            else if (BotHasChangedPosition)
            {
                TimeStartedChangingPosition = 0f;
            }

            _lastPos = Bot.Position;
        }
    }

    private void Teleport(Vector3 position)
    {
        Player.Teleport(position + Vector3.up * 0.25f);
#if DEBUG
        if (SAINPlugin.DebugMode)
            Logger.LogDebug($"{BotOwner.name} has teleported because they were stuck after vaulting, and no human players are visible to them, and no human players are close.");
#endif
        BotOwner.Mover?.Stop();
        BotOwner.Mover?.RecalcWay();
    }

    private bool IsHumanVisible() => Bot.EnemyController.HumanEnemyInLineofSight;

    private bool IsHumanClose()
    {
        bool closeHuman = false;
        foreach (var player in Singleton<GameWorld>.Instance.AllAlivePlayersList)
        {
            if (player != null
                && !player.IsAI
                && player.HealthController.IsAlive
                && (player.Position - Bot.Position).sqrMagnitude < 50f * 50f)
            {
                closeHuman = true;
                break;
            }
        }
        return closeHuman;
    }

    private bool _botStuckAfterVault;


    private bool TryVault()
    {
        if (!Bot.Info.FileSettings.Move.VAULT_UNSTUCK_TOGGLE || !GlobalSettingsClass.Instance.Move.VAULT_UNSTUCK_TOGGLE)
        {
            return false;
        }
        if (Bot.Mover.TryVault())
        {
            _botVaultedTime = Time.time;
            return true;
        }
        return false;
    }

    private float _nextVaultCheckTime;
    private bool DontUnstuckMe;

    private static readonly List<WildSpawnType> DontUnstuckTheseTypes = new()
    {
        WildSpawnType.marksman,
        WildSpawnType.shooterBTR,
    };

    private void checkResetPathFromVault()
    {
        if (_botVaulted
            && !_botStuckAfterVault
            && _botVaultedTime + 1f < Time.time)
        {
            _botVaulted = false;
            Bot.Mover.RecalcPath();
        }
    }

    private bool _botVaulted;
    private float _botVaultedTime;

    private void TryAutoVault()
    {
        if (!Bot.Info.FileSettings.Move.VAULT_TOGGLE || !GlobalSettingsClass.Instance.Move.VAULT_TOGGLE)
        {
            return;
        }
        if (_nextVaultCheckTime < Time.time
            && (BotOwner?.Mover?.IsMoving == true || Bot.Mover.Moving))
        {
            float timeAdd;
            Vector3 lookDir = Player.LookDirection.normalized;
            Vector3 targetDir = BotOwner.Mover.NormDirCurPoint;
            if (Vector3.Dot(lookDir, targetDir) > 0.85f && TryVault())
            {
                _botVaulted = true;
                timeAdd = 2f;
            }
            else
            {
                timeAdd = 0.5f;
            }
            _nextVaultCheckTime = Time.time + timeAdd;
        }
    }

    public override void ManualUpdate()
    {
        if (!DontUnstuckMe && !Bot.BotActivation.BotInStandBy)
        {
            StartCoroutine();
        }
        else if (_botUnstuckCoroutine != null)
        {
            Bot.StopCoroutine(_botUnstuckCoroutine);
        }
        base.ManualUpdate();
    }

    private void StartCoroutine()
    {
        if (_botUnstuckCoroutine == null)
        {
            _botUnstuckCoroutine = Bot.StartCoroutine(BotUnstuck());
        }
    }

    private Coroutine _botUnstuckCoroutine;

    private IEnumerator BotUnstuck()
    {
        while (true)
        {
            //if (Bot.BotActive
            //&& !Bot.GameEnding)
            //{
            //    tryAutoVault();
            //    checkResetPathFromVault();
            //}
            yield return null;
        }
    }

    private const float _minDistance = 100f;
    private const float _maxDistance = 300f;
    private const float _pathLengthCoef = 1.25f;
    private const float _minDistancePathLength = _minDistance * _pathLengthCoef;

    //private Coroutine TeleportCoroutine;

    private IEnumerator CheckIfTeleport()
    {
        bool shallTeleport = true;
        var humanPlayers = GetHumanPlayers();

        Vector3? teleportDestination = null;

        const float minTeleDist = 1f;
        if (BotOwner.Mover.HasPathAndNoComplete)
        {
            for (int i = PathController.CurPath.CurIndex; i < PathController.CurPath.Length - 1; i++)
            {
                Vector3 corner = PathController.CurPath.GetPoint(i);
                Vector3 cornerDirection = corner - Bot.Position;
                float cornerDistance = cornerDirection.sqrMagnitude;
                if (cornerDirection.sqrMagnitude >= minTeleDist * minTeleDist)
                {
                    teleportDestination = new Vector3?(corner);
                    break;
                }
                yield return null;
            }
        }

        Vector3 botPosition = Bot.Position;

        if (teleportDestination != null)
        {
            var allPlayers = Singleton<GameWorld>.Instance?.AllAlivePlayersList;
            if (allPlayers != null)
            {
                foreach (var player in allPlayers)
                {
                    if (ShallCheckPlayer(player))
                    {
                        if (!BotIsStuck)
                        {
                            shallTeleport = false;
                            yield break;
                        }

                        Vector3 playerPosition = player.Position;

                        // Makes sure the bot isn't too close to a human for them to hear
                        float sqrMag = (playerPosition - botPosition).sqrMagnitude;
                        if (sqrMag < _minDistance * _minDistance)
                        {
                            shallTeleport = false;
                            break;
                        }

                        // Checks the max distance to do a path calculation
                        if (sqrMag < _maxDistance * _maxDistance)
                        {
                            NavMeshPath path = CalcPath(botPosition, playerPosition, out float pathLength);
                            if (CheckPathLength(playerPosition, path, pathLength) == false)
                            {
                                shallTeleport = false;
                                break;
                            }
                        }

                        // Check next player on the next frame
                        yield return null;
                    }
                }
            }
        }

        IsTeleporting = BotIsStuck && shallTeleport && teleportDestination != null;

        if (IsTeleporting)
        {
            TeleportWithLimit(teleportDestination.Value + Vector3.up * 0.25f);
            float distance = (teleportDestination.Value - botPosition).magnitude;
            Logger.LogDebug($"Teleporting stuck bot: [{Player.name}] [{distance}] meters to the next corner they are trying to go to");
            _botStuckAfterVault = false;
            BotIsStuck = false;
        }

        yield return null;
    }

    private bool IsTeleporting;

    private bool ShallCheckPlayer(Player player)
    {
        if (Player == null || Player.HealthController == null || Player.AIData == null)
        {
            return false;
        }
        return Player.HealthController.IsAlive == true && Player.AIData.IsAI == false;
    }

    private void TeleportWithLimit(Vector3 position)
    {
        if (teleportTimer < Time.time)
        {
            teleportTimer = Time.time + 3f;
            Player.Teleport(position);
        }
    }

    private float teleportTimer;

    public PathControllerClass PathController { get; private set; }

    private static NavMeshPath CalcPath(Vector3 start, Vector3 end, out float pathLength)
    {
        if (_pathToPlayer == null)
        {
            _pathToPlayer = new NavMeshPath();
        }
        if (NavMesh.SamplePosition(end, out NavMeshHit hit, 1f, -1))
        {
            _pathToPlayer.ClearCorners();
            if (NavMesh.CalculatePath(start, hit.position, -1, _pathToPlayer))
            {
                pathLength = _pathToPlayer.CalculatePathLength();
                return _pathToPlayer;
            }
        }
        pathLength = 0f;
        return null;
    }

    private static bool CheckPathLength(Vector3 end, NavMeshPath path, float pathLength)
    {
        if (path == null)
        {
            return false;
        }
        if (path.status == NavMeshPathStatus.PathPartial)
        {
            Vector3 lastCorner = path.corners[path.corners.Length - 1];
            float sqrMag = (lastCorner - end).magnitude;
            float combinedLength = sqrMag + pathLength;
            if (combinedLength < _minDistancePathLength)
            {
                return false;
            }
        }
        if (path.status == NavMeshPathStatus.PathComplete && pathLength < _minDistancePathLength)
        {
            return false;
        }
        return path.status != NavMeshPathStatus.PathInvalid;
    }

    private List<Player> GetHumanPlayers()
    {
        _humanPlayers.Clear();
        var allPlayers = Singleton<GameWorld>.Instance?.AllAlivePlayersList;
        if (allPlayers != null)
        {
            foreach (var player in allPlayers)
            {
                if (player != null && player.AIData.IsAI == false && player.HealthController.IsAlive)
                {
                    _humanPlayers.Add(player);
                }
            }
        }
        return _humanPlayers;
    }

    private static NavMeshPath _pathToPlayer;
    private readonly List<Player> _humanPlayers = [];
    public float TimeSinceStuck => Time.time - TimeStuck;
    public float TimeStuck { get; }

    private float _checkPositionTimer = 0f;

    private Vector3 _lastPos = Vector3.zero;

    public float TimeSpentNotMoving => Time.time - TimeStartedChangingPosition;

    public float TimeStartedChangingPosition { get; private set; }

    public bool BotIsStuck { get; private set; }

    public bool BotHasChangedPosition { get; private set; }

}