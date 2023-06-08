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
        }

        public override void Update()
        {
        }

        public ManualLogSource Logger;

        public override void Start()
        {
            Vector3 targetPosition;
            if (BotOwner.Memory.GoalEnemy != null)
            {
                targetPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
            }
            else if (BotOwner.Memory.GoalTarget?.GoalTarget?.Position != null)
            {
                targetPosition = BotOwner.Memory.GoalTarget.GoalTarget.Position;
            }
            else
            {
                targetPosition = BotOwner.Transform.position;
            }

            BotOwner.GetOrAddComponent<EnemySearchComponent>().Init(targetPosition);
        }

        public override void Stop()
        {
             BotOwner.gameObject.GetComponent<EnemySearchComponent>()?.Dispose();
        }
    }
}