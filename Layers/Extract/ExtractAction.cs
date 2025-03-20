using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using EFT.Interactive;
using SAIN.Components;
using SAIN.Components.BotController;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.Memory;
using System.Collections;
using Systems.Effects;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers
{
    internal class ExtractAction : CombatAction, ISAINAction
    {
        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public static float MinDistanceToStartExtract { get; } = 6f;

        public ExtractAction(BotOwner bot) : base(bot, "Extract")
        {
        }

        private Vector3? Exfil => Bot.Memory.Extract.ExfilPosition;

        public override void Start()
        {
            Toggle(true);
            Bot.Memory.Extract.ExtractStatus = EExtractStatus.Extracting;
        }

        public override void Stop()
        {
            Toggle(false);
            Bot.Memory.Extract.ExtractStatus = EExtractStatus.None;
            BotOwner.Mover.MovementResume();
        }

        public override void Update(CustomLayer.ActionData data)
        {
            float stamina = Bot.Player.Physical.Stamina.NormalValue;
            bool fightingEnemy = isFightingEnemy();
            // Environment id of 0 means a bot is outside.
            if (Bot.Player.AIData.EnvironmentId != 0)
            {
                shallSprint = false;
            }
            else if (fightingEnemy)
            {
                shallSprint = false;
            }
            else if (stamina > 0.75f)
            {
                shallSprint = true;
            }
            else if (stamina < 0.2f)
            {
                shallSprint = false;
            }

            if (!BotOwner.GetPlayer.MovementContext.CanSprint)
            {
                shallSprint = false;
            }

            if (!Exfil.HasValue)
            {
                return;
            }

            Vector3 point = Exfil.Value;
            float distance = (point - BotOwner.Position).sqrMagnitude;

            if (distance < 8f)
            {
                shallSprint = false;
            }

            if (ExtractStarted)
            {
                setStatus(EExtractStatus.ExtractingNow);
                StartExtract(point);
                Bot.Mover.SetTargetPose(0f);
                Bot.Mover.SetTargetMoveSpeed(0f);
                if (_sayExitLocatedTime < Time.time)
                {
                    _sayExitLocatedTime = Time.time + 10;
                    Bot.Talk.GroupSay(EPhraseTrigger.ExitLocated, null, true, 70);
                }
            }
            else
            {
                if (fightingEnemy)
                {
                    setStatus(EExtractStatus.Fighting);
                }
                else
                {
                    setStatus(EExtractStatus.MovingTo);
                }
                MoveToExtract(distance, point);
                Bot.Mover.SetTargetPose(1f);
                Bot.Mover.SetTargetMoveSpeed(1f);
            }

            Bot.Steering.SteerByPriority();
            Shoot.CheckAimAndFire();

        }

        private void setStatus(EExtractStatus status)
        {
            Bot.Memory.Extract.ExtractStatus = status;
        }

        private float _sayExitLocatedTime;

        private bool isFightingEnemy()
        {
            return Bot.Enemy != null
                && Bot.Enemy.Seen
                && (Bot.Enemy.Path.PathDistance < 50f || Bot.Enemy.InLineOfSight);
        }

        private bool shallSprint;

        private void MoveToExtract(float distance, Vector3 point)
        {
            if (BotOwner.Mover == null)
            {
                return;
            }

            Bot.Mover.Sprint(shallSprint);

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

                Bot.Memory.Extract.ExtractStatus = EExtractStatus.MovingTo;
                NavMeshPathStatus pathStatus = BotOwner.Mover.GoToPoint(point, true, 0.5f, false, false);
                var pathController = HelpersGClass.GetPathControllerClass(BotOwner.Mover);
                if (pathController?.CurPath != null)
                {
                    float distanceToEndOfPath = Vector3.Distance(BotOwner.Position, pathController.CurPath.LastCorner());
                    bool reachedEndOfIncompletePath = (pathStatus == NavMeshPathStatus.PathPartial) && (distanceToEndOfPath < BotExtractManager.MinDistanceToExtract);

                    // If the path to the extract is invalid or the path is incomplete and the bot reached the end of it, select a new extract
                    if ((pathStatus == NavMeshPathStatus.PathInvalid) || reachedEndOfIncompletePath)
                    {
                        // Need to reset the search timer to prevent the bot from immediately selecting (possibly) the same extract
                        SAINBotController.Instance.BotExtractManager.ResetExfilSearchTime(Bot);

                        Bot.Memory.Extract.ExfilPoint = null;
                        Bot.Memory.Extract.ExfilPosition = null;
                    }
                }
            }
        }

        private void StartExtract(Vector3 point)
        {
            Bot.Memory.Extract.ExtractStatus = EExtractStatus.ExtractingNow;
            if (ExtractTimer == -1f)
            {
                ExtractTimer = SAINBotController.Instance.BotExtractManager.GetExfilTime(Bot.Memory.Extract.ExfilPoint);

                // Needed to get car extracts working
                activateExfil(Bot.Memory.Extract.ExfilPoint);

                float timeRemaining = ExtractTimer - Time.time;
                Logger.LogInfo($"{BotOwner.name} Starting Extract Timer of {timeRemaining}");
                BotOwner.Mover.MovementPause(timeRemaining);
            }

            if (ExtractTimer < Time.time)
            {
                Logger.LogInfo($"{BotOwner.name} Extracted at {point} for extract {Bot.Memory.Extract.ExfilPoint.Settings.Name} at {System.DateTime.UtcNow}");
                SAINBotController.Instance?.BotExtractManager?.LogExtractionOfBot(BotOwner, point, Bot.Memory.Extract.ExtractReason.ToString(), Bot.Memory.Extract.ExfilPoint);

                var botgame = Singleton<IBotGame>.Instance;
                Player player = Bot.Player;
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
            exfil.OnItemTransferred(Bot.Player);

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
                            Logger.LogInfo($"bot {Bot.name} has started the VEX exfil");
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