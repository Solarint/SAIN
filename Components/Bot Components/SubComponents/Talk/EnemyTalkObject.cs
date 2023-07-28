using UnityEngine;

namespace SAIN.Classes
{
    public class EnemyTalkObject
    {
        public EnemyTalkObject()
        {
            TalkTime = Time.time;
            TalkDelay = Random.Range(0.75f, 1.5f);
        }

        public float TalkDelay { get; private set; }
        public float TalkTime { get; private set; }
    }
}