using EFT;
using System.Collections;
using UnityEngine;
using DrakiaXYZ.BigBrain.Brains;

namespace SAIN.Layers.Combat.Solo
{
    internal class DogFightAction : CombatAction, ISAINAction
    {
        public DogFightAction(BotOwner bot) : base(bot, "Dog Fight")
        {
        }

        public override void Update(CustomLayer.ActionData actionData)
        {
            Bot.Mover.SetTargetPose(1f);
            Bot.Mover.SetTargetMoveSpeed(1f);
            Bot.Steering.SteerByPriority();
            Bot.Mover.DogFight.DogFightMove(true);
            Shoot.CheckAimAndFire();
        }

        public override void Start()
        {
            Toggle(true);
            Bot.Mover.Sprint(false);
            BotOwner.Mover.SprintPause(0.5f);
        }

        public override void Stop()
        {
            Toggle(false);
            Bot.Mover.DogFight.ResetDogFightStatus();
            BotOwner.MovementResume();
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }
    }
}