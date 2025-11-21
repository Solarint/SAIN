using Comfort.Common;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Models.Structs;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings;
using SAIN.Preset.Personalities;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Talk;

public class EnemyTalk : BotBase
{
    public EnemyTalk(BotComponent bot) : base(bot)
    {
        _randomizationFactor = Random.Range(0.75f, 1.25f);
    }

    public override void Init()
    {
        if (Singleton<BotEventHandler>.Instance != null)
        {
            Singleton<BotEventHandler>.Instance.OnGrenadeExplosive += tryFakeDeathGrenade;
        }
        BotManagerComponent.Instance.BotHearing.PlayerTalk += playerTalked;
        Bot.EnemyController.Events.OnEnemyKilled += enemyKilled;
        base.Init();
    }

    private void enemyKilled(Player player)
    {
        if (Bot.Talk.CanTalk &&
            CanTaunt &&
            EFTMath.RandomBool(70))
        {
            EPhraseTrigger trigger;
            if (EFTMath.RandomBool(15) ||
                (Bot.Memory.Health.HealthStatus == ETagStatus.Healthy && EFTMath.RandomBool(50)))
            {
                trigger = EPhraseTrigger.GoodWork;
            }
            else if (EFTMath.RandomBool(10))
            {
                trigger = EPhraseTrigger.BadWork;
            }
            else
            {
                trigger = EPhraseTrigger.OnFight;
            }
            Bot.Talk.Say(trigger, ETagStatus.Combat, false);
        }
    }

    public override void ManualUpdate()
    {
        base.ManualUpdate();
        float time = Time.time;
        if (_nextCheckTime < time)
        {
            if (ShallBegForLife())
            {
                _nextCheckTime = time + 1f;
                return;
            }
            if (Bot?.GoalEnemy != null)
            {
                if (ShallFakeDeath())
                {
                    _nextCheckTime = time + 15f;
                    return;
                }
                if (CanTaunt &&
                    _tauntTimer < time)
                {
                    float freq = TauntFreq * Random.Range(0.5f, 1.5f);
                    _tauntTimer = time;
                    if (EFTMath.RandomBool(PersonalitySettings.TauntChance) &&
                        TauntEnemy())
                    {
                        _nextCheckTime = time + 1f;
                        _tauntTimer += freq;
                    }
                    _nextCheckTime = time + 0.1f;
                    _tauntTimer += freq / 3f;
                    return;
                }
            }
            _nextCheckTime = time + 0.1f;
        }
    }

    private float _nextCheckTime;
    private float _randomizationFactor = 1f;

    public override void Dispose()
    {
        if (Singleton<BotEventHandler>.Instance != null)
        {
            Singleton<BotEventHandler>.Instance.OnGrenadeExplosive -= tryFakeDeathGrenade;
        }
        BotManagerComponent.Instance.BotHearing.PlayerTalk -= playerTalked;
        if (Bot?.EnemyController != null)
        {
            Bot.EnemyController.Events.OnEnemyKilled -= enemyKilled;
        }
        base.Dispose();
    }

    private PersonalityTalkSettings PersonalitySettings => Bot?.Info?.PersonalitySettings.Talk;
    private SAINSettingsClass FileSettings => Bot?.Info?.FileSettings;

    private float FakeDeathChance = 2f;

