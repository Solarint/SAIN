using BepInEx.Logging;
using EFT;
using SAIN.UserSettings;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.UserSettings.TalkConfig;

namespace SAIN.Classes
{
    public class BotTalk : SAINBot
    {
        private float BotTalkDelaySetting => GlobalTalkLimit.Value;

        public BotTalk(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);

            PhraseObjectsAdd();

            CanTalk = BotOwner.Settings.FileSettings.Mind.CAN_TALK;
        }

        public bool CanBotTalk
        {
            get
            {
                if (!CanTalk || NoTalkGlobal.Value)
                {
                    return false;
                }

                bool isBoss = SAIN.Info.IsBoss;
                bool isFollower = SAIN.Info.IsFollower;
                var type = SAIN.Info.BotType;

                if (isBoss && NoBossTalk.Value)
                {
                    return false;
                }
                if (isFollower && NoFollowerTalk.Value)
                {
                    return false;
                }

                bool scav = type == WildSpawnType.assault || type == WildSpawnType.cursedAssault || type == WildSpawnType.marksman;
                if (scav && NoScavTalk.Value)
                {
                    return false;
                }

                bool isPMC = !scav && type != WildSpawnType.pmcBot && type != WildSpawnType.exUsec && !isBoss && !isFollower;
                if (isPMC && NoPMCTalk.Value)
                {
                    return false;
                }

                return true;
            }
        }

        public void Say(EPhraseTrigger type, ETagStatus? additionalMask = null, bool withGroupDelay = false)
        {
            if (!CanBotTalk)
            {
                return;
            }

            CheckDictionaryMissing(type);

            //if (withGroupDelay && !BotOwner.BotsGroup.GroupTalk.CanSay(BotOwner, type))
            //{
                //return;
            //}

            ETagStatus mask = SetETagMask(additionalMask);

            CheckPhrase(type, mask);
        }

        public void ManualUpdate()
        {
            if (!CanTalk)
            {
                return;
            }

            if (TalkPriorityTimer < Time.time && talkPackage != null)
            {
                TalkPriorityActive = false;

                if (allTalkDelay < Time.time)
                {
                    SendSayCommand(talkPackage);

                    allTalkDelay = Time.time + BotTalkDelaySetting;
                }

                talkPackage = null;
            }
        }

        private void SendSayCommand(EPhraseTrigger type, ETagStatus mask)
        {
            BotOwner.BotsGroup.GroupTalk.PhraseSad(BotOwner, type);
            BotOwner.GetPlayer.Say(type, false, 0f, mask);
            PhraseDictionary[type].TimeLastSaid = Time.time;
        }

        private void SendSayCommand(BotTalkPackage talkPackage)
        {
            var phrase = talkPackage.phraseInfo.Phrase;

            if (DebugLogs.Value)
            {
                Logger.LogDebug($"Bot Said Phrase: [{phrase}] Mask: [{talkPackage.Mask}]");
            }

            BotOwner.BotsGroup.GroupTalk.PhraseSad(BotOwner, phrase);
            BotOwner.GetPlayer.Say(phrase, false, 0f, talkPackage.Mask);
            PhraseDictionary[phrase].TimeLastSaid = Time.time;
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
            if (BotOwner.Memory.GoalEnemy != null)
            {
                etagStatus |= ETagStatus.Combat;
                switch (BotOwner.Memory.GoalEnemy.Person.Side)
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
            //etagStatus = ETagStatus.Combat;
            return etagStatus;
        }

        private void CheckPhrase(EPhraseTrigger type, ETagStatus mask)
        {
            if (type == EPhraseTrigger.OnDeath || type == EPhraseTrigger.OnAgony || type == EPhraseTrigger.OnBeingHurt)
            {
                SendSayCommand(type, mask);
                return;
            }

            var phrase = PhraseDictionary[type];

            if (phrase.TimeLastSaid + phrase.TimeDelay * TalkDelayModifier.Value < Time.time)
            {
                var data = new BotTalkPackage(phrase, mask);

                talkPackage = CheckPriority(data, talkPackage);

                if (!TalkPriorityActive)
                {
                    TalkPriorityActive = true;
                    TalkPriorityTimer = Time.time + 0.15f;
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

        private void CheckDictionaryMissing(EPhraseTrigger trigger)
        {
            if (!PhraseDictionary.ContainsKey(trigger))
            {
                //Logger.LogError($"Phrase [{trigger}] is not in dictionary, adding it.");

                var phrase = new PhraseInfo(trigger, 10, 20f);
                Phrases.Add(phrase);

                PhraseDictionary.Add(trigger, phrase);
            }
        }

        private void PhraseObjectsAdd()
        {
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnGoodWork, 1, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnBreath, 3, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.EnemyHit, 4, 3f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Rat, 5, 60f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnMutter, 6, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnEnemyDown, 7, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnEnemyConversation, 8, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.GoForward, 9, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Gogogo, 10, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Going, 11, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnFight, 12, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnEnemyShot, 13, 3f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnLostVisual, 14, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnRepeatedContact, 15, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnFirstContact, 16, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnBeingHurtDissapoinment, 17, 35f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.StartHeal, 18, 75f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HurtLight, 19, 60f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnWeaponReload, 20, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnOutOfAmmo, 21, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HurtMedium, 22, 60f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HurtHeavy, 23, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LegBroken, 24, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HandBroken, 25, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HurtNearDeath, 26, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnFriendlyDown, 27, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.FriendlyFire, 28, 2f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.NeedHelp, 29, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.GetInCover, 30, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.LeftFlank, 31, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.RightFlank, 32, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.NeedWeapon, 33, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.WeaponBroken, 34, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnGrenade, 35, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnEnemyGrenade, 36, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Stop, 37, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnBeingHurt, 38, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnAgony, 39, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnDeath, 40, 1f));

            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnSix, 15, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.InTheFront, 15, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.FollowMe, 15, 20f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.HoldPosition, 6, 30f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Suppress, 20, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Roger, 10, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Negative, 10, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.PhraseNone, 1, 1f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Attention, 25, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.OnYourOwn, 25, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.Repeat, 25, 15f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.CoverMe, 25, 10f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.NoisePhrase, 5, 60f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.UnderFire, 1, 5f));
            Phrases.Add(new PhraseInfo(EPhraseTrigger.MumblePhrase, 10, 35f));

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
                    //Logger.LogWarning($"Enum Value is not in dict [{value}]");

                    var phrase = new PhraseInfo(value, 5, 10f);

                    Phrases.Add(phrase);
                    PhraseDictionary.Add(value, phrase);
                }
            }
        }

        private readonly List<PhraseInfo> Phrases = new List<PhraseInfo>();

        private BotTalkPackage talkPackage;

        private bool TalkPriorityActive = false;

        private float TalkPriorityTimer = 0f;

        private float allTalkDelay = 0f;

        private readonly bool CanTalk = true;

        private readonly Dictionary<EPhraseTrigger, PhraseInfo> PhraseDictionary = new Dictionary<EPhraseTrigger, PhraseInfo>();

        private ManualLogSource Logger;
    }

    public class BotTalkPackage
    {
        public BotTalkPackage(PhraseInfo phrase, ETagStatus mask)
        {
            phraseInfo = phrase;
            Mask = mask;
        }

        public PhraseInfo phraseInfo;

        public ETagStatus Mask;
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