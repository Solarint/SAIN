using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SAIN.Layers;
using SAIN.Components;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace SAIN.Classes
{
    public class GroupTalk : SAINBot
    {
        protected ManualLogSource Logger;

        public GroupTalk(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);

            BotSquad = SAIN.BotSquad;

            if (BotSquad.BotInGroup)
            {
                Subscribed = true;

                BotOwner.BotsGroup.OnMemberRemove += FriendlyDown;
                BotOwner.BotsGroup.OnReportEnemy += Contact;
                BotOwner.DeadBodyWork.OnStartLookToBody += LootStuff;
            }
        }

        private const float LeaderFreq = 1f;
        private const float TalkFreq = 0.5f;
        private const float FriendTooFar = 30f;
        private const float FriendTooClose = 5f;
        private const float EnemyTooClose = 10f;

        private void FriendlyDown(BotOwner bot)
        {
            if (BotOwner.IsDead || BotOwner.BotState != EBotState.Active)
            {
                return;
            }

            SAIN.Talk.Talk.Say(EPhraseTrigger.OnFriendlyDown, ETagStatus.Combat, true);
        }

        private float FirstContactTimer = 0f;
        private const float FirstContactFreq = 5f;

        private void Contact(IAIDetails person, Vector3 enemyPos, Vector3 weaponRootLast, EEnemyPartVisibleType isVisibleOnlyBySense)
        {
            if (BotOwner.IsDead || BotOwner.BotState != EBotState.Active)
            {
                return;
            }

            if (FirstContactTimer < Time.time)
            {
            }
        }

        private void LootStuff(float num)
        {
            if (BotOwner.IsDead || BotOwner.BotState != EBotState.Active)
            {
                return;
            }

            var trigger = EPhraseTrigger.LootBody;
            trigger |= EPhraseTrigger.LootContainer;
            trigger |= EPhraseTrigger.LootGeneric;
            trigger |= EPhraseTrigger.LootKey;
            trigger |= EPhraseTrigger.LootMoney;
            trigger |= EPhraseTrigger.LootNothing;
            trigger |= EPhraseTrigger.LootWeapon;
            trigger |= EPhraseTrigger.OnLoot;

            SAIN.Talk.Talk.Say(trigger, null, true);
        }

        public void Dispose()
        {
            if (Subscribed)
            {
                BotOwner.BotsGroup.OnMemberRemove -= FriendlyDown;
                BotOwner.BotsGroup.OnReportEnemy -= Contact;
                BotOwner.DeadBodyWork.OnStartLookToBody -= LootStuff;
                Subscribed = false;
            }
        }

        public void ManualUpdate()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

            if (!BotSquad.BotInGroup)
            {
                Dispose();
                return;
            }
            else
            {
                if (LeaderComponent == null)
                {
                    LeaderComponent = BotSquad.Leader.GetComponent<BotTalkComponent>();
                }

                if (TalkTimer < Time.time)
                {
                    if (AreFriendsClose())
                    {
                        GetMemberDecisions();

                        TalkTimer = Time.time + TalkFreq * Randomized;

                        GetEnemies();

                        if (!TalkHurt())
                        {
                            if (!TalkCurrentAction())
                            {
                                if (BotOwner.Memory.GoalEnemy != null)
                                {
                                    TalkEnemyLocation();
                                }
                            }
                        }
                    }
                    if (BotOwner.Memory.GoalEnemy != null && SAIN.BotSquad.IsSquadLead)
                    {
                        UpdateLeaderCommand();
                    }
                }
            }
        }

        private bool AreFriendsClose()
        {
            bool closeFriend = false;
            foreach (var member in SAIN.BotSquad.SquadMembers.Keys)
            {
                if (member != null && !member.IsDead)
                {
                    if (Vector3.Distance(member.Transform.position, BotOwner.Transform.position) < 30f)
                    {
                        closeFriend = true;
                        break;
                    }
                }
            }
            return closeFriend;
        }

        private void GetMemberDecisions()
        {
            List <SAINLogicDecision> decisions = new List <SAINLogicDecision>();
            foreach (var member in BotSquad.SquadMembers.Values)
            {
                if (member != null)
                {
                    decisions.Add(member.CurrentDecision);
                }
            }

            GroupDecisions = decisions;
        }

        private void AllMembersSay(TalkEventObject talk, float delay = 1f, float chance = 100f)
        {
            foreach (var member in BotSquad.SquadMembers.Keys)
            {
                if (member != null && !member.IsDead)
                {
                    if (Vector3.Distance(member.Transform.position, SAIN.BotSquad.Leader.Transform.position) < 30f)
                    {
                        if (EFT_Math.RandomBool(chance))
                        {
                            member?.GetComponent<BotTalkComponent>()?.TalkAfterDelay(talk, delay);
                        }
                    }
                }
            }
        }

        private void UpdateLeaderCommand()
        {
            if (LeaderComponent != null)
            {
                if (BotSquad.IsSquadLead && LeaderTimer < Time.time)
                {
                    LeaderTimer = Time.time + LeaderFreq * Randomized;

                    if (!CheckIfLeaderShouldCommand())
                    {
                        if (CheckFriendlyLocation(out var trigger))
                        {
                            SAIN.Talk.Talk.Say(trigger, ETagStatus.Combat, true);
                            AllMembersSay(new TalkEventObject(EPhraseTrigger.Roger, ETagStatus.Aware, false), Random.Range(0.5f, 1.5f), 60f);
                        }
                    }
                }
            }
        }

        private void GetEnemies()
        {
            Enemies = BotOwner.BotsGroup.Enemies.Keys.ToList();
        }

        private bool TalkHurt()
        {
            bool BotTalked = false;
            var trigger = EPhraseTrigger.PhraseNone;
            var mask = ETagStatus.Aware;

            if (BotOwner.Medecine.FirstAid.Using)
            {
                mask = ETagStatus.Aware;
                trigger = EPhraseTrigger.CoverMe;
                BotTalked = true;
            }
            else if (HurtTalkTimer < Time.time)
            {
                HurtTalkTimer = Time.time + 20f * Random.Range(0.75f, 1.25f);

                if (BotOwner.Memory.GoalEnemy == null)
                {
                    mask = ETagStatus.Unaware;
                }
                else
                {
                    mask = EFT_Math.RandomBool() ? ETagStatus.Combat : ETagStatus.Aware;
                }

                var botStatus = SAIN.BotStatus;

                if (botStatus.Injured)
                {
                    if (EFT_Math.RandomBool(25))
                    {
                        trigger = EFT_Math.RandomBool() ? EPhraseTrigger.HurtMedium : EPhraseTrigger.HurtLight;
                        BotTalked = true;
                    }
                }
                else if (botStatus.BadlyInjured)
                {
                    trigger = EPhraseTrigger.HurtHeavy;
                    BotTalked = true;
                }
                else if (botStatus.Dying)
                {
                    trigger = EPhraseTrigger.HurtNearDeath;
                    BotTalked = true;
                }
            }

            if (BotTalked)
            {
                //BotTalkComponent.Talk.Say(trigger, true, mask);
                SAIN.Talk.Talk.Say(trigger, mask, false);
            }

            return BotTalked;
        }

        public bool TalkCurrentAction()
        {
            EPhraseTrigger trigger;
            ETagStatus mask;

            if (!RespondToAudio(out trigger, out mask))
            {
                if (BotOwner.Memory.GoalEnemy != null)
                {
                    if (!TalkBotDecision(out trigger, out mask) && BotOwner.Memory.IsUnderFire)
                    {
                        trigger = EPhraseTrigger.UnderFire;
                        mask = ETagStatus.Combat;
                    }
                }
            }

            if (trigger != EPhraseTrigger.PhraseNone)
            {
                //BotTalkComponent.Talk.Say(trigger, true, mask);
                SAIN.Talk.Talk.Say(trigger, mask, true);
                return true;
            }
            return false;
        }

        private bool RespondToAudio(out EPhraseTrigger trigger, out ETagStatus mask)
        {
            trigger = EPhraseTrigger.PhraseNone;
            mask = ETagStatus.Aware;
            var hear = BotOwner.BotsGroup.YoungestPlace(BotOwner, 40f, true);

            if (hear != null)
            {
                if (!hear.IsDanger)
                {
                    if (hear.CreatedTime + 0.5f < Time.time && hear.CreatedTime + 1f > Time.time)
                    {
                        if (BotOwner.Memory.GoalEnemy != null && !BotOwner.Memory.GoalEnemy.CanShoot)
                        {
                            trigger = EPhraseTrigger.NoisePhrase;
                            mask = ETagStatus.Aware;
                        }
                        else if (BotOwner.Memory.GoalEnemy == null)
                        {
                            trigger = EPhraseTrigger.NoisePhrase;
                            mask = ETagStatus.Aware;
                        }
                    }
                }
            }

            return trigger != EPhraseTrigger.PhraseNone;
        }

        private bool TalkBotDecision(out EPhraseTrigger trigger, out ETagStatus mask)
        {
            var decision = SAIN.CurrentDecision;
            mask = ETagStatus.Combat;

            switch (decision)
            {
                case SAINLogicDecision.Reload:
                    trigger = EPhraseTrigger.OnWeaponReload;
                    break;

                case SAINLogicDecision.RunAway:
                    trigger = EPhraseTrigger.OnYourOwn;
                    break;

                case SAINLogicDecision.Suppress:
                    trigger = EPhraseTrigger.Suppress;
                    break;

                case SAINLogicDecision.FirstAid:
                case SAINLogicDecision.Surgery:
                    trigger = EPhraseTrigger.CoverMe;
                    break;

                default:
                    trigger = EPhraseTrigger.PhraseNone;
                    break;
            }

            return trigger != EPhraseTrigger.PhraseNone;
        }

        public bool CheckIfLeaderShouldCommand()
        {
            var commandTrigger = EPhraseTrigger.PhraseNone;
            var commmandMask = ETagStatus.Unaware;

            if (CommandSayTimer < Time.time)
            {
                CommandSayTimer = Time.time + 0.5f;

                var trigger = EPhraseTrigger.PhraseNone;
                var mask = ETagStatus.Unaware;

                if (GroupDecisions.Contains(SAINLogicDecision.Suppress))
                {
                    commandTrigger = EPhraseTrigger.Suppress;
                    commmandMask = ETagStatus.Combat;

                    trigger = EPhraseTrigger.Roger;
                    mask = ETagStatus.Aware;
                }
                else if (GroupDecisions.Contains(SAINLogicDecision.Search))
                {
                    commandTrigger = EFT_Math.RandomBool() ? EPhraseTrigger.GoForward : EPhraseTrigger.Gogogo;
                    commmandMask = ETagStatus.Combat;

                    trigger = EPhraseTrigger.Going;
                    mask = ETagStatus.Combat;
                }
                else if (SAIN.CurrentDecision == SAINLogicDecision.Search)
                {
                    commandTrigger = EPhraseTrigger.FollowMe;
                    commmandMask = ETagStatus.Aware;

                    trigger = EPhraseTrigger.Going;
                    mask = ETagStatus.Aware;
                }
                else if (GroupDecisions.Contains(SAINLogicDecision.RunForCover) || GroupDecisions.Contains(SAINLogicDecision.WalkToCover))
                {
                    commandTrigger = EFT_Math.RandomBool() ? EPhraseTrigger.GetInCover : EPhraseTrigger.Attention;
                    commmandMask = ETagStatus.Combat;

                    trigger = EPhraseTrigger.Going;
                    mask = ETagStatus.Combat;
                }
                else if (BotOwner.DoorOpener.Interacting && EFT_Math.RandomBool(33f))
                {
                    //commandTrigger = EPhraseTrigger.OpenDoor;
                    //commmandMask = ETagStatus.Aware;

                    //trigger = EPhraseTrigger.Roger;
                    //mask = ETagStatus.Aware;
                }
                else if (SAIN.CurrentDecision == SAINLogicDecision.RunAway)
                {
                    commandTrigger = EPhraseTrigger.OnYourOwn;
                    commmandMask = ETagStatus.Aware;

                    trigger = EFT_Math.RandomBool() ? EPhraseTrigger.Repeat : EPhraseTrigger.Stop;
                    mask = ETagStatus.Aware;
                }

                if (commandTrigger != EPhraseTrigger.PhraseNone)
                {
                    SAIN.Talk.Talk.Say(commandTrigger, commmandMask, true);
                    AllMembersSay(new TalkEventObject(trigger, mask, false), Random.Range(0.5f, 1.5f), 50f);
                }
            }

            return commandTrigger != EPhraseTrigger.PhraseNone;
        }

        public bool TalkEnemyLocation()
        {
            bool BotTalked = false;
            var trigger = EPhraseTrigger.PhraseNone;
            var mask = ETagStatus.Aware;

            var enemy = SAIN.Enemies;
            if (SAIN.HasEnemyAndCanShoot)
            {
                Vector3 enemyPosition = BotOwner.Memory.GoalEnemy.CurrPosition;

                if (enemy.PriorityEnemy != null && enemy.PriorityEnemy.EnemyLookingAtMe)
                {
                    mask = ETagStatus.Combat;
                    bool injured = !SAIN.BotStatus.Healthy && !SAIN.BotStatus.Injured;
                    trigger = injured ? EPhraseTrigger.NeedHelp : EPhraseTrigger.OnRepeatedContact;

                    BotTalked = true;
                }
                else if (EnemyDirectionCheck(enemyPosition, out trigger, out mask))
                {
                    BotTalked = true;
                }
            }

            if (!BotTalked && SayRatCheck())
            {
                mask = ETagStatus.Aware;
                trigger = EPhraseTrigger.Rat;
                BotTalked = true;
            }

            if (BotTalked)
            {
                //BotTalkComponent.Talk.Say(trigger, true, mask);
                SAIN.Talk.Talk.Say(trigger, mask, true);
            }

            return BotTalked;
        }

        private bool EnemyDirectionCheck(Vector3 enemyPosition, out EPhraseTrigger trigger, out ETagStatus mask)
        {
            // Check Behind
            if (IsEnemyInDirection(enemyPosition, 180f, AngleToDot(75f)))
            {
                mask = ETagStatus.Aware;
                trigger = EPhraseTrigger.OnSix;
                return true;
            }

            // Check Left Flank
            if (IsEnemyInDirection(enemyPosition, -90f, AngleToDot(33f)))
            {
                mask = ETagStatus.Aware;
                trigger = EPhraseTrigger.LeftFlank;
                return true;
            }

            // Check Right Flank
            if (IsEnemyInDirection(enemyPosition, 90f, AngleToDot(33f)))
            {
                mask = ETagStatus.Aware;
                trigger = EPhraseTrigger.RightFlank;
                return true;
            }

            // Check Front
            if (IsEnemyInDirection(enemyPosition, 0f, AngleToDot(33f)))
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
            int tooFar = 0;
            int tooClose = 0;
            int tooCloseEnemy = 0;
            int total = 0;
            foreach (var bot in BotOwner.BotsGroup.Allies)
            {
                if (bot != null)
                {
                    total++;

                    float distance = Vector3.Distance(bot.Transform.position, BotOwner.Transform.position);

                    if (distance >= FriendTooFar)
                    {
                        tooFar++;
                    }
                    else if (distance <= FriendTooClose)
                    {
                        tooClose++;
                    }
                    else
                    {
                        foreach (var enemy in Enemies)
                        {
                            if (enemy != null)
                            {
                                float enemyDistance = Vector3.Distance(bot.Transform.position, enemy.Transform.position);
                                if (enemyDistance <= EnemyTooClose)
                                {
                                    tooCloseEnemy++;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            float tooCloseRatio = (float)tooClose / total;
            float tooFarRatio = (float)tooFar / total;

            if (tooCloseRatio > 0.5f)
            {
                trigger = EPhraseTrigger.Spreadout;
            }
            else if (tooFarRatio > 0.5f)
            {
                trigger = EPhraseTrigger.Regroup;
            }
            else if (tooCloseEnemy > 0)
            {
                trigger = EPhraseTrigger.GetBack;
            }
            else
            {
                trigger = EPhraseTrigger.PhraseNone;
            }

            return trigger != EPhraseTrigger.PhraseNone;
        }

        private bool IsEnemyInDirection(Vector3 enemyPosition, float angle, float threshold)
        {
            Vector3 enemyDirectionFromBot = enemyPosition - BotOwner.Transform.position;

            Vector3 enemyDirectionNormalized = enemyDirectionFromBot.normalized;
            Vector3 botLookDirectionNormalized = BotOwner.GetPlayer.MovementContext.PlayerRealForward.normalized;

            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * botLookDirectionNormalized;

            return Vector3.Dot(enemyDirectionNormalized, direction) > threshold;
        }

        private bool SayRatCheck()
        {
            if (SAIN.Enemies.LastSeen.TimeSinceSeen > 30f && RatTimer < Time.time && BotOwner.Memory.GoalEnemy != null)
            {
                RatTimer = Time.time + 120f * Random.Range(0.75f, 1.25f);

                if (EFT_Math.RandomBool(33))
                {
                    return true;
                }
            }
            return false;
        }

        public List<SAINLogicDecision> GroupDecisions { get; private set; } = new List<SAINLogicDecision>();
        public List<IAIDetails> Enemies { get; private set; }
        public BotTalkComponent LeaderComponent { get; private set; }
        private float Randomized => Random.Range(0.75f, 1.25f);

        private readonly SquadClass BotSquad;

        private float CommandSayTimer = 0f;
        private float LeaderTimer = 0f;
        private float TalkTimer = 0f;
        private float HurtTalkTimer = 0f;
        private float RatTimer = 0f;
        private bool Subscribed = false;
    }
}