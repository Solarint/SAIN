using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using UnityEngine;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using Systems.Effects;
using EFT.Interactive;
using System.Linq;
using SAIN.Components.BotController;
using UnityEngine.AI;

namespace SAIN.Layers
{
    internal class ExtractAction : SAINAction
    {
        public static float MinDistanceToStartExtract { get; } = 6f;

        private static readonly string Name = typeof(ExtractAction).Name;
        public ExtractAction(BotOwner bot) : base(bot, Name)
        {
        }

        private Vector3? Exfil => SAIN.Memory.ExfilPosition;

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
            float stamina = SAIN.Player.Physical.Stamina.NormalValue;
            if (SAIN.Enemy != null && SAIN.Enemy.Seen && (SAIN.Enemy.PathDistance < 50f || SAIN.Enemy.IsVisible))
            {
                NoSprint = true;
            }
            else if (stamina > 0.5f)
            {
                NoSprint = false;
            }
            else if (stamina < 0.1f)
            {
                NoSprint = true;
            }

            if (!Exfil.HasValue)
            {
                return;
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
            if (BotOwner.Mover == null)
            {
                return;
            }

            if (NoSprint)
            {
                BotOwner.Mover.Sprint(false);
            }
            else
            {
                BotOwner.Mover.Sprint(true);
            }

            if (distance > MinDistanceToStartExtract * 2)
            {
                ExtractStarted = false;
            }
            if (distance < MinDistanceToStartExtract)
            {
                ExtractStarted = true;
            }

            if (ExtractStarted)
            {
                return;
            }

            if (ReCalcPathTimer < Time.time)
            {
                ExtractTimer = -1f;
                ReCalcPathTimer = Time.time + 4f;

                NavMeshPathStatus pathStatus = BotOwner.Mover.GoToPoint(point, true, 0.5f, false, false);
                float distanceToEndOfPath = Vector3.Distance(BotOwner.Position, BotOwner.Mover.CurPathLastPoint);
                bool reachedEndOfIncompletePath = (pathStatus == NavMeshPathStatus.PathPartial) && (distanceToEndOfPath < BotExtractManager.MinDistanceToExtract);

                // If the path to the extract is invalid or the path is incomplete and the bot reached the end of it, select a new extract
                if ((pathStatus == NavMeshPathStatus.PathInvalid) || reachedEndOfIncompletePath)
                {
                    // Need to reset the search timer to prevent the bot from immediately selecting (possibly) the same extract
                    BotController.BotExtractManager.ResetExfilSearchTime(SAIN);

                    SAIN.Memory.ExfilPoint = null;
                    SAIN.Memory.ExfilPosition = null;
                }
            }
        }

        private void StartExtract(Vector3 point)
        {
            if (ExtractTimer == -1f)
            {
                ExtractTimer = BotController.BotExtractManager.GetExfilTime(SAIN.Memory.ExfilPoint);
                
                // Needed to get car extracts working
                activateExfil(SAIN.Memory.ExfilPoint);

                float timeRemaining = ExtractTimer - Time.time;
                Logger.LogInfo($"{BotOwner.name} Starting Extract Timer of {timeRemaining}");
            }

            if (ExtractTimer < Time.time)
            {
                Logger.LogInfo($"{BotOwner.name} Extracted at {point} for extract {SAIN.Memory.ExfilPoint.Settings.Name} at {System.DateTime.UtcNow}");

                var botgame = Singleton<IBotGame>.Instance;
                Player player = SAIN.Player;
                Singleton<Effects>.Instance.EffectsCommutator.StopBleedingForPlayer(player);
                BotOwner.Deactivate();
                BotOwner.Dispose();
                botgame.BotsController.BotDied(BotOwner);
                botgame.BotsController.DestroyInfo(player);
                Object.DestroyImmediate(BotOwner.gameObject);
                Object.Destroy(BotOwner);
            }
        }

        private void activateExfil(ExfiltrationPoint exfil)
        {
            // Needed to start the car extract
            exfil.OnItemTransferred(SAIN.Player);

            // Copied from the end of ExfiltrationPoint.Proceed()
            if (exfil.Status == EExfiltrationStatus.UncompleteRequirements)
            {
                switch (exfil.Settings.ExfiltrationType)
                {
                    case EExfiltrationType.Individual:
                        exfil.SetStatusLogged(EExfiltrationStatus.RegularMode, "Proceed-3");
                        break;
                    case EExfiltrationType.SharedTimer:
                        exfil.SetStatusLogged(EExfiltrationStatus.Countdown, "Proceed-1");

                        if (SAINPlugin.DebugMode)
                        {
                            Logger.LogInfo($"bot {SAIN.name} has started the VEX exfil");
                        }

                        break;
                    case EExfiltrationType.Manual:
                        exfil.SetStatusLogged(EExfiltrationStatus.AwaitsManualActivation, "Proceed-2");
                        break;
                }
            }
        }

        private bool ExtractStarted = false;
        private float ReCalcPathTimer = 0f;
        private float ExtractTimer = -1f;
    }
}