using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using Movement.Components;
using Movement.Helpers;
using Movement.UserSettings;
using System.Configuration;
using static Movement.UserSettings.Debug;
using System;

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
                Move.Update(DebugMode, DebugDrawPoints);

                Steering.Update(Move.FallingBack, DebugMode);

                if (!EnemyIsNull && CanShootEnemyAndVisible)
                {
                    Targeting.Update();
                }
            }

            private readonly UpdateTarget Targeting;
            private readonly UpdateMove Move;
            private readonly UpdateSteering Steering;

            public bool EnemyIsNull => BotOwner.Memory.GoalEnemy == null;
            public bool CanShootEnemyAndVisible => CanShootEnemy && CanSeeEnemy;
            public bool CanShootEnemy => BotOwner.Memory.GoalEnemy.IsVisible;
            public bool CanSeeEnemy => BotOwner.Memory.GoalEnemy.CanShoot;
            public bool DebugMode => DebugDogFightLayer.Value;
            public bool DebugDrawPoints => DebugDogFightLayerDraw.Value;

            public ManualLogSource Logger;
        }
    }
}