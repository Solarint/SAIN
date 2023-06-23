using EFT;
using Newtonsoft.Json.Schema;
using SAIN.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.Classes
{
    public class EnemyBodyPart
    {
        public BodyPartType BodyPartType { get; private set; }
        public BodyPartClass BodyPartClass { get; private set; }
        public bool Visible;
        public bool CanShoot;
        public float TimeSinceVisible => Time.time - TimeVisibleActive;
        public float TimeSinceCanShoot => Time.time - TimeCanShootActive;
        public float BotReactionTime { get; private set; }
        public float Distance => Direction.magnitude;
        public Vector3 PartPosition => BodyPartClass.Position;
        public Vector3 Direction => PartPosition - BotTransform.position;
        public IAIDetails EnemyPerson { get; private set; }

        private readonly BifacialTransform BotTransform;

        public EnemyBodyPart(BifacialTransform botTransform, IAIDetails enemy, BodyPartType partType)
        {
            BotTransform = botTransform;
            EnemyPerson = enemy;
            BodyPartType = partType;
            BodyPartClass = enemy.MainParts[partType];
        }

        public void UpdateVisibilty(bool value, float seenCoef)
        {
            BotReactionTime = 0.1f;
            if (value)
            {
                if (TimeVisibleActive == -1f)
                {
                    TimeVisibleActive = Time.time;
                }
            }
            else
            {
                TimeVisibleActive = -1f;
            }
            Visible = value;
        }

        public void UpdateCanShoot(bool value, float seenCoef)
        {
            BotReactionTime = 0.1f;
            if (value)
            {
                if (TimeCanShootActive == -1f)
                {
                    TimeCanShootActive = Time.time;
                }
            }
            else
            {
                TimeCanShootActive = -1f;
            }
            CanShoot = value;
        }

        private float TimeVisibleActive = -1f;
        private float TimeCanShootActive = -1f;
    }
}
