using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class InvestigateAction : CustomLogic
    {
        public InvestigateAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot);
        }

        private readonly ShootClass Shoot;

        public override void Update()
        {
            if (SAIN.Enemy == null)
            {
                if (Sound != null)
                {
                    SAIN.Steering.SteerByPriority();
                    if ((Sound.Position - MovePos).sqrMagnitude > 25f)
                    {
                        MovePos = Sound.Position;
                        SAIN.Mover.GoToPoint(MovePos, false, false);
                    }
                }
                else
                {
                    Sound = BotOwner.BotsGroup.YoungestPlace(BotOwner, 150f, true);
                    SAIN.Steering.LookToRandomPosition();
                }
            }
            else
            {
                SAIN.Steering.SteerByPriority();
                Shoot.Update();
            }
        }

        private Vector3 MovePos;
        private float RandomLookTimer = 0f;

        private bool BackUp(out Vector3 trgPos)
        {
            Vector3 a = -GClass782.NormalizeFastSelf(BotOwner.Memory.GoalEnemy.Direction);
            trgPos = Vector3.zero;
            float num = 0f;
            if (NavMesh.SamplePosition(BotOwner.Position + a * 2f / 2f, out NavMeshHit navMeshHit, 1f, -1))
            {
                trgPos = navMeshHit.position;
                Vector3 a2 = trgPos - BotOwner.Position;
                float magnitude = a2.magnitude;
                if (magnitude != 0f)
                {
                    Vector3 a3 = a2 / magnitude;
                    num = magnitude;
                    if (NavMesh.SamplePosition(BotOwner.Position + a3 * 2f, out navMeshHit, 1f, -1))
                    {
                        trgPos = navMeshHit.position;
                        num = (trgPos - BotOwner.Position).magnitude;
                    }
                }
            }
            if (num != 0f && num > BotOwner.Settings.FileSettings.Move.REACH_DIST)
            {
                this.NavPath.ClearCorners();
                if (NavMesh.CalculatePath(BotOwner.Position, trgPos, -1, this.NavPath) && this.NavPath.status == NavMeshPathStatus.PathComplete)
                {
                    trgPos = this.NavPath.corners[this.NavPath.corners.Length - 1];
                    return this.CheckLength(this.NavPath, num);
                }
            }
            return false;
        }

        private bool CheckLength(NavMeshPath path, float straighDist)
        {
            return path.CalculatePathLength() < straighDist * 1.2f;
        }

        // Token: 0x04000C27 RID: 3111
        private readonly NavMeshPath NavPath = new NavMeshPath();

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;

        public override void Start()
        {
            Sound = BotOwner.BotsGroup.YoungestPlace(BotOwner, 150f, true);
            if (Sound != null)
            {
                MovePos = Sound.Position;
                SAIN.Mover.GoToPoint(MovePos, false, false);
            }
        }

        private GClass270 Sound;

        public override void Stop()
        {
        }
    }
}