using Comfort.Common;
using EFT;
using System;
using UnityEngine;

namespace SAIN.Components.BotController
{
    public class BotExtractManager
    {
        public void Update()
        {
            if (CheckExtractTimer < Time.time)
            {
                CheckExtractTimer = Time.time + 5f;
                CheckTimeRemaining();
                Console.WriteLine($"Extra Time Remainging: {TimeRemaining} Percentage of raid left {PercentageRemaining}");
            }
        }

        private float CheckExtractTimer = 0f;
        public float TimeRemaining { get; private set; }
        public float PercentageRemaining { get; private set; }

        public void CheckTimeRemaining()
        {
            var GameTime = Singleton<AbstractGame>.Instance?.GameTimer;
            if (GameTime?.StartDateTime != null && GameTime?.EscapeDateTime != null)
            {
                var StartTime = GameTime.StartDateTime.Value;
                var EscapeTime = GameTime.EscapeDateTime.Value;
                var Span = EscapeTime - StartTime;
                float TotalSeconds = (float)Span.TotalSeconds;
                TimeRemaining = EscapeTimeSeconds(GameTime);
                float ratio = TimeRemaining / TotalSeconds;
                PercentageRemaining = Mathf.Round(ratio * 100f);
            }
        }

        public float EscapeTimeSeconds(GameTimerClass timer)
        {
            DateTime? escapeDateTime = timer.EscapeDateTime;
            return (float)((escapeDateTime != null) ? (escapeDateTime.Value - GClass1251.UtcNow) : TimeSpan.MaxValue).TotalSeconds;
        }
    }
}