    protected override void UpdatePresetSettings(SAINPresetClass preset)
    {
        if (PersonalitySettings != null && FileSettings != null)
        {
            CanFakeDeath = PersonalitySettings.CanFakeDeathRare;
            FakeDeathChance = PersonalitySettings.FakeDeathChance;
            CanBegForLife = PersonalitySettings.CanBegForLife;
            CanTaunt = PersonalitySettings.CanTaunt && FileSettings.Mind.BotTaunts;
            TauntDist = PersonalitySettings.TauntMaxDistance * _randomizationFactor;
            TauntFreq = PersonalitySettings.TauntFrequency * _randomizationFactor;
            _canRespondToEnemy = PersonalitySettings.CanRespondToEnemyVoice;
        }
        else
        {
            Logger.LogAndNotifyError("Personality settings or filesettings are null! Cannot Apply Settings!");
        }

        var talkSettings = preset.GlobalSettings.Talk;
        _friendlyResponseChance = talkSettings.FriendlyReponseChance;
        _friendlyResponseChanceAI = talkSettings.FriendlyReponseChanceAI;
        _friendlyResponseDistance = talkSettings.FriendlyReponseDistance;
        _friendlyResponseDistanceAI = talkSettings.FriendlyReponseDistanceAI;
        _friendlyResponseFrequencyLimit = talkSettings.FriendlyResponseFrequencyLimit;
        _friendlyResponseMinRandom = talkSettings.FriendlyResponseMinRandomDelay;
        _friendlyResponseMaxRandom = talkSettings.FriendlyResponseMaxRandomDelay;
    }

    private bool CanTaunt = true;
    private bool CanFakeDeath = false;
    private bool CanBegForLife = false;
    private bool _canRespondToEnemy = true;
    private float TauntDist = 40f;
    private float TauntFreq = 30f;

    private bool ShallFakeDeath()
    {
        if (CanFakeDeath
            && EFTMath.RandomBool(FakeDeathChance)
            && Bot.GoalEnemy != null
            && !Bot.Squad.BotInGroup
            && _fakeDeathTimer < Time.time
            && (Bot.Memory.Health.HealthStatus == ETagStatus.Dying || Bot.Memory.Health.HealthStatus == ETagStatus.BadlyInjured)
            && (Bot.GoalEnemy.EnemyPosition - BotOwner.Position).sqrMagnitude < 70f * 70f)
        {
            _fakeDeathTimer = Time.time + 30f;
            Bot.Talk.Say(EPhraseTrigger.OnDeath);
            return true;
        }
        return false;
    }

    private void tryFakeDeathGrenade(Vector3 grenadeExplosionPosition, string playerProfileID, bool isSmoke, float smokeRadius, float smokeLifeTime, int throwableId)
    {
        if (CanFakeDeath
            && EFTMath.RandomBool(FakeDeathChance)
            && !isSmoke
            && Bot.GoalEnemy != null
            && _fakeDeathTimer < Time.time
            && playerProfileID != Bot.ProfileId
            && (grenadeExplosionPosition - Bot.Position).sqrMagnitude < 25f * 25f)
        {
            _fakeDeathTimer = Time.time + 30f;
            Bot.Talk.Say(EPhraseTrigger.OnDeath);
        }
    }

    private bool ShallBegForLife()
    {
        if (_begTimer > Time.time)
        {
            return false;
        }
        if (!EFTMath.RandomBool(25))
        {
            _begTimer = Time.time + 10f;
            return false;
        }
        Vector3? currentTarget = Bot.GoalEnemy?.LastKnownPosition;
        if (currentTarget == null)
        {
            _begTimer = Time.time + 10f;
            return false;
        }

        _begTimer = Time.time + 3f;

        bool shallBeg = Bot.Info.Profile.IsPMC ? !Bot.Squad.BotInGroup : Bot.Info.Profile.IsScav;
        if (shallBeg
            && CanBegForLife
            && Bot.Memory.Health.HealthStatus != ETagStatus.Healthy
            && (currentTarget.Value - Bot.Position).sqrMagnitude < 50f * 50f)
        {
            IsBeggingForLife = true;
            Bot.Talk.Say(BegPhrases.PickRandom());
            return true;
        }
        return false;
    }

    public bool IsBeggingForLife
    {
        get
        {
            if (_isBegging && _beggingTimer < Time.time)
            {
                _isBegging = false;
            }
            return _isBegging;
        }
        private set
        {
            if (value)
            {
                _beggingTimer = Time.time + 60f;
            }
            _isBegging = value;
        }
    }

