using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers
{
    internal class SearchAction : CustomLogic
    {
        public SearchAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot);
        }

        private ShootClass Shoot;

        private SearchClass Search;

        public override void Start()
        {
            Vector3 targetPosition;
            if (BotOwner.Memory.GoalEnemy != null)
            {
                targetPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
            }
            else if (BotOwner.Memory.GoalTarget?.GoalTarget?.Position != null)
            {
                targetPosition = BotOwner.Memory.GoalTarget.GoalTarget.Position;
            }
            else
            {
                targetPosition = BotOwner.Transform.position;
            }

            Search = new SearchClass(BotOwner);
            if (Search.GoToPoint(targetPosition) == NavMeshPathStatus.PathInvalid)
            {
                Logger.LogError($"Could not Start Search!");
            }
        }

        public override void Stop()
        {
        }

        public override void Update()
        {
            CheckShouldSprint();
            Search.Update(!SprintEnabled, SprintEnabled);
            Steer(Search.ActiveDestination);
        }

        private void CheckShouldSprint()
        {
            if (Search.PeekingCorner)
            {
                SprintEnabled = false;
                return;
            }

            var pers = SAIN.Info.BotPersonality;
            if (RandomSprintTimer < Time.time && (pers == BotPersonality.GigaChad || pers == BotPersonality.Chad))
            {
                RandomSprintTimer = Time.time + 3f * Random.Range(0.33f, 2f);
                float chance = pers == BotPersonality.GigaChad ? 40f : 20f;
                SprintEnabled = EFTMath.RandomBool(chance);
            }
        }

        private bool SprintEnabled = false;
        private float RandomSprintTimer = 0f;

        private void Steer(Vector3 pos)
        {
            if (Search.PeekingCorner)
            {
                SAIN.Mover.SetTargetMoveSpeed(0.33f);
                SAIN.Mover.SetTargetPose(0.8f);
            }
            else if (SprintEnabled)
            {
                SAIN.Mover.SetTargetMoveSpeed(1f);
                SAIN.Mover.SetTargetPose(1f);
            }
            else
            {
                SAIN.Mover.SetTargetMoveSpeed(0.75f);
                SAIN.Mover.SetTargetPose(0.9f);
            }

            if (SprintEnabled && !BotOwner.Memory.IsUnderFire)
            {
                BotOwner.GetPlayer.EnableSprint(true);
                SAIN.Steering.LookToMovingDirection();
            }
            else
            {
                BotOwner.GetPlayer.EnableSprint(false);
                float soundDistance = 999f;
                if (SAIN.LastHeardSound != null)
                {
                    soundDistance = Vector3.Distance(SAIN.LastHeardSound.Position, BotOwner.Position);
                }
                if (BotOwner.Memory.IsUnderFire)
                {
                    SAIN.Steering.SteerByPriority();
                }
                else if (soundDistance < 30f && SAIN.LastHeardSound.TimeSinceHeard < 1f)
                {
                    SAIN.Steering.SteerByPriority();
                }
                else
                {
                    pos.y += 1f;
                    if (!SeenSearchPoint && !Physics.Raycast(SAIN.HeadPosition, pos - SAIN.HeadPosition, (pos - SAIN.HeadPosition).magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        SeenSearchPoint = true;
                    }

                    if (SeenSearchPoint)
                    {
                        SAIN.Steering.SteerByPriority();
                    }
                    else
                    {
                        SAIN.Steering.LookToPoint(pos);
                    }
                }
            }
        }

        private bool SeenSearchPoint = false;

        private SAINComponent SAIN;
        public NavMeshPath Path = new NavMeshPath();
        private readonly ManualLogSource Logger;
    }
}