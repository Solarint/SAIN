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
    internal class DogFight : CustomLogic
    {
        public DogFight(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private readonly ShootClass Shoot;

        public override void Update()
        {
            if (SAIN.Enemy == null)
            {
                return;
            }

            SAIN.Mover.SetTargetPose(1f);
            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Steering.SteerByPriority(false);
            bool EnemyVisible = SAIN.Enemy.IsVisible;
            if (EnemyVisible && BackUp(out var pos))
            {
                BotOwner.GoToPoint(pos, false, -1, false, false, false);
            }
            else if (!EnemyVisible && (SAIN.Enemy.Position - BotOwner.Position).sqrMagnitude > 2f)
            {
                BotOwner.MoveToEnemyData.TryMoveToEnemy(SAIN.Enemy.Position);
            }

            Shoot.Update();
        }

        private bool BackUp(out Vector3 trgPos)
        {
            Vector3 a = -GClass792.NormalizeFastSelf(SAIN.Enemy.Direction);
            trgPos = Vector3.zero;
            float num = 0f;
            Vector3 random = Random.onUnitSphere * 1f;
            random.y = 0f;
            if (NavMesh.SamplePosition((BotOwner.Position + a * 2f / 2f) + random, out NavMeshHit navMeshHit, 1f, -1))
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
            SAIN.Mover.Sprint(false);
        }

        public override void Stop()
        {
        }
    }
}