using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using SAIN.Helpers;
using SAIN.SAINComponent;

namespace SAIN.SAINComponent.Classes.Talk
{
    public class SAINBotTalkClass : SAINBase, ISAINClass
    {
        static SAINBotTalkClass()
        {
            GlobalPhraseDictionary = new Dictionary<EPhraseTrigger, PhraseInfo>();
            PhraseObjectsAdd();
        }

        public SAINBotTalkClass(SAINComponentClass sain) : base(sain)
        {
            PersonalPhraseDict = new Dictionary<EPhraseTrigger, PhraseInfo>(GlobalPhraseDictionary);
            GroupTalk = new GroupTalk(sain);
            EnemyTalk = new EnemyTalk(sain);
        }

        public void Init()
        {
        }

        public void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (CanTalk)
            {
                GroupTalk.Update();

                EnemyTalk.Update();

                BotTalkPackage TalkPack = null;

                if (TalkAfterDelayTimer < Time.time && TalkDelayPack != null)
                {
                    TalkPack = TalkDelayPack;
                    TalkDelayPack = null;
                }
                else if (TalkPriorityTimer < Time.time && NormalBotTalk != null)
                {
                    TalkPack = NormalBotTalk;
                    NormalBotTalk = null;
                    TalkPriorityActive = false;
                }

                if (allTalkDelay < Time.time && TalkPack != null)
                {
                    allTalkDelay = Time.time + SAIN.Info.FileSettings.Mind.TalkFrequency;
                    if (TalkPack.phraseInfo.Phrase == EPhraseTrigger.Roger || TalkPack.phraseInfo.Phrase == EPhraseTrigger.Negative)
                    {
                        if (SAIN.Squad.VisibleMembers != null && SAIN.Squad.LeaderComponent != null && SAIN.Squad.VisibleMembers.Contains(SAIN.Squad.LeaderComponent) && SAIN.Enemy?.IsVisible == false)
                        {
                            if (TalkPack.phraseInfo.Phrase == EPhraseTrigger.Roger)
                            {
                                Player.HandsController.ShowGesture(EGesture.Good);
                            }
                            else
                            {
                                Player.HandsController.ShowGesture(EGesture.Bad);
                            }
                            return;
                        }
                    }
                    SendSayCommand(TalkPack);
                }
            }
        }

        public void Dispose()
        {
        }

        public bool CanTalk => SAIN.Info.FileSettings.Mind.CanTalk;

        public void Say(EPhraseTrigger phrase, ETagStatus? additionalMask = null, bool withGroupDelay = false)
        {
            if (CanTalk || phrase == EPhraseTrigger.OnDeath || phrase == EPhraseTrigger.OnAgony || phrase == EPhraseTrigger.OnBeingHurt)
            {
                ETagStatus mask = SetETagMask(additionalMask);
                CheckPhrase(phrase, mask);
            }
        }

        private float TalkAfterDelayTimer = 0f;
        private BotTalkPackage TalkDelayPack;

        public void TalkAfterDelay(EPhraseTrigger trigger, ETagStatus mask, float delay)
        {
            var talk = new BotTalkPackage(PersonalPhraseDict[trigger], mask);
            TalkDelayPack = CheckPriority(talk, TalkDelayPack, out bool changeTalk);
            if (changeTalk)
            {
                TalkAfterDelayTimer = Time.time + delay;
            }
        }

        public EnemyTalk EnemyTalk { get; private set; }
        public GroupTalk GroupTalk { get; private set; }


        private void SendSayCommand(EPhraseTrigger type, ETagStatus mask)
        {
            BotOwner.BotsGroup.GroupTalk.PhraseSad(BotOwner, type);
            Player.Say(type, false, 0f, mask);
            PersonalPhraseDict[type].TimeLastSaid = Time.time;
            if (SAINPlugin.GlobalDebugMode)
            {
                Logger.LogDebug(type);
            }
        }

        private void SendSayCommand(BotTalkPackage talkPackage)
        {
            SendSayCommand(talkPackage.phraseInfo.Phrase, talkPackage.Mask);
        }

        private ETagStatus SetETagMask(ETagStatus? additionaMask = null)
        {
            ETagStatus etagStatus;
            if (BotOwner.BotsGroup.MembersCount > 1)
            {
                etagStatus = ETagStatus.Coop;
            }
            else
            {
                etagStatus = ETagStatus.Solo;
            }

            if (BotOwner.Memory.IsUnderFire)
            {
                etagStatus |= ETagStatus.Combat;
            }
            else if (SAIN.Enemy != null)
            {
                if (SAIN.Enemy.Seen && SAIN.Enemy.TimeSinceSeen < 10f)
                {
                    etagStatus |= ETagStatus.Combat;
                }
                else
                {
                    etagStatus |= ETagStatus.Aware;
                }

                switch (SAIN.Enemy.EnemyIAIDetails.Side)
                {
                    case EPlayerSide.Usec:
                        etagStatus |= ETagStatus.Usec;
                        break;

                    case EPlayerSide.Bear:
                        etagStatus |= ETagStatus.Bear;
                        break;

                    case EPlayerSide.Savage:
                        etagStatus |= ETagStatus.Scav;
                        break;
                }
            }
            else if (BotOwner.Memory.GoalTarget.HavePlaceTarget())
            {
                etagStatus |= ETagStatus.Aware;
            }
            else
            {
                etagStatus |= ETagStatus.Unaware;
            }

            if (additionaMask != null)
            {
                etagStatus |= additionaMask.Value;
            }

            return etagStatus;
        }

