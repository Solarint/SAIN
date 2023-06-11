using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Classes
{
    public class GroupTalk : SAINBot
    {
        protected ManualLogSource Logger;
        private BotTalkClass Talk => SAIN.Talk;

        public GroupTalk(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private void Subscribe()
        {
            if (!Subscribed)
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
        private const float EnemyTooClose = 5f;

        private void FriendlyDown(BotOwner bot)
        {
            if (BotOwner.IsDead || BotOwner.BotState != EBotState.Active)
            {
                return;
            }

            Talk.Say(EPhraseTrigger.OnFriendlyDown, ETagStatus.Combat, true);
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

            SAIN.Talk.Say(trigger, null, true);
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

        public void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding || !Talk.CanTalk || !BotSquad.BotInGroup)
            {
                if (Subscribed)
                {
                    Dispose();
                }
                return;
            }

            if (!Subscribed)
            {
                Subscribe();
            }

            if (TalkTimer < Time.time)
            {
                TalkTimer = Time.time + 0.1f;

                if (AreFriendsClose())
                {
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

                    if (BotOwner.Memory.GoalEnemy != null && SAIN.BotSquad.IAmLeader)
                    {
                        UpdateLeaderCommand();
                    }
                }
            }
        }

        private float MemberDistance(BotOwner member)
        {
            return (member.Position - BotOwner.Position).magnitude;
        }

        private bool AreFriendsClose()
        {
            bool closeFriend = false;
            foreach (var member in SAIN.BotSquad.SquadMembers.Keys)
            {
                if (member != null && !member.IsDead)
                {
                    if (MemberDistance(member) < 20f)
                    {
                        closeFriend = true;
                        break;
                    }
                }
            }
            return closeFriend;
        }

        private void AllMembersSay(EPhraseTrigger trigger, ETagStatus mask, float delay = 1.5f, float chance = 100f)
        {
            foreach (var member in BotSquad.SquadMembers.Values)
            {
                if (member != null && !member.BotOwner.IsDead)
                {
                    if (Vector3.Distance(member.Position, SAIN.BotSquad.Leader.Position) < 20f)
                    {
                        if (EFT_Math.RandomBool(chance))
                        {
                            member.Talk.TalkAfterDelay(trigger, mask, delay);
                        }
                    }
                }
            }
        }

        private void UpdateLeaderCommand()
        {
            if (LeaderComponent != null)
            {
                if (BotSquad.IAmLeader && LeaderTimer < Time.time)
                {
                    LeaderTimer = Time.time + LeaderFreq * Randomized;

                    if (!CheckIfLeaderShouldCommand())
                    {
                        if (CheckFriendliesTimer < Time.time && CheckFriendlyLocation(out var trigger))
                        {
                            CheckFriendliesTimer = Time.time + 10f;

                            SAIN.Talk.Say(trigger, ETagStatus.Combat, true);
                            AllMembersSay(EPhraseTrigger.Roger, ETagStatus.Aware, Random.Range(0.5f, 1.5f), 60f);
                        }
                    }
                }
            }
        }

        private float CheckFriendliesTimer = 0f;

        private bool TalkHurt()
        {
            if (HurtTalkTimer < Time.time)
            {
                var trigger = EPhraseTrigger.PhraseNone;
                HurtTalkTimer = Time.time + 15f * Random.Range(0.66f, 1.33f);

                if (SAIN.HasEnemy && SAIN.Enemy.PathDistance < 10f)
                {
                    return false;
                }

                var health = SAIN.HealthStatus;
                switch (health)
                {
                    case ETagStatus.Injured:
                        if (EFT_Math.RandomBool(25))
                        {
                            trigger = EFT_Math.RandomBool() ? EPhraseTrigger.HurtMedium : EPhraseTrigger.HurtLight;
                        }
                        break;

                    case ETagStatus.BadlyInjured:
                        trigger = EPhraseTrigger.HurtHeavy; break;
                    case ETagStatus.Dying:
                        trigger = EPhraseTrigger.HurtNearDeath; break;
                    default:
                        trigger = EPhraseTrigger.PhraseNone; break;
                }

                if (trigger != EPhraseTrigger.PhraseNone)
                {
                    Talk.Say(trigger);
                    return true;
                }
            }
            return false;
        }

        public bool TalkRetreat => SAIN.Enemy?.IsVisible == true && SAIN.Decision.RetreatDecisions.Contains(SAIN.CurrentDecision);

        public bool TalkCurrentAction()
        {
            EPhraseTrigger trigger;

            if (TalkRetreat)
            {
                trigger = EPhraseTrigger.NeedHelp;
            }
            else if (!RespondToAudio(out trigger, out var mask))
            {
                if (BotOwner.Memory.GoalEnemy != null)
                {
                    if (!TalkBotDecision(out trigger, out mask) && BotOwner.Memory.IsUnderFire)
                    {
                        trigger = EPhraseTrigger.NeedHelp;
                    }
                }
            }

            if (trigger != EPhraseTrigger.PhraseNone)
            {
                Talk.Say(trigger, null, true);
                return true;
            }
            return false;
        }

        private bool RespondToAudio(out EPhraseTrigger trigger, out ETagStatus mask)
        {
            trigger = EPhraseTrigger.PhraseNone;
            mask = ETagStatus.Aware;
            var hear = BotOwner.BotsGroup.YoungestPlace(BotOwner, 40f, true);

            if (BotOwner.Memory.GoalEnemy != null)
            {
                return false;
            }

            if (hear != null)
            {
                if (!hear.IsDanger)
                {
                    if (hear.CreatedTime + 0.5f < Time.time && hear.CreatedTime + 1f > Time.time)
                    {
                        if (BotOwner.Memory.GoalEnemy == null)
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
            mask = ETagStatus.Combat;
            switch (SelfDecision)
            {
                case SAINSelfDecision.Reload:
                    trigger = EPhraseTrigger.OnWeaponReload;
                    break;

                case SAINSelfDecision.RunAway:
                    trigger = EPhraseTrigger.OnYourOwn;
                    break;

                case SAINSelfDecision.FirstAid:
                case SAINSelfDecision.Stims:
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
            if (CommandSayTimer < Time.time)
            {
                CommandSayTimer = Time.time + 3f;
                var commandTrigger = EPhraseTrigger.PhraseNone;
                var commmandMask = ETagStatus.Unaware;
                var trigger = EPhraseTrigger.PhraseNone;
                var mask = ETagStatus.Unaware;

                if (SquadDecisions.Contains(SAINSquadDecision.Suppress))
                {
                    commandTrigger = EPhraseTrigger.Suppress;
                    commmandMask = ETagStatus.Combat;

                    trigger = EPhraseTrigger.Roger;
                    mask = ETagStatus.Aware;
                }
                else if (SAIN.CurrentDecision == SAINSoloDecision.Search)
                {
                    commandTrigger = EPhraseTrigger.FollowMe;
                    commmandMask = ETagStatus.Aware;

                    trigger = EPhraseTrigger.Going;
                    mask = ETagStatus.Aware;
                }
                else if (SoloDecisions.Contains(SAINSoloDecision.Search))
                {
                    commandTrigger = EFT_Math.RandomBool() ? EPhraseTrigger.GoForward : EPhraseTrigger.Gogogo;
                    commmandMask = ETagStatus.Combat;

                    trigger = EPhraseTrigger.Going;
                    mask = ETagStatus.Combat;
                }
                else if (SoloDecisions.Contains(SAINSoloDecision.RunForCover) || SoloDecisions.Contains(SAINSoloDecision.MoveToCover))
                {
                    commandTrigger = EPhraseTrigger.GetBack;
                    commmandMask = ETagStatus.Combat;

                    trigger = EPhraseTrigger.Going;
                    mask = ETagStatus.Combat;
                }
                else if (BotOwner.DoorOpener.Interacting && EFT_Math.RandomBool(33f))
                {
                    commandTrigger = EPhraseTrigger.OpenDoor;
                    commmandMask = ETagStatus.Aware;

                    trigger = EPhraseTrigger.Roger;
                    mask = ETagStatus.Aware;
                }
                else if (SAIN.CurrentDecision == SAINSoloDecision.RunAway)
                {
                    commandTrigger = EPhraseTrigger.OnYourOwn;
                    commmandMask = ETagStatus.Aware;

                    trigger = EFT_Math.RandomBool() ? EPhraseTrigger.Repeat : EPhraseTrigger.Stop;
                    mask = ETagStatus.Aware;
                }
                else if (!SoloDecisions.Contains(SAINSoloDecision.Search) && SAIN.CurrentDecision != SAINSoloDecision.Search)
                {
                    commandTrigger = EPhraseTrigger.HoldPosition;
                    commmandMask = ETagStatus.Aware;

                    trigger = EPhraseTrigger.Roger;
                    mask = ETagStatus.Aware;
                }

                if (commandTrigger != EPhraseTrigger.PhraseNone)
                {
                    Talk.Say(commandTrigger, commmandMask, true);
                    AllMembersSay(trigger, mask, Random.Range(0.5f, 1.5f), 50f);
                    return true;
                }
            }

            return false;
        }

        private float EnemyPosTimer = 0f;

        public bool TalkEnemyLocation()
        {
            if (EnemyPosTimer < Time.time)
            {
                EnemyPosTimer = Time.time + 1f;
                var trigger = EPhraseTrigger.PhraseNone;
                var mask = ETagStatus.Aware;

                if (SAIN.HasEnemyAndCanShoot)
                {
                    var enemy = SAIN.Enemy;
                    if (SAIN.HasEnemy && enemy.EnemyLookingAtMe)
                    {
                        mask = ETagStatus.Combat;
                        bool injured = !SAIN.Healthy && !SAIN.Injured;
                        trigger = injured ? EPhraseTrigger.NeedHelp : EPhraseTrigger.OnRepeatedContact;
                    }
                    else
                    {
                        EnemyDirectionCheck(enemy.Person.Position, out trigger, out mask);
                    }
                }

                if (trigger == EPhraseTrigger.PhraseNone && SayRatCheck())
                {
                    trigger = EPhraseTrigger.Rat;
                }

                if (trigger != EPhraseTrigger.PhraseNone)
                {
                    //BotTalkComponent.Talk.Say(trigger, true, mask);
                    Talk.Say(trigger, mask, true);
                    return true;
                }
            }

            return false;
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
            trigger = EPhraseTrigger.PhraseNone;

            int tooClose = 0;
            int total = 0;
            var locations = SAIN.BotSquad.SquadLocations;

            if (locations == null)
            {
                return false;
            }

            foreach (var location in locations)
            {
                if (location == null) continue;

                total++;
                bool close = Vector3.Distance(location, BotOwner.Position) <= FriendTooClose;
                tooClose += close ? 1 : 0;
            }

            float tooCloseRatio = (float)tooClose / total;

            if (tooCloseRatio > 0.5f)
            {
                trigger = EPhraseTrigger.Spreadout;
            }
            else if (SquadDecisions.Contains(SAINSquadDecision.Regroup))
            {
                trigger = EPhraseTrigger.Regroup;
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
            if (SAIN.Enemy != null && BotOwner.Memory.GoalEnemy != null)
            {
                if (SAIN.Enemy.GoalEnemy.TimeLastSeenReal > 30f && RatTimer < Time.time)
                {
                    RatTimer = Time.time + 60f * Random.Range(0.75f, 1.25f);

                    if (EFT_Math.RandomBool(33))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public SAINSoloDecision[] SoloDecisions => BotSquad.SquadSoloDecisions;
        public SAINSquadDecision[] SquadDecisions => BotSquad.SquadDecisions;
        public BotTalkClass LeaderComponent => SAIN.BotSquad.LeaderComponent?.Talk;
        private float Randomized => Random.Range(0.75f, 1.25f);
        private SquadClass BotSquad => SAIN.BotSquad;

        private float CommandSayTimer = 0f;
        private float LeaderTimer = 0f;
        private float TalkTimer = 0f;
        private float HurtTalkTimer = 0f;
        private float RatTimer = 0f;
        private bool Subscribed = false;
    }
}