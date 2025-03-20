using DrakiaXYZ.BigBrain.Brains;
using EFT;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Run
{
    internal class RunningAction : CombatAction, ISAINAction
    {
        public RunningAction(BotOwner bot) : base(bot, nameof(RunningAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            Bot.Mover.SetTargetPose(1f);
            Bot.Mover.SetTargetMoveSpeed(1f);

            if (nextRandomRunTime > Time.time && (_runDestination - Bot.Position).sqrMagnitude < 1f)
            {
                nextRandomRunTime = 0f;
            }

            if (!Bot.Mover.SprintController.Running
                && findRandomPlace(out var path)
                && Bot.Mover.SprintController.RunToPoint(_runDestination, SAINComponent.Classes.Mover.ESprintUrgency.High, false))
            {
                nextRandomRunTime = Time.time + 20f;
            }
        }

        private Vector3 _runDestination;
        private float nextRandomRunTime;

        public override void Start()
        {
            Toggle(true);
        }

        private bool findRandomPlace(out NavMeshPath path)
        {
            for (int i = 0; i < 10;  i++)
            {
                Vector3 random = UnityEngine.Random.onUnitSphere * 100f;
                if (NavMesh.SamplePosition(random + Bot.Position, out var hit, 10f, -1))
                {
                    path = new NavMeshPath();
                    if (NavMesh.CalculatePath(Bot.Position, hit.position, -1, path))
                    {
                        _runDestination = path.corners[path.corners.Length - 1];
                        return true;
                    }
                }
            }
            path = null;
            return false;
        }

        public override void Stop()
        {
            Toggle(false);
            Bot.Mover.SprintController.CancelRun();
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("Run Info");
            var cover = Bot.Cover;
            stringBuilder.AppendLabeledValue("Run State", $"{Bot.Mover.SprintController.CurrentRunStatus}", Color.white, Color.yellow, true);
        }
    }
}