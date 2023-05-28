using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Helpers;
using SAIN_Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Talk.Components
{
    public class BotTalkComponent : MonoBehaviour
    {
        public BotTalk Talk { get; private set; }
        public EnemyTalk EnemyTalk { get; private set; }
        public GroupTalk GroupTalk { get; private set; }

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            Init();
        }

        private void Init()
        {
            Talk = new BotTalk(BotOwner);
            GroupTalk = new GroupTalk(BotOwner);
            EnemyTalk = new EnemyTalk(BotOwner);
        }

        private const float CheckFreq = 0.33f;
        private float CheckTalkEventTimer = 0f;

        private void Update()
        {
            if (BotOwner?.HealthController?.IsAlive == false || BotOwner.BotState != EBotState.Active)
            {
                return;
            }

            Talk.ManualUpdate();

            GroupTalk.ManualUpdate();

            EnemyTalk.ManualUpdate();

            if (CheckTalkEventTimer < Time.time)
            {
                CheckTalkEventTimer = Time.time + CheckFreq * Random.Range(0.75f, 1.25f);

                ResetTalk();
            }
        }

        public void TalkAfterDelay(TalkEventObject talk, float delay)
        {
            StartCoroutine(TalkDelayCoroutine(talk, delay));
        }

        private IEnumerator TalkDelayCoroutine(TalkEventObject talk, float delay)
        {
            yield return new WaitForSeconds(delay);
            Talk.Say(talk.PhraseSaid, talk.TagStatus, true);
        }

        private void ResetTalk()
        {
            if (RecentTalk != null)
            {
                TimeSinceTalk = Time.time - RecentTalk.TalkTime;

                if (TimeSinceTalk > Random.Range(1f, 2f))
                {
                    ThisBotTalked = false;
                    RecentTalk = null;
                }
            }
        }

        public void TalkEvent(EPhraseTrigger trigger, ETagStatus status, bool aggressive)
        {
            ThisBotTalked = true;
            RecentTalk = new TalkEventObject(trigger, status, aggressive);
        }

        public TalkEventObject RecentTalk { get; private set; }
        public bool ThisBotTalked { get; private set; } = false;
        public float TimeSinceTalk { get; private set; } = 0f;
        public BotOwner BotOwner { get; private set; }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
    }

    public class TalkEventObject
    {
        public TalkEventObject(EPhraseTrigger trigger, ETagStatus status, bool aggressive)
        {
            TalkTime = Time.time;
            TalkAggressive = aggressive;
            PhraseSaid = trigger;
            TagStatus = status;
        }

        public bool TalkAggressive { get; private set; } = false;
        public EPhraseTrigger PhraseSaid { get; private set; } = EPhraseTrigger.PhraseNone;
        public ETagStatus TagStatus { get; private set; } = ETagStatus.Unaware;
        public float TalkTime { get; private set; } = 0f;
    }
}