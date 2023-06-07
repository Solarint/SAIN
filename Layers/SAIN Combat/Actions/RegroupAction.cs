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

        private Vector3? SquadLeadPos;

        public override void Update()
        {
            SAIN.Steering.ManualUpdate();

            if (SAIN.HasEnemy && SAIN.EnemyIsVisible)
            {
                BotOwner.GetPlayer.EnableSprint(false);
                SAIN.Steering.ManualUpdate();
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
                UpdateTimer = Time.time + 1f;
                BotOwner.DoorOpener.Update();

                UpdateLeaderPosition();

                if (SquadLeadPos != null)
                {
                    MoveToPoint(SquadLeadPos.Value);
                    CheckShouldSprint();
                }
            }
        }

        private void MoveToPoint(Vector3 point)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.GoToPoint(point, true, -1, false, false);
        }

        private void CheckShouldSprint()
        {
            float dist = (SquadLeadPos.Value - BotOwner.Position).magnitude;
            if (dist < 30f)
            {
                BotOwner.GetPlayer.EnableSprint(false);
                SAIN.Steering.ManualUpdate();
            }
            else
            {
                if (SAIN.HasEnemy && SAIN.EnemyInLineOfSight && Vector3.Distance(SAIN.Enemy.SAINEnemy.Person.Position, BotOwner.Position) < 50f)
                {
                    BotOwner.GetPlayer.EnableSprint(false);
                    SAIN.Steering.ManualUpdate();
                }
                else
                {
                    BotOwner.GetPlayer.EnableSprint(true);
                    BotOwner.Steering.LookToMovingDirection();
                }
            }
        }

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            UpdateLeaderPosition();

            if (SquadLeadPos != null)
            {
                MoveToPoint(SquadLeadPos.Value);
            }
        }

        private void UpdateLeaderPosition()
        {
            SquadLeadPos = null;
            if (SAIN.BotSquad.SquadMembers != null)
            {
                foreach (var member in SAIN.BotSquad.SquadMembers.Values)
                {
                    if (member != null)
                    {
                        if (member.BotSquad.IsSquadLead)
                        {
                            SquadLeadPos = member.BotOwner.Position;
                        }
                    }
                }
            }
        }

        public override void Stop()
        {
        }

        public ManualLogSource Logger;
    }
}