    private bool TauntEnemy()
    {
        bool tauntEnemy = false;
        var enemy = Bot.GoalEnemy;

        if (!canTauntEnemy(enemy))
        {
            return false;
        }

        if (Bot.Info.PersonalitySettings.Talk.ConstantTaunt)
        {
            tauntEnemy = EFTMath.RandomBool(50);
        }
        if (!tauntEnemy &&
            (enemy.IsVisible || enemy.TimeSinceSeen < 15f))
        {
            tauntEnemy = enemy.EnemyLookingAtMe || Bot.Info.PersonalitySettings.Talk.FrequentTaunt;
        }

        if (!tauntEnemy &&
            enemy.TimeSinceLastKnownUpdated < 5f &&
            EFTMath.RandomBool(5))
        {
            tauntEnemy = true;
        }

        if (tauntEnemy)
        {
            if (!enemy.IsVisible
                && enemy.Seen)
            {
                if (enemy.TimeSinceSeen > 60f
                    && EFTMath.RandomBool(10) &&
                    Bot.Talk.Say(EPhraseTrigger.Rat, ETagStatus.Combat, true))
                {
                    return true;
                }
                if (enemy.TimeSinceSeen > 30f
                    && EFTMath.RandomBool(20) &&
                    Bot.Talk.Say(EPhraseTrigger.OnLostVisual, ETagStatus.Combat, true))
                {
                    return true;
                }
            }
            EPhraseTrigger? trigger = tauntTrigger();
            if (trigger == null)
            {
                return false;
            }
            if (Bot.Talk.Say(trigger.Value, ETagStatus.Combat, false))
            {
                return true;
            }
        }
        return false;
    }

