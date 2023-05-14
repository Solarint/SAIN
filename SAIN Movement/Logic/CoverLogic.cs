using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers.Logic
{
    public class CoverLogic : SAINBot
    {
        public CoverLogic(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            Dodge = new BotDodge(bot);
            DynamicLean = bot.gameObject.GetComponent<LeanComponent>();
            CoverFinder = bot.gameObject.GetComponent<CoverFinderComponent>();
        }

        public bool CheckSelfForCover()
        {
            if (SelfCover != null && SelfCoverCheckTime < Time.time)
            {
                SelfCoverCheckTime = Time.time + 0.5f;

                if (CoverFinder.Analyzer.CheckPosition(SAIN.Enemy.LastSeen.EnemyPosition, BotOwner.Transform.position, out SelfCover, 0.33f))
                {
                    return true;
                }
            }
            return false;
        }

        public void DebugDrawFallback()
        {
            if (DebugMode)
            {
                DebugDrawer.Line(FallBackCoverPoint.CoverPosition, BotOwner.MyHead.position, 0.1f, Color.green, 3f);
                DebugDrawer.Line(BotOwner.Memory.GoalEnemy.Owner.MyHead.position, BotOwner.MyHead.position, 0.1f, Color.green, 3f);
                DebugDrawer.Line(BotOwner.Memory.GoalEnemy.Owner.MyHead.position, FallBackCoverPoint.CoverPosition, 0.1f, Color.green, 3f);
            }
        }

        public bool TakeCover()
        {
            if (SAIN.Enemy.CanSee)
            {
                if (CheckSelfForCover() && SAIN.Enemy.CanShoot)
                {
                    DynamicLean.HoldLean = true;
                    DecidePoseFromCoverLevel(SelfCover);
                }
                else if (CoverFinder.SafeCoverPoints.Count > 0)
                {
                    BotOwner.GoToPoint(CoverFinder.SafeCoverPoints[0].Position, false);
                }
                else if (!CanBotBackUp() && SAIN.Enemy.CanShoot)
                {
                    Dodge.Execute();
                }
                else
                {
                    BotOwner.SetPose(0f);
                }
                return true;
            }
            return false;
        }

        public void DecidePoseFromCoverLevel(CustomCoverPoint cover)
        {
            if (cover != null)
            {
                float coverLevel = cover.CoverLevel;
                float poseLevel;

                if (coverLevel > 0.75)
                {
                    poseLevel = 0.5f;
                }
                else if (coverLevel > 0.5f)
                {
                    poseLevel = 0.25f;
                }
                else
                {
                    poseLevel = 0.0f;
                }

                BotOwner.SetPose(poseLevel);
            }
        }

        public bool CanBotBackUp()
        {
            if (FightCover != null)
            {
                if (CoverFinder.Analyzer.CheckPosition(BotOwner.Memory.GoalEnemy.CurrPosition, FightCover.CoverPosition, out FightCover, 0.25f))
                {
                    BotOwner.GoToPoint(FightCover.CoverPosition, false);
                    UpdateDoorOpener();
                    return true;
                }
            }

            const float angleStep = 15f;
            const float rangeStep = 2f;
            const int max = 10;
            int i = 0;
            while (i < max)
            {
                float angleAdd = angleStep * i;
                float currentAngle = UnityEngine.Random.Range(-5f - angleAdd, 5f + angleAdd);
                float currentRange = rangeStep * i + 2f;

                Vector3 DodgeFallBack = HelperClasses.FindArcPoint(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, currentRange, currentAngle);
                if (NavMesh.SamplePosition(DodgeFallBack, out NavMeshHit hit, 5f, -1))
                {
                    if (CoverFinder.Analyzer.CheckPosition(BotOwner.Memory.GoalEnemy.CurrPosition, hit.position, out FightCover, 0.25f))
                    {
                        if (BotOwner.GoToPoint(hit.position, false) == NavMeshPathStatus.PathComplete)
                        {
                            UpdateDoorOpener();
                            return true;
                        }
                    }
                }

                i++;
            }
            return false;
        }

        private CustomCoverPoint FightCover;

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        public LeanComponent DynamicLean { get; private set; }
        private bool DebugMode => DebugUpdateMove.Value;
        private float SelfCoverCheckTime = 0f;
        private readonly ManualLogSource Logger;
        private CustomCoverPoint SelfCover;
        public CustomCoverPoint FallBackCoverPoint;
        private readonly BotDodge Dodge;
        public CoverFinderComponent CoverFinder { get; private set; }
    }
}