using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Talk.Components
{
    public class BotTalk : SAINTalk
    {
        public static List<EPhraseTrigger> GoodPhrasesForPatch = new List<EPhraseTrigger>();

        private const float BotTalkDelaySetting = 3f;

        public BotTalk(BotOwner bot) : base(bot)
        {
            AddPhrases();
            Activate();
        }

        public void Say(EPhraseTrigger type, bool sayImmediately = false, ETagStatus? additionalMask = null)
        {
            if (!CanTalk)
            {
                //return;
            }

            ETagStatus mask = SetETagMask(additionalMask);

            if (!sayImmediately)
            {
                CheckDelay(type, true, 0f, mask);
                return;
            }

            Logger.LogDebug($"Bot Said: Phrase: [{type}] Mask: [{additionalMask}]");

            talkDelay = Time.time + BotTalkDelaySetting * UnityEngine.Random.Range(0.75f, 1.25f);

            BotOwner.GetPlayer.Say(type, true, 0f, mask, 100, false);
        }

        public void TrySay(EPhraseTrigger type, ETagStatus? additionalMask, bool withGroupDelay)
        {
            if (withGroupDelay && !BotOwner.BotsGroup.GroupTalk.CanSay(BotOwner, type))
            {
                return;
            }

            ETagStatus mask = SetETagMask(additionalMask);

            CheckDelay(type, true, 0f, mask);
        }

        public void ManualUpdate()
        {
            if (!CanTalk)
            {
                return;
            }

            if (!TalkLimited)
            {
                return;
            }

            if (TalkLimiter < Time.time && talkPackage != null)
            {
                TalkLimited = false;

                if (talkDelay < Time.time)
                {
                    BotOwner.BotsGroup.GroupTalk.PhraseSad(BotOwner, talkPackage.type);

                    BotOwner.GetPlayer.Say(talkPackage.type, talkPackage.demand, talkPackage.delay, talkPackage.mask, 100, false);

                    Logger.LogDebug($"Bot Said After Query: Phrase: [{talkPackage.type}] Mask: [{talkPackage.mask}]");

                    talkDelay = Time.time + BotTalkDelaySetting * UnityEngine.Random.Range(0.75f, 1.25f);
                }

                talkPackage = null;
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

        private void CheckDelay(EPhraseTrigger type, bool demand, float delay, ETagStatus mask)
        {
            GClass467 data = new GClass467(type, demand, delay, mask);

            talkPackage = CheckPriorityPhrase(data, talkPackage);

            if (!TalkLimited)
            {
                TalkLimited = true;
                TalkLimiter = Time.time + 0.15f;
            }
        }

        private GClass467 CheckPriorityPhrase(GClass467 newTalk, GClass467 oldTalk)
        {
            if (oldTalk == null)
            {
                return newTalk;
            }
            if (newTalk == null)
            {
                return oldTalk;
            }

            float New = PhraseDictionary.ContainsKey(newTalk.type) ? PhraseDictionary[newTalk.type] : 101f;
            float Old = PhraseDictionary.ContainsKey(oldTalk.type) ? PhraseDictionary[oldTalk.type] : 100f;

            bool ChangeTalk = Old < New;

            //Logger.LogDebug($"Talk Priority: Old: [{Old}] for [{oldTalk.type}] New: [{New}] for [{newTalk.type}]");

            return ChangeTalk ? newTalk : oldTalk;
        }

        public void TrySay(EPhraseTrigger type)
        {
            TrySay(type, null, false);
        }

        public void TrySay(EPhraseTrigger type, bool withGroupDelay)
        {
            TrySay(type, null, withGroupDelay);
        }

        public void Activate()
        {
            CanTalk = BotOwner.Settings.FileSettings.Mind.CAN_TALK;
        }

        private void AddPhrases()
        {
            PhraseDictionary.Add(EPhraseTrigger.OnGoodWork, 5f);
            PhraseDictionary.Add(EPhraseTrigger.LootBody, 10f);
            PhraseDictionary.Add(EPhraseTrigger.OnBreath, 15f);
            PhraseDictionary.Add(EPhraseTrigger.EnemyHit, 20f);
            PhraseDictionary.Add(EPhraseTrigger.Rat, 25f);
            PhraseDictionary.Add(EPhraseTrigger.OnMutter, 30f);
            PhraseDictionary.Add(EPhraseTrigger.OnEnemyDown, 35f);
            PhraseDictionary.Add(EPhraseTrigger.OnEnemyConversation, 40f);
            PhraseDictionary.Add(EPhraseTrigger.OnFight, 45f);
            PhraseDictionary.Add(EPhraseTrigger.OnEnemyShot, 50f);
            PhraseDictionary.Add(EPhraseTrigger.OnLostVisual, 55f);
            PhraseDictionary.Add(EPhraseTrigger.OnRepeatedContact, 60f);
            PhraseDictionary.Add(EPhraseTrigger.OnFirstContact, 65f);
            PhraseDictionary.Add(EPhraseTrigger.OnBeingHurtDissapoinment, 70f);
            PhraseDictionary.Add(EPhraseTrigger.StartHeal, 75f);
            PhraseDictionary.Add(EPhraseTrigger.HurtLight, 80f);
            PhraseDictionary.Add(EPhraseTrigger.OnBeingHurt, 82f);
            PhraseDictionary.Add(EPhraseTrigger.OnWeaponReload, 83f);
            PhraseDictionary.Add(EPhraseTrigger.OnOutOfAmmo, 84f);
            PhraseDictionary.Add(EPhraseTrigger.HurtMedium, 85f);
            PhraseDictionary.Add(EPhraseTrigger.HurtHeavy, 90f);
            PhraseDictionary.Add(EPhraseTrigger.LegBroken, 95f);
            PhraseDictionary.Add(EPhraseTrigger.HandBroken, 100f);
            PhraseDictionary.Add(EPhraseTrigger.HurtNearDeath, 105f);
            PhraseDictionary.Add(EPhraseTrigger.OnFriendlyDown, 125f);
            PhraseDictionary.Add(EPhraseTrigger.FriendlyFire, 130f);
            PhraseDictionary.Add(EPhraseTrigger.NeedHelp, 135f);
            PhraseDictionary.Add(EPhraseTrigger.GetInCover, 140f);
            PhraseDictionary.Add(EPhraseTrigger.LeftFlank, 145f);
            PhraseDictionary.Add(EPhraseTrigger.RightFlank, 150f);
            PhraseDictionary.Add(EPhraseTrigger.NeedWeapon, 155f);
            PhraseDictionary.Add(EPhraseTrigger.WeaponBroken, 160f);
            PhraseDictionary.Add(EPhraseTrigger.OnGrenade, 165f);
            PhraseDictionary.Add(EPhraseTrigger.OnEnemyGrenade, 170f);
            PhraseDictionary.Add(EPhraseTrigger.Stop, 175f);
            PhraseDictionary.Add(EPhraseTrigger.OnAgony, 180f);
            PhraseDictionary.Add(EPhraseTrigger.OnDeath, 185f);
        }

        private GClass467 talkPackage;
        private bool TalkLimited = false;
        private float TalkLimiter = 0f;
        private float talkDelay = 0f;
        private bool CanTalk = false;
        private readonly Dictionary<EPhraseTrigger, float> PhraseDictionary = new Dictionary<EPhraseTrigger, float>();
    }
}