using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.Components;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Solo
{
    public class StandAndShootAction : SAINAction
    {
        public StandAndShootAction(BotOwner bot) : base(bot, nameof(StandAndShootAction))
        {
        }

        public override void Update()
        {
            SAIN.Steering.SteerByPriority();

            if (!Stopped && Time.time - StartTime > 1f || SAIN.Cover.CheckLimbsForCover())
            {
                Stopped = true;
                BotOwner.StopMove();
            }

            Shoot.Update();

            if (SAIN.Cover.BotIsAtCoverPoint())
            {
                return;
            }
            else
            {
                bool prone = SAIN.Mover.Prone.ShallProne(true);
                SAIN.Mover.Prone.SetProne(prone);
            }
        }

        private float StartTime = 0f;
        private bool Stopped = false;

        public override void Start()
        {
            SAIN.Mover.Sprint(false);
            StartTime = Time.time;
        }

        public override void Stop()
        {
        }
    }
}