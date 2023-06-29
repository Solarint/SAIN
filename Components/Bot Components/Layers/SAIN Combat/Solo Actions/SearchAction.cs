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
            Shoot = new ShootClass(bot, SAIN);
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
            if (SAIN?.CurrentTargetPosition != null)
            {
                if (Search?.GoToPoint(SAIN.CurrentTargetPosition.Value) != NavMeshPathStatus.PathInvalid)
                {
                    TargetPosition = SAIN.CurrentTargetPosition.Value;
                }
            }
        }

        private Vector3? TargetPosition;

        public override void Stop()
        {
            TargetPosition = null;
        }

        public override void Update()
        {
            Shoot.Update();

            if (SAIN.Enemy?.IsVisible == false && SAIN.Decision.SelfActionDecisions.CheckLowAmmo(0.66f))
            {
                SAIN.SelfActions.TryReload();
            }

            if ( TargetPosition != null )
            {
                if (SAIN.Enemy == null && (BotOwner.Position - TargetPosition.Value).sqrMagnitude < 2f)
                {
                    SAIN.Decision.ResetDecisions();
                    return;
                }
                CheckShouldSprint();
                Search.Update(!SprintEnabled, SprintEnabled);
                if (Search?.ActiveDestination != null)
                {
                    Steer(Search.ActiveDestination);
                }
                else
                {
                    SAIN.Steering.SteerByPriority();
                }
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
                return;
            }

            var pers = SAIN.Info.BotPersonality;
            if (RandomSprintTimer < Time.time && (pers == BotPersonality.GigaChad || pers == BotPersonality.Chad))
            {
                RandomSprintTimer = Time.time + 3f * Random.Range(0.5f, 2f);
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
                SAIN.Mover.SetTargetMoveSpeed(0.25f);
                SAIN.Mover.SetTargetPose(0.75f);
            }
            else if (SprintEnabled)
            {
                SAIN.Mover.SetTargetMoveSpeed(1f);
                SAIN.Mover.SetTargetPose(1f);
            }
            else
            {
                SAIN.Mover.SetTargetMoveSpeed(0.66f);
                SAIN.Mover.SetTargetPose(0.85f);
            }

            if (!SprintEnabled || BotOwner.Memory.IsUnderFire)
            {
                SAIN.Mover.Sprint(false);
                if (SAIN.Steering.SteerByPriority())
                {
                }
                else
                {
                    return;
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