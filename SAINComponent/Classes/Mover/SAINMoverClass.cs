using EFT;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class SAINMoverClass : SAINBase, ISAINClass
    {
        public SAINMoverClass(SAINComponentClass sain) : base(sain)
        {
            BlindFire = new BlindFireClass(sain);
            SideStep = new SideStepClass(sain);
            Lean = new LeanClass(sain);
            Prone = new ProneClass(sain);
            Pose = new PoseClass(sain);
        }

        public void Init()
        {
        }

        public void Update()
        {
            SetStamina();

            Pose.Update();
            Lean.Update();
            SideStep.Update();
            Prone.Update();
            BlindFire.Update();
        }

        public void Dispose()
        {
        }

        public BlindFireClass BlindFire { get; private set; }
        public SideStepClass SideStep { get; private set; }
        public LeanClass Lean { get; private set; }
        public PoseClass Pose { get; private set; }
        public ProneClass Prone { get; private set; }

        public bool GoToPoint(Vector3 point, float reachDist = -1f, bool crawl = false)
        {
            if (CanGoToPoint(point, out Vector3 pointToGo))
            {
                if (reachDist < 0f)
                {
                    reachDist = BotOwner.Settings.FileSettings.Move.REACH_DIST;
                }
                BotOwner.Mover?.GoToPoint(pointToGo, false, reachDist, false, false, false);
                if (crawl)
                {
                    Prone.SetProne(true);
                }
                BotOwner.DoorOpener.Update();
                return true;
            }
            return false;
        }

        public bool CanGoToPoint(Vector3 point, out Vector3 pointToGo, bool mustHaveCompletePath = false, float navSampleRange = 10f)
        {
            pointToGo = point;
            if (NavMesh.SamplePosition(point, out var navHit, navSampleRange, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(SAIN.Transform.Position, navHit.position, -1, Path) && Path.corners.Length > 1)
                {
                    if (mustHaveCompletePath && Path.status != NavMeshPathStatus.PathComplete)
                    {
                        return false;
                    }
                    pointToGo = navHit.position;

                    SAINVaultClass.FindVaultPoint(Player, Path, out SAINVaultPoint vaultPoint);

                    return true;
                }
            }
            return false;
        }

        private void SetStamina()
        {
            var stamina = Player.Physical.Stamina;
            if (SAIN.LayersActive && stamina.NormalValue < 0.1f)
            {
                Player.Physical.Stamina.UpdateStamina(stamina.TotalCapacity / 8f);
            }
        }

        public void SetTargetPose(float pose)
        {
            Pose.SetTargetPose(pose);
        }

        public void SetTargetMoveSpeed(float speed)
        {
            BotOwner.Mover?.SetTargetMoveSpeed(speed);
        }

        public void StopMove()
        {
            BotOwner.Mover?.Stop();
            if (IsSprinting)
            {
                Sprint(false);
            }
        }

        public void Sprint(bool value)
        {
            IsSprinting = value;
            BotOwner.Mover?.Sprint(value);
            if (value)
            {
                SAIN.Steering.LookToMovingDirection();
                FastLean(0f);
            }
        }

        public bool IsSprinting { get; private set; }

        public void TryJump()
        {
            if (JumpTimer < Time.time && CanJump)
            {
                JumpTimer = Time.time + 1f;
                Player.MovementContext.TryJump();
            }
        }

        public void FastLean(LeanSetting value)
        {
            float num;
            switch (value)
            {
                case LeanSetting.Left:
                    num = -5f; break;
                case LeanSetting.Right:
                    num = 5f; break;
                default:
                    num = 0f; break;
            }
            FastLean(num);
        }

        public void FastLean(float value)
        {
            if (Player.MovementContext.Tilt != value)
            {
                Player.MovementContext.SetTilt(value);
            }
        }

        public bool CanJump => Player.MovementContext.CanJump;

        private float JumpTimer = 0f;
    }
}