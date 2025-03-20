using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Squad
{
    internal class HuntAction : CombatAction, ISAINAction
    {
        public HuntAction(BotOwner bot) : base(bot, nameof(HuntAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData data)
        {
        }

        public override void Start()
        {
            Toggle(true);
        }

        public override void Stop()
        {
            Toggle(false);
        }

        
    }

    internal class SquadHuntAction : CombatAction, ISAINAction
    {
        public SquadHuntAction(BotOwner bot) : base(bot, nameof(SquadHuntAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData data)
        {
        }

        public override void Start()
        {
            Toggle(true);
        }

        public override void Stop()
        {
            Toggle(false);
        }
    }
}