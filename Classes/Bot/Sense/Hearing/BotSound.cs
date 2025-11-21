using SAIN.Components.PlayerComponentSpace;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class BotSound
{
    public BotSound(SoundInfoData info, Enemy enemy, float baseRange)
    {
        Enemy = enemy;
        Distance = enemy.RealDistance;
        Info = info;
        Results = new SoundResultsData(false);
        Range = new SoundRangeData(baseRange);
        Dispersion = new SoundDispersionData(1f);
        BulletData = new BulletData(false);
    }

    public Enemy Enemy { get; }
    public float Distance { get; }

    public SoundInfoData Info;
    public SoundResultsData Results;
    public SoundRangeData Range;
    public SoundDispersionData Dispersion;
    public BulletData BulletData;
}

public struct SoundInfoData
{
    public PlayerComponent SourcePlayer;
    public bool IsAI;
    public Vector3 Position;
    public SAINSoundType SoundType;
    public bool IsGunShot;
    public float Power;
    public float Volume;
}

public class SoundDispersionData
{
    public SoundDispersionData(float defaults = 1f)
    {
        EstimatedPosition = Vector3.zero;
        DistanceDispersion = defaults;
        AngleDispersionX = defaults;
        AngleDispersionY = defaults;
        DispersionModifier = defaults;
        DispersionType = ESoundDispersionType.None;
        Dispersion = 0f;
    }

    public float Dispersion;
    public Vector3 EstimatedPosition;

    // Unused
    public float DistanceDispersion;
    public float AngleDispersionX;
    public float AngleDispersionY;
    public float DispersionModifier;
    public ESoundDispersionType DispersionType;
}

public class SoundRangeData
{
    public SoundRangeData(float baseRange)
    {
        FinalRange = baseRange;
        BaseRange = baseRange;
        Modifiers = new SoundRangeModifiers(1f);
    }

    public float FinalRange;
    public float BaseRange;
    public SoundRangeModifiers Modifiers;
}

public class SoundRangeModifiers
{
    public float PreClampedMod => EnvironmentModifier * ConditionModifier * OcclusionModifier;

    public SoundRangeModifiers(float defaults = 1f)
    {
        FinalModifier = defaults;
        EnvironmentModifier = defaults;
        ConditionModifier = defaults;
        OcclusionModifier = defaults;
    }

    public float CalcFinalModifier(float min, float max)
    {
        return Mathf.Clamp(PreClampedMod, min, max);
    }

    public float FinalModifier;
    public float EnvironmentModifier;
    public float ConditionModifier;
    public float OcclusionModifier;
}

public class SoundResultsData
{
    public SoundResultsData(bool defaults = false)
    {
        Heard = defaults;
        VisibleSource = false;
        LimitedByAI = false;
        SoundFarFromPlayer = false;
        ChanceToHear = 100f;
        EstimatedPosition = Vector3.zero;
    }

    public bool Heard;
    public bool VisibleSource;
    public bool LimitedByAI;
    public bool SoundFarFromPlayer;
    public float ChanceToHear;
    public Vector3 EstimatedPosition;
}

public class BulletData
{
    public BulletData(bool defaults = false)
    {
        BulletFelt = defaults;
        BulletFiredAtMe = defaults;
        ProjectionPoint = Vector3.zero;
        ProjectionPointDistance = float.MaxValue;
        Suppressed = defaults;
    }

    public bool BulletFelt;
    public bool BulletFiredAtMe;
    public Vector3 ProjectionPoint;
    public float ProjectionPointDistance;
    public bool Suppressed;
}