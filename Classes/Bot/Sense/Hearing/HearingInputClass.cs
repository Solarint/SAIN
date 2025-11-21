using EFT;
using SAIN.Components;
using SAIN.Components.BotController;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Models.Structs;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class HearingInputClass : BotSubClass<SAINHearingSensorClass>, IBotClass
{
    private const float BOT_DEAF_TIME_INTERVAL = 0.75f;
    private const float DeafenCoef_Gunfire = 0.33f;
    private const float DeafenCoef_Suppressed = 0.33f;
    private const float DeafenCoef_Convo = 0.4f;
    private const float DeafenCoef_Generic = 0.3f;

    private const float IMPACT_HEAR_FREQUENCY = 0.5f;
    private const float IMPACT_HEAR_FREQUENCY_FAR = 0.05f;
    private const float IMPACT_MAX_HEAR_DISTANCE = 50f * 50f;
    private const float IMPACT_DISPERSION = 5f * 5f;

    public bool IgnoreUnderFire { get; private set; }
    public bool IgnoreHearing { get; private set; }

    public bool IsBotDeafened {
        get
        {
            if (_BotDeafedTime > 0)
            {
                if (_BotDeafedTime < Time.time)
                {
                    return true;
                }
                _BotDeafedTime = -1;
            }
            return false;
        }
    }

    public HearingInputClass(SAINHearingSensorClass hearing) : base(hearing)
    {
    }

    public override void Init()
    {
        PlayerComponent.OnBulletFlyBy += OnBulletFlyBy;
        //SAINBotController.Instance.BotHearing.AISoundPlayed += soundHeard;
        BotManagerComponent.Instance.BotHearing.BulletImpact += bulletImpacted;
        base.Init();
    }

    protected void OnBulletFlyBy(PlayerComponent Source, EftBulletClass Bullet)
    {
        Enemy Enemy = Bot.EnemyController.CheckAddEnemy(Source.Player);
        if (Enemy != null)
        {
            SoundEvent SoundEvent = new(SAINSoundType.BulletImpact, Source.Position, Source, 100, 1, 999);
            AISoundData Sound = new(SoundEvent, Bot, PlayerComponent.GetDistanceToPlayer(Source.ProfileId), Enemy);
            Vector3 BulletPosition = Bullet.CurrentPosition;
            Vector3 MyHeadPosition = Bot.Transform.HeadData.HeadPosition;
            float Dist = (BulletPosition - MyHeadPosition).magnitude;
            BaseClass.ReactToBulletFlyBy(Sound, Dist);
        }
    }

    public override void ManualUpdate()
    {
        checkResetHearing();
        base.ManualUpdate();
    }

    public event Action<AISoundData> OnFriendlySoundHeard;

    public readonly List<AISoundData> SoundDataToReactTo = [];
    public readonly List<AISoundData> AISoundCachedEvents = [];
    public readonly List<AISoundData> AISoundCachedEvents_Conversations = [];
    public readonly List<AISoundData> AISoundCachedEvents_Gunshots = [];
    public readonly List<AISoundData> AISoundCachedEvents_Gunshots_Suppressed = [];

    public void CheckAddSoundToCache(SoundEvent Sound, float PlayerDistance)
    {
        if (!Sound.SoundType.IsGunShot() && IgnoreHearing)
        {
            return;
        }
        if (Bot.EnemyController != null)
        {
            Enemy enemy = Bot.EnemyController.CheckAddEnemy(Sound.GetPlayer());
            if (enemy == null && Sound.SoundType != SAINSoundType.Conversation)
            {
                return;
            }
            AISoundData Data = new(Sound, Bot, PlayerDistance, enemy);
            switch (Sound.SoundType)
            {
                case SAINSoundType.Shot:
                    AISoundCachedEvents_Gunshots.Add(Data);
                    break;

                case SAINSoundType.SuppressedShot:
                    AISoundCachedEvents_Gunshots_Suppressed.Add(Data);
                    break;

                case SAINSoundType.Conversation:
                    AISoundCachedEvents_Conversations.Add(Data);
                    break;

                default:
                    AISoundCachedEvents.Add(Data);
                    break;
            }
        }
    }

    public void ProcessAISoundCache()
    {
        UnityEngine.Profiling.Profiler.BeginSample("Process Sounds For Bots");

        bool DeafeningShot = false;
        bool AlreadyDeafened = IsBotDeafened;

        // Process gunshots first, since they can trigger a bot to be deaf to other sounds
        if (AISoundCachedEvents_Gunshots.Count > 0 && 
            ProcessGunshots(AISoundCachedEvents_Gunshots, false, DeafenCoef_Gunfire, SoundDataToReactTo))
        {
            DeafeningShot = true;
            AlreadyDeafened = true;
        }

        if (AISoundCachedEvents_Gunshots_Suppressed.Count > 0 && 
            ProcessGunshots(AISoundCachedEvents_Gunshots_Suppressed, AlreadyDeafened, DeafenCoef_Suppressed, SoundDataToReactTo))
        {
            DeafeningShot = true;
            AlreadyDeafened = true;
        }

        // Process most sounds if we aren't deafened
        if (AISoundCachedEvents_Conversations.Count > 0)
        {
            ProcessSounds(AISoundCachedEvents, AlreadyDeafened, DeafenCoef_Convo, SoundDataToReactTo);
        }
        if (AISoundCachedEvents.Count > 0)
        {
            ProcessSounds(AISoundCachedEvents, AlreadyDeafened, DeafenCoef_Generic, SoundDataToReactTo);
        }

        bool SoundRemoved = false;
        for (int i = SoundDataToReactTo.Count - 1; i >= 0; i--)
        {
            if (SoundDataToReactTo[i].CanReport(0.2f))
            {
                TryReactToSound(SoundDataToReactTo[i]);
                SoundDataToReactTo.RemoveAt(i);
                SoundRemoved = true;
            }
        }

        if (SoundRemoved)
            SoundDataToReactTo.TrimExcess();
        if (DeafeningShot)
            _BotDeafedTime = Time.time + BOT_DEAF_TIME_INTERVAL;

        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void TryReactToSound(AISoundData Sound)
    {
        if (Sound.Enemy != null)
        {
            BaseClass.ReactToHeardSound(Sound);
        }
        else
        {
            OnFriendlySoundHeard?.Invoke(Sound);
        }
    }

    private float _BotDeafedTime = -1;

    private static bool ProcessSounds(List<AISoundData> Sounds, bool PreviouslyDeaf, float DeafenCoef, List<AISoundData> Results)
    {
        bool DeafeningShot = false;
        int Count = Sounds.Count;
        if (Count > 0)
        {
            Sounds.Sort((a, b) => a.PlayerDistance.CompareTo(b.PlayerDistance));
            for (int i = 0; i < Count; i++)
            {
                AISoundData Sound = Sounds[i];
                // If Sounds is closer than or equal to the input fraction of the Baserange of this sound, always report it. If we are checking gunshots, then this sound will deafen the bot for a duration.
                if (Sound.PlayerDistance <= Sound.Sound.BaseRangeWithVolume)
                {
                    if (PreviouslyDeaf && Sound.PlayerDistance > Sound.Sound.BaseRangeWithVolume * DeafenCoef)
                        continue;
                    Results.Add(Sound);
                }
            }
            Sounds.Clear();
        }
        return DeafeningShot;
    }

    private static bool ProcessGunshots(List<AISoundData> Sounds, bool previouslyDeaf, float DeafenCoef, List<AISoundData> Results)
    {
        bool DeafeningShot = false;
        int Count = Sounds.Count;
        if (Count > 0)
        {
            Sounds.Sort((a, b) => a.PlayerDistance.CompareTo(b.PlayerDistance));
            for (int i = 0; i < Count; i++)
            {
                AISoundData Sound = Sounds[i];
                // If Sounds is closer than or equal to the input fraction of the Baserange of this sound, always report it. If we are checking gunshots, then this sound will deafen the bot for a duration.
                if (Sound.PlayerDistance <= Sound.Sound.BaseRangeWithVolume)
                {
                    bool thisShotDeafened = Sound.PlayerDistance <= Sound.Sound.BaseRangeWithVolume * DeafenCoef;
                    if (previouslyDeaf && !thisShotDeafened)
                        continue;
                    if (!DeafeningShot)
                        DeafeningShot = thisShotDeafened;
                    Results.Add(Sound);
                }
            }
            Sounds.Clear();
        }
        return DeafeningShot;
    }

    private void checkResetHearing()
    {
        if (!IgnoreHearing)
        {
            if (IgnoreUnderFire)
                IgnoreUnderFire = false;
            return;
        }
        if (_ignoreUntilTime > 0 &&
            _ignoreUntilTime < Time.time)
        {
            IgnoreHearing = false;
            IgnoreUnderFire = false;
            return;
        }
        if (Bot.EnemyController.VisibleEnemies.Count > 0)
        {
            IgnoreHearing = false;
            IgnoreUnderFire = false;
            return;
        }
    }

    public override void Dispose()
    {
        //SAINBotController.Instance.BotHearing.AISoundPlayed -= soundHeard;
        BotManagerComponent.Instance.BotHearing.BulletImpact -= bulletImpacted;
        PlayerComponent.OnBulletFlyBy -= OnBulletFlyBy;
        base.Dispose();
    }

    private void bulletImpacted(EftBulletClass bullet)
    {
        if (!canHearSounds())
        {
            return;
        }
        float currentTime = Time.time;
        if (_nextHearImpactTime > currentTime)
        {
            return;
        }
        if (Bot.HasEnemy)
        {
            return;
        }
        var player = bullet.Player?.iPlayer;
        if (player == null)
        {
            return;
        }
        var enemy = Bot.EnemyController.GetEnemy(player.ProfileId, true);
        if (enemy == null)
        {
            return;
        }
        if (!soundListenerStarted(enemy.EnemyPlayerComponent))
        {
            return;
        }
        if (Bot.PlayerComponent.AIData.PlayerLocation.InBunker != enemy.EnemyPlayerComponent.AIData.PlayerLocation.InBunker)
        {
            return;
        }
        float distance = (bullet.CurrentPosition - Bot.Position).sqrMagnitude;
        if (distance > IMPACT_MAX_HEAR_DISTANCE)
        {
            _nextHearImpactTime = currentTime + IMPACT_HEAR_FREQUENCY_FAR;
            return;
        }
        _nextHearImpactTime = currentTime + IMPACT_HEAR_FREQUENCY;

        float dispersion = distance / IMPACT_DISPERSION;
        Vector3 random = UnityEngine.Random.onUnitSphere;
        random.y = 0;
        random = random.normalized * dispersion;
        Vector3 estimatedPos = enemy.EnemyPosition + random;

        SAINHearingReport report = new() {
            position = estimatedPos,
            soundType = SAINSoundType.BulletImpact,
            placeType = EEnemyPlaceType.Hearing,
            isDanger = distance < 25f * 25f,
            shallReportToSquad = true,
        };
        enemy.Hearing.SetHeard(report, currentTime);
    }

    private bool canHearSounds()
    {
        if (!Bot.BotActive)
        {
            return false;
        }
        if (Bot.GameEnding)
        {
            return false;
        }
        return true;
    }

    private bool soundListenerStarted(PlayerComponent player)
    {
        if (!player.IsAI)
        {
            return true;
        }
        if (!_hearingStarted)
        {
            if (!PlayerComponent.AIData.AISoundPlayer.SoundMakerStarted)
            {
                return false;
            }
            _hearingStarted = true;
        }
        return true;
    }

    private bool _hearingStarted;

    public bool SetIgnoreHearingExternal(bool value, bool ignoreUnderFire, float duration, out string reason)
    {
        // Only allow the bot to ignore hearing if it's not in combat
        if (value)
        {
            if (Bot.GoalEnemy?.IsVisible == true)
            {
                reason = "Enemy Visible";
                return false;
            }
            if (BotOwner.Memory.IsUnderFire && !ignoreUnderFire)
            {
                reason = "Under Fire";
                return false;
            }
        }

        IgnoreUnderFire = ignoreUnderFire;
        IgnoreHearing = value;
        if (value && duration > 0f)
        {
            _ignoreUntilTime = Time.time + duration;
        }
        else
        {
            _ignoreUntilTime = -1f;
        }
        reason = string.Empty;
        return true;
    }

    private float _nextHearImpactTime;
    private float _ignoreUntilTime;
}