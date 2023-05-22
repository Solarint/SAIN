using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class DebugMoveToPlayerAction : CustomLogic
    {
        public DebugMoveToPlayerAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            this.gclass105_0 = new GClass105(bot);
        }

        private GClass105 gclass105_0;

        public override void Update()
        {
            var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            var playerPos = mainPlayer.Transform.position;
            if (BotOwner.Memory.GoalEnemy != null)
            {
                if (BotOwner.Memory.GoalEnemy.Person.GetPlayer == mainPlayer)
                {
                    if (BotOwner.Memory.GoalEnemy.CanShoot)
                    {
                        gclass105_0.Update();
                    }
                }
            }

            if (Vector3.Distance(BotOwner.Transform.position, playerPos) > 30f)
            {
                BotOwner.GetPlayer.EnableSprint(true);
                BotOwner.Steering.LookToMovingDirection();
            }

            NavMeshPath Path = new NavMeshPath();
            if (NavMesh.CalculatePath(BotOwner.Transform.position, playerPos, -1, Path))
            {
                if (Path.status == NavMeshPathStatus.PathComplete)
                {
                    BotOwner.GoToPoint(playerPos, true, 20f, false, false);
                }
                if (Path.status == NavMeshPathStatus.PathPartial)
                {
                    BotOwner.GoToPoint(Path.corners[Path.corners.Length - 1], true, 20f, false, false);
                }
            }
        }

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;
        public bool DebugDrawPoints => DebugLayersDraw.Value;

        public ManualLogSource Logger;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }
    }
}