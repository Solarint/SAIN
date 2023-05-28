using UnityEngine;

namespace SAIN.Classes
{
    public class EnemyTalkObject
    {
        public EnemyTalkObject(EPhraseTrigger trigger, ETagStatus status, bool withGroupDelay)
        {
            Trigger = trigger;
            Status = status;
            WithGroupDelay = withGroupDelay;
            TalkTime = Time.time;
            TalkDelay = Random.Range(0.75f, 1.5f);
        }

        public float TalkDelay = 0f;
        public float TalkTime { get; private set; }
        public bool WithGroupDelay { get; private set; }
        public EPhraseTrigger Trigger { get; private set; }
        public ETagStatus Status { get; private set; }
    }
}