        private void CheckPhrase(EPhraseTrigger type, ETagStatus mask)
        {
            if (type == EPhraseTrigger.OnDeath || type == EPhraseTrigger.OnAgony || type == EPhraseTrigger.OnBeingHurt)
            {
                SendSayCommand(type, mask);
                return;
            }

            if (PersonalPhraseDict.ContainsKey(type))
            {
                var phrase = PersonalPhraseDict[type];
                if (phrase.TimeLastSaid + phrase.TimeDelay < Time.time)
                {
                    var data = new BotTalkPackage(phrase, mask);

                    NormalBotTalk = CheckPriority(data, NormalBotTalk);

                    if (!TalkPriorityActive)
                    {
                        TalkPriorityActive = true;
                        TalkPriorityTimer = Time.time + 0.15f;
                    }
                }
            }
        }

        private BotTalkPackage CheckPriority(BotTalkPackage newTalk, BotTalkPackage oldTalk)
        {
            if (oldTalk == null)
            {
                return newTalk;
            }
            if (newTalk == null)
            {
                return oldTalk;
            }

            int newPriority = newTalk.phraseInfo.Priority;
            int oldPriority = oldTalk.phraseInfo.Priority;

            bool ChangeTalk = oldPriority < newPriority;

            return ChangeTalk ? newTalk : oldTalk;
        }

        private BotTalkPackage CheckPriority(BotTalkPackage newTalk, BotTalkPackage oldTalk, out bool ChangeTalk)
        {
            if (oldTalk == null)
            {
                ChangeTalk = true;
                return newTalk;
            }
            if (newTalk == null)
            {
                ChangeTalk = false;
                return oldTalk;
            }

            int newPriority = newTalk.phraseInfo.Priority;
            int oldPriority = oldTalk.phraseInfo.Priority;

            ChangeTalk = oldPriority < newPriority;

            return ChangeTalk ? newTalk : oldTalk;
        }

