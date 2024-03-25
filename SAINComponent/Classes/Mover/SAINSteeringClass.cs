using EFT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class SAINSteeringClass : SAINBase, ISAINClass
    {
        public SAINSteeringClass(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            if (SteerPriorityUpdateTick >= SteerPriorityUpdateFreq)
            {
                SteerPriorityUpdateTick = 0;
                //UpdateSteer();
            }
            SteerPriorityUpdateTick++;
        }

        private int SteerPriorityUpdateTick = 0;
        private static int SteerPriorityUpdateFreq = 5;

        private SteerPriority UpdateSteer()
        {
            LastSteerPriority = CurrentSteerPriority;
            CurrentSteerPriority = FindSteerPriority();
            return CurrentSteerPriority;
        }

        public void Dispose()
        {
        }

        public bool SteerRandomToggle;
        private PlaceForCheck LastHeardSound;

        public bool SteerByPriority(bool lookRandomifFalse = true)
        {
            UpdateSteer();

            SteerRandomToggle = lookRandomifFalse;

            HeardSoundSanityCheck();

            switch (CurrentSteerPriority)
            {
                case SteerPriority.None: 
                    break;
                case SteerPriority.Shooting:
                    break;
                case SteerPriority.Enemy: 
                    break;
                case SteerPriority.LastHit:
                    LookToLastHitPos();
                    break;
                case SteerPriority.UnderFire:
                    LookToUnderFirePos();
                    break;
                case SteerPriority.LastSeenEnemy:
                    LookToEnemyLastSeenPos();
                    break;
                case SteerPriority.Hear:
                    if (LastHeardSound != null)
                    {
                        LookToHearPos(LastHeardSound.Position);
                    }
                    else if (SAINPlugin.DebugMode)
                    {
                        Logger.LogError("Cannot look toward null PlaceForCheck.");
                    }
                    break;
                case SteerPriority.LastSeenEnemyLong:
                    LookToEnemyLastSeenPos();
                    break;
                case SteerPriority.MoveDirection:
                    LookToMovingDirection();
                    break;
                case SteerPriority.Random:
                    LookToRandomPosition();
                    break;
            }

            return CurrentSteerPriority != SteerPriority.None && CurrentSteerPriority != SteerPriority.Random;
        }

        private void HeardSoundSanityCheck()
        {
            if (CurrentSteerPriority == SteerPriority.Hear && LastHeardSound == null)
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogDebug("Bot was told to steer toward something they heard, but the place to check is null.");
                }
                UpdateSteer();
            }
        }

        // How long a bot will look in the direction they were shot from instead of other places
        private readonly float Steer_LastHitTime = 1f;
        // How long a bot will look at where they last saw an enemy instead of something they hear
        private readonly float Steer_TimeSinceSeen_Short = 2f;
        // How long a bot will look at where they last saw an enemy if they don't hear any other threats
        private readonly float Steer_TimeSinceSeen_Long = 12f;
        // How far a sound can be for them to react by looking toward it.
        private readonly float Steer_HeardSound_Dist = 30f;
        // How old a sound can be, in seconds, for them to react by looking toward it.
        private readonly float Steer_HeardSound_Age = 2f;

        public SteerPriority FindSteerPriority()
        {
            // return values are ordered by priority, so the targets get less "important" as they descend down this function.
            if (LookToAimTarget())
            {
                return SteerPriority.Shooting;
            }
            if (EnemyVisible())
            {
                return SteerPriority.Enemy;
            }
            if (Time.time - BotOwner.Memory.LastTimeHit < Steer_LastHitTime)
            {
                return SteerPriority.LastHit;
            }
            if (BotOwner.Memory.IsUnderFire)
            {
                return SteerPriority.UnderFire;
            }
            if (SAIN.Enemy?.TimeSinceSeen < Steer_TimeSinceSeen_Short && SAIN.Enemy.Seen)
            {
                return SteerPriority.LastSeenEnemy;
            }
            LastHeardSound = BotOwner.BotsGroup.YoungestFastPlace(BotOwner, Steer_HeardSound_Dist, Steer_HeardSound_Age);
            if (LastHeardSound != null)
            {
                return SteerPriority.Hear;
            }
            if (SAIN.Enemy?.TimeSinceSeen < Steer_TimeSinceSeen_Long && SAIN.Enemy.Seen)
            {
                return SteerPriority.LastSeenEnemyLong;
            }
            if (SAIN.Memory.Decisions.Main.Current == SoloDecision.Investigate)
            {
                return SteerPriority.MoveDirection;
            }
            if (SteerRandomToggle)
            {
                return SteerPriority.Random;
            }
            return SteerPriority.None;
        }


        public SteerPriority CurrentSteerPriority { get; private set; } = SteerPriority.None;
        public SteerPriority LastSteerPriority { get; private set; } = SteerPriority.None;

        private bool LookToVisibleSound()
        {
            return false;
        }

        public bool LookToEnemyLastSeenPos()
        {
            var enemy = SAIN.Enemy;
            var LastSeenPosition = enemy?.LastSeenPosition;
            if (LastSeenPosition != null && enemy?.IsVisible == false)
            {
                Vector3 pos = LastSeenPosition.Value + Vector3.up * 1f;
                LookToPoint(pos);
                return true;
            }
            return false;
        }

        public bool LookToEnemyLastSeenClose()
        {
            var enemy = SAIN.Enemy;
            if (enemy?.LastSeenPosition != null && enemy.Path.EnemyDistanceFromLastSeen < 10f)
            {
                LookToPoint(enemy.EnemyPosition);
                return true;
            }
            return false;
        }

        public void LookToMovingDirection()
        {
            BotOwner.Steering.LookToMovingDirection();
        }

        public void LookToPoint(Vector3 point)
        {
            BotOwner.Steering.LookToPoint(point);
        }

        public void LookToPoint(Vector3? point)
        {
            if (point != null)
                BotOwner.Steering.LookToPoint(point.Value);
        }

        public void LookToDirection(Vector3 direction, bool flat)
        {
            if (flat)
            {
                direction.y = 0f;
            }
            Vector3 pos = SAIN.Transform.Head + direction;
            LookToPoint(pos);
        }

        public bool LookToAimTarget()
        {
            if (SAIN.Enemy?.IsVisible == true && SAIN.Enemy?.CanShoot == true)
            {
                return true;
            }
            if (BotOwner.ShootData.Shooting)
            {
                return true;
            }
            return false;
        }

        public bool EnemyVisible()
        {
            SAINEnemyClass enemy = SAIN.Enemy;
            if (enemy != null && enemy.IsVisible)
            {
                LookToEnemy(enemy);
                return true;
            }
            return false;
        }

        public void LookToEnemy(SAINEnemyClass enemy)
        {
            if (enemy != null)
            {
                LookToPoint(enemy.EnemyPosition + Vector3.up * 0.85f);
            }
        }

        public void LookToEnemy()
        {
            SAINEnemyClass enemy = SAIN.Enemy;
            LookToEnemy(enemy);
        }

        private bool LookToCloseEnemyHear()
        {
            SAINEnemyClass enemy = SAIN.Enemy;
            if (enemy != null)
            {
                var player = enemy.EnemyIPlayer as Player;
                // Need to double check that ActualLinearSpeed and ActualLinearVelocity are the same, just renamed
                if (enemy.RealDistance < 30f && player.MovementContext.ActualLinearVelocity > 0.33f)
                {
                    LookToEnemy(enemy);
                    return true;
                }
            }
            return false;
        }

        public void LookToUnderFirePos()
        {
            var pos = SAIN.Memory.UnderFireFromPosition;
            pos.y += 1f;
            LookToPoint(pos);
        }

        public void LookToHearPos(Vector3 soundPos, bool visionCheck = false)
        {
            if (visionCheck)
            {
                soundPos.y += 0.1f;
                Vector3 headPos = SAIN.Transform.Head;
                var direction = soundPos - headPos;

                if (!Physics.Raycast(headPos, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    LookToPoint(soundPos);
                    return;
                }
            }

            if (HearPath == null)
            {
                HearPath = new NavMeshPath();
            }
            if (LastSoundTimer < Time.time || (LastSoundCheckPos - soundPos).magnitude > 1f)
            {
                LastSoundTimer = Time.time + 1f;
                LastSoundCheckPos = soundPos;
                LastSoundHeardCorner = Vector3.zero;

                HearPath.ClearCorners();
                if (NavMesh.CalculatePath(SAIN.Position, soundPos, -1, HearPath))
                {
                    if (HearPath.corners.Length > 2)
                    {
                        for (int i = HearPath.corners.Length - 1; i > 0; i--)
                        {
                            Vector3 corner = HearPath.corners[i];
                            corner.y += 1f;
                            Vector3 headPos = SAIN.Transform.Head;
                            Vector3 cornerDir = corner - headPos;
                            if (!Physics.Raycast(headPos, cornerDir.normalized, cornerDir.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                            {
                                LastSoundHeardCorner = corner;
                                break;
                            }
                        }
                    }
                }
            }
            if (LastSoundHeardCorner != Vector3.zero)
            {
                LookToPoint(LastSoundHeardCorner);
            }
            else
            {
                LookToPoint(soundPos);
            }
        }

        private float LastSoundTimer;
        private Vector3 LastSoundCheckPos;
        private Vector3 LastSoundHeardCorner;
        private NavMeshPath HearPath;

        public void LookToLastHitPos()
        {
            var pos = BotOwner.Memory.LastHitPos;
            pos.y += 1f;
            LookToPoint(pos);
        }

        private bool LookRandom;

        public void LookToRandomPosition()
        {
            if (RandomLookTimer < Time.time)
            {
                RandomLookTimer = Time.time + 2f * Random.Range(0.66f, 1.33f);
                Vector3 pointToLook = Vector3.zero;
                if (LookRandom)
                {
                    LookRandom = false;
                    var Mask = LayerMaskClass.HighPolyWithTerrainMask;
                    var headPos = SAIN.Transform.Head;
                    float pointDistance = 0f;
                    for (int i = 0; i < 10; i++)
                    {
                        var random = Random.onUnitSphere * 5f;
                        random.y = 0f;
                        if (!Physics.Raycast(headPos, random, out var hit, 15f, Mask))
                        {
                            pointToLook = random + headPos;
                            break;
                        }
                        else
                        {
                            if (hit.distance > pointDistance)
                            {
                                pointDistance = hit.distance;
                                pointToLook = hit.point;
                            }
                        }
                    }
                }
                else
                {
                    LookRandom = true;
                    if (LookToPathToEnemy())
                    {
                        return;
                    }
                    if (LookToEnemyLastSeenPos())
                    {
                        return;
                    }
                    if (LookToEnemyLastSeenClose())
                    {
                        return;
                    }
                    if (SAIN.CurrentTargetPosition != null)
                    {
                        pointToLook = SAIN.CurrentTargetPosition.Value;
                    }
                }
                LookToPoint(pointToLook);
            }
        }

        public bool LookToPathToEnemy()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null && !enemy.IsVisible)
            {
                if (enemy.Seen && enemy.TimeSinceSeen < 10f)
                {
                    if (enemy.NavMeshPath.corners.Length > 1)
                    {
                        Vector3 pos = enemy.NavMeshPath.corners[1];
                        pos += Vector3.up * 1f;
                        LookToPoint(pos);
                        return true;
                    }
                }
            }
            return false;
        }

        private float RandomLookTimer = 0f;

        // Old code, pre refactor on 3/25/2024
        public bool SteerByPriorityOld(bool lookRandomifFalse = true)
        {
            if (LookToAimTarget())
            {
                CurrentSteerPriority = SteerPriority.None;
                return true;
            }
            if (EnemyVisible())
            {
                CurrentSteerPriority = SteerPriority.Enemy;
                return true;
            }
            if (Time.time - BotOwner.Memory.LastTimeHit < 1f)
            {
                CurrentSteerPriority = SteerPriority.LastHit;
                LookToLastHitPos();
                return true;
            }
            if (BotOwner.Memory.IsUnderFire)
            {
                CurrentSteerPriority = SteerPriority.UnderFire;
                LookToUnderFirePos();
                return true;
            }
            if (SAIN.Enemy?.TimeSinceSeen < 2f && SAIN.Enemy.Seen)
            {
                CurrentSteerPriority = SteerPriority.LastSeenEnemy;
                LookToEnemyLastSeenPos();
                return true;
            }
            var sound = BotOwner.BotsGroup.YoungestFastPlace(BotOwner, 30f, 2f);
            if (sound != null)
            {
                CurrentSteerPriority = SteerPriority.Hear;
                LookToHearPos(sound.Position);
                return true;
            }
            if (SAIN.Enemy?.TimeSinceSeen < 12f && SAIN.Enemy.Seen)
            {
                CurrentSteerPriority = SteerPriority.LastSeenEnemy;
                LookToEnemyLastSeenPos();
                return true;
            }
            if (SAIN.Memory.Decisions.Main.Current == SoloDecision.Investigate)
            {
                CurrentSteerPriority = SteerPriority.MoveDirection;
                LookToMovingDirection();
                return true;
            }

            if (lookRandomifFalse)
            {
                CurrentSteerPriority = SteerPriority.Random;
                LookToRandomPosition();
            }
            return false;
        }

    }

    public enum SteerPriority
    {
        None,
        Shooting,
        Enemy,
        Hear,
        LastSeenEnemy,
        LastSeenEnemyLong,
        Random,
        LastHit,
        UnderFire,
        MoveDirection
    }
}