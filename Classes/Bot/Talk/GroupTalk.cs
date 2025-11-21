using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Talk;

public class GroupTalk : BotBase
{
    public float RANDOM_TALK_INTERVAL_MIN = 30f;
    public float RANDOM_TALK_INTERVAL_MAX = 120f;
    public float FRIEND_DISTANCE_INTERVAL = 1f;

    public GroupTalk(BotComponent bot) : base(bot)
    {
        _nextRandomTalkTime = Time.time + 15f;
    }

    public bool FriendIsClose {
        get
        {
            if (Player == null)
            {
                return false;
            }
            if (_nextCheckFriendsTime > Time.time)
            {
                return _friendIsClose;
            }

            _nextCheckFriendsTime = Time.time + FRIEND_DISTANCE_INTERVAL;
            updateFriendClose();
            return _friendIsClose;
        }
    }

    public override void ManualUpdate()
    {
        base.ManualUpdate();
        if (!Bot.Talk.CanTalk)
        {
            return;
        }
        if (!BotSquad.BotInGroup ||
            !Bot.Info.FileSettings.Mind.SquadTalk ||
            SAINPlugin.LoadedPreset.GlobalSettings.Talk.DisableBotTalkPatching)
        {
            if (Subscribed)
                unsub();

            return;
        }

        if (!Subscribed)
            sub();

        if (Bot.Talk.IsSpeaking)
        {
            return;
        }

        CheckGroupTalk();
    }

    private void CheckGroupTalk()
    {
        if (TalkTimer < Time.time)
        {
            TalkTimer = Time.time + _groupTalkFreq;
            if (FriendIsClose)
            {
                if (Bot.Squad.IAmLeader
                    && UpdateLeaderCommand())
                {
                    return;
                }
                Enemy botEnemy = Bot.GoalEnemy;
                if (CheckEnemyContact(botEnemy))
                {
                    return;
                }
                if (TalkEnemyLocation(botEnemy))
                {
                    return;
                }
                if (ShallReportLostVisual(botEnemy))
                {
                    return;
                }
                if (ShallReportNeedHelp(botEnemy))
                {
                    return;
                }
                randomTalk();
            }
        }
    }

    private void randomTalk()
    {
        if (_nextRandomTalkTime < Time.time &&
            BotOwner.Memory.IsPeace &&
            Bot.Talk.EnemyTalk.ShallBeChatty())
        {
            float delay = UnityEngine.Random.Range(RANDOM_TALK_INTERVAL_MIN, RANDOM_TALK_INTERVAL_MAX);
            _nextRandomTalkTime = Time.time + delay;
            Bot.Talk.Say(EPhraseTrigger.MumblePhrase, null);
        }
    }


    private float _nextRandomTalkTime;

    private void OnDecisionMade(ECombatDecision solo, ESquadDecision squad, ESelfActionType self, Enemy enemy, BotComponent me)
    {
        if (!Bot.Talk.CanTalk)
        {
            return;
        }
        if (Bot.Talk.IsSpeaking)
        {
            return;
        }
        bool imLeader = Bot.Squad.IAmLeader;
        if (imLeader &&
            _leaderCommandTime < Time.time &&
            LeaderMadeDecision(solo, squad))
        {
            return;
        }
        if (TalkSelfDecision(self))
        {
            return;
        }
        if (TalkSoloDecision(solo, enemy))
        {
            return;
        }
    }

