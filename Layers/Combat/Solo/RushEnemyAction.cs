using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo
{
    internal class RushEnemyAction : SAINAction
    {
        public RushEnemyAction(BotOwner bot) : base(bot, nameof(RushEnemyAction))
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            SAIN = bot.GetComponent<SAINComponentClass>();
            Shoot = new ShootClass(bot);
        }

        private readonly ShootClass Shoot;
        private float TryJumpTimer;

        public override void Update()
        {
            if (SAIN.Enemy == null)
            {
                return;
            }

            SAIN.Mover.SetTargetPose(1f);
            SAIN.Mover.SetTargetMoveSpeed(1f);

            if (SAIN.Enemy.InLineOfSight)
            {
                Shoot.Update();
                if (SAIN.Info.PersonalitySettings.CanJumpCorners)
                {
                    if (TryJumpTimer < Time.time)
                    {
                        TryJumpTimer = Time.time + 5f;
                        SAIN.Mover.TryJump();
                    }
                }
                SAIN.Mover.Sprint(false);
                if (SAIN.Enemy.IsVisible && SAIN.Enemy.CanShoot)
                {
                    SAIN.Steering.SteerByPriority();
                }
                else
                {
                    SAIN.Steering.LookToEnemy(SAIN.Enemy);
                }
                return;
            }

            Vector3[] EnemyPath = SAIN.Enemy.NavMeshPath.corners;
            Vector3 EnemyPos = SAIN.Enemy.EnemyPosition;
            if (NewDestTimer < Time.time)
            {
                NewDestTimer = Time.time + 1f;
                Vector3 Destination = EnemyPos;
                /*
                if (SAIN.Info.Personality == Personality.GigaChad)
                {
                    if (PathToEnemy.Length > 2)
                    {
                        Vector3 SecondToLastCorner = PathToEnemy[PathToEnemy.Length - 3];
                        Vector3 LastCornerDirection = LastCorner - SecondToLastCorner;
                        Vector3 AddToLast = LastCornerDirection.normalized * 3f;
                        Vector3 widePush = LastCorner + AddToLast;
                        if (SAIN.Mover.CanGoToPoint(widePush, out Vector3 PointToGo))
                        {
                            Destination = PointToGo;
                        }
                    }
                    else
                    {
                        Destination = Vector3.Lerp(EnemyPos, LastCorner, 0.75f);
                    }
                }
                else
                {
                    if (PathToEnemy.Length > 2)
                    {
                        Vector3 SecondToLastCorner = PathToEnemy[PathToEnemy.Length - 3];
                        Vector3 LastCornerDirection = LastCorner - SecondToLastCorner;
                        Vector3 AddToLast = LastCornerDirection.normalized * 0.15f;
                        Vector3 shortPush = LastCorner - AddToLast;
                        if (SAIN.Mover.CanGoToPoint(shortPush, out Vector3 PointToGo))
                        {
                            Destination = PointToGo;
                        }
                    }
                    else
                    {
                        Destination = LastCorner;
                    }
                }
                */
                BotOwner.BotRun.Run(Destination, false);
            }

            if (SAIN.Info.PersonalitySettings.CanJumpCorners && TryJumpTimer < Time.time)
            {
                var corner = SAIN.Enemy?.LastCornerToEnemy;
                if (corner != null)
                {
                    float distance = (corner.Value - BotOwner.Position).magnitude;
                    if (distance < 0.5f)
                    {
                        TryJumpTimer = Time.time + 3f;
                        SAIN.Mover.TryJump();
                    }
                }
            }

        }

        private float NewDestTimer = 0f;
        private Vector3? PushDestination;

        private readonly SAINComponentClass SAIN;

        public ManualLogSource Logger;

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}