    private bool canTauntEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            return false;
        }
        if (!enemy.Seen && !enemy.Heard)
        {
            return false;
        }
        if (enemy.KnownPlaces.EnemyDistanceFromLastKnown > TauntDist)
        {
            return false;
        }
        if (Bot.Decision.IsSearching)
        {
            return true;
        }
        if (!enemy.Status.ShotMeRecently && !enemy.Status.ShotAtMeRecently)
        {
            if (!enemy.Seen)
            {
                return false;
            }
            if (enemy.TimeSinceSeen > 90f)
            {
                return false;
            }
            if (enemy.TimeSinceHeard > 20f)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator RespondToFriendly(EPhraseTrigger trigger, ETagStatus mask, float delay, Player sourcePlayer, float chance = 100f)
    {
        if (!EFTMath.RandomBool(chance))
        {
            yield break;
        }

        yield return new WaitForSeconds(delay);

        if (sourcePlayer == null ||
            BotOwner == null ||
            Player == null ||
            Bot == null)
        {
            yield break;
        }
        if (!sourcePlayer.HealthController.IsAlive
            || !Player.HealthController.IsAlive)
        {
            yield break;
        }

        if (Bot.Talk.IsSpeaking)
        {
            yield break;
        }

        if (!BotOwner.Memory.IsPeace)
        {
            Bot.Talk.Say(trigger, null, false);
            yield break;
        }
        if (_nextGestureTime < Time.time)
        {
            _nextGestureTime = Time.time + 6f;
            Player.HandsController.ShowGesture(EInteraction.FriendlyGesture);
            //Bot.Steering.LookToPoint(sourcePlayer.Position + Vector3.up * 1.4f);
        }
        Bot.Talk.Say(trigger, mask, false);
    }

    private IEnumerator RespondToEnemy(EPhraseTrigger trigger, ETagStatus mask, float delay, Enemy enemy)
    {
        yield return new WaitForSeconds(delay);

        if (enemy == null || !enemy.CheckValid())
        {
            yield break;
        }
        if (BotOwner == null ||
            Player == null ||
            Bot == null)
        {
            yield break;
        }
        if (Bot.Talk.IsSpeaking)
        {
            yield break;
        }
        if (!Player.HealthController.IsAlive)
        {
            yield break;
        }
        Bot.Talk.Say(trigger, mask, false, true);
    }

    private EPhraseTrigger? tauntTrigger()
    {
        if (EFTMath.RandomBool(10))
        {
            if (EFTMath.RandomBool(80f) &&
                Bot.Talk.CanSay(EPhraseTrigger.BadWork, false, false))
            {
                return EPhraseTrigger.BadWork;
            }
            if (EFTMath.RandomBool(20) &&
                Bot.Talk.CanSay(EPhraseTrigger.GoodWork, false, false))
            {
                return EPhraseTrigger.GoodWork;
            }
        }
        if (Bot.Talk.CanSay(EPhraseTrigger.OnFight, false, false))
        {
            return EPhraseTrigger.OnFight;
        }
        return null;
    }

    public void SetEnemyTalk(Enemy enemy)
    {
        if (_canRespondToEnemy &&
            _nextResponseTime < Time.time)
        {
            if (!EFTMath.RandomBool(60))
            {
                return;
            }
            var trigger = tauntTrigger();
            if (trigger == null)
            {
                return;
            }
            _nextResponseTime = Time.time + 2f;
            Bot.StartCoroutine(RespondToEnemy(trigger.Value, ETagStatus.Combat, Random.Range(0.4f, 0.75f), enemy));
        }
    }

    private void playerTalked(EPhraseTrigger phrase, ETagStatus mask, Player player)
    {
        if (Bot == null || !Bot.BotActive || player == null)
        {
            return;
        }
        if (Bot.ProfileId == player.ProfileId)
        {
            return;
        }

        bool isPain = phrase == EPhraseTrigger.OnAgony || phrase == EPhraseTrigger.OnBeingHurt;
        float painRange = 40f;
        float breathRange = player.HeavyBreath ? 35f : 15f;

        Enemy enemy = Bot.EnemyController.GetEnemy(player.ProfileId, true);
        if (enemy == null)
        {
            if (!isPain &&
                phrase != EPhraseTrigger.OnBreath &&
                phrase != EPhraseTrigger.OnFight)
            {
                SetFriendlyTalked(player);
            }
            return;
        }

        if (isPain)
        {
            if (enemy.RealDistance <= painRange)
            {
                Vector3 randomizedPos = randomizePos(player.Position, enemy.RealDistance, 20f);
                SAINHearingReport report = new()
                {
                    position = randomizedPos,
                    soundType = SAINSoundType.Pain,
                    placeType = EEnemyPlaceType.Hearing,
                    isDanger = enemy.RealDistance < 25f || enemy.InLineOfSight,
                    shallReportToSquad = true,
                };
                enemy.Hearing.SetHeard(report, Time.time);
            }
            return;
        }
        if (phrase == EPhraseTrigger.OnBreath)
        {
            if (enemy.RealDistance <= breathRange)
            {
                Vector3 randomizedPos = randomizePos(player.Position, enemy.RealDistance, 20f);
                SAINHearingReport report = new()
                {
                    position = randomizedPos,
                    soundType = SAINSoundType.Breathing,
                    placeType = EEnemyPlaceType.Hearing,
                    isDanger = enemy.RealDistance < 25f || enemy.InLineOfSight,
                    shallReportToSquad = true,
                };
                enemy.Hearing.SetHeard(report, Time.time);
            }
            return;
        }

        if (enemy.RealDistance <= 50f)
        {
            Vector3 randomizedPos = randomizePos(player.Position, enemy.RealDistance, 20f);
            SAINHearingReport report = new()
            {
                position = randomizedPos,
                soundType = SAINSoundType.Conversation,
                placeType = EEnemyPlaceType.Hearing,
                isDanger = enemy.RealDistance < 25f || enemy.InLineOfSight,
                shallReportToSquad = true,
            };
            enemy.Hearing.SetHeard(report, Time.time);
        }

        if (Bot.Talk.IsSpeaking)
        {
            return;
        }
        if (enemy.RealDistance > TauntDist)
        {
            return;
        }

        if (phrase == EPhraseTrigger.OnFight || phrase == EPhraseTrigger.MumblePhrase)
        {
            SetEnemyTalk(enemy);
        }
    }

    private Vector3 randomizePos(Vector3 position, float distance, float dispersionFactor = 20f)
    {
        float disp = distance / dispersionFactor;
        Vector3 random = UnityEngine.Random.insideUnitSphere * disp;
        random.y = 0;
        return position + random;
    }

    private float _nextGestureTime;

    public bool ShallBeChatty()
    {
        if (Bot.Info.Profile.IsScav)
        {
            return SAINPlugin.LoadedPreset.GlobalSettings.Talk.TalkativeScavs;
        }
        if (Bot.Info.Profile.IsPMC)
        {
            return SAINPlugin.LoadedPreset.GlobalSettings.Talk.TalkativePMCs;
        }
        var role = Bot.Info.Profile.WildSpawnType;
        if ((Bot.Info.Profile.IsBoss || Bot.Info.Profile.IsFollower))
        {
            if ((role == WildSpawnType.bossKnight || role == WildSpawnType.followerBirdEye || role == WildSpawnType.followerBigPipe))
            {
                return SAINPlugin.LoadedPreset.GlobalSettings.Talk.TalkativeGoons;
            }
            return SAINPlugin.LoadedPreset.GlobalSettings.Talk.TalkativeBosses;
        }
        if ((role == WildSpawnType.pmcBot || role == WildSpawnType.exUsec))
        {
            return SAINPlugin.LoadedPreset.GlobalSettings.Talk.TalkativeRaidersRogues;
        }
        return false;
    }

    public void SetFriendlyTalked(Player player)
    {
        float maxDist = player.IsAI ? _friendlyResponseDistanceAI : _friendlyResponseDistance;
        if ((player.Position - Bot.Position).sqrMagnitude > maxDist * maxDist)
        {
            return;
        }

        if ((BotOwner.Memory.IsPeace || (Bot.Squad.HumanFriendClose && !player.IsAI))
            && _nextResponseTime < Time.time)
        {
            _nextResponseTime = Time.time + _friendlyResponseFrequencyLimit;

            if (player.IsAI == false || ShallBeChatty())
            {
                float chance = player.IsAI ? _friendlyResponseChanceAI : _friendlyResponseChance;

                Bot.StartCoroutine(RespondToFriendly(
                    EPhraseTrigger.MumblePhrase,
                    Bot.EnemyController.AtPeace ? ETagStatus.Unaware : ETagStatus.Combat,
                    Random.Range(_friendlyResponseMinRandom, _friendlyResponseMaxRandom),
                    player,
                    chance
                ));
            }
        }
        else if (Bot?.Squad.SquadInfo != null
            && Bot.Talk.GroupTalk.FriendIsClose
            && (Bot.Squad.SquadInfo.SquadPersonality != ESquadPersonality.GigaChads
                || Bot.Squad.SquadInfo.SquadPersonality != ESquadPersonality.Elite)
            && (Bot.Info.Personality == EPersonality.GigaChad
                || Bot.Info.Personality == EPersonality.Chad))
        {
            if (_saySilenceTime < Time.time)
            {
                _saySilenceTime = Time.time + 20f;
                Bot.StartCoroutine(RespondToFriendly(
                    EPhraseTrigger.Silence,
                    Bot.EnemyController.AtPeace ? ETagStatus.Unaware : ETagStatus.Aware,
                    Random.Range(0.2f, 0.5f),
                    player,
                    33f
                    ));
            }
        }
    }

    private float _friendlyResponseFrequencyLimit = 1f;
    private float _friendlyResponseMinRandom = 0.33f;
    private float _friendlyResponseMaxRandom = 0.75f;
    private float _saySilenceTime;
    private float _beggingTimer;
    private bool _isBegging;
    private float _fakeDeathTimer = 0f;
    private float _begTimer = 0f;

    private static readonly EPhraseTrigger[] BegPhrases =
    [
        EPhraseTrigger.Stop,
        EPhraseTrigger.HoldFire,
        EPhraseTrigger.GetBack
    ];

    private float _friendlyResponseDistance = 60f;
    private float _friendlyResponseDistanceAI = 35f;
    private float _friendlyResponseChance = 85f;
    private float _friendlyResponseChanceAI = 80f;
    private float _nextResponseTime;
    private float _tauntTimer = 0f;
}