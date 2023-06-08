using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using UnityEngine;

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

        private CoverPoint CoverPoint => SAIN.Cover.CurrentCoverPoint ?? SAIN.Cover.CurrentFallBackPoint;

        public override void Update()
        {
            BotOwner.DoorOpener.Update();

            if (SAIN.HasGoalEnemy)
            {
                if (SAIN.EnemyInLineOfSight)
                {
                    LineOfSight = true;
                }
                else
                {
                    WaitForRunTime = 3f * Random.Range(0.5f, 1.5f);
                    ActivatedTime = Time.time + WaitForRunTime;
                    LineOfSight = false;
                }
            }

            if (CoverPoint != null)
            {
                MoveToPoint(CoverPoint.Position);

                if (TimeForRun)
                {
                    BotOwner.Steering.LookToMovingDirection();
                }
                else
                {
                    SAIN.Steering.ManualUpdate();

                    if (SAIN.HasEnemyAndCanShoot)
                    {
                        AimData.Update();
                    }
                }
            }
            else
            {
                Logger.LogError($"Point null?!");
            }

            if (!SAIN.BotIsMoving && SAIN.Decisions.TimeSinceChangeDecision > 2f)
            {
                SAIN.Steering.ManualUpdate();

                if (SAIN.HasEnemyAndCanShoot)
                {
                    AimData.Update();
                }

                //Logger.LogWarning($"{BotOwner.name} is not moving while trying to move to cover!");
            }
        }

        private void MoveToPoint(Vector3 point)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);

            if (TimeForRun)
            {
                BotOwner.GetPlayer.EnableSprint(true);
            }

            BotOwner.GoToPoint(point, true, 0.15f, false, false);
        }

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            WaitForRunTime = 3f * Random.Range(0.5f, 1.5f);
            ActivatedTime = Time.time + WaitForRunTime;
            BotOwner.PatrollingData.Pause();
        }

        private bool LineOfSight = false;

        private bool TimeForRun
        {
            get
            {
                bool time = ActivatedTime < Time.time && LineOfSight;

                if (CoverPoint != null && time)
                {
                    var status = CoverPoint.Status();
                    bool far = status == CoverStatus.FarFromCover || status == CoverStatus.MidRangeToCover;
                    return time;
                }

                return false;
            }
        }

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