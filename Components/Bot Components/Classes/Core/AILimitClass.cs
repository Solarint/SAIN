using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Classes
{
    public class AILimitClass : SAINBot
    {
        public AILimitClass(BotOwner owner) : base(owner) { }

        public float TimeAdd { get; private set; }
        public float Timer { get; private set; }
        public bool Enabled => Timer > Time.time;

        private float UpdateLimitFreqTimer = 0f;

        public void Update()
        {
            if (UpdateLimitFreqTimer < Time.time)
            {
                UpdateLimitFreqTimer = Time.time + 0.5f;

                float TimeAdd;
                if (BotOwner.Memory.GoalEnemy?.Person?.GetPlayer?.ProfileId == SAINBotController.MainPlayer?.ProfileId)
                {
                    TimeAdd = 0f;
                }
                else if (SAIN.GoalTargetPos != null && (SAIN.GoalTargetPos.Value - SAINBotController.MainPlayerPosition).sqrMagnitude < 2500f)
                {
                    TimeAdd = 0f;
                }
                else
                {
                    float distanceToPlayer = SAIN.DistanceToMainPlayer;
                    if (distanceToPlayer > 500f)
                    {
                        TimeAdd = 3f;
                    }
                    else if (distanceToPlayer > 300f)
                    {
                        TimeAdd = 1f;
                    }
                    else if (distanceToPlayer > 200f)
                    {
                        TimeAdd = 0.33f;
                    }
                    else if (distanceToPlayer > 150f)
                    {
                        TimeAdd = 0.1f;
                    }
                    else
                    {
                        TimeAdd = 0f;
                    }
                }

                Timer = TimeAdd == 0f ? 0f : Time.time + TimeAdd;
                this.TimeAdd = TimeAdd;
            }
        }
    }
}
