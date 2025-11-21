using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using System.Text;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo.Cover;

internal class DoSurgeryAction(BotOwner botOwner) : BotAction(botOwner, "Surgery") , IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        
        checkDoSurgery();
    }

    private void checkDoSurgery()
    {
        BotComponent bot = Bot;
        var sainSurgery = bot.Medical.Surgery;
        if (sainSurgery.AreaClearForSurgery)
        {
            bot.Mover.ActivePath?.Cancel(0.1f);
            bot.Mover.SetTargetMoveSpeed(0f);
            bot.Cover.TrySetProneConditional(bot.GoalEnemy);
            var eftSurgery = bot.BotOwner.Medecine.SurgicalKit;
            if (_startSurgeryTime < Time.time
                && !eftSurgery.Using
                && eftSurgery.ShallStartUse())
            {
                sainSurgery.SurgeryStarted = true;
                eftSurgery.ApplyToCurrentPart(new System.Action(onSurgeryDone));
            }
            else if (_actionStartedTime + 30f < Time.time)
            {
                bot.Player?.ActiveHealthController?.RestoreFullHealth();
                bot.Decision.ResetDecisions(true);
            }
        }
        else
        {
            //Bot.Mover.SetTargetMoveSpeed(1);
            bot.Mover.SetTargetPose(1);

            bot.Medical.Surgery.SurgeryStarted = false;
            bot.Medical.TryCancelHeal();
            bot.Mover.DogFight.DogFightMove(false, bot.GoalEnemy);
        }
    }

    public override void OnSteeringTicked()
    {
        if (!TryShootAnyTarget(Bot.GoalEnemy))
        {
        }
            Bot.Steering.SteerByPriority(Bot.GoalEnemy, true);
    }

    private void onSurgeryDone()
    {
        Bot.Medical.Surgery.SurgeryStarted = false;
        _actionStartedTime = Time.time;
        _startSurgeryTime = Time.time + 1f;

        if (BotOwner.Medecine.SurgicalKit.HaveWork)
        {
            if (Bot.GoalEnemy == null || Bot.GoalEnemy.TimeSinceSeen > 90f)
            {
                Bot.Player?.ActiveHealthController?.RestoreFullHealth();
                Bot.Decision.ResetDecisions(true);
            }
            return;
        }
        Bot.Decision.ResetDecisions(true);
    }

    public override void Start()
    {
        base.Start();   
        Bot.Mover.PauseMovement(3f);
        _startSurgeryTime = Time.time + 1f;
        _actionStartedTime = Time.time;
    }

    private float _startSurgeryTime;
    private float _actionStartedTime;

    public override void Stop()
    {
        base.Stop();
        Bot.Medical.Surgery.SurgeryStarted = false;
        Bot.Mover?.ActivePath?.UnPause();
        foreach (EBodyPart part in SurgeryParts)
        {
            Bot.Player.ActiveHealthController?.RestoreBodyPart(part, 1f);
        }
    }

    private static readonly EBodyPart[] SurgeryParts =
    [
        EBodyPart.LeftArm,
        EBodyPart.RightArm,
        EBodyPart.LeftLeg,
        EBodyPart.RightLeg,
        EBodyPart.Stomach,
    ];

    public override void BuildDebugText(StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine($"Health Status {Bot.Memory.Health.HealthStatus}");
        stringBuilder.AppendLine($"Surgery Started? {Bot.Medical.Surgery.SurgeryStarted}");
        stringBuilder.AppendLine($"Time Since Surgery Started {Time.time - Bot.Medical.Surgery.SurgeryStartTime}");
        stringBuilder.AppendLine($"Area Clear? {Bot.Medical.Surgery.AreaClearForSurgery}");
        stringBuilder.AppendLine($"ShallStartUse Surgery? {BotOwner.Medecine.SurgicalKit.ShallStartUse()}");
        stringBuilder.AppendLine($"IsBleeding? {BotOwner.Medecine.FirstAid.IsBleeding}");
    }
}