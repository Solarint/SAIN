using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers.Logic
{
    public class CoverLogic : SAINBotExt
    {
        public CoverLogic(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        public bool CheckSelfForCover(out float ratio)
        {
            int rays = 0;
            int cover = 0;
            foreach (var part in BotOwner.MainParts.Values)
            {
                BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);
                Vector3 direction = part.Position - EnemyHead.Position;
                if (Physics.Raycast(EnemyHead.Position, direction, direction.magnitude, Components.SAINCoreComponent.SightMask))
                {
                    cover++;
                }
                rays++;
            }

            ratio = (float)cover / rays;

            return ratio > 0.33f;
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
            if (SAIN.Core.Enemy.CanSee)
            {
                if (SAIN.Core.Enemy.CanShoot)
                {
                    if (CheckSelfForCover(out float ratio))
                    {
                        BotOwner.SetPose(ratio);
                        BotOwner.MovementPause(1f);
                    }
                    else
                    {
                        BotOwner.SetPose(1f);
                        if (SAIN.CoverFinder.SafeCoverPoints.Count > 0)
                        {
                            BotOwner.GoToPoint(SAIN.CoverFinder.SafeCoverPoints[0].Position, false);
                        }
                        else if (!CanBotBackUp())
                        {
                            SAIN.Dodge.Execute();
                        }
                    }
                }
                else
                {
                    if (SAIN.CoverFinder.SafeCoverPoints.Count > 0)
                    {
                        BotOwner.GoToPoint(SAIN.CoverFinder.SafeCoverPoints.PickRandom().Position, false);
                    }
                    else
                    {
                        SAIN.Dodge.Execute();
                    }
                }
                return true;
            }
            return false;
        }

        public bool CanBotBackUp()
        {
            if (FightCover != null)
            {
                if (SAIN.CoverFinder.Analyzer.CheckPosition(FightCover.CoverPosition, out FightCover, 0.25f))
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
                    if (SAIN.CoverFinder.Analyzer.CheckPosition(hit.position, out FightCover, 0.25f))
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

        private bool DebugMode => DebugUpdateMove.Value;
        private float SelfCoverCheckTime = 0f;
        private readonly ManualLogSource Logger;
        private CustomCoverPoint SelfCover;
        public CustomCoverPoint FallBackCoverPoint;
    }
}