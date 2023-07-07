using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.UserSettings.TalkConfig;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Classes
{
    public class BotTalkClass : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN?.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            PhraseObjectsAdd();
            GroupTalk = new GroupTalk(BotOwner);
            EnemyTalk = new EnemyTalk(BotOwner);
        }

        public bool CanTalk => SAIN.Info.CanBotTalk;

        public void Say(EPhraseTrigger type, ETagStatus? additionalMask = null, bool withGroupDelay = false)
        {
            if (CanTalk)
            {
                ETagStatus mask = SetETagMask(additionalMask);
                CheckPhrase(type, mask);
            }
        }

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (CanTalk)
            {
                GroupTalk.Update();

                EnemyTalk.Update();

                if (CheckTalkEventTimer < Time.time)
                {
                    CheckTalkEventTimer = Time.time + 0.33f * Random.Range(0.75f, 1.25f);
                    ResetTalk();
                }

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
                    allTalkDelay = Time.time + GlobalTalkLimit.Value;
                    if (TalkPack.phraseInfo.Phrase == EPhraseTrigger.Roger || TalkPack.phraseInfo.Phrase == EPhraseTrigger.Negative)
                    {
                        if (SAIN.Squad.VisibleMembers != null && SAIN.Squad.LeaderComponent != null && SAIN.Squad.VisibleMembers.Contains(SAIN.Squad.LeaderComponent) && SAIN.Enemy?.IsVisible == false)
                        {
                            if (TalkPack.phraseInfo.Phrase == EPhraseTrigger.Roger)
                            {
                                BotOwner.GetPlayer.HandsController.ShowGesture(EGesture.Good);
                            }
                            else
                            {
                                BotOwner.GetPlayer.HandsController.ShowGesture(EGesture.Bad);
                            }
                            return;
                        }
                    }
                    SendSayCommand(TalkPack);
                }
            }
        }

        private float TalkAfterDelayTimer = 0f;
        private BotTalkPackage TalkDelayPack;

        public void TalkAfterDelay(EPhraseTrigger trigger, ETagStatus mask, float delay)
        {
            var talk = new BotTalkPackage(PhraseDictionary[trigger], mask);
            TalkDelayPack = CheckPriority(talk, TalkDelayPack, out bool changeTalk);
            if (changeTalk)
            {
                TalkAfterDelayTimer = Time.time + delay;
            }
        }

        private void ResetTalk()
        {
            if (RecentTalk != null)
            {
                if (Time.time - RecentTalk.TimeCreated > Random.Range(1.5f, 2.5f))
                {
                    RecentTalk = null;
                }
            }
        }

        public void TalkEvent(EPhraseTrigger trigger, ETagStatus mask)
        {
            if (RecentTalk == null)
            {
                RecentTalk = new BotTalkPackage(PhraseDictionary[trigger], mask);
            }
        }

        public BotTalkPackage RecentTalk { get; private set; }
        public bool ThisBotTalked { get; private set; } = false;
        public float TimeSinceTalk { get; private set; } = 0f;

        private float CheckTalkEventTimer = 0f;
        public EnemyTalk EnemyTalk { get; private set; }
        public GroupTalk GroupTalk { get; private set; }


        private void SendSayCommand(EPhraseTrigger type, ETagStatus mask)
        {
            BotOwner.BotsGroup.GroupTalk.PhraseSad(BotOwner, type);
            BotOwner.GetPlayer.Say(type, false, 0f, mask);
            PhraseDictionary[type].TimeLastSaid = Time.time;

            if (DebugLogs.Value)
            {
                Logger.LogDebug($"Bot Said Phrase: [{type}] Mask: [{mask}]");
            }
        }

        private void SendSayCommand(BotTalkPackage talkPackage)
        {
            var phrase = talkPackage.phraseInfo.Phrase;
            BotOwner.BotsGroup.GroupTalk.PhraseSad(BotOwner, phrase);
            BotOwner.GetPlayer.Say(phrase, false, 0f, talkPackage.Mask);
            PhraseDictionary[phrase].TimeLastSaid = Time.time;

            if (DebugLogs.Value)
            {
                Logger.LogDebug($"Bot Said Phrase: [{phrase}] Mask: [{talkPackage.Mask}]");
            }
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

                switch (SAIN.Enemy.Person.Side)
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

            if (PhraseDictionary.ContainsKey(type))
            {
                var phrase = PhraseDictionary[type];
                if (phrase.TimeLastSaid + phrase.TimeDelay * TalkGlobalFreq.Value < Time.time)
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

        private void CheckDictionaryMissing(EPhraseTrigger trigger)
        {
            if (!PhraseDictionary.ContainsKey(trigger))
            {
                //DefaultLogger.LogError($"Phrase [{trigger}] is not in dictionary, adding it.");

                var phrase = new PhraseInfo(trigger, 10, 20f);
                Phrases.Add(phrase);

                PhraseDictionary.Add(trigger, phrase);
            }
        }

        private void PhraseObjectsAdd()
        {
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnGoodWork, 1, 60f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnBreath, 3, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.EnemyHit, 4, 3f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Rat, 5, 120f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnMutter, 6, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnEnemyDown, 7, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnEnemyConversation, 8, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.GoForward, 9, 40f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Gogogo, 10, 40f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Going, 11, 60f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnFight, 12, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnEnemyShot, 13, 3f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnLostVisual, 14, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnRepeatedContact, 15, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnFirstContact, 16, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnBeingHurtDissapoinment, 17, 35f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.StartHeal, 18, 75f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HurtLight, 19, 60f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnWeaponReload, 20, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnOutOfAmmo, 21, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HurtMedium, 22, 60f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HurtHeavy, 23, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LegBroken, 24, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HandBroken, 25, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HurtNearDeath, 26, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnFriendlyDown, 27, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.FriendlyFire, 28, 2f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.NeedHelp, 29, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.GetInCover, 30, 40f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LeftFlank, 31, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.RightFlank, 32, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.NeedWeapon, 33, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.WeaponBroken, 34, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnGrenade, 35, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnEnemyGrenade, 36, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Stop, 37, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnBeingHurt, 38, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnAgony, 39, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnDeath, 40, 1f));

            Phrases.Add(new PhraseInfo(EPhraseTrigger.Regroup, 10, 80f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnSix, 15, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.InTheFront, 15, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.FollowMe, 15, 45f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HoldPosition, 6, 60f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Suppress, 20, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Roger, 10, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Negative, 10, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.PhraseNone, 1, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Attention, 25, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnYourOwn, 25, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Repeat, 25, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.CoverMe, 25, 45f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.NoisePhrase, 5, 120f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.UnderFire, 34, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.MumblePhrase, 10, 35f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.GetBack, 10, 45f));

            Phrases.Add(new PhraseInfo(EPhraseTrigger.LootBody, 5, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LootContainer, 5, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LootGeneric, 5, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LootKey, 5, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LootMoney, 5, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LootNothing, 5, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LootWeapon, 5, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnLoot, 5, 30f));

            foreach (var phrase in Phrases)
            {
                PhraseDictionary.Add(phrase.Phrase, phrase);
            }

            foreach (EPhraseTrigger value in System.Enum.GetValues(typeof(EPhraseTrigger)))
            {
                if (!PhraseDictionary.ContainsKey(value))
                {
                    var phrase = new PhraseInfo(value, 5, 10f);
                    Phrases.Add(phrase);
                    PhraseDictionary.Add(value, phrase);
                }
            }
        }

        private readonly List<PhraseInfo> Phrases = new List<PhraseInfo>();

        private BotTalkPackage NormalBotTalk;

        private bool TalkPriorityActive = false;

        private float TalkPriorityTimer = 0f;

        private float allTalkDelay = 0f;

        private readonly Dictionary<EPhraseTrigger, PhraseInfo> PhraseDictionary = new Dictionary<EPhraseTrigger, PhraseInfo>();

        private ManualLogSource Logger;
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
            TimeDelay = timeDelay * Random.Range(0.75f, 1.25f);
        }

        public EPhraseTrigger Phrase { get; private set; }
        public int Priority { get; private set; }
        public float TimeDelay { get; set; }

        public float TimeLastSaid = 0f;
    }
}