        static void PhraseObjectsAdd()
        {
            AddPhrase(EPhraseTrigger.OnGoodWork, 1, 60f);
            AddPhrase(EPhraseTrigger.OnBreath, 3, 15f);
            AddPhrase(EPhraseTrigger.EnemyHit, 4, 3f);
            AddPhrase(EPhraseTrigger.Rat, 5, 120f);
            AddPhrase(EPhraseTrigger.OnMutter, 6, 20f);
            AddPhrase(EPhraseTrigger.OnEnemyDown, 7, 10f);
            AddPhrase(EPhraseTrigger.OnEnemyConversation, 8, 30f);
            AddPhrase(EPhraseTrigger.GoForward, 9, 40f);
            AddPhrase(EPhraseTrigger.Gogogo, 10, 40f);
            AddPhrase(EPhraseTrigger.Going, 11, 60f);
            AddPhrase(EPhraseTrigger.OnFight, 12, 5f);
            AddPhrase(EPhraseTrigger.OnEnemyShot, 13, 3f);
            AddPhrase(EPhraseTrigger.OnLostVisual, 14, 10f);
            AddPhrase(EPhraseTrigger.OnRepeatedContact, 15, 5f);
            AddPhrase(EPhraseTrigger.OnFirstContact, 16, 5f);
            AddPhrase(EPhraseTrigger.OnBeingHurtDissapoinment, 17, 35f);
            AddPhrase(EPhraseTrigger.StartHeal, 18, 75f);
            AddPhrase(EPhraseTrigger.HurtLight, 19, 60f);
            AddPhrase(EPhraseTrigger.OnWeaponReload, 20, 10f);
            AddPhrase(EPhraseTrigger.OnOutOfAmmo, 21, 15f);
            AddPhrase(EPhraseTrigger.HurtMedium, 22, 60f);
            AddPhrase(EPhraseTrigger.HurtHeavy, 23, 30f);
            AddPhrase(EPhraseTrigger.LegBroken, 24, 30f);
            AddPhrase(EPhraseTrigger.HandBroken, 25, 30f);
            AddPhrase(EPhraseTrigger.HurtNearDeath, 26, 20f);
            AddPhrase(EPhraseTrigger.OnFriendlyDown, 27, 10f);
            AddPhrase(EPhraseTrigger.FriendlyFire, 28, 2f);
            AddPhrase(EPhraseTrigger.NeedHelp, 29, 30f);
            AddPhrase(EPhraseTrigger.GetInCover, 30, 40f);
            AddPhrase(EPhraseTrigger.LeftFlank, 31, 5f);
            AddPhrase(EPhraseTrigger.RightFlank, 32, 5f);
            AddPhrase(EPhraseTrigger.NeedWeapon, 33, 15f);
            AddPhrase(EPhraseTrigger.WeaponBroken, 34, 15f);
            AddPhrase(EPhraseTrigger.OnGrenade, 35, 10f);
            AddPhrase(EPhraseTrigger.OnEnemyGrenade, 36, 10f);
            AddPhrase(EPhraseTrigger.Stop, 37, 1f);
            AddPhrase(EPhraseTrigger.OnBeingHurt, 38, 1f);
            AddPhrase(EPhraseTrigger.OnAgony, 39, 1f);
            AddPhrase(EPhraseTrigger.OnDeath, 40, 1f);
            AddPhrase(EPhraseTrigger.Regroup, 10, 80f);
            AddPhrase(EPhraseTrigger.OnSix, 15, 10f);
            AddPhrase(EPhraseTrigger.InTheFront, 15, 20f);
            AddPhrase(EPhraseTrigger.FollowMe, 15, 45f);
            AddPhrase(EPhraseTrigger.HoldPosition, 6, 60f);
            AddPhrase(EPhraseTrigger.Suppress, 20, 15f);
            AddPhrase(EPhraseTrigger.Roger, 10, 30f);
            AddPhrase(EPhraseTrigger.Negative, 10, 30f);
            AddPhrase(EPhraseTrigger.PhraseNone, 1, 1f);
            AddPhrase(EPhraseTrigger.Attention, 25, 30f);
            AddPhrase(EPhraseTrigger.OnYourOwn, 25, 15f);
            AddPhrase(EPhraseTrigger.Repeat, 25, 30f);
            AddPhrase(EPhraseTrigger.CoverMe, 25, 45f);
            AddPhrase(EPhraseTrigger.NoisePhrase, 5, 120f);
            AddPhrase(EPhraseTrigger.UnderFire, 34, 5f);
            AddPhrase(EPhraseTrigger.MumblePhrase, 10, 35f);
            AddPhrase(EPhraseTrigger.GetBack, 10, 45f);
            AddPhrase(EPhraseTrigger.LootBody, 5, 30f);
            AddPhrase(EPhraseTrigger.LootContainer, 5, 30f);
            AddPhrase(EPhraseTrigger.LootGeneric, 5, 30f);
            AddPhrase(EPhraseTrigger.LootKey, 5, 30f);
            AddPhrase(EPhraseTrigger.LootMoney, 5, 30f);
            AddPhrase(EPhraseTrigger.LootNothing, 5, 30f);
            AddPhrase(EPhraseTrigger.LootWeapon, 5, 30f);
            AddPhrase(EPhraseTrigger.OnLoot, 5, 30f);

            foreach (EPhraseTrigger value in System.Enum.GetValues(typeof(EPhraseTrigger)))
            {
                AddPhrase(value, 5, 10f);
            }
        }

        static void AddPhrase(EPhraseTrigger phrase, int priority, float timeDelay)
        {
            if (!GlobalPhraseDictionary.ContainsKey(phrase))
            {
                GlobalPhraseDictionary.Add(phrase, new PhraseInfo(phrase, priority, timeDelay));
            }
        }

        private BotTalkPackage NormalBotTalk;

        private bool TalkPriorityActive = false;

        private float TalkPriorityTimer = 0f;

        private float allTalkDelay = 0f;

        public static readonly Dictionary<EPhraseTrigger, PhraseInfo> GlobalPhraseDictionary = new Dictionary<EPhraseTrigger, PhraseInfo>();
        private Dictionary<EPhraseTrigger, PhraseInfo> PersonalPhraseDict;
    }

    public class BotTalkPackage
    {
        public BotTalkPackage(PhraseInfo phrase, ETagStatus mask)
        {
            phraseInfo = phrase;
            Mask = mask;
            TimeCreated = Time.time;
        }

        public PhraseInfo phraseInfo;

        public ETagStatus Mask;

        public float TimeCreated { get; private set; }
    }

    public class PhraseInfo
    {
        public PhraseInfo(EPhraseTrigger trigger, int priority, float timeDelay)
        {
            Phrase = trigger;
            Priority = priority;
            TimeDelay = timeDelay;
        }

        public EPhraseTrigger Phrase { get; private set; }
        public int Priority { get; private set; }
        public float TimeDelay { get; set; }

        public float TimeLastSaid = 0f;
    }
}