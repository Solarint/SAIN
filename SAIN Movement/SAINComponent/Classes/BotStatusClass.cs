using EFT;
using SAIN.Components;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Classes
{
    public class StatusClass : SAINBot
    {
        public StatusClass(BotOwner bot) : base(bot)
        {
        }

        public void Update()
        {
            HealthStatus = BotOwner.GetPlayer.HealthStatus;
        }

        public bool Healthy => HealthStatus == ETagStatus.Healthy;
        public bool Injured => HealthStatus == ETagStatus.Injured;
        public bool BadlyInjured => HealthStatus == ETagStatus.BadlyInjured;
        public bool Dying => HealthStatus == ETagStatus.Dying;

        private ETagStatus HealthStatus = ETagStatus.Healthy;
    }
}