using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Components
{
    public class PlayerTalkComponent : MonoBehaviour
    {
        public Player Player { get; private set; }

        private void Awake()
        {
            Player = GetComponent<Player>();
        }

        private void Update()
        {
            if (TalkTime != 0)
            {
                TimeSinceTalk = Time.time - TalkTime;

                if (TimeSinceTalk > 1.5f)
                {
                    TalkTime = 0f;
                    PlayerTalked = false;
                }
            }
        }

        public void TalkEvent(EPhraseTrigger trigger, ETagStatus status, bool aggressive)
        {
            TalkTime = Time.time;
            PlayerTalked = true;

            TalkAggressive = aggressive;
            PhraseSaid = trigger;
            TagStatus = status;
        }

        public bool TalkAggressive { get; private set; } = false;
        public bool PlayerTalked { get; private set; } = false;
        public EPhraseTrigger PhraseSaid { get; private set; } = EPhraseTrigger.PhraseNone;
        public ETagStatus TagStatus { get; private set; } = ETagStatus.Unaware;
        public float TalkTime { get; private set; } = 0f;
        public float TimeSinceTalk { get; private set; } = 0f;
    }
}