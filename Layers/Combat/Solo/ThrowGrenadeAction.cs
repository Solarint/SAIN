using DrakiaXYZ.BigBrain.Brains;
using EFT;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo;

public class ThrowGrenadeAction(BotOwner bot) : BotAction(bot, nameof(ThrowGrenadeAction)), IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        if (!Stopped && Time.time - StartTime > 1f || Bot.Cover.CheckLimbsForCover(Bot.GoalEnemy))
        {
            Stopped = true;
            
        }
    }

    private float StartTime = 0f;
    private bool Stopped = false;

    public override void Start()
    {
        base.Start();
        StartTime = Time.time;
        if (Bot.Squad.BotInGroup && Bot.Talk.GroupTalk.FriendIsClose)
        {
            Bot.Talk.Say(EPhraseTrigger.OnGrenade, null, false);
        }
    }
}