using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using Movement.Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.DebugDrawer;
using static SAIN_Helpers.SAIN_Math;

namespace SAIN.Movement.Layers
{
    internal class DogFightLogic : CustomLogic
    {
        public DogFightLogic(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);

            updateTarget_0 = new UpdateTarget(bot);
            //updateMove_0 = new UpdateMove(bot);
        }

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }

        public override void Update()
        {
            Logger.LogDebug($"Started");

            var goalEnemy = BotOwner.Memory.GoalEnemy;

            Fight();

            if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
            {
                BotOwner.Steering.LookToPoint(goalEnemy.CurrPosition);

                updateTarget_0.Update();

                return;
            }

            BotOwner.LookData.SetLookPointByHearing(null);
        }

        //private readonly BotOwner BotOwner;
        private float ReactionTimer = 0f;

        public void Fight()
        {
            if (ReactionTimer < Time.time)
            {
                ReactionTimer = Time.time + 0.25f;

                //updateMove_0.Update();
            }
        }

        public bool CheckEnemyDistance(out Vector3 trgPos)
        {
            Vector3 a = -NormalizeFastSelf(BotOwner.Memory.GoalEnemy.Direction);

            trgPos = Vector3.zero;

            float num = 0f;
            if (NavMesh.SamplePosition(BotOwner.Position + a * 2f, out NavMeshHit navMeshHit, 1f, -1))
            {
                trgPos = navMeshHit.position;

                Vector3 a2 = trgPos - BotOwner.Position;

                float magnitude = a2.magnitude;

                if (magnitude != 0f)
                {
                    Vector3 a3 = a2 / magnitude;

                    num = magnitude;

                    if (NavMesh.SamplePosition(BotOwner.Position + a3 * 10f, out navMeshHit, 1f, -1))
                    {
                        trgPos = navMeshHit.position;

                        num = (trgPos - BotOwner.Position).magnitude;
                    }
                }
            }
            if (num != 0f)
            {
                navMeshPath_0.ClearCorners();

                if (NavMesh.CalculatePath(BotOwner.Position, trgPos, -1, navMeshPath_0) && navMeshPath_0.status == NavMeshPathStatus.PathComplete)
                {
                    trgPos = navMeshPath_0.corners[navMeshPath_0.corners.Length - 1];

                    Sphere(trgPos, 0.25f, Color.white, 1f);
                    Line(trgPos, BotOwner.Transform.position, 0.05f, Color.white, 1f);

                    return CheckStraightDistance(navMeshPath_0, num);
                }
            }
            Sphere(trgPos, 0.15f, Color.yellow, 1f);

            return false;
        }

        private bool CheckStraightDistance(NavMeshPath path, float straighDist)
        {
            return path.CalculatePathLength() < straighDist * 1.2f;
        }

        private NavMeshPath navMeshPath_0 = new NavMeshPath();
        private readonly UpdateTarget updateTarget_0;
        private readonly UpdateMove updateMove_0;

        private class DebugDogFight
        {
            public static void DrawRunAway(BotOwner bot, Vector3 runposition, float linewidth, Color color, float expiretime)
            {
                Line(runposition, bot.Memory.GoalEnemy.CurrPosition, linewidth, color, expiretime);
                Line(runposition, bot.Position, linewidth, color, expiretime);
                Line(bot.Position, bot.Memory.GoalEnemy.CurrPosition, linewidth, color, expiretime);
            }
        }

        public ManualLogSource Logger { get; private set; }
    }

    internal class UpdateTarget : DogFightLogic
    {
        //protected ManualLogSource Logger;
        public UpdateTarget(BotOwner bot) : base(bot)
        {
            //Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);

            updateShoot_0 = new UpdateShoot(bot);

            Logger.LogDebug($"Constructed");
        }

        public override void Update()
        {
            Logger.LogDebug($"Started");

            if (BotFightInterface == null)
            {
                BotFightInterface = BotOwner.AimingData;
            }

            BotOwner.BotLight.TurnOn(BotFightInterface.AlwaysTurnOnLight);

            Vector3? pointToShoot = GetPointToShoot();

            if (pointToShoot != null)
            {
                BotTarget = pointToShoot.Value;

                if (BotFightInterface.IsReady)
                {
                    ReadyToShoot();

                    updateShoot_0.Update();
                }
            }
        }

        protected void ReadyToShoot()
        {
        }

        protected Vector3? GetTarget()
        {
            float ShootToCenter = BotOwner.Settings.FileSettings.Aiming.DIST_TO_SHOOT_TO_CENTER;
            var goalEnemy = BotOwner.Memory.GoalEnemy;

            if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
            {
                Vector3 aimTarget;

                if (goalEnemy.Distance < ShootToCenter)
                {
                    aimTarget = goalEnemy.GetCenterPart();
                }
                else
                {
                    aimTarget = goalEnemy.GetPartToShoot();
                }

                return new Vector3?(aimTarget);
            }

            Vector3? neutralTarget = null;

            if (BotOwner.Memory.LastEnemy != null)
            {
                neutralTarget = new Vector3?(BotOwner.Memory.LastEnemy.CurrPosition + Vector3.up * BotOwner.Settings.FileSettings.Aiming.DANGER_UP_POINT);
            }

            return neutralTarget;
        }

        protected Vector3? GetPointToShoot()
        {
            Vector3? target = GetTarget();

            if (target != null)
            {
                BotTarget = target.Value;

                if (TalkDelay < Time.time)
                {
                    TalkDelay = Time.time + Random(5f, 8f);

                    BotOwner.BotTalk.TrySay(EPhraseTrigger.OnFight, true);
                }

                BotFightInterface.SetTarget(BotTarget);

                BotFightInterface.NodeUpdate();

                return new Vector3?(BotTarget);
            }
            return null;
        }

        private readonly UpdateShoot updateShoot_0;
        private GInterface5 BotFightInterface;
        protected Vector3 BotTarget;
        private float TalkDelay;

        public class UpdateShoot : UpdateTarget
        {
            public UpdateShoot(BotOwner bot) : base(bot)
            {
                Logger.LogDebug($"Constructed {this.GetType().Name}");
            }

            public override void Update()
            {
                Logger.LogDebug($"Started UpdateShoot");

                if (!BotOwner.WeaponManager.HaveBullets)
                {
                    BotOwner.WeaponManager.Reload.TryReload();
                    return;
                }

                Vector3 position = BotOwner.GetPlayer.PlayerBones.WeaponRoot.position;
                Vector3 realTargetPoint = BotOwner.AimingData.RealTargetPoint;
                if (BotOwner.ShootData.ChecFriendlyFire(position, realTargetPoint))
                {
                    return;
                }

                if (WithTalk)
                {
                    Talk();
                }

                if (BotOwner.ShootData.Shoot() && Bullets > BotOwner.WeaponManager.Reload.BulletCount)
                {
                    Bullets = BotOwner.WeaponManager.Reload.BulletCount;
                }
            }

            private void Talk()
            {
                if (SilentUntil > Time.time)
                {
                    return;
                }

                if (IsTrue100(50f))
                {
                    BotOwner.BotTalk.TrySay(EPhraseTrigger.OnFight, true);
                    SilentUntil = Time.time + 15f;
                }
            }

            public float SilentUntil;
            public bool WithTalk = true;
            private int Bullets;
        }
    }

    internal class UpdateMove : DogFightLogic // GClass125
    {
        //protected ManualLogSource Logger;
        private readonly BotDodge Dodge;

        private float DodgeTimer = 0f;
        private bool MovingToEnemy = false;
        public UpdateMove(BotOwner bot) : base(bot)
        {
            //Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            Dodge = new BotDodge(bot);

            Logger.LogDebug($"Constructed");
        }

        public override void Update()
        {
            Logger.LogDebug($"Started");

            if (BotOwner.Memory.GoalEnemy.IsVisible && DodgeTimer < Time.time)
            {
                DodgeTimer = Time.time + 0.5f;

                FullSpeed();
                Dodge.Execute();
                MovingToEnemy = false;

                return;
            }

            if (!MovingToEnemy)
            {
                FullSpeed();
                return;
            }

            if (BotOwner.Memory.GoalEnemy.CanShoot)
            {
                MovingToEnemy = false;
                return;
            }

            MovingToEnemy = true;

            BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);

            // If we have a target position, and we're already there, clear it
            if (targetPos != null && ((targetPos.Value - BotOwner.Position).sqrMagnitude < 4f || !BotOwner.Memory.GoalEnemy.IsVisible))
            {
                targetPos = null;
            }

            RunAwayLikeABitch();
            UpdateDoorOpener();

            if (targetPos == null)
            {
                System.Console.WriteLine($"Unable to find a location for {BotOwner.name}");
            }
        }

        private Vector3? targetPos = null;

        private void RunAwayLikeABitch()
        {
            // If we don't have a target position yet, pick one
            int i = 0;
            while (targetPos == null && i < 100)
            {
                Vector3 randomPos = UnityEngine.Random.insideUnitSphere * 20f;
                randomPos += BotOwner.Position;
                if (NavMesh.SamplePosition(randomPos, out var navHit, 100f, NavMesh.AllAreas))
                {
                    // Debug
                    Ray(navHit.position, Vector3.up, 3f, 0.1f, Color.white, 3f);

                    targetPos = navHit.position;

                    Vector3 RunPosition = navHit.position;
                    RunPosition.y += 1.3f;

                    Vector3 direction = BotOwner.Memory.GoalEnemy.CurrPosition - RunPosition;
                    float distance = Vector3.Distance(RunPosition, BotOwner.Memory.GoalEnemy.CurrPosition);

                    Ray visionCheck = new Ray(RunPosition, direction);

                    if (Physics.Raycast(visionCheck, out RaycastHit hit, distance, LayerMaskClass.HighPolyWithTerrainMask) && hit.transform != BotOwner.Memory.GoalEnemy.Person.GetPlayer.gameObject.transform)
                    {
                        Logger.LogDebug("FLEE YOU FOOL!");

                        BotOwner.Steering.LookToMovingDirection();

                        BotOwner.GoToPoint(targetPos.Value, true, -1f, false, true, true);

                        SetToSprint();

                        // Debug
                        //DebugDogFight.DrawRunAway(BotOwner, RunPosition, 0.05f, Color.red, 1f);
                    }
                    else
                    {
                        targetPos = null;
                    }
                }

                i++;
            }
        }

        protected bool UpdateDoorOpener()
        {
            return BotOwner.DoorOpener.Update();
        }

        protected void SetToSprint()
        {
            BotOwner.Sprint(true);
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
        }

        protected void FullSpeed()
        {
            BotOwner.Sprint(false);
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
        }

        protected void CrouchWalk()
        {
            BotOwner.Sprint(false);
            BotOwner.SetPose(0f);
            BotOwner.SetTargetMoveSpeed(1f);
        }
    }
}