    private void OnMemberMadeDecision(ECombatDecision solo, ESquadDecision squad, ESelfActionType self, string enemyProfileId, BotComponent member)
    {
        if (!Bot.Squad.IAmLeader)
        {
            return;
        }
        if (!Bot.Talk.CanTalk)
        {
            return;
        }
        if (Bot.Talk.IsSpeaking)
        {
            return;
        }

        if (_leaderCommandTime > Time.time)
        {
            return;
        }

        _leaderCommandTime = Time.time + Bot.Info.FileSettings.Mind.SquadLeadTalkFreq;
        var commandTrigger = EPhraseTrigger.PhraseNone;
        var memberTrigger = EPhraseTrigger.PhraseNone;
        var gesture = EInteraction.None;

        switch (solo)
        {
            case ECombatDecision.Retreat:
            case ECombatDecision.RunAway:
            case ECombatDecision.SeekCover:
                if (member.Cover.CoverInUse == null)
                {
                    gesture = EInteraction.ComeWithMeGesture;
                    commandTrigger = EFTMath.RandomBool() ? EPhraseTrigger.GetInCover : EPhraseTrigger.GetBack;
                    memberTrigger = EPhraseTrigger.Roger;
                }
                break;

            case ECombatDecision.RushEnemy:
                gesture = EInteraction.ThereGesture;
                commandTrigger = EPhraseTrigger.Gogogo;
                memberTrigger = EPhraseTrigger.OnFight;
                break;

            default: break;
        }

        if (commandTrigger == EPhraseTrigger.PhraseNone)
        {
            switch (squad)
            {
                case ESquadDecision.Suppress:
                    gesture = EInteraction.ThereGesture;
                    commandTrigger = EPhraseTrigger.Suppress;
                    memberTrigger = EPhraseTrigger.Covering;
                    break;

                case ESquadDecision.PushSuppressedEnemy:
                    gesture = EInteraction.ThereGesture;
                    commandTrigger = EPhraseTrigger.Gogogo;
                    memberTrigger = EPhraseTrigger.Going;
                    break;

                case ESquadDecision.Regroup:
                    gesture = EInteraction.ComeWithMeGesture;
                    commandTrigger = EPhraseTrigger.Regroup;
                    memberTrigger = EPhraseTrigger.Roger;
                    break;

                default:
                    break;
            }
        }

        if (commandTrigger != EPhraseTrigger.PhraseNone &&
            Bot.Talk.GroupSay(commandTrigger, ETagStatus.Combat, false, 66f))
        {
            bool shallGesture = gesture != EInteraction.None && Bot.Squad.VisibleMembers.Count > 0 && Bot.GoalEnemy?.IsVisible == false;
            if (shallGesture)
                Player.HandsController.ShowGesture(gesture);

            member.Talk.Say(memberTrigger, ETagStatus.Combat, false);
        }
    }

    private bool TalkSelfDecision(ESelfActionType self)
    {
        switch (self)
        {
            case ESelfActionType.Reload:
                if (_nextReportReloadTime < Time.time &&
                    Bot.Talk.GroupSay(reloadPhrases.PickRandom(), null, false, _reportReloadingChance))
                {
                    _nextReportReloadTime = Time.time + _reportReloadingFreq;
                    return true;
                }
                break;

            case ESelfActionType.FirstAid:
            case ESelfActionType.Stims:
            case ESelfActionType.Surgery:
                if (Bot.Talk.GroupSay(EPhraseTrigger.CoverMe, null, false))
                {
                    return true;
                }
                break;

            default: break;
        }
        return false;
    }

    private bool TalkSoloDecision(ECombatDecision solo, Enemy enemy)
    {
        if (enemy == null)
        {
            return false;
        }
        switch (solo)
        {
            case ECombatDecision.Retreat:
            case ECombatDecision.SeekCover:
            case ECombatDecision.RunAway:
                if (_nextCheckTalkRetreatTime < Time.time && 
                    (enemy.IsVisible || enemy.InLineOfSight) && 
                    Bot.Talk.GroupSay(_talkRetreatTrigger, _talkRetreatMask, _talkRetreatGroupDelay, _talkRetreatChance))
                {
                    _nextCheckTalkRetreatTime = Time.time + _talkRetreatFreq;
                    return true;
                }
                break;

            default: break;
        }
        return false;
    }

    private bool LeaderMadeDecision(ECombatDecision solo, ESquadDecision squad)
    {
        var commandTrigger = EPhraseTrigger.PhraseNone;
        var trigger = EPhraseTrigger.PhraseNone;
        var gesture = EInteraction.None;

        switch (squad)
        {
            case ESquadDecision.Search:
            case ESquadDecision.GroupSearch:
                gesture = EInteraction.ThereGesture;
                commandTrigger = EPhraseTrigger.FollowMe;
                trigger = EPhraseTrigger.Going;
                break;

            case ESquadDecision.Help:
                gesture = EInteraction.ThereGesture;
                commandTrigger = EPhraseTrigger.Gogogo;
                trigger = EPhraseTrigger.Going;
                break;

            case ESquadDecision.Suppress:
            case ESquadDecision.PushSuppressedEnemy:
                gesture = EInteraction.ThereGesture;
                commandTrigger = EPhraseTrigger.Suppress;
                trigger = EPhraseTrigger.Covering;
                break;

            default: break;
        }

        switch (solo)
        {
            case ECombatDecision.SeekCover:
                if (Bot.Cover.CoverInUse != null)
                {
                    gesture = EInteraction.HoldGesture;
                    commandTrigger = EPhraseTrigger.HoldPosition;
                    trigger = EPhraseTrigger.Roger;
                }
                break;

            case ECombatDecision.Retreat:
                commandTrigger = EPhraseTrigger.OnYourOwn;
                trigger = EFTMath.RandomBool() ? EPhraseTrigger.Repeat : EPhraseTrigger.Stop;
                break;

            case ECombatDecision.RushEnemy:
                gesture = EInteraction.ThereGesture;
                commandTrigger = EPhraseTrigger.Gogogo;
                trigger = EPhraseTrigger.OnFight;
                break;

            default: break;
        }
        if (commandTrigger != EPhraseTrigger.PhraseNone &&
            checkLeaderTalk(gesture, commandTrigger, trigger))
        {
            return true;
        }
        return false;
    }

