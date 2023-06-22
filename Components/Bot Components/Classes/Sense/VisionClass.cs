using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace SAIN.Classes.Sense
{
    public class VisionClass : SAINBot
    {
        public VisionClass(BotOwner owner) : base(owner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            FlashLightDazzle = new FlashLightDazzle(owner);
        }

        public void Update()
        {
            if (SAIN.Enemy != null && SAIN.Enemy.IsVisible)
            {
                FlashLightDazzle.CheckIfDazzleApplied(SAIN.Enemy.Person);
            }
        }

        public float GetSeenCoef(BifacialTransform enemy, AiDataClass aiData, float personalLastSeenTime, Vector3 personalLastSeenPos)
        {
            float num = 1f;
            if (Time.time - personalLastSeenTime < BotOwner.Settings.FileSettings.Look.SEC_REPEATED_SEEN && (double)(personalLastSeenPos - enemy.position).sqrMagnitude < this.BotOwner.Settings.FileSettings.Look.DIST_SQRT_REPEATED_SEEN)
            {
                num = this.BotOwner.Settings.FileSettings.Look.COEF_REPEATED_SEEN;
            }
            Vector3 from = BotOwner.Transform.rotation * Vector3.forward;
            Vector3 to = enemy.position - BotOwner.Transform.position;
            float num2;
            if (this.BotOwner.LookSensor.IsFullSectorView)
            {
                num2 = 0.1f;
            }
            else
            {
                num2 = Vector3.Angle(from, to);
            }
            float time = num2 / 90f;
            if (num2 > 90f)
            {
                return 8888f;
            }
            float magnitude = (enemy.position - BotOwner.Transform.position).magnitude;
            float num3 = BotOwner.Settings.Curv.VisionAngCoef.Evaluate(time);
            float num4 = 1f - num3;
            float num5 = magnitude;
            float max_DIST_CLAMP_TO_SEEN_SPEED = this.BotOwner.Settings.FileSettings.Look.MAX_DIST_CLAMP_TO_SEEN_SPEED;
            if (magnitude > max_DIST_CLAMP_TO_SEEN_SPEED)
            {
                num5 = max_DIST_CLAMP_TO_SEEN_SPEED;
            }
            return num5 * BotOwner.Settings.Current.CurrentGainSightCoef * aiData.VisibilityCoef * num4 * num;
        }

        public FlashLightDazzle FlashLightDazzle { get; private set; }

        private readonly ManualLogSource Logger;
    }
}
