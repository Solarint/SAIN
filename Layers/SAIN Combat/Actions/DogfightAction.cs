using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Layers;
using UnityEngine.AI;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class DogfightAction : CustomLogic
    {
        public DogfightAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            gclass105_0 = new GClass105(bot);
        }

        private GClass105 gclass105_0;

        public override void Update()
        {
            if (SAIN.Enemy?.InLineOfSight == true)
            {
                BotOwner.Steering.LookToPoint(SAIN.Enemy.EnemyChestPosition);
            }
            else
            {
                SAIN.Steering.ManualUpdate();
            }

            if (BotOwner.Memory.GoalEnemy != null)
            {
                if (SAIN.HasEnemyAndCanShoot)
                {
                    gclass105_0.Update();
                }

                if (SAIN.Enemy?.IsVisible == true)
                {
                    if (BackUp(out var pos))
                    {
                        BotOwner.GoToPoint(pos, false, -1, false, false);
                    }
                    else
                    {
                        BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
                    }
                }
                else
                {
                    BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
                }
            }
        }

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
                this.navMeshPath_0.ClearCorners();
                if (NavMesh.CalculatePath(BotOwner.Position, trgPos, -1, this.navMeshPath_0) && this.navMeshPath_0.status == NavMeshPathStatus.PathComplete)
                {
                    trgPos = this.navMeshPath_0.corners[this.navMeshPath_0.corners.Length - 1];
                    return this.CheckLength(this.navMeshPath_0, num);
                }
            }
            return false;
        }
        private bool CheckLength(NavMeshPath path, float straighDist)
        {
            return path.CalculatePathLength() < straighDist * 1.2f;
        }

        // Token: 0x04000C27 RID: 3111
        private readonly NavMeshPath navMeshPath_0 = new NavMeshPath();

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}