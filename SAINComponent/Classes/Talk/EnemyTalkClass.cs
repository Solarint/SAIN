using EFT;
using SAIN.Preset.BotSettings.SAINSettings;
using UnityEngine;
using static SAIN.Preset.Personalities.PersonalitySettingsClass;

namespace SAIN.SAINComponent.Classes.Talk
{
    public class EnemyTalk : SAINBase, ISAINClass
    {
        public EnemyTalk(SAINComponentClass bot) : base(bot)
        {
        }

        public void Init()
        {
            var settings = PersonalitySettings;
            TauntDist = settings.TauntMaxDistance;
            TauntFreq = settings.TauntFrequency;
            CanTaunt = settings.CanTaunt && FileSettings.Mind.BotTaunts;
            CanRespond = settings.CanRespondToVoice;

            TauntDist *= Random.Range(0.66f, 1.33f);
            TauntFreq *= Random.Range(0.66f, 1.33f);
            TauntTimer = Time.time + TauntFreq;
            ResponseDist = TauntDist * Random.Range(0.66f, 1.33f);
        }

        public void Update()
        {
            if (PersonalitySettings == null || FileSettings == null)
            {
                return;
            }
            if (TauntFreq == 0f)
            {
                Init();
            }

            if (SAIN?.Enemy != null)
            {
                if (BegForLife())
                {
                    return;
                }

                if (FakeDeath())
                {
                    return;
                }

                if (CanRespond && LastEnemyCheckTime < Time.time)
                {
                    LastEnemyCheckTime = Time.time + EnemyCheckFreq;
                    StartResponse();
                }
                if (CanTaunt && TauntTimer < Time.time)
                {
                    TauntTimer = Time.time + TauntFreq * Random.Range(0.5f, 1.5f);
                    TauntEnemy();
                }
            }
        }

        public void Dispose()
        {
        }

        private float ResponseDist;
        private bool CanTaunt;
        private bool CanRespond;

        private const float EnemyCheckFreq = 0.25f;
        private float TauntDist = 0f;
        private float TauntFreq = 0f;

        private PersonalityVariablesClass PersonalitySettings => SAIN?.Info?.PersonalitySettings;
        private SAINSettingsClass FileSettings => SAIN?.Info?.FileSettings;

        private bool FakeDeath()
        {
            if (SAIN.Enemy != null && !SAIN.Squad.BotInGroup)
            {
                if (SAIN.Info.PersonalitySettings.CanFakeDeathRare)
                {
                    if (FakeTimer < Time.time)
                    {
                        FakeTimer = Time.time + 10f;
                        var health = SAIN.Memory.HealthStatus;
                        if (health != ETagStatus.Healthy && health != ETagStatus.Injured)
                        {
                            float dist = (SAIN.Enemy.EnemyPosition - BotOwner.Position).magnitude;
                            if (dist < 30f)
                            {
                                bool random = Helpers.EFTMath.RandomBool(1f);
                                if (random)
                                {
                                    SAIN.Talk.Say(EPhraseTrigger.OnDeath);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private float FakeTimer = 0f;

        private bool BegForLife()
        {
            if (BegTimer < Time.time && SAIN.HasEnemy && !SAIN.Squad.BotInGroup)
            {
                bool random = Helpers.EFTMath.RandomBool(25);
                float timeAdd = random ? 8f : 2f;
                BegTimer = Time.time + timeAdd;

                var personality = SAIN.Info.Personality;
                if (personality == SAINPersonality.Timmy || personality == SAINPersonality.Coward)
                {
                    var health = SAIN.Memory.HealthStatus;
                    if (health != ETagStatus.Healthy)
                    {
                        float dist = (SAIN.Enemy.EnemyPosition - BotOwner.Position).magnitude;
                        if (dist < 30f)
                        {
                            if (random)
                            {
                                SAIN.Talk.Say(BegPhrases.PickRandom());
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private float BegTimer = 0f;
        private readonly EPhraseTrigger[] BegPhrases = { EPhraseTrigger.Stop, EPhraseTrigger.OnBeingHurtDissapoinment, EPhraseTrigger.NeedHelp, EPhraseTrigger.HoldFire };

        private bool TauntEnemy()
        {
            bool tauntEnemy = false;

            var sainEnemy = SAIN.Enemy;
            var type = SAIN.Info.Personality;

            float distanceToEnemy = Vector3.Distance(sainEnemy.EnemyPosition, BotOwner.Position);

            if (distanceToEnemy <= TauntDist)
            {
                if (sainEnemy.CanShoot && sainEnemy.IsVisible)
                {
                    tauntEnemy = sainEnemy.EnemyLookingAtMe || SAIN.Info.PersonalitySettings.FrequentTaunt;
                }
                if (SAIN.Info.PersonalitySettings.ConstantTaunt)
                {
                    tauntEnemy = true;
                }
            }

            if (!tauntEnemy && BotOwner.AimingData != null)
            {
                var aim = BotOwner.AimingData;
                if (aim != null && aim.IsReady)
                {
                    if (aim.LastDist2Target < TauntDist)
                    {
                        tauntEnemy = true;
                    }
                }
            }

            if (tauntEnemy)
            {
                SAIN.Talk.Say(EPhraseTrigger.OnFight, ETagStatus.Combat, true);
            }

            return tauntEnemy;
        }

        private void StartResponse()
        {
            if (LastEnemyTalk != null)
            {
                float delay = LastEnemyTalk.TalkDelay;
                if (LastEnemyTalk.TalkTime + delay < Time.time)
                {
                    SAIN.Talk.Say(EPhraseTrigger.OnFight, ETagStatus.Combat, true);
                    LastEnemyTalk = null;
                }
            }
        }

        public void SetEnemyTalk(Player player)
        {
            if (LastEnemyTalk == null)
            {
                if (Vector3.Distance(player.Position, BotOwner.Position) < ResponseDist)
                {
                    LastEnemyTalk = new EnemyTalkObject();
                }
            }
        }

        private float LastEnemyCheckTime = 0f;
        private EnemyTalkObject LastEnemyTalk;
        private float TauntTimer = 0f;
    }
}