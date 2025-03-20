using EFT;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Talk
{
    public class SAINBotTalkClass : BotBase, IBotClass
    {
        public bool CanTalk => Bot.Info.FileSettings.Mind.CanTalk && _timeCanTalk < Time.time;
        public bool IsSpeaking => Player.Speaker?.Speaking == true;
        public EnemyTalk EnemyTalk { get; private set; }
        public GroupTalk GroupTalk { get; private set; }

        public SAINBotTalkClass(BotComponent sain) : base(sain)
        {
            PhraseObjectsAdd(_phraseDictionary);
            GroupTalk = new GroupTalk(sain);
            EnemyTalk = new EnemyTalk(sain);
            _timeCanTalk = Time.time + UnityEngine.Random.Range(1f, 2f);
        }

        public void Init()
        {
            //Player.BeingHitAction += GetHit;
            GroupTalk.Init();
            EnemyTalk.Init();
        }

        private void GetHit(DamageInfoStruct DamageInfoStruct, EBodyPart bodyPart, float floatVal)
        {
            if (Player == null || BotOwner == null || Bot == null) {
                return;
            }

            if (EFTMath.RandomBool(25) &&
                _nextGetHitTime < Time.time &&
                GroupTalk.FriendIsClose) {
                _nextGetHitTime = Time.time + 1f;
                EPhraseTrigger trigger = EPhraseTrigger.OnBeingHurt;
                ETagStatus mask = ETagStatus.Combat | ETagStatus.Aware;
                GroupSay(trigger, mask, false, 100);
            }
        }

        public void Update()
        {
            GroupTalk.Update();

            if (SAINPlugin.LoadedPreset.GlobalSettings.Talk.DisableBotTalkPatching) {
                return;
            }

            if (IsSpeaking) {
                return;
            }

            if (CanTalk
                && _timeCanTalk < Time.time) {
                EnemyTalk.Update();
                if (_allTalkDelay < Time.time) {
                    checkTalk();
                }
            }
        }

        private void checkTalk()
        {
            BotTalkPackage? TalkPack = null;

            if (_talkCacheTimer < Time.time && _botTalkPackage != null) {
                TalkPack = _botTalkPackage;
                _botTalkPackage = null;
                _talkCacheActive = false;
            }
            else if (_talkDelayTimer < Time.time && _talkDelayPackage != null) {
                TalkPack = _talkDelayPackage;
                _talkDelayPackage = null;
            }

            if (TalkPack != null) {
                _allTalkDelay = Time.time + Bot.Info.FileSettings.Mind.TalkFrequency;

                if (TalkPack.Value.phraseInfo.Phrase == EPhraseTrigger.Roger || TalkPack.Value.phraseInfo.Phrase == EPhraseTrigger.Negative) {
                    if (Bot.Squad.VisibleMembers != null && Bot.Squad.LeaderComponent != null && Bot.Squad.VisibleMembers.Contains(Bot.Squad.LeaderComponent) && Bot.Enemy?.IsVisible == false) {
                        if (TalkPack.Value.phraseInfo.Phrase == EPhraseTrigger.Roger) {
                            Player.HandsController.ShowGesture(EInteraction.OkGesture);
                        }
                        else {
                            Player.HandsController.ShowGesture(EInteraction.NoGesture);
                        }
                        return;
                    }
                }
                tellSpeakerToSay(TalkPack.Value.phraseInfo.Phrase, TalkPack.Value.Mask);
            }
        }

        public void Dispose()
        {
            if (Player != null) {
                //Player.BeingHitAction -= GetHit;
            }
            _phraseDictionary.Clear();
            GroupTalk.Dispose();
            EnemyTalk.Dispose();
        }

        public bool CanSay(EPhraseTrigger trigger, bool withGroupDelay, bool skipCheck)
        {
            var speaker = Player?.Speaker;
            if (speaker == null) {
                return false;
            }

            if (trigger == EPhraseTrigger.OnDeath ||
                trigger == EPhraseTrigger.OnAgony) {
                return true;
            }

            if (speaker.Speaking) {
                return false;
            }
            if (!CanTalk) {
                return false;
            }

            if (skipCheck) {
                return true;
            }

            if (!checkDictionaryDelay(trigger)) {
                return false;
            }
            if (withGroupDelay &&
                !BotOwner.BotsGroup.GroupTalk.CanSay(BotOwner, trigger)) {
                return false;
            }
            return true;
        }

        public bool Say(EPhraseTrigger phrase, ETagStatus? additionalMask = null, bool withGroupDelay = false, bool skipCheck = false)
        {
            if (SAINPlugin.LoadedPreset.GlobalSettings.Talk.DisableBotTalkPatching) {
                return false;
            }
            if (!CanSay(phrase, withGroupDelay, skipCheck)) {
                return false;
            }

            var mask = SetETagMask(additionalMask);

            if (skipCheck) {
                tellSpeakerToSay(phrase, mask);
                return true;
            }

            if (!_phraseDictionary.ContainsKey(phrase)) {
                tellSpeakerToSay(phrase, mask);
                return true;
            }

            var phraseInfo = _phraseDictionary[phrase];
            var data = new BotTalkPackage(phraseInfo, mask);
            _botTalkPackage = CheckPriority(data, _botTalkPackage);
            if (!_talkCacheActive) {
                _talkCacheActive = true;
                _talkCacheTimer = Time.time + 0.25f;
            }
            return true;
        }

        public bool GroupSay(EPhraseTrigger phrase, ETagStatus? additionalMask = null, bool withGroupDelay = false, float chance = 60)
        {
            var squadSettings = Bot.Squad.SquadInfo?.SquadPersonalitySettings;
            if (squadSettings != null) {
                float vocalization = squadSettings.VocalizationLevel * 10f - 25f;
                chance += vocalization;
            }
            return EFTMath.RandomBool(chance)
                && GroupTalk.FriendIsClose
                && Say(phrase, additionalMask, withGroupDelay);
        }

        public void TalkAfterDelay(EPhraseTrigger phrase, ETagStatus? mask = null, float delay = 0.5f)
        {
            if (CanTalk && _timeCanTalk < Time.time) {
                if (!_phraseDictionary.ContainsKey(phrase)) {
                    Logger.LogWarning($"Phrase: [{phrase}] Not in Dictionary, adding it manually.");
                    _phraseDictionary.Add(phrase, new PhraseInfo(phrase, 10, 5f));
                }
                var talk = new BotTalkPackage(_phraseDictionary[phrase], SetETagMask(mask));
                _talkDelayPackage = CheckPriority(talk, _talkDelayPackage, out bool changeTalk);
                if (changeTalk) {
                    _talkDelayTimer = Time.time + delay;
                }
            }
        }

        private void tellSpeakerToSay(EPhraseTrigger phrase, ETagStatus mask)
        {
            tellSpeakerToSay(phrase, mask, mask == ETagStatus.Combat);
        }

        private void tellSpeakerToSay(EPhraseTrigger trigger, ETagStatus mask = (ETagStatus)0, bool aggressive = false)
        {
            if (trigger == EPhraseTrigger.MumblePhrase) {
                trigger = ((aggressive || Time.time < Player.Awareness) ? EPhraseTrigger.OnFight : EPhraseTrigger.OnMutter);
            }
            ETagStatus etagStatus = (aggressive || Player.Awareness > Time.time) ? ETagStatus.Combat : ETagStatus.Unaware;
            if (PlayerComponent.PlayVoiceLine(trigger, Bot.Memory.Health.HealthStatus | mask | etagStatus, aggressive)) {
                SAINBotController.Instance?.BotHearing.PlayerTalked(trigger, etagStatus, Player);
                BotOwner.BotsGroup.GroupTalk.PhraseSad(BotOwner, trigger);

                if (_phraseDictionary.TryGetValue(trigger, out var phrase)) {
                    phrase.TimeLastSaid = Time.time;
                }
            }
            //Player.Speaker.Play(trigger, Bot.Memory.Health.HealthStatus | mask | etagStatus, true, null);
        }

        private ETagStatus SetETagMask(ETagStatus? additionaMask = null)
        {
            ETagStatus etagStatus;
            if (BotOwner.BotsGroup.MembersCount > 1) {
                etagStatus = ETagStatus.Coop;
            }
            else {
                etagStatus = ETagStatus.Solo;
            }

            if (BotOwner.Memory.IsUnderFire ||
                Bot.Suppression.IsSuppressed ||
                Bot.Suppression.IsHeavySuppressed) {
                etagStatus |= ETagStatus.Combat;
            }
            else if (Bot.Enemy != null) {
                if (Bot.Enemy.Seen && Bot.Enemy.TimeSinceSeen < 30f) {
                    etagStatus |= ETagStatus.Combat;
                }
                else {
                    etagStatus |= ETagStatus.Aware;
                }

                switch (Bot.Enemy.EnemyIPlayer.Side) {
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
            else if (!Bot.EnemyController.AtPeace) {
                etagStatus |= ETagStatus.Aware;
            }
            else {
                etagStatus |= ETagStatus.Unaware;
            }

            if (additionaMask != null) {
                etagStatus |= additionaMask.Value;
            }

            return etagStatus;
        }

        private bool checkDictionaryDelay(EPhraseTrigger trigger)
        {
            if (!_phraseDictionary.ContainsKey(trigger)) {
                _phraseDictionary.Add(trigger, new PhraseInfo(trigger, 10, 5f));
            }

            if (_phraseDictionary.ContainsKey(trigger)) {
                var phraseInfo = _phraseDictionary[trigger];
                if (phraseInfo.TimeLastSaid + phraseInfo.TimeDelay < Time.time) {
                    return true;
                }
            }
            return false;
        }

        private BotTalkPackage? CheckPriority(BotTalkPackage? newTalk, BotTalkPackage? oldTalk)
        {
            if (oldTalk == null) {
                return newTalk;
            }
            if (newTalk == null) {
                return oldTalk;
            }

            int newPriority = newTalk.Value.phraseInfo.Priority;
            int oldPriority = oldTalk.Value.phraseInfo.Priority;

            bool ChangeTalk = oldPriority < newPriority;

            return ChangeTalk ? newTalk : oldTalk;
        }

        private BotTalkPackage? CheckPriority(BotTalkPackage? newTalk, BotTalkPackage? oldTalk, out bool ChangeTalk)
        {
            if (oldTalk == null) {
                ChangeTalk = true;
                return newTalk;
            }
            if (newTalk == null) {
                ChangeTalk = false;
                return oldTalk;
            }

            int newPriority = newTalk.Value.phraseInfo.Priority;
            int oldPriority = oldTalk.Value.phraseInfo.Priority;

            ChangeTalk = oldPriority < newPriority;

            return ChangeTalk ? newTalk : oldTalk;
        }

        private static void PhraseObjectsAdd(Dictionary<EPhraseTrigger, PhraseInfo> dictionary)
        {
            AddPhrase(EPhraseTrigger.OnGoodWork, 1, 60f, dictionary);
            AddPhrase(EPhraseTrigger.OnBreath, 3, 15f, dictionary);
            AddPhrase(EPhraseTrigger.EnemyHit, 4, 3f, dictionary);
            AddPhrase(EPhraseTrigger.Rat, 5, 120f, dictionary);
            AddPhrase(EPhraseTrigger.OnMutter, 6, 20f, dictionary);
            AddPhrase(EPhraseTrigger.OnEnemyDown, 7, 10f, dictionary);
            AddPhrase(EPhraseTrigger.OnEnemyConversation, 8, 30f, dictionary);
            AddPhrase(EPhraseTrigger.GoForward, 9, 40f, dictionary);
            AddPhrase(EPhraseTrigger.Gogogo, 10, 40f, dictionary);
            AddPhrase(EPhraseTrigger.Going, 11, 60f, dictionary);
            AddPhrase(EPhraseTrigger.OnFight, 38, 1f, dictionary);
            AddPhrase(EPhraseTrigger.BadWork, 37, 1f, dictionary);
            AddPhrase(EPhraseTrigger.OnEnemyShot, 13, 3f, dictionary);
            AddPhrase(EPhraseTrigger.OnLostVisual, 14, 10f, dictionary);
            AddPhrase(EPhraseTrigger.OnRepeatedContact, 15, 5f, dictionary);
            AddPhrase(EPhraseTrigger.OnFirstContact, 16, 5f, dictionary);
            AddPhrase(EPhraseTrigger.OnBeingHurtDissapoinment, 17, 35f, dictionary);
            AddPhrase(EPhraseTrigger.StartHeal, 18, 75f, dictionary);
            AddPhrase(EPhraseTrigger.HurtLight, 19, 60f, dictionary);
            AddPhrase(EPhraseTrigger.OnWeaponReload, 20, 10f, dictionary);
            AddPhrase(EPhraseTrigger.OnOutOfAmmo, 21, 15f, dictionary);
            AddPhrase(EPhraseTrigger.HurtMedium, 22, 60f, dictionary);
            AddPhrase(EPhraseTrigger.HurtHeavy, 23, 30f, dictionary);
            AddPhrase(EPhraseTrigger.LegBroken, 24, 30f, dictionary);
            AddPhrase(EPhraseTrigger.HandBroken, 25, 30f, dictionary);
            AddPhrase(EPhraseTrigger.HurtNearDeath, 26, 20f, dictionary);
            AddPhrase(EPhraseTrigger.OnFriendlyDown, 27, 10f, dictionary);
            AddPhrase(EPhraseTrigger.FriendlyFire, 28, 2f, dictionary);
            AddPhrase(EPhraseTrigger.NeedHelp, 29, 30f, dictionary);
            AddPhrase(EPhraseTrigger.GetInCover, 30, 40f, dictionary);
            AddPhrase(EPhraseTrigger.LeftFlank, 31, 5f, dictionary);
            AddPhrase(EPhraseTrigger.RightFlank, 32, 5f, dictionary);
            AddPhrase(EPhraseTrigger.NeedWeapon, 33, 15f, dictionary);
            AddPhrase(EPhraseTrigger.WeaponBroken, 34, 15f, dictionary);
            AddPhrase(EPhraseTrigger.OnGrenade, 35, 10f, dictionary);
            AddPhrase(EPhraseTrigger.OnEnemyGrenade, 36, 10f, dictionary);
            AddPhrase(EPhraseTrigger.Stop, 37, 1f, dictionary);
            AddPhrase(EPhraseTrigger.OnBeingHurt, 38, 1f, dictionary);
            AddPhrase(EPhraseTrigger.OnAgony, 39, 1f, dictionary);
            AddPhrase(EPhraseTrigger.OnDeath, 40, 1f, dictionary);
            AddPhrase(EPhraseTrigger.Regroup, 10, 80f, dictionary);
            AddPhrase(EPhraseTrigger.OnSix, 15, 10f, dictionary);
            AddPhrase(EPhraseTrigger.InTheFront, 15, 20f, dictionary);
            AddPhrase(EPhraseTrigger.FollowMe, 15, 45f, dictionary);
            AddPhrase(EPhraseTrigger.HoldPosition, 6, 60f, dictionary);
            AddPhrase(EPhraseTrigger.Suppress, 20, 15f, dictionary);
            AddPhrase(EPhraseTrigger.Roger, 10, 30f, dictionary);
            AddPhrase(EPhraseTrigger.Negative, 10, 30f, dictionary);
            AddPhrase(EPhraseTrigger.PhraseNone, 1, 1f, dictionary);
            AddPhrase(EPhraseTrigger.Look, 25, 30f, dictionary);
            AddPhrase(EPhraseTrigger.OnYourOwn, 25, 15f, dictionary);
            AddPhrase(EPhraseTrigger.Repeat, 25, 30f, dictionary);
            AddPhrase(EPhraseTrigger.CoverMe, 25, 45f, dictionary);
            AddPhrase(EPhraseTrigger.NoisePhrase, 5, 120f, dictionary);
            AddPhrase(EPhraseTrigger.UnderFire, 34, 5f, dictionary);
            AddPhrase(EPhraseTrigger.MumblePhrase, 10, 35f, dictionary);
            AddPhrase(EPhraseTrigger.GetBack, 10, 45f, dictionary);
            AddPhrase(EPhraseTrigger.LootBody, 5, 30f, dictionary);
            AddPhrase(EPhraseTrigger.LootContainer, 5, 30f, dictionary);
            AddPhrase(EPhraseTrigger.LootGeneric, 5, 30f, dictionary);
            AddPhrase(EPhraseTrigger.LootKey, 5, 30f, dictionary);
            AddPhrase(EPhraseTrigger.LootMoney, 5, 30f, dictionary);
            AddPhrase(EPhraseTrigger.LootNothing, 5, 30f, dictionary);
            AddPhrase(EPhraseTrigger.LootWeapon, 5, 30f, dictionary);
            AddPhrase(EPhraseTrigger.OnLoot, 5, 30f, dictionary);

            foreach (EPhraseTrigger value in System.Enum.GetValues(typeof(EPhraseTrigger))) {
                AddPhrase(value, 25, 5f, dictionary);
            }
        }

        private static void AddPhrase(EPhraseTrigger phrase, int priority, float timeDelay, Dictionary<EPhraseTrigger, PhraseInfo> dictionary)
        {
            if (!dictionary.ContainsKey(phrase)) {
                dictionary.Add(phrase, new PhraseInfo(phrase, priority, timeDelay));
            }
        }

        private BotTalkPackage? _botTalkPackage;
        private BotTalkPackage? _talkDelayPackage;
        private bool _talkCacheActive = false;
        private float _talkCacheTimer = 0f;
        private float _allTalkDelay = 0f;
        private float _nextGetHitTime;
        private float _timeCanTalk;
        private float _nextCanTalkTime;
        private float _talkDelayTimer = 0f;

        private readonly Dictionary<EPhraseTrigger, PhraseInfo> _phraseDictionary = new Dictionary<EPhraseTrigger, PhraseInfo>();
    }

    public struct BotTalkPackage
    {
        public BotTalkPackage(PhraseInfo phrase, ETagStatus mask)
        {
            phraseInfo = phrase;
            Mask = mask;
        }

        public PhraseInfo phraseInfo;
        public ETagStatus Mask;
    }

    public struct PhraseInfo
    {
        public PhraseInfo(EPhraseTrigger trigger, int priority, float timeDelay)
        {
            Phrase = trigger;
            Priority = priority;
            TimeDelay = timeDelay;
            TimeLastSaid = 0f;
        }

        public EPhraseTrigger Phrase { get; }
        public int Priority { get; }
        public float TimeDelay { get; }

        public float TimeLastSaid;
    }
}