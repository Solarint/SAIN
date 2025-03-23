using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using System.Collections;
using UnityEngine.Profiling;

namespace SAIN.Layers.Combat.Solo
{
    internal class ShootAction : CombatAction, ISAINAction
    {
        public ShootAction(BotOwner bot) : base(bot, nameof(ShootAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Start()
        {
            Toggle(true);
        }

        public override void Stop()
        {
            Toggle(false);
        }

        public override void Update(CustomLayer.ActionData actionData)
        {
            this.StartProfilingSample("Update");
            Bot.Steering.SteerByPriority();
            Shoot.CheckAimAndFire();
            this.EndProfilingSample();
        }
    }
}