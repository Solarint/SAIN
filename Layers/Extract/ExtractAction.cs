using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using EFT.Interactive;
using SAIN.Components;
using SAIN.Components.BotController;
using SAIN.SAINComponent.Classes.Memory;
using Systems.Effects;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers;

internal class ExtractAction(BotOwner bot) : BotAction(bot, "Extract"), IBotAction
{
    public static float MinDistanceToStartExtract { get; } = 6f;

    private Vector3? Exfil => Bot.Memory.Extract.ExfilPosition;

    public override void Start()
    {
        base.Start();
        Bot.Memory.Extract.ExtractStatus = EExtractStatus.Extracting;
    }

    public override void Stop()
    {
        base.Stop();
        Bot.Memory.Extract.ExtractStatus = EExtractStatus.None;
        BotOwner.Mover.MovementResume();
    }

    public override void Update(CustomLayer.ActionData data)
    {
        bool fightingEnemy = IsFightingEnemy();
        // Environment id of 0 means a bot is outside.
        if (Bot.Player.AIData.EnvironmentId != 0)
        {
            shallSprint = false;
        }
        else if (fightingEnemy)
        {
            shallSprint = false;
        }
        else 
        {
            shallSprint = true;
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
            SetStatus(EExtractStatus.ExtractingNow);
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
                SetStatus(EExtractStatus.Fighting);
            }
            else
            {
                SetStatus(EExtractStatus.MovingTo);
            }
            MoveToExtract(distance, point);
            Bot.Mover.SetTargetPose(1f);
            Bot.Mover.SetTargetMoveSpeed(1f);
        }
    }

    private void SetStatus(EExtractStatus status)
    {
        Bot.Memory.Extract.ExtractStatus = status;
    }

    private float _sayExitLocatedTime;

    private bool IsFightingEnemy()
    {
        return Bot.GoalEnemy != null
            && Bot.GoalEnemy.Seen
            && (Bot.GoalEnemy.Path.PathLength < 50f || Bot.GoalEnemy.InLineOfSight);
    }

    private bool shallSprint;

    private void MoveToExtract(float distance, Vector3 point)
    {
        if (BotOwner.Mover == null)
        {
            return;
        }

        if (shallSprint && Bot.Mover.Moving)
        {
            Bot.Mover.ActivePath.RequestStartSprint(SAINComponent.Classes.Mover.ESprintUrgency.High, "extract");
        }

        if (shouldStartExtract(distance))
        {
            return;
        }

        if (ReCalcPathTimer > Time.time)
        {
            return;
        }

        ExtractTimer = -1f;
        ReCalcPathTimer = Time.time + 4f;

        if (!canMoveToExtract(point))
        {
            Bot.Memory.Extract.ExtractStatus = EExtractStatus.None;
            return;
        }

        Bot.Memory.Extract.ExtractStatus = EExtractStatus.MovingTo;

        var pathController = Bot.Mover;
        if (pathController.ActivePath.PathStatus == NavMeshPathStatus.PathComplete)
        {
            return;
        }

        // If the path to the extract is invalid or the path is incomplete and the bot reached the end of it, select a new extract
        float distanceToEndOfPath = Vector3.Distance(BotOwner.Position, pathController.ActivePath.GetLastCorner().Position);
        if (distanceToEndOfPath < BotExtractManager.MinDistanceToExtract)
        {
            // Need to reset the search timer to prevent the bot from immediately selecting (possibly) the same extract
            BotManagerComponent.Instance.BotExtractManager.ResetExfilSearchTime(Bot);

            Bot.Memory.Extract.ExfilPoint = null;
            Bot.Memory.Extract.ExfilPosition = null;

            Bot.Memory.Extract.ExtractStatus = EExtractStatus.None;

            Logger.LogWarning($"{BotOwner.name} reached the end of an incomplete path when trying to find its extract. Searching for a new extract...");
        }
    }

    private bool shouldStartExtract(float distance)
    {
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
            return true;
        }

        return false;
    }

    private bool canMoveToExtract(Vector3 point)
    {
        if (!Bot.Mover.WalkToPoint(point, false, 0.5f))
        {
            return false;
        }

        var pathController = Bot.Mover;
        if (pathController?.ActivePath == null)
        {
            Logger.LogError($"{BotOwner.name} has a null path to its extract");

            return false;
        }

        // Need to first check if the bot has started moving to extract because there is currently a race condition that sometimes resets PathStatus
        // to PathInvalid. If the path is invalid and the bot has not already started moving, it might actually be unable to travel there. 
        if ((Bot.Memory.Extract.ExtractStatus == EExtractStatus.None) && (pathController.ActivePath.PathStatus == NavMeshPathStatus.PathInvalid))
        {
            Logger.LogWarning($"{BotOwner.name} has an invalid path to its extract");

            return false;
        }

        return true;
    }

    public void StartExtract(Vector3 point)
    {
        Bot.Memory.Extract.ExtractStatus = EExtractStatus.ExtractingNow;
        if (ExtractTimer == -1f)
        {
            ExtractTimer = BotManagerComponent.Instance.BotExtractManager.GetExfilTime(Bot.Memory.Extract.ExfilPoint);

            // Needed to get car extracts working
            ActivateExfil(Bot.Memory.Extract.ExfilPoint);

            float timeRemaining = ExtractTimer - Time.time;
            Logger.LogInfo($"{BotOwner.name} Starting Extract Timer of {timeRemaining}");
            Bot.Mover.PauseMovement(timeRemaining);
        }

        if (ExtractTimer < Time.time)
        {
            Logger.LogInfo($"{BotOwner.name} Extracted at {point} for extract {Bot.Memory.Extract.ExfilPoint.Settings.Name} at {System.DateTime.UtcNow}");
            BotManagerComponent.Instance?.BotExtractManager?.LogExtractionOfBot(BotOwner, point, Bot.Memory.Extract.ExtractReason.ToString(), Bot.Memory.Extract.ExfilPoint);

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

    private void ActivateExfil(ExfiltrationPoint exfil)
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
                    
#if DEBUG
                    if (SAINPlugin.DebugMode)
                    {
                        Logger.LogInfo($"bot {Bot.name} has started the VEX exfil");
                    }
#endif

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