using EFT;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Solo
{
    internal class SearchAction : SAINAction
    {
        public SearchAction(BotOwner bot) : base(bot, nameof(SearchAction))
        {
        }

        public override void Start()
        {
            FindTarget();
        }

        private void FindTarget()
        {
            Vector3 pos = Search.SearchMovePos();
            if (Search?.CalculatePath(pos, false) != NavMeshPathStatus.PathInvalid)
            {
                TargetPosition = Search.FinalDestination;
            }
        }

        private Vector3? TargetPosition;

        public override void Stop()
        {
            Search.Reset();
            TargetPosition = null;
        }

        private float CheckMagTimer;
        private float CheckChamberTimer;
        private float NextCheckTimer;
        private float ReloadTimer;

        public override void Update()
        {
            Shoot.Update();
            CheckWeapon();

            if (TargetPosition != null)
            {
                MoveToEnemy();
            }
            else
            {
                FindTarget();
            }
        }

        private void CheckWeapon()
        {
            if (SAIN.Enemy != null)
            {
                if (SAIN.Enemy.Seen && SAIN.Enemy.TimeSinceSeen > 10f || !SAIN.Enemy.Seen && SAIN.Enemy.TimeSinceEnemyCreated > 10f)
                {
                    if (ReloadTimer < Time.time && SAIN.Decision.SelfActionDecisions.LowOnAmmo(0.5f))
                    {
                        ReloadTimer = Time.time + 10f;
                        SAIN.SelfActions.TryReload();
                    }
                    else if (CheckMagTimer < Time.time && NextCheckTimer < Time.time)
                    {
                        NextCheckTimer = Time.time + 3f;
                        CheckMagTimer = Time.time + 240f * Random.Range(0.5f, 1.5f);
                        SAIN.Player.HandsController.FirearmsAnimator.CheckAmmo();
                    }
                    else if (CheckChamberTimer < Time.time && NextCheckTimer < Time.time)
                    {
                        NextCheckTimer = Time.time + 3f;
                        CheckChamberTimer = Time.time + 240f * Random.Range(0.5f, 1.5f);
                        SAIN.Player.HandsController.FirearmsAnimator.CheckChamber();
                    }
                }
            }
        }

        private void MoveToEnemy()
        {
            if (SAIN.Enemy == null && (BotOwner.Position - TargetPosition.Value).sqrMagnitude < 2f)
            {
                SAIN.Decision.ResetDecisions();
                return;
            }
            CheckShouldSprint();
            Search.Search(SprintEnabled);
            Steer();
        }

        private SAINSearchClass Search => SAIN.Search;

        private void CheckShouldSprint()
        {
            if (Search.CurrentState == ESearchMove.MoveToEndPeak)
            {
                SprintEnabled = false;
                return;
            }

            var persSettings = SAIN.Info.PersonalitySettings;
            if (RandomSprintTimer < Time.time && persSettings.SprintWhileSearch)
            {
                float timeAdd;
                if (SprintEnabled)
                {
                    timeAdd = 1.5f * Random.Range(0.75f, 1.5f);
                }
                else
                {
                    timeAdd = 2f * Random.Range(0.33f, 1.5f);
                }
                RandomSprintTimer = Time.time + timeAdd;
                float chance = persSettings.FrequentSprintWhileSearch ? 40f : 20f;
                SprintEnabled = EFTMath.RandomBool(chance);
            }
        }

        private bool SprintEnabled = false;
        private float RandomSprintTimer = 0f;

        private void Steer()
        {
            if (BotOwner.Memory.IsUnderFire)
            {
                SAIN.Mover.Sprint(false);
                SteerByPriority(true);
                return;
            }
            if (SprintEnabled)
            {
                LookToMovingDirection();
                return;
            }
            if (Search.CurrentState == ESearchMove.MoveToDangerPoint)
            {
                float distance = (Search.SearchMovePoint.DangerPoint - SAIN.Position).sqrMagnitude;
                if (distance > 2f)
                {
                    LookToMovingDirection();
                    return;
                }
            }
            if (SteerByPriority(false) == false)
            {
                if (CanSeeDangerOrCorner(out Vector3 point))
                {
                    SAIN.Steering.LookToPoint(point);
                }
                else
                {
                    SAIN.Steering.LookToRandomPosition();
                }
            }
        }

        private bool CanSeeDangerOrCorner(out Vector3 point)
        {
            point = Vector3.zero;
            
            if (Search.SearchMovePoint == null || Search.CurrentState == ESearchMove.MoveToDangerPoint)
            {
                LookPoint = Vector3.zero;
                return false;
            }
            
            if (CheckSeeTimer < Time.time)
            {
                LookPoint = Vector3.zero;
                CheckSeeTimer = Time.time + 1f * Random.Range(0.66f, 1.33f);
                var headPosition = SAIN.Transform.Head;

                var canSeePoint = !Vector.Raycast(headPosition,
                    Search.SearchMovePoint.DangerPoint,
                    LayerMaskClass.HighPolyWithTerrainMaskAI);

                if (canSeePoint)
                {
                    LookPoint = Search.SearchMovePoint.DangerPoint;
                }
                else
                {
                    canSeePoint = !Vector.Raycast(headPosition,
                        Search.SearchMovePoint.Corner,
                        LayerMaskClass.HighPolyWithTerrainMaskAI);
                    if (canSeePoint)
                    {
                        LookPoint = Search.SearchMovePoint.Corner;
                    }
                }
                
                if (LookPoint != Vector3.zero)
                {
                    LookPoint.y = 0;
                    LookPoint += headPosition;
                }
            }
            
            point = LookPoint;
            return point != Vector3.zero;
        }

        private Vector3 LookPoint;
        private float CheckSeeTimer;

        private bool SteerByPriority(bool value) => SAIN.Steering.SteerByPriority(value);

        private void LookToMovingDirection() => SAIN.Steering.LookToMovingDirection();

        public NavMeshPath Path = new NavMeshPath();
    }
}