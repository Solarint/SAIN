using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Layers
{
    internal class RegroupAction : CustomLogic
    {
        public RegroupAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override void Update()
        {
            if (SAIN.Steering.SteerByPriority())
            {
                SAIN.Mover.Sprint(false);
            }
            else
            {
                Regroup();
            }
        }

        private float UpdateTimer = 0f;

        private void Regroup()
        {
            if (UpdateTimer < Time.time)
            {
                UpdateTimer = Time.time + 3f;
                MoveToLead();
            }
        }


        private readonly SAINComponent SAIN;

        public override void Start()
        {
            MoveToLead();
        }

        private void MoveToLead()
        {
            var SquadLeadPos = SAIN.Squad.LeaderComponent?.BotOwner.Position;
            if (SquadLeadPos != null)
            {
                SAIN.Mover.SetTargetPose(1f);
                SAIN.Mover.SetTargetMoveSpeed(1f);
                SAIN.Mover.GoToPoint(SquadLeadPos.Value);
                CheckShouldSprint(SquadLeadPos.Value);
                BotOwner.DoorOpener.Update();
            }
        }

        private void CheckShouldSprint(Vector3 pos)
        {
            bool hasEnemy = SAIN.HasEnemy;
            bool enemyLOS = SAIN.Enemy?.InLineOfSight == true;
            float leadDist = (pos - BotOwner.Position).magnitude;
            float enemyDist = hasEnemy ? (SAIN.Enemy.Person.Position - BotOwner.Position).magnitude : 999f;

            bool sprint = hasEnemy && leadDist > 30f && enemyLOS && enemyDist > 50f;

            if (SAIN.Steering.SteerByPriority())
            {
                sprint = false;
            }
            else if (sprint)
            {
                SAIN.Steering.LookToMovingDirection();
            }
            else
            {
                SAIN.Steering.LookToRandomPosition();
            }
            SAIN.Mover.Sprint(sprint);
        }

        public override void Stop()
        {
        }

        public ManualLogSource Logger;
    }
}