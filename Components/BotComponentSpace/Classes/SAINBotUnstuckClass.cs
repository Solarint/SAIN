using Comfort.Common;
using EFT;
using HarmonyLib;
using SAIN.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Debug
{
    public class SAINBotUnstuckClass : BotBase, IBotClass
    {
        static SAINBotUnstuckClass()
        {
            _pathControllerField = AccessTools.Field(typeof(BotMover), "_pathController");
        }

        private static readonly FieldInfo _pathControllerField;

        public SAINBotUnstuckClass(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
            base.SubscribeToPreset(null);
            PathController = _pathControllerField.GetValue(BotOwner.Mover) as PathControllerClass;
            DontUnstuckMe = DontUnstuckTheseTypes.Contains(Bot.Info.Profile.WildSpawnType);
        }

        public bool BotIsMoving { get; private set; }
        private bool _botIsMoving;

        public float TimeStartedMoving { get; private set; }
        public float TimeStoppedMoving { get; private set; }

        private float _timeStartMoving;

        private float _timeStopMoving;
        public float TimeSinceMovingStarted => TimeStartedMoving - Time.time;

        private void CheckIfMoving()
        {
            float time = Time.time;

            // Check if a bot is being told to move by the botowner pathfinder, and record the time this starts and stops
            bool botWasMoving = _botIsMoving;
            _botIsMoving = BotOwner.Mover.IsMoving || Bot.Mover.SprintController.Running;
            if (_botIsMoving && !botWasMoving)
            {
                _timeStartMoving = time;
            }
            else if (!_botIsMoving && botWasMoving)
            {
                _timeStopMoving = time;
            }

            // How long a bot must be "moving" for before we check
            const float movingForPeriod = 0.25f;

            // Has the bot been told to move for over x seconds? then record the state for use in other classes
            if (_botIsMoving && time - _timeStartMoving > movingForPeriod)
            {
                if (!BotIsMoving)
                {
                    TimeStartedMoving = time;
                }
                BotIsMoving = true;
            }
            else if (!_botIsMoving && time - _timeStopMoving > movingForPeriod)
            {
                if (BotIsMoving)
                {
                    TimeStoppedMoving = time;
                }
                BotIsMoving = false;
            }
        }

        private bool checkFixOffMeshBot()
        {
            if (_nextCheckNavMeshTime < Time.time)
            {
                _nextCheckNavMeshTime = Time.time + 1f;
                bool wasOnNavMesh = _isOnNavMesh;
                _isOnNavMesh = CheckBotIsOnNavMesh();

                if (!_isOnNavMesh)
                {
                    if (wasOnNavMesh)
                    {
                        _timeOffMeshStart = Time.time;
                    }
                    if (Time.time - _timeOffMeshStart > 3f && _nextResetTime < Time.time)
                    {
                        _nextResetTime = Time.time + 5f;
                        //Logger.LogWarning($"{BotOwner.name} is off navmesh! Trying to fix...");
                        Bot.Mover.ResetPath(0.33f);
                    }
                }
                if (_isOnNavMesh)
                {
                    if (_timeOffMeshStart > 0)
                    {
                        _timeOffMeshStart = -1f;
                    }
                }
            }
            return _isOnNavMesh;
        }

        private float _timeOffMeshStart;
        private float _nextResetTime;

        public Vector2 findMoveDirection(Vector3 direction)
        {
            Vector2 v = new Vector2(direction.x, direction.z);
            Vector3 vector = Quaternion.Euler(0f, 0f, Player.Rotation.x) * v;
            vector = Helpers.Vector.NormalizeFastSelf(vector);
            return new Vector2(vector.x, vector.y);
        }

        private bool _isOnNavMesh;
        private float _nextCheckNavMeshTime;

        private bool CheckBotIsOnNavMesh()
        {
            return NavMesh.SamplePosition(Bot.Position, out _, 0.25f, -1);
        }

        private void CheckIfPositionChanged()
        {
            if (CheckPositionTimer < Time.time)
            {
                CheckPositionTimer = Time.time + 0.5f;

                bool botChangedPositionLast = BotHasChangedPosition;

                const float DistThreshold = 0.1f;
                BotHasChangedPosition = (LastPos - Bot.Position).sqrMagnitude > DistThreshold * DistThreshold;

                if (botChangedPositionLast && !BotHasChangedPosition)
                {
                    TimeStartedChangingPosition = Time.time;
                }
                else if (BotHasChangedPosition)
                {
                    TimeStartedChangingPosition = 0f;
                }

                LastPos = Bot.Position;
            }
        }

        private IEnumerator trackPostVault(Vector3 preVaultPosition)
        {
            WaitForSeconds wait = new WaitForSeconds(1f);
            yield return wait;

            if (Bot == null || BotOwner == null || Player == null || !Player.HealthController.IsAlive)
                yield break;

            if (NavMesh.SamplePosition(preVaultPosition, out var hit1, 0.5f, -1))
            {
                preVaultPosition = hit1.position;
            }

            NavMeshPath path = new NavMeshPath();
            float startTime = Time.time;
            bool botIsStuck = true;

            while (botIsStuck)
            {
                if (Bot == null || BotOwner == null || Player == null || !Player.HealthController.IsAlive)
                    break;

                botIsStuck = isStuck(preVaultPosition);
                if (!botIsStuck)
                {
                    break;
                }
                _botStuckAfterVault = botIsStuck;
                //Logger.LogWarning($"{BotOwner.name} has vaulted to somewhere they can't get down from! Trying to fix...");

                if (!isHumanVisible() && 
                    !isHumanClose())
                {
                    teleport(preVaultPosition);
                    break;
                }
                yield return wait;
            }
            _botStuckAfterVault = false;
            yield break;
        }

        private bool isStuck(Vector3 targetPosition)
        {
            NavMeshPath path = new NavMeshPath();
            return !NavMesh.SamplePosition(Bot.Position, out var hit, 0.5f, -1)
                || !NavMesh.CalculatePath(hit.position, targetPosition, -1, path)
                || path.status != NavMeshPathStatus.PathComplete;
        }

        private void teleport(Vector3 position)
        {
            Player.Teleport(position + Vector3.up * 0.25f);
            Logger.LogWarning($"{BotOwner.name} has teleported because they were stuck after vaulting, and no human players are visible to them, and no human players are close.");
            BotOwner.Mover?.Stop();
            BotOwner.Mover?.RecalcWay();
        }

        private bool isHumanVisible() => Bot.EnemyController.HumanEnemyInLineofSight;

        private bool isHumanClose()
        {
            bool closeHuman = false;
            var allPlayers = Singleton<GameWorld>.Instance.AllAlivePlayersList;
            foreach (var player in allPlayers)
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

        private Coroutine postVaultTracker;

        private bool tryVault()
        {
            Vector3 currentPos = Bot.Position;
            if (Bot.Mover.TryVault())
            {
                _botVaultedTime = Time.time;
                if (postVaultTracker != null)
                {
                    Bot.StopCoroutine(postVaultTracker);
                    _botStuckAfterVault = false;
                }
                postVaultTracker = Bot.StartCoroutine(trackPostVault(currentPos));
                return true;
            }
            return false;
        }

        private float _nextVaultCheckTime;
        private bool DontUnstuckMe;

        private static readonly List<WildSpawnType> DontUnstuckTheseTypes = new List<WildSpawnType>
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
                Bot.Mover.ResetPath(0.1f);
            }
        }

        private bool _botVaulted;
        private float _botVaultedTime;

        private void tryAutoVault()
        {
            if (_nextVaultCheckTime < Time.time
                && (BotOwner?.Mover?.IsMoving == true || Bot.Mover.SprintController.Running))
            {
                float timeAdd;
                Vector3 lookDir = Player.LookDirection.normalized;
                Vector3 targetDir = BotOwner.Mover.NormDirCurPoint;
                if (Vector3.Dot(lookDir, targetDir) > 0.85f && tryVault())
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

        public void Update()
        {
            if (!DontUnstuckMe && !Bot.BotActivation.BotInStandBy)
            {
                startCoroutine();
            }
            else if (botUnstuckCoroutine != null)
            {
                Bot.StopCoroutine(botUnstuckCoroutine);
            }
        }

        private void startCoroutine()
        {
            if (botUnstuckCoroutine == null)
            {
                botUnstuckCoroutine = Bot.StartCoroutine(botUnstuck());
            }
        }

        private Coroutine botUnstuckCoroutine;

        private IEnumerator botUnstuck()
        {
            while (true)
            {
                if (Bot.BotActive
                && !Bot.GameEnding)
                {
                    checkFixOffMeshBot();
                    tryAutoVault();
                    checkResetPathFromVault();
                    doStuckChecks();
                }
                yield return null;
            }
        }

        private void doStuckChecks()
        {
            CheckIfMoving();
            CheckIfPositionChanged();
            if (CheckStuckTimer < Time.time)
            {
                checkIfBotStuck();
                checkCancelUnstuck();
                tryFixStuckBot();
            }
        }

        private void checkIfBotStuck()
        {
            if (CheckStuckTimer < Time.time)
            {
                if (BotOwner.DoorOpener.Interacting)
                {
                    CheckStuckTimer = Time.time + 1f;
                    BotIsStuck = false;
                }
                else
                {
                    CheckStuckTimer = Time.time + 0.5f;

                    bool stuck = _botStuckAfterVault
                        || BotStuckGeneric()
                        || BotStuckOnObject()
                        || BotStuckOnPlayer();

                    if (!BotIsStuck && stuck)
                    {
                        TimeStuck = Time.time;
                    }
                    BotIsStuck = stuck;
                }
            }
        }

        private void checkCancelUnstuck()
        {
            // If the bot is no longer stuck, but we are checking if we can teleport them, cancel the coroutine
            if (!BotIsStuck
                && TeleportCoroutine != null)
            {
                Bot.StopCoroutine(TeleportCoroutine);
                HasTriedJumpOrVault = false;
                JumpTimer = Time.time + 1f;
                IsTeleporting = false;
            }
        }

        private void tryFixStuckBot()
        {
            if (BotIsStuck && TimeSinceStuck > 2f)
            {
                if (DebugStuckTimer < Time.time && TimeSinceStuck > 5f)
                {
                    DebugStuckTimer = Time.time + 10f;
                    Logger.LogWarning($"[{BotOwner.name}] has been stuck for [{TimeSinceStuck}] seconds " +
                        $"on [{StuckHit.transform?.name}] object " +
                        $"at [{StuckHit.transform?.position}] " +
                        $"with Current Decision as [{Bot.Decision.CurrentCombatDecision}]");
                }

                if (HasTriedJumpOrVault
                    && TimeSinceStuck > 6f
                    && TimeSinceTriedJumpOrVault + 2f < Time.time &&
                    !isHumanVisible() &&
                    !isHumanClose())
                {
                    TeleportCoroutine = Bot.StartCoroutine(CheckIfTeleport());
                }

                if (JumpTimer < Time.time && TimeSinceStuck > 2f)
                {
                    JumpTimer = Time.time + 1f;

                    if (!tryVault())
                    {
                        Bot.Mover.ResetPath(0.1f);
                        HasTriedJumpOrVault = true;
                        TimeSinceTriedJumpOrVault = Time.time;
                    }
                    else
                    {
                        _botVaulted = true;
                    }
                }
            }
        }

        private float TimeSinceTriedJumpOrVault;
        private bool HasTriedJumpOrVault;
        private const float MinDistance = 100f;
        private const float MaxDistance = 300f;
        private const float PathLengthCoef = 1.25f;
        private const float MinDistancePathLength = MinDistance * PathLengthCoef;

        private Coroutine TeleportCoroutine;

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
                            if (sqrMag < MinDistance * MinDistance)
                            {
                                shallTeleport = false;
                                break;
                            }

                            // Checks the max distance to do a path calculation
                            if (sqrMag < MaxDistance * MaxDistance)
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
                Teleport(teleportDestination.Value + Vector3.up * 0.25f);
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

        private void Teleport(Vector3 position)
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
            if (PathToPlayer == null)
            {
                PathToPlayer = new NavMeshPath();
            }
            if (NavMesh.SamplePosition(end, out NavMeshHit hit, 1f, -1))
            {
                PathToPlayer.ClearCorners();
                if (NavMesh.CalculatePath(start, hit.position, -1, PathToPlayer))
                {
                    pathLength = PathToPlayer.CalculatePathLength();
                    return PathToPlayer;
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
                if (combinedLength < MinDistancePathLength)
                {
                    return false;
                }
            }
            if (path.status == NavMeshPathStatus.PathComplete && pathLength < MinDistancePathLength)
            {
                return false;
            }
            return path.status != NavMeshPathStatus.PathInvalid;
        }

        private List<Player> GetHumanPlayers()
        {
            HumanPlayers.Clear();
            var allPlayers = Singleton<GameWorld>.Instance?.AllAlivePlayersList;
            if (allPlayers != null)
            {
                foreach (var player in allPlayers)
                {
                    if (player != null && player.AIData.IsAI == false && player.HealthController.IsAlive)
                    {
                        HumanPlayers.Add(player);
                    }
                }
            }
            return HumanPlayers;
        }

        private static NavMeshPath PathToPlayer;
        private List<Player> HumanPlayers = new List<Player>();

        public void Dispose()
        {
        }

        private RaycastHit StuckHit = new RaycastHit();
        private float DebugStuckTimer = 0f;
        private float CheckStuckTimer = 0f;
        public float TimeSinceStuck => Time.time - TimeStuck;
        public float TimeStuck { get; private set; }

        private float CheckPositionTimer = 0f;

        private Vector3 LastPos = Vector3.zero;

        public float TimeSpentNotMoving => Time.time - TimeStartedChangingPosition;

        public float TimeStartedChangingPosition { get; private set; }

        public bool BotIsStuck { get; private set; }

        private bool CanBeStuckDecisions(ECombatDecision decision)
        {
            return decision == ECombatDecision.Search || decision == ECombatDecision.MoveToCover || decision == ECombatDecision.DogFight || decision == ECombatDecision.RunToCover || decision == ECombatDecision.RunAway;
        }

        public bool BotStuckOnPlayer()
        {
            if (!BotHasChangedPosition && CanBeStuckDecisions(Bot.Decision.CurrentCombatDecision))
            {
                if (BotOwner.Mover == null)
                {
                    return false;
                }
                Vector3 botPos = BotOwner.Position;
                botPos.y += 0.4f;
                Vector3 moveDir = BotOwner.Mover.DirCurPoint;
                moveDir.y = 0;
                Vector3 lookDir = BotOwner.LookDirection;
                lookDir.y = 0;

                var moveHits = Physics.SphereCastAll(botPos, 0.15f, moveDir, 0.5f, LayerMaskClass.PlayerMask);
                if (moveHits.Length > 0)
                {
                    foreach (var move in moveHits)
                    {
                        if (move.transform.name != BotOwner.name)
                        {
                            StuckHit = move;
                            return true;
                        }
                    }
                }

                var lookHits = Physics.SphereCastAll(botPos, 0.15f, lookDir, 0.5f, LayerMaskClass.PlayerMask);
                if (lookHits.Length > 0)
                {
                    foreach (var look in lookHits)
                    {
                        if (look.transform.name != BotOwner.name)
                        {
                            StuckHit = look;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool BotStuckGeneric()
        {
            return BotIsMoving && !BotHasChangedPosition && !BotOwner.DoorOpener.Interacting && TimeSpentNotMoving > 2f;
        }

        public bool BotStuckOnObject()
        {
            if (CanBeStuckDecisions(Bot.Decision.CurrentCombatDecision) && 
                !BotHasChangedPosition && 
                !BotOwner.DoorOpener.Interacting && 
                Bot.Decision.TimeSinceChangeDecision > 1f)
            {
                if (BotOwner.Mover == null)
                {
                    return false;
                }
                Vector3 botPos = BotOwner.Position;
                botPos.y += 0.4f;
                Vector3 moveDir = BotOwner.Mover.DirCurPoint;
                moveDir.y = 0;
                if (Physics.SphereCast(botPos, 0.15f, moveDir, out var hit, 0.25f, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    StuckHit = hit;
                    return true;
                }
            }
            return false;
        }

        public bool BotHasChangedPosition { get; private set; }

        private float JumpTimer = 0f;
    }
}