    private bool ShallReportLostVisual(Enemy enemy)
    {
        if (enemy != null && enemy.Vision.ShallReportLostVisual)
        {
            enemy.Vision.ShallReportLostVisual = false;
            if (EFTMath.RandomBool(_reportLostVisualChance))
            {
                ETagStatus mask = PersonIsClose(enemy.EnemyPlayer) ? ETagStatus.Combat : ETagStatus.Aware;
                if (enemy.TimeSinceSeen > _reportRatTimeSinceSeen && EFTMath.RandomBool(_reportRatChance))
                {
                    return Bot.Talk.GroupSay(EPhraseTrigger.Rat, null, false, 100);
                }
                else
                {
                    return Bot.Talk.GroupSay(EPhraseTrigger.LostVisual, null, false, 100);
                }
            }
        }
        return false;
    }

    private void EnemyConversation(EPhraseTrigger trigger, ETagStatus status, Player player)
    {
        if (player == null)
        {
            return;
        }
        if (Bot.Talk.IsSpeaking)
        {
            return;
        }
        if (Bot.HasEnemy || !FriendIsClose)
        {
            return;
        }
        Enemy enemy = Bot.EnemyController.GetEnemy(player.ProfileId, true);
        if (enemy == null)
        {
            return;
        }
        if (enemy.RealDistance > _reportEnemyMaxDist)
        {
            return;
        }
        Bot.Talk.GroupSay(EPhraseTrigger.OnEnemyConversation, null, false, _reportEnemyConversationChance);
    }

    public void TalkEnemySniper()
    {
        if (FriendIsClose)
        {
            Bot.Talk.TalkAfterDelay(EPhraseTrigger.SniperPhrase, ETagStatus.Combat, UnityEngine.Random.Range(0.5f, 1f));
        }
    }

    public override void Dispose()
    {
        unsub();
        base.Dispose();
    }

    private void unsub()
    {
        if (Subscribed)
        {
            Subscribed = false;
            var squad = Bot?.Squad?.SquadInfo;
            if (squad != null)
            {
                squad.OnMemberKilled -= friendlyDown;
                squad.OnMemberHeardEnemy -= enemyHeard;
                squad.OnMemberDecisionMade -= OnMemberMadeDecision;
            }

            var botController = BotManagerComponent.Instance;

            if (botController != null)
                botController.BotHearing.PlayerTalk -= EnemyConversation;

            if (Bot.EnemyController != null)
            {
                Bot.EnemyController.Events.OnEnemyKilled -= OnEnemyDown;
                Bot.EnemyController.Events.OnEnemyHealthChanged -= onEnemyHealthChanged;
            }

            BotOwner.DeadBodyWork.OnStartLookToBody -= OnLootBody;
            Bot.Decision.DecisionManager.OnDecisionMade -= OnDecisionMade;
            Bot.Memory.Health.HealthStatusChanged -= myHealthChanged;
        }
    }

    private void sub()
    {
        var squad = Bot?.Squad?.SquadInfo;
        if (!Subscribed && squad != null)
        {
            Subscribed = true;
            squad.OnMemberKilled += friendlyDown;
            squad.OnMemberHeardEnemy += enemyHeard;
            squad.OnMemberDecisionMade += OnMemberMadeDecision;

            BotManagerComponent.Instance.BotHearing.PlayerTalk += EnemyConversation;

            BotOwner.DeadBodyWork.OnStartLookToBody += OnLootBody;
            Bot.EnemyController.Events.OnEnemyKilled += OnEnemyDown;
            Bot.EnemyController.Events.OnEnemyHealthChanged += onEnemyHealthChanged;
            Bot.Decision.DecisionManager.OnDecisionMade += OnDecisionMade;
            Bot.Memory.Health.HealthStatusChanged += myHealthChanged;
        }
    }

    private void onEnemyHealthChanged(ETagStatus health, Enemy enemy)
    {
        if (!Bot.Talk.CanTalk)
        {
            return;
        }
        if (Bot.Talk.IsSpeaking)
        {
            return;
        }
        if (enemy == null)
        {
            return;
        }
        if (!enemy.IsCurrentEnemy)
        {
            return;
        }
        if (health != ETagStatus.Dying && health != ETagStatus.BadlyInjured)
        {
            return;
        }
        if (!EFTMath.RandomBool(_reportEnemyHealthChance))
        {
            return;
        }
        if (_nextCheckEnemyHPTime < Time.time)
        {
            _nextCheckEnemyHPTime = Time.time + _reportEnemyHealthFreq;
            Bot.Talk.GroupSay(EPhraseTrigger.OnEnemyShot, null, false, 100);
        }
    }

    private bool CheckEnemyContact(Enemy enemy)
    {
        if (FriendIsClose
            && enemy != null)
        {
            if (enemy.FirstContactOccured
                && !enemy.FirstContactReported)
            {
                enemy.FirstContactReported = true;
                if (EFTMath.RandomBool(40))
                {
                    ETagStatus mask = PersonIsClose(enemy.EnemyPlayer) ? ETagStatus.Combat : ETagStatus.Aware;
                    return Bot.Talk.GroupSay(EPhraseTrigger.OnFirstContact, mask, true, 100);
                }
            }
            if (enemy.Vision.ShallReportRepeatContact)
            {
                enemy.Vision.ShallReportRepeatContact = false;
                if (EFTMath.RandomBool(40))
                {
                    ETagStatus mask = PersonIsClose(enemy.EnemyPlayer) ? ETagStatus.Combat : ETagStatus.Aware;
                    return Bot.Talk.GroupSay(EPhraseTrigger.OnRepeatedContact, mask, false, 100);
                }
            }
        }
        return false;
    }

    private void OnEnemyDown(Player player)
    {
        if (Bot.Talk.IsSpeaking)
        {
            return;
        }
        if (!_reportEnemyKilledToxicSquadLeader)
        {
            var settings = player?.Profile?.Info?.Settings;
            if (settings == null || !BotOwner.BotsGroup.IsPlayerEnemy(player))
            {
                return;
            }
        }
        if (!FriendIsClose || !PersonIsClose(player))
        {
            return;
        }

        if (EFTMath.RandomBool(_reportEnemyKilledChance))
        {
            float randomTime = UnityEngine.Random.Range(0.2f, 0.6f);
            Bot.Talk.TalkAfterDelay(EPhraseTrigger.EnemyDown, null, randomTime);

            var leader = Bot.Squad.SquadInfo?.LeaderComponent;
            if (leader?.Player != null
                && !Bot.Squad.IAmLeader
                && EFTMath.RandomBool(_reportEnemyKilledSquadLeadChance)
                && PersonIsClose(leader.Player))
            {
                leader.Talk.TalkAfterDelay(EPhraseTrigger.GoodWork, null, randomTime + 0.75f);
            }
        }
    }

    private bool PersonIsClose(IPlayer player)
    {
        return player != null && BotOwner != null && (player.Position - BotOwner.Position).sqrMagnitude < 30f * 30f;
    }

    private bool PersonIsClose(Player player)
    {
        return player != null && BotOwner != null && (player.Position - BotOwner.Position).sqrMagnitude < 30f * 30f;
    }

    private void updateFriendClose()
    {
        _friendIsClose = false;
        foreach (var member in Bot.Squad.Members.Values)
        {
            if (member != null
                && !member.IsDead
                && member.Player.ProfileId != Player.ProfileId
                && member.PlayerComponent.GetDistanceToPlayer(Bot.ProfileId) < _friendCloseDist)
            {
                _friendIsClose = true;
                break;
            }
        }
        if (!_friendIsClose && Bot.Squad.HumanFriendClose)
        {
            _friendIsClose = true;
        }
    }

    private void friendlyDown(IPlayer player, DamageInfoStruct damage, float time)
    {
        if (!Bot.Talk.CanTalk)
        {
            return;
        }
        if (Bot.Talk.IsSpeaking)
        {
            return;
        }
        if (BotOwner.IsDead || !Bot.BotActive || !EFTMath.RandomBool(_reportFriendKilledChance))
        {
            return;
        }

        updateFriendClose();
        if (!_friendIsClose || !PersonIsClose(player))
        {
            return;
        }
        Bot.Talk.TalkAfterDelay(EPhraseTrigger.OnFriendlyDown, ETagStatus.Combat, UnityEngine.Random.Range(0.33f, 0.66f));
    }

    private void OnLootBody(float num)
    {
        if (!Bot.BotActive)
        {
            return;
        }
        if (Bot.Talk.IsSpeaking)
        {
            return;
        }
        if (!FriendIsClose)
        {
            return;
        }

        EPhraseTrigger trigger = LootPhrases.PickRandom();
        Bot.Talk.Say(trigger, null, true);
    }

    private void allMembersSay(EPhraseTrigger trigger, ETagStatus mask, EPhraseTrigger commandTrigger, float delay = 1.5f, float chance = 100f)
    {
        if (Bot.Squad.LeaderComponent == null)
        {
            return;
        }

        bool memberTalked = false;
        foreach (var member in BotSquad.Members.Values)
        {
            if (member != null &&
                !member.IsDead &&
                !member.Talk.IsSpeaking &&
                EFTMath.RandomBool(chance) &&
                !member.Squad.IAmLeader &&
                member.Squad.DistanceToSquadLeader <= 40f)
            {
                memberTalked = true;

                EPhraseTrigger myTrigger = trigger;
                switch (commandTrigger)
                {
                    case EPhraseTrigger.GetBack:
                    case EPhraseTrigger.HoldPosition:
                        if (member.Decision.CurrentSquadDecision == ESquadDecision.GroupSearch)
                        {
                            myTrigger = EPhraseTrigger.Negative;
                            break;
                        }
                        switch (member.Decision.CurrentCombatDecision)
                        {
                            case ECombatDecision.Search:
                                myTrigger = EPhraseTrigger.Negative;
                                break;

                            case ECombatDecision.SeekCover:
                                myTrigger = EFTMath.RandomBool() ? EPhraseTrigger.Roger : EPhraseTrigger.OnPosition;
                                break;

                            case ECombatDecision.RushEnemy:
                                myTrigger = EPhraseTrigger.Negative;
                                break;

                            default:
                                break;
                        }
                        break;

                    case EPhraseTrigger.Gogogo:
                    case EPhraseTrigger.FollowMe:
                        if (member.Decision.CurrentSquadDecision == ESquadDecision.GroupSearch)
                        {
                            myTrigger = EFTMath.RandomBool() ? EPhraseTrigger.Ready : EPhraseTrigger.Going;
                            break;
                        }
                        switch (member.Decision.CurrentCombatDecision)
                        {
                            case ECombatDecision.Search:
                                myTrigger = EFTMath.RandomBool() ? EPhraseTrigger.Ready : EPhraseTrigger.Going;
                                break;

                            case ECombatDecision.SeekCover:
                                myTrigger = EFTMath.RandomBool() ? EPhraseTrigger.Negative : EPhraseTrigger.Covering;
                                break;

                            case ECombatDecision.RushEnemy:
                                myTrigger = EPhraseTrigger.OnFight;
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }

                member.Talk.TalkAfterDelay(myTrigger, mask, delay * UnityEngine.Random.Range(0.75f, 1.25f));
            }
        }

        if (memberTalked && EFTMath.RandomBool(5))
        {
            //SAIN.Squad.LeaderComponent?.Talk.TalkAfterDelay(EPhraseTrigger.Silence, ETagStatus.Aware, 1.25f);
        }
    }

    private bool UpdateLeaderCommand()
    {
        if (LeaderComponent == null)
        {
            return false;
        }
        if (!BotSquad.IAmLeader)
        {
            return false;
        }
        if (_leadTime >= Time.time)
        {
            return false;
        }

        _leadTime = Time.time + Randomized * Bot.Info.FileSettings.Mind.SquadLeadTalkFreq;

        if (CheckIfLeaderShouldCommand())
        {
            return true;
        }

        if (CheckFriendliesTimer < Time.time &&
            CheckFriendlyLocation(out var trigger) &&
            Bot.Talk.Say(trigger))
        {
            CheckFriendliesTimer = Time.time + Bot.Info.FileSettings.Mind.SquadLeadTalkFreq * 5f;
            var mask = EFTMath.RandomBool() ? ETagStatus.Aware : ETagStatus.Unaware;
            allMembersSay(EPhraseTrigger.Roger, mask, trigger, Random.Range(0.65f, 1.25f), 50f);
            return true;
        }
        return false;
    }

    private void myHealthChanged(ETagStatus status)
    {
        if (!Bot.Talk.CanTalk)
        {
            return;
        }
        if (Bot.Talk.IsSpeaking)
        {
            return;
        }
        if (Bot.HasEnemy && Bot.GoalEnemy.RealDistance < 30f)
        {
            return;
        }
        if (!FriendIsClose)
        {
            return;
        }

        if (HurtTalkTimer < Time.time)
        {
            if ((status == ETagStatus.Dying || status == ETagStatus.BadlyInjured) &&
                BotOwner.Medecine.FirstAid?.HaveSmth2Use == false &&
                Bot.Talk.Say(EPhraseTrigger.NeedMedkit, null, true, false))
            {
                HurtTalkTimer = Time.time + Bot.Info.FileSettings.Mind.SquadMemberTalkFreq * 5f * Random.Range(0.5f, 1.5f);
                return;
            }

            var trigger = EPhraseTrigger.PhraseNone;
            switch (status)
            {
                case ETagStatus.Injured:
                    if (EFTMath.RandomBool(60))
                        trigger = EFTMath.RandomBool() ? EPhraseTrigger.Hit : EPhraseTrigger.HurtLight;
                    break;

                case ETagStatus.BadlyInjured:
                    if (EFTMath.RandomBool(75))
                        trigger = EFTMath.RandomBool() ? EPhraseTrigger.HurtLight : EPhraseTrigger.HurtHeavy;
                    break;

                case ETagStatus.Dying:
                    if (EFTMath.RandomBool(75))
                        trigger = EPhraseTrigger.HurtNearDeath;
                    break;

                default:
                    return;
            }

            if (trigger != EPhraseTrigger.PhraseNone && Bot.Talk.Say(trigger))
            {
                HurtTalkTimer = Time.time + Bot.Info.FileSettings.Mind.SquadMemberTalkFreq * 5f * Random.Range(0.5f, 1.5f);
                return;
            }
        }
    }

    private bool ShallReportNeedHelp(Enemy enemy)
    {
        if (!FriendIsClose || enemy == null)
        {
            return false;
        }
        if (_underFireNeedHelpTime < Time.time
            && EFTMath.RandomBool(_underFireNeedHelpChance)
            && BotOwner.Memory.IsUnderFire
            && (object)Bot.Memory.LastUnderFireSource == enemy.EnemyPlayer)
        {
            _underFireNeedHelpTime = Time.time + _underFireNeedHelpFreq;
            return Bot.Talk.Say(_underFireNeedHelpTrigger, _underFireNeedHelpMask, _underFireNeedHelpGroupDelay);
        }
        return false;
    }

    private void enemyHeard(EnemyPlace place, Enemy enemy, SAINSoundType soundType)
    {
        if (!Bot.Talk.CanTalk)
        {
            return;
        }
        if (Bot.Talk.IsSpeaking)
        {
            return;
        }
        float time = Time.time;
        if (!Bot.BotActive ||
            _hearNoiseTime > time)
        {
            return;
        }
        if (Bot.HasEnemy && Bot.GoalEnemy.TimeSinceSeen < 120f)
        {
            return;
        }
        if (!Bot.Talk.GroupTalk.FriendIsClose)
        {
            return;
        }
        if (place == null || soundType.IsGunShot())
        {
            return;
        }
        if (enemy.RealDistance > _hearNoiseMaxDist)
        {
            return;
        }
        if (!EFTMath.RandomBool(_hearNoiseChance))
        {
            return;
        }
        _hearNoiseTime = time + _hearNoiseFreq;
        EPhraseTrigger trigger = soundType == SAINSoundType.Conversation ? EPhraseTrigger.OnEnemyConversation : EPhraseTrigger.NoisePhrase;
        Bot.Talk.TalkAfterDelay(trigger, ETagStatus.Aware, 0.33f);
    }

    public bool CheckIfLeaderShouldCommand()
    {
        if (_leaderCommandTime < Time.time)
        {
            if (Bot.DoorOpener.Interacting &&
                EFTMath.RandomBool(33f) &&
                checkLeaderTalk(EInteraction.None, EPhraseTrigger.OpenDoor, EPhraseTrigger.Roger))
            {
                _leaderCommandTime = Time.time + Bot.Info.FileSettings.Mind.SquadLeadTalkFreq;
                return true;
            }
            if (_nextsayNeedSniperTime < Time.time &&
                Bot.GoalEnemy?.IsSniper == true && Bot.Talk.CanSay(EPhraseTrigger.NeedSniper, true, false))
            {
                _nextsayNeedSniperTime = Time.time + _needSniperFreq;
                if (EFTMath.RandomBool(_needSniperChance) && Bot.Talk.Say(EPhraseTrigger.NeedSniper, ETagStatus.Combat, false, true))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private float _nextsayNeedSniperTime;
    private float _needSniperFreq = 60f;
    private float _needSniperChance = 50f;

    private bool checkLeaderTalk(EInteraction gesture, EPhraseTrigger commandTrigger, EPhraseTrigger memberTrigger)
    {
        int visibleCount = Bot.Squad.VisibleMembers.Count;
        bool shallGesture = gesture != EInteraction.None && visibleCount > 0 && Bot.GoalEnemy?.IsVisible == false;
        bool mostMembersNotVisible = (float)visibleCount / (float)Bot.Squad.Members.Count < 0.5f;
        if (mostMembersNotVisible &&
            Bot.Talk.GroupSay(commandTrigger, null, false, 100))
        {
            _leaderCommandTime = Time.time + Bot.Info.FileSettings.Mind.SquadLeadTalkFreq;

            if (shallGesture)
                Player.HandsController.ShowGesture(gesture);

            allMembersSay(memberTrigger, ETagStatus.Aware, commandTrigger, Random.Range(0.75f, 1.5f), 35f);
            return true;
        }
        else if (shallGesture)
        {
            Player.HandsController.ShowGesture(gesture);
        }
        return false;
    }

    public bool TalkEnemyLocation(Enemy enemy)
    {
        if (enemy != null && EnemyPosTimer < Time.time)
        {
            EnemyPosTimer = Time.time + _enemyLocationTalkFreq;
            var trigger = EPhraseTrigger.PhraseNone;
            var mask = ETagStatus.Aware;

            if (Bot.GoalEnemy.IsVisible
                && enemy.EnemyLookingAtMe
                && EFTMath.RandomBool(_enemyNeedHelpChance))
            {
                mask = ETagStatus.Combat;
                bool injured = !Bot.Memory.Health.Healthy && !Bot.Memory.Health.Injured;
                trigger = injured ? EPhraseTrigger.NeedHelp : EPhraseTrigger.OnRepeatedContact;
            }
            else if ((enemy.IsVisible || (enemy.Seen && enemy.TimeSinceSeen < _enemyLocationTalkTimeSinceSeen))
                && EFTMath.RandomBool(_enemyLocationTalkChance))
            {
                EnemyDirectionCheck(enemy.EnemyPosition, out trigger, out mask);
            }

            if (trigger != EPhraseTrigger.PhraseNone)
            {
                return Bot.Talk.Say(trigger, mask, true);
            }
        }

        return false;
    }

    private bool EnemyDirectionCheck(Vector3 enemyPosition, out EPhraseTrigger trigger, out ETagStatus mask)
    {
        // Check Behind
        if (IsEnemyInDirection(enemyPosition, 180f, AngleToDot(_enemyLocationBehindAngle)))
        {
            mask = ETagStatus.Aware;
            trigger = EPhraseTrigger.OnSix;
            return true;
        }

        // Check Left Flank
        if (IsEnemyInDirection(enemyPosition, -90f, AngleToDot(_enemyLocationSideAngle)))
        {
            mask = ETagStatus.Aware;
            trigger = EPhraseTrigger.LeftFlank;
            return true;
        }

        // Check Right Flank
        if (IsEnemyInDirection(enemyPosition, 90f, AngleToDot(_enemyLocationSideAngle)))
        {
            mask = ETagStatus.Aware;
            trigger = EPhraseTrigger.RightFlank;
            return true;
        }

        // Check Front
        if (IsEnemyInDirection(enemyPosition, 0f, AngleToDot(_enemyLocationFrontAngle)))
        {
            mask = ETagStatus.Combat;
            trigger = EPhraseTrigger.InTheFront;
            return true;
        }

        trigger = EPhraseTrigger.PhraseNone;
        mask = ETagStatus.Unaware;
        return false;
    }

    private float AngleToRadians(float angle)
    {
        return (angle * (Mathf.PI)) / 180;
    }

    private float AngleToDot(float angle)
    {
        return Mathf.Cos(AngleToRadians(angle));
    }

    private bool CheckFriendlyLocation(out EPhraseTrigger trigger)
    {
        trigger = EPhraseTrigger.PhraseNone;
        if (Bot.Squad.SquadInfo?.MemberIsRegrouping == true)
        {
            trigger = EPhraseTrigger.Regroup;
            return true;
        }
        return false;
    }

    private bool IsEnemyInDirection(Vector3 enemyPosition, float angle, float threshold)
    {
        Vector3 enemyDirectionFromBot = enemyPosition - BotOwner.Transform.position;

        Vector3 enemyDirectionNormalized = enemyDirectionFromBot.normalized;
        Vector3 botLookDirectionNormalized = Player.MovementContext.PlayerRealForward.normalized;

        Vector3 direction = Quaternion.Euler(0f, angle, 0f) * botLookDirectionNormalized;

        return Vector3.Dot(enemyDirectionNormalized, direction) > threshold;
    }

    public void updateConfigSettings(SAINPresetClass preset)
    {
        var squadTalk = SAINPlugin.LoadedPreset?.GlobalSettings?.SquadTalk;
        if (squadTalk != null)
        {
            _reportReloadingChance = squadTalk._reportReloadingChance;
            _reportReloadingFreq = squadTalk._reportReloadingFreq;
            _reportLostVisualChance = squadTalk._reportLostVisualChance;
            _reportRatChance = squadTalk._reportRatChance;
            _reportRatTimeSinceSeen = squadTalk._reportRatTimeSinceSeen;
            _reportEnemyConversationChance = squadTalk._reportEnemyConversationChance;
            _reportEnemyMaxDist = squadTalk._reportEnemyMaxDist;
            _reportEnemyHealthChance = squadTalk._reportEnemyHealthChance;
            _reportEnemyHealthFreq = squadTalk._reportEnemyHealthFreq;
            _reportEnemyKilledChance = squadTalk._reportEnemyKilledChance;
            _reportEnemyKilledSquadLeadChance = squadTalk._reportEnemyKilledSquadLeadChance;
            _reportEnemyKilledToxicSquadLeader = squadTalk._reportEnemyKilledToxicSquadLeader;
            _friendCloseDist = squadTalk._friendCloseDist;
            _reportFriendKilledChance = squadTalk._reportFriendKilledChance;
            _talkRetreatChance = squadTalk._talkRetreatChance;
            _talkRetreatFreq = squadTalk._talkRetreatFreq;
            _talkRetreatTrigger = squadTalk._talkRetreatTrigger;
            _talkRetreatMask = squadTalk._talkRetreatMask;
            _talkRetreatGroupDelay = squadTalk._talkRetreatGroupDelay;
            _underFireNeedHelpChance = squadTalk._underFireNeedHelpChance;
            _underFireNeedHelpTrigger = squadTalk._underFireNeedHelpTrigger;
            _underFireNeedHelpMask = squadTalk._underFireNeedHelpMask;
            _underFireNeedHelpGroupDelay = squadTalk._underFireNeedHelpGroupDelay;
            _underFireNeedHelpFreq = squadTalk._underFireNeedHelpFreq;
            _hearNoiseChance = squadTalk._hearNoiseChance;
            _hearNoiseMaxDist = squadTalk._hearNoiseMaxDist;
            _hearNoiseFreq = squadTalk._hearNoiseFreq;
            _enemyLocationTalkChance = squadTalk._enemyLocationTalkChance;
            _enemyLocationTalkTimeSinceSeen = squadTalk._enemyLocationTalkTimeSinceSeen;
            _enemyNeedHelpChance = squadTalk._enemyNeedHelpChance;
            _enemyLocationTalkFreq = squadTalk._enemyLocationTalkFreq;
            _enemyLocationBehindAngle = squadTalk._enemyLocationBehindAngle;
            _enemyLocationSideAngle = squadTalk._enemyLocationSideAngle;
            _enemyLocationFrontAngle = squadTalk._enemyLocationFrontAngle;
        }
    }

    public SAINBotTalkClass LeaderComponent => Bot.Squad.LeaderComponent?.Talk;
    private float Randomized => Random.Range(0.75f, 1.25f);
    private BotSquadContainer BotSquad => Bot.Squad;

    private float _groupTalkFreq = 0.5f;
    private readonly List<EPhraseTrigger> LootPhrases = new() { EPhraseTrigger.LootBody, EPhraseTrigger.LootGeneric, EPhraseTrigger.OnLoot, EPhraseTrigger.CheckHim };
    private readonly List<EPhraseTrigger> reloadPhrases = new() { EPhraseTrigger.OnWeaponReload, EPhraseTrigger.NeedAmmo, EPhraseTrigger.OnOutOfAmmo };
    private float _nextReportReloadTime;
    private float _nextCheckEnemyHPTime;
    private bool _friendIsClose;
    private float _nextCheckFriendsTime;
    private float CheckFriendliesTimer = 0f;
    private float EnemyPosTimer = 0f;
    private float _nextCheckTalkRetreatTime;
    private float _underFireNeedHelpTime;
    private float _hearNoiseTime;
    private float _leaderCommandTime = 0f;
    private float _leadTime = 0f;
    private float TalkTimer = 0f;
    private float HurtTalkTimer = 0f;
    private bool Subscribed = false;

    private float _reportReloadingChance = 33f;
    private float _reportReloadingFreq = 1f;
    private float _reportLostVisualChance = 40f;
    private float _reportRatChance = 33f;
    private float _reportRatTimeSinceSeen = 60f;
    private float _reportEnemyConversationChance = 10f;
    private float _reportEnemyMaxDist = 70f;
    private float _reportEnemyHealthChance = 40f;
    private float _reportEnemyHealthFreq = 8f;
    private float _reportEnemyKilledChance = 60f;
    private float _reportEnemyKilledSquadLeadChance = 60f;
    private bool _reportEnemyKilledToxicSquadLeader = false;
    private float _friendCloseDist = 40f;
    private float _reportFriendKilledChance = 60f;
    private float _talkRetreatChance = 60f;
    private float _talkRetreatFreq = 10f;
    private EPhraseTrigger _talkRetreatTrigger = EPhraseTrigger.CoverMe;
    private ETagStatus _talkRetreatMask = ETagStatus.Combat;
    private bool _talkRetreatGroupDelay = true;
    private float _underFireNeedHelpChance = 45f;
    private EPhraseTrigger _underFireNeedHelpTrigger = EPhraseTrigger.NeedHelp;
    private ETagStatus _underFireNeedHelpMask = ETagStatus.Combat;
    private bool _underFireNeedHelpGroupDelay = true;
    private float _underFireNeedHelpFreq = 1f;
    private float _hearNoiseChance = 40f;
    private float _hearNoiseMaxDist = 70f;
    private float _hearNoiseFreq = 1f;
    private float _enemyLocationTalkChance = 60f;
    private float _enemyLocationTalkTimeSinceSeen = 3f;
    private float _enemyNeedHelpChance = 40f;
    private float _enemyLocationTalkFreq = 1f;
    private float _enemyLocationBehindAngle = 90f;
    private float _enemyLocationSideAngle = 45f;
    private float _enemyLocationFrontAngle = 90f;
}