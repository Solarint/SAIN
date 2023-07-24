using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using System.Drawing;

namespace SAIN.Layers
{
    internal class ExtractAction : CustomLogic
    {
        public ExtractAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private ShootClass Shoot;

        private Vector3? Exfil => SAIN.ExfilPosition;

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData?.Pause();
        }

        public override void Stop()
        {
            BotOwner.PatrollingData?.Unpause();
        }

        public override void Update()
        {
            float stamina = BotOwner.GetPlayer.Physical.Stamina.NormalValue;
            if (SAIN.Enemy != null && SAIN.Enemy.Seen && (SAIN.Enemy.PathDistance < 50f || SAIN.Enemy.IsVisible))
            {
                NoSprint = true;
                SAIN.Steering.SteerByPriority();
                Shoot.Update();
            }
            else if (stamina > 0.5f)
            {
                NoSprint = false;
            }
            else if (stamina < 0.1f)
            {
                NoSprint = true;
            }

            Vector3 point = Exfil.Value;
            float distance = (point - BotOwner.Position).sqrMagnitude;

            if (distance < 1f)
            {
                SAIN.Mover.SetTargetPose(0f);
                NoSprint = true;
            }
            else
            {
                SAIN.Mover.SetTargetPose(1f);
                SAIN.Mover.SetTargetMoveSpeed(1f);
            }

            if (ExtractStarted)
            {
                StartExtract(point);
            }
            else
            {
                MoveToExtract(distance, point);
            }

            if (BotOwner.BotState == EBotState.Active)
            {
                if (NoSprint)
                {
                    SAIN.Mover.Sprint(false);
                    SAIN.Steering.SteerByPriority();
                    Shoot.Update();
                }
                else
                {
                    SAIN.Steering.LookToMovingDirection();
                }
            }
        }

        private bool NoSprint;

        private void MoveToExtract(float distance, Vector3 point)
        {
            if (distance > 12f)
            {
                ExtractStarted = false;
            }
            if (distance < 6f)
            {
                ExtractStarted = true;
            }

            if (!ExtractStarted)
            {
                if (ReCalcPathTimer < Time.time)
                {
                    ExtractTimer = -1f;
                    ReCalcPathTimer = Time.time + 4f;
                    if (NoSprint)
                    {
                        BotOwner.Mover.GoToPoint(point, true, 0.5f, false, false);
                    }
                    else
                    {
                        BotOwner.BotRun.Run(point, false);
                    }
                }
            }
        }

        private void StartExtract(Vector3 point)
        {
            if (ExtractTimer == -1f)
            {
                float timer = 5f * Random.Range(0.75f, 1.5f);
                Logger.LogInfo($"{BotOwner.name} Starting Extract Timer of {timer}");
                ExtractTimer = Time.time + timer;
            }

            if (ExtractTimer < Time.time)
            {
                Logger.LogInfo($"{BotOwner.name} Extracted at {point} at {System.DateTime.UtcNow}");

                var botgame = Singleton<IBotGame>.Instance;
                BotOwner.Deactivate();
                BotOwner.Dispose();
                botgame.BotsController.BotDied(BotOwner);
                botgame.BotsController.DestroyInfo(BotOwner.GetPlayer);
                Object.DestroyImmediate(BotOwner.gameObject);
                Object.Destroy(BotOwner);
            }
        }

        private bool ExtractStarted = false;
        private float ReCalcPathTimer = 0f;
        private float ExtractTimer = -1f;
        private readonly ManualLogSource Logger;
    }
}