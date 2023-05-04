using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using Movement.Components;
using static Movement.UserSettings.Debug;

namespace SAIN.Movement.Layers
{
    namespace DogFight
    {
        internal class DogFightLogic : CustomLogic
        {
            public DogFightLogic(BotOwner bot) : base(bot)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
                Targeting = new UpdateTarget(bot);
                Move = new UpdateMove(bot);
                Steering = new UpdateSteering(bot);
                Decisions = new BotDecision(bot);
            }

            public override void Start()
            {
                BotOwner.PatrollingData.Pause();
            }

            public override void Stop()
            {
                BotOwner.PatrollingData.Unpause();
            }

            public override void Update()
            {
                Decisions.GetDecision();

                Move.Update(DebugMode, DebugDrawPoints);

                if (CanShootEnemyAndVisible)
                {
                    Targeting.Update();
                }

                Steering.Update(Move.IsSprintingFallback, DebugMode);
            }

            private readonly UpdateTarget Targeting;
            private readonly UpdateMove Move;
            private readonly UpdateSteering Steering;
            public readonly BotDecision Decisions;

            public bool EnemyIsNull => BotOwner.Memory.GoalEnemy == null;
            public bool CanShootEnemyAndVisible => CanShootEnemy && CanSeeEnemy;
            public bool CanShootEnemy => !EnemyIsNull && BotOwner.Memory.GoalEnemy.IsVisible;
            public bool CanSeeEnemy => !EnemyIsNull && BotOwner.Memory.GoalEnemy.CanShoot;
            public bool DebugMode => DebugDogFightLayer.Value;
            public bool DebugDrawPoints => DebugDogFightLayerDraw.Value;

            public ManualLogSource Logger;
        }
    }
}