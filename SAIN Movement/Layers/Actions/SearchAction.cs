using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using UnityEngine.AI;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class SearchAction : CustomLogic
    {
        public SearchAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            this.gclass105_0 = new GClass105(bot);
        }

        private GClass105 gclass105_0;

        public override void Update()
        {
            if (EnemyPosition == null)
            {
                return;
            }

            var sainEnemy = SAIN.Core.Enemy;

            if (Vector3.Distance(BotOwner.Transform.position, EnemyPosition.Value) < 2f)
            {
                int i = 0;
                while (i < 5)
                {
                    Vector3 randomPoint = Random.onUnitSphere * 10f;
                    randomPoint.y = 0f;
                    if (NavMesh.SamplePosition(randomPoint + EnemyPosition.Value, out var hit, 2f, NavMesh.AllAreas))
                    {
                        EnemyPosition = hit.position;
                        break;
                    }
                }
            }

            SAIN.Steering.ManualUpdate();

            if (BotOwner.Memory.GoalEnemy != null)
            {
                if (sainEnemy.CanSee)
                {
                    gclass105_0.Update();
                }

                SAIN.BotOwner.MoveToEnemyData.TryMoveToEnemy(EnemyPosition.Value);
            }
            else
            {
                BotOwner.GoToPoint(EnemyPosition.Value);
            }
        }

        private Vector3? EnemyPosition;
        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;
        public bool DebugDrawPoints => DebugLayersDraw.Value;

        public ManualLogSource Logger;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();

            if (BotOwner.Memory.GoalEnemy != null)
            {
                EnemyPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
            }
            else if (BotOwner.Memory.GoalTarget?.Position != null)
            {
                EnemyPosition = BotOwner.Memory.GoalTarget.Position.Value;
            }
            else
            {
                EnemyPosition = BotOwner.Transform.position;
            }
        }

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }
    }
}