using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using EFT.Interactive;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace SAIN.Layers
{
    internal class ExtractAction : CustomLogic
    {
        public ExtractAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot);
        }

        private ShootClass Shoot;

        private Vector3? Exfil => SAIN.ExfilPosition;

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }

        public override void Update()
        {
            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Mover.SetTargetPose(1f);
            float stamina = BotOwner.GetPlayer.Physical.Stamina.NormalValue;
            if (stamina > 0.5f)
            {
                if (SAIN.Enemy != null && SAIN.Enemy.RealDistance < 50f)
                {
                    SAIN.Mover.Sprint(false);
                    SAIN.Steering.SteerByPriority();
                    Shoot.Update();
                }
                else
                {
                    SAIN.Mover.Sprint(true);
                }
            }
            else
            {
                if (stamina < 0.1f || (SAIN.Enemy != null && SAIN.Enemy.RealDistance < 100f))
                {
                    SAIN.Mover.Sprint(false);
                    SAIN.Steering.SteerByPriority();
                    Shoot.Update();
                }
            }
            GoTo(Exfil.Value);
        }

        private void GoTo(Vector3 point)
        {
            if ((point - BotOwner.Position).sqrMagnitude < 6f)
            {
                if (ExtractTimer == -1f)
                {
                    Logger.LogInfo($"{BotOwner.name} Starting Extract Timer");
                    ExtractTimer = Time.time + 5f;
                }

                if (ExtractTimer < Time.time)
                {
                    Logger.LogInfo($"{BotOwner.name} Extracted");
                    Singleton<IBotGame>.Instance.BotUnspawn(BotOwner);
                }
                return;
            }
            else
            {
                ExtractTimer = -1f;
                SAIN.Mover.Sprint(true);
                SAIN.Mover.GoToPoint(point);
                BotOwner.DoorOpener.Update();
            }
        }
        private float ExtractTimer = -1f;
        private ManualLogSource Logger;
    }
}