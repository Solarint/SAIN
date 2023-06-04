using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers
{
    internal class MoveToCoverAction : CustomLogic
    {
        public MoveToCoverAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            AimData = new GClass105(bot);
        }

        private readonly GClass105 AimData;

        public override void Update()
        {
            CheckVis();
            BotOwner.DoorOpener.Update();

            if (!SwitchToRun)
            {
                SAIN.Steering.ManualUpdate();

                if (SAIN.HasEnemyAndCanShoot)
                {
                    AimData.Update();
                }
            }
            else
            {
                BotOwner.Steering.LookToMovingDirection();
            }

            CoverPoint PointToGo = null;
            if (SAIN.Cover.CurrentFallBackPoint != null)
            {
                PointToGo = SAIN.Cover.CurrentFallBackPoint;
            }
            else if (SAIN.Cover.CurrentCoverPoint != null)
            {
                PointToGo = SAIN.Cover.CurrentCoverPoint;
            }

            if (PointToGo != null)
            {
                MoveToPoint(PointToGo.Position);
            }
        }

        private void MoveToPoint(Vector3 point)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);

            if (SwitchToRun)
            {
                BotOwner.GetPlayer.EnableSprint(true);
            }

            if ((BotOwner.Mover.RealDestPoint - point).magnitude < 0.25f)
            {
                return;
            }

            BotOwner.GoToPoint(point, true, -1, false, false);
        }

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            WaitForRunTime = 3f * Random.Range(0.5f, 1.5f);
            ActivatedTime = Time.time + WaitForRunTime;
            BotOwner.PatrollingData.Pause();
        }

        private void CheckVis()
        {
            if (CheckVisTimer < Time.time && SAIN.HasGoalEnemy)
            {
                CheckVisTimer = Time.time + 0.5f;

                var enemyheadPos = BotOwner.Memory.GoalEnemy.Person.MainParts[BodyPartType.head].Position;
                var bothead = SAIN.HeadPosition;
                var direction = enemyheadPos - bothead;

                if (Physics.Raycast(bothead, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    ActivatedTime = Time.time + WaitForRunTime;
                    LineOfSight = false;
                }
                else
                {
                    LineOfSight = true;
                }
            }
        }

        private float CheckVisTimer = 0f;
        private bool LineOfSight = false;
        private bool SwitchToRun => ActivatedTime < Time.time && LineOfSight;

        private float WaitForRunTime = 0f;
        private float ActivatedTime = 0f;

        public override void Stop()
        {
            BotOwner.GetPlayer.EnableSprint(false);

            SAIN.Steering.ManualUpdate();

            BotOwner.PatrollingData.Unpause();
        }

        public ManualLogSource Logger;
    }
}