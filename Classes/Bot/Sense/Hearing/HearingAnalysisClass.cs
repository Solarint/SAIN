using EFT;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class HearingAnalysisClass : BotSubClass<SAINHearingSensorClass>, IBotClass
{
    public HearingAnalysisClass(SAINHearingSensorClass hearing) : base(hearing)
    {
    }

    public bool CheckIfSoundHeard(AISoundData sound)
    {
        if (ShallLimitAI(sound))
        {
            return false;
        }
        if (!sound.IsGunShot && !DoIDetectFootsteps(sound))
        {
            return false;
        }
        if ((sound.Sound.Position - sound.Sound.PlayerComponent.Position).sqrMagnitude > 5f * 5f)
        {
            return false;
        }
        
        float EnvironmentModifier = CalcEnvironmentMod(sound);
        float ConditionModifier = CalcConditionMod(sound.SoundType);
        float OcclusionModifier = CalcOcclusionMod(sound.Enemy, sound.SoundType);
        float FinalModifier = Mathf.Clamp(1.0f * EnvironmentModifier * ConditionModifier * OcclusionModifier * Bot.Info.Difficulty.HearingDistanceModifier, _settings.HEAR_MODIFIER_MIN_CLAMP, _settings.HEAR_MODIFIER_MAX_CLAMP);
        float FinalRange = sound.Sound.Range * sound.Sound.Volume * FinalModifier;
        if (sound.PlayerDistance > FinalRange)
        {
            return false;
        }

        if (!sound.Enemy.Player.IsAI)
        {
            //Logger.LogDebug($"Heard Sound : Final Range [{sound.Range.FinalRange}] : Modifier {sound.Range.Modifiers.FinalModifier}");
        }
        return true;
    }

    private static HearingSettings _settings => GlobalSettingsClass.Instance.Hearing;

    private float CalcBunkerVolumeReduction(AISoundData sound)
    {
        var botLocation = Bot.PlayerComponent.AIData.PlayerLocation;
        var enemyLocation = sound.HeardPlayerComponent.AIData.PlayerLocation;

        bool botinBunker = botLocation.InBunker;
        bool playerinBunker = enemyLocation.InBunker;
        if (botinBunker != playerinBunker)
        {
            return _settings.BUNKER_REDUCTION_COEF;
        }
        if (botinBunker)
        {
            float diff = Mathf.Abs(botLocation.BunkerDepth - enemyLocation.BunkerDepth);
            if (diff > 0)
            {
                return _settings.BUNKER_ELEV_DIFF_COEF;
            }
        }
        return 1f;
    }

    private bool DoIDetectFootsteps(AISoundData sound)
    {
        bool hasheadPhones = Bot.PlayerComponent.Equipment.GearInfo.HasEarPiece;
        float closehearing = hasheadPhones ? _settings.HEAR_CHANCE_MIN_DIST_HEADPHONES : _settings.HEAR_CHANCE_MIN_DIST;
        float distance = sound.PlayerDistance;
        if (distance <= closehearing)
        {
            return true;
        }

        float farhearing = hasheadPhones ? SAINPlugin.LoadedPreset.GlobalSettings.Hearing.MaxFootstepAudioDistance : SAINPlugin.LoadedPreset.GlobalSettings.Hearing.MaxFootstepAudioDistanceNoHeadphones;
        if (distance > farhearing)
        {
            return false;
        }

        float minimumChance = 0f;
        if (hasheadPhones)
        {
            if (distance < farhearing * _settings.HEAR_CHANCE_MIDRANGE_COEF)
            {
                minimumChance += _settings.HEAR_CHANCE_MIDRANGE_MINCHANCE_HEADPHONES;
            }
            else
            {
                minimumChance += _settings.HEAR_CHANCE_LONGRANGE_MINCHANCE_HEADPHONES;
            }
            if (sound.SoundType != SAINSoundType.FootStep)
            {
                minimumChance += _settings.HEAR_CHANCE_HEADPHONES_OTHERSOUNDS;
            }
        }

        if (Bot.PlayerComponent.Transform.VelocityData.VelocityMagnitudeNormal < _settings.HEAR_CHANCE_NOTMOVING_VELOCITY)
        {
            minimumChance += hasheadPhones ? _settings.HEAR_CHANCE_NOTMOVING_MINCHANCE_HEADPHONES : _settings.HEAR_CHANCE_NOTMOVING_MINCHANCE;
        }

        if (Bot.HasEnemy &&
            Bot.GoalEnemy.EnemyProfileId == sound.Sound.PlayerComponent.ProfileId)
        {
            minimumChance += hasheadPhones ? _settings.HEAR_CHANCE_CURRENTENEMY_MINCHANCE_HEADPHONES : _settings.HEAR_CHANCE_CURRENTENEMY_MINCHANCE;
        }

        float num = farhearing - closehearing;
        float num2 = distance - closehearing;
        float chanceToHear = 1f - num2 / num;
        chanceToHear *= 100f;

        chanceToHear = Mathf.Clamp(chanceToHear, minimumChance, 100f);
        if (!sound.Enemy.Player.IsAI)
        {
            //Logger.LogDebug($"chanceToHear [{chanceToHear}] : minChance [{minimumChance}] : distance [{distance}]");
        }
        return EFTMath.RandomBool(chanceToHear);
    }

    private static float CalcOcclusionMod(Enemy Enemy, SAINSoundType SoundType)
    {
        if (Enemy.InLineOfSight)
        {
            return 1f;
        }
        switch (SoundType)
        {
            case SAINSoundType.Shot:
                return _settings.GUNSHOT_OCCLUSION_MOD;

            case SAINSoundType.SuppressedShot:
                return _settings.GUNSHOT_OCCLUSION_MOD_SUPP;

            case SAINSoundType.Sprint:
                return _settings.FOOTSTEP_OCCLUSION_MOD_SPRINT;

            case SAINSoundType.FootStep:
                if (Enemy.EnemyPlayer?.IsSprintEnabled == true)
                {
                    return _settings.FOOTSTEP_OCCLUSION_MOD_SPRINT;
                }
                return _settings.FOOTSTEP_OCCLUSION_MOD;

            default:
                return _settings.OTHER_OCCLUSION_MOD;
        }
    }

    private float CalcEnvironmentMod(AISoundData sound)
    {
        if (Player.AIData.EnvironmentId == sound.EnvironmentId)
        {
            return 1f;
        }
        float envMod = sound.IsGunShot ? _settings.GUNSHOT_ENVIR_MOD : _settings.FOOTSTEP_ENVIR_MOD;
        float bunkerMod = CalcBunkerVolumeReduction(sound);
        float result = envMod * bunkerMod;
        result = Mathf.Clamp(result, _settings.MIN_ENVIRONMENT_MOD, 1f);
        return result;
    }

    private bool ShallLimitAI(AISoundData sound)
    {
        if (!sound.Enemy.IsAI)
            return false;

        var aiLimit = GlobalSettingsClass.Instance.General.AILimit;
        if (!aiLimit.LimitAIvsAIGlobal)
            return false;

        if (!aiLimit.LimitAIvsAIHearing)
            return false;

        var enemyPlayer = sound.Sound.PlayerComponent;
        if (Bot.GoalEnemy?.EnemyProfileId == enemyPlayer.ProfileId)
            return false;

        var enemyBot = enemyPlayer.BotComponent;
        float maxRange;
        if (enemyBot == null)
        {
            if (enemyPlayer.BotOwner?.Memory.GoalEnemy?.ProfileId == Bot.ProfileId)
            {
                return false;
            }
            maxRange = GetMaxRange(Bot.CurrentAILimit);
        }
        else
        {
            if (enemyBot.GoalEnemy?.EnemyProfileId == Bot.ProfileId)
            {
                return false;
            }
            maxRange = GetMaxRange(enemyBot.CurrentAILimit);
        }

        if (sound.PlayerDistance <= maxRange)
        {
            return false;
        }
        return true;
    }

    private static float GetMaxRange(AILimitSetting aiLimit)
    {
        switch (aiLimit)
        {
            case AILimitSetting.Far:
                return _farDistance;

            case AILimitSetting.VeryFar:
                return _veryFarDistance;

            case AILimitSetting.Narnia:
                return _narniaDistance;

            default:
                return float.MaxValue;
        }
    }

    private float CalcConditionMod(SAINSoundType SoundType)
    {
        float modifier = 1f;
        float? currentHearSense = BotOwner?.Settings?.Current?.CurrentHearingSense;
        if (currentHearSense != null)
        {
            modifier *= currentHearSense.Value;
        }
        modifier *= Bot.Info.FileSettings.Core.HearingDistanceMulti;

        if (SoundType != SAINSoundType.Shot)
        {
            if (!Bot.PlayerComponent.Equipment.GearInfo.HasEarPiece)
            {
                modifier *= _settings.HEAR_MODIFIER_NO_EARS;
            }
            if (Bot.PlayerComponent.Equipment.GearInfo.HasHeavyHelmet)
            {
                modifier *= _settings.HEAR_MODIFIER_HEAVY_HELMET;
            }
            if (Bot.Memory.Health.Dying &&
                !Bot.Memory.Health.OnPainKillers)
            {
                modifier *= _settings.HEAR_MODIFIER_DYING;
            }
            if (Player.IsSprintEnabled)
            {
                modifier *= _settings.HEAR_MODIFIER_SPRINT;
            }
            if (Player.HeavyBreath)
            {
                modifier *= _settings.HEAR_MODIFIER_HEAVYBREATH;
            }
        }
        return modifier;
    }

    private void updateSettings(SAINPresetClass preset)
    {
        int frame = Time.frameCount;
        if (_lastCalcFrame == frame)
        {
            return;
        }
        _lastCalcFrame = frame;
        var maxHeadRanges = preset.GlobalSettings.General.AILimit.MaxHearingRanges;
        _farDistance = maxHeadRanges[AILimitSetting.Far];
        _veryFarDistance = maxHeadRanges[AILimitSetting.VeryFar];
        _narniaDistance = maxHeadRanges[AILimitSetting.Narnia];
    }

    private static int _lastCalcFrame;
    private static float _farDistance;
    private static float _veryFarDistance;
    private static float _narniaDistance;
}