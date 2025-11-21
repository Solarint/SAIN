using DrakiaXYZ.BigBrain.Brains;
using EFT;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Run;

internal class RunningAction(BotOwner bot) : BotAction(bot, nameof(RunningAction)), IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        Bot.Mover.SetTargetPose(1f);
        Bot.Mover.SetTargetMoveSpeed(1f);


        if (!Bot.Mover.Moving)
            nextRandomRunTime = 0f;
        else if (nextRandomRunTime > Time.time && (_runDestination - Bot.Position).sqrMagnitude < 2f)
        {
            nextRandomRunTime = 0f;
        }

        if (nextRandomRunTime > Time.time)
            return;

        if (findRandomPlace(out var path) && Bot.Mover.RunToPoint(_runDestination, false, -1, SAINComponent.Classes.Mover.ESprintUrgency.High, true))
        {
            nextRandomRunTime = Time.time + 20f;
        }
    }

    private Vector3 _runDestination;
    private float nextRandomRunTime;

    private bool findRandomPlace(out NavMeshPath path)
    {
        for (int i = 0; i < 10; i++)
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

    public override void BuildDebugText(StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine("Run Info");
        stringBuilder.AppendLabeledValue("Run State", $"{Bot.Mover.ActivePath?.CurrentSprintStatus}", Color.white, Color.yellow, true);
    }
}