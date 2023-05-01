using EFT;
using Movement.Components;
using UnityEngine;
using BepInEx.Logging;
using JetBrains.Annotations;
using SAIN_Helpers;
using UnityEngine.AI;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using static SAIN_Helpers.SAIN_Math;

namespace SAIN.Movement.Components.DogFight_State
{
    public class DogFightUpdate : GClass125
    {
        public DogFightUpdate(BotOwner bot) : base(bot)
        {
            this.gclass105_0 = new GClass105(bot);
        }

        public override void Update()
        {

            var goalEnemy = botOwner_0.Memory.GoalEnemy;

            if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
            {
                botOwner_0.Steering.LookToPoint(goalEnemy.CurrPosition);

                gclass105_0.Update();
                return;
            }

            botOwner_0.LookData.SetLookPointByHearing(null);
        }

        private readonly GClass105 gclass105_0;
    }
}