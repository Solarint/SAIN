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
            Search = new SearchClass(BotOwner);
            FindTarget();
        }

        private void FindTarget()
        {
            if (SAIN.CurrentTargetPosition != null)
            {
                if (Search.GoToPoint(SAIN.CurrentTargetPosition.Value) != NavMeshPathStatus.PathInvalid)
                {
                    TargetPosition = SAIN.CurrentTargetPosition.Value;
                }
            }
        }

        private Vector3? TargetPosition;

        public override void Stop()
        {
        }

        public override void Update()
        {
            if (SAIN.Enemy != null)
            {
                Shoot.Update();
            }
            if ( TargetPosition != null )
            {
                if (SAIN.Enemy == null && (BotOwner.Position - TargetPosition.Value).sqrMagnitude < 2f)
                {
                    BotOwner.Memory.GoalTarget?.Clear();
                    return;
                }
                CheckShouldSprint();
                Search.Update(!SprintEnabled, SprintEnabled);
                Steer(Search.ActiveDestination);
            }
            else
            {
                FindTarget();
            }
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
                SAIN.Mover.Sprint(true);
            }
            else
            {
                SAIN.Mover.Sprint(false);
                if (SAIN.Steering.SteerByPriority(false))
                {
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