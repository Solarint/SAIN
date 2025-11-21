using SAIN.Components;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class CurrentTargetClass : BotComponentClassBase
{
    public CurrentTargetClass(BotComponent bot) : base(bot)
    {
        TickRequirement = ESAINTickState.OnlyBotActive;
    }

    public override void ManualUpdate()
    {
        UpdateGoalTarget();
    }

    private void UpdateGoalTarget()
    {
        if (_updateGoalTargetTime < Time.time)
        {
            _updateGoalTargetTime = Time.time + 0.5f;

            var goalTarget = BotOwner.Memory.GoalTarget;
            var Target = goalTarget?.Position;
            if (Target != null)
            {
                if ((Target.Value - Bot.Position).sqrMagnitude < 1f ||
                    goalTarget.CreatedTime > 120f)
                {
                    goalTarget.Clear();
                    //BotOwner.CalcGoal();
                }
            }
        }
    }

    private float _updateGoalTargetTime;
}