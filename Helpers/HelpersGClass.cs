using EFT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using EFTCore = LocalBotSettingsProviderClass;
using EFTStatModifiersClass = BotLastBlindEffectModifierClass;

namespace SAIN.Helpers;

internal class HelpersGClass
{
    public static float LAY_DOWN_ANG_SHOOT => EFTCore.Core.LAY_DOWN_ANG_SHOOT;
    public static float Gravity => EFTCore.Core.G;
    public static float SMOKE_GRENADE_RADIUS_COEF => EFTCore.Core.SMOKE_GRENADE_RADIUS_COEF;
}

public class TemporaryStatModifiers
{
    public TemporaryStatModifiers(float precision = 1f, float accuracySpeed = 1f, float gainSight = 1f, float scatter = 1f, float priorityScatter = 1f, float visibleDistance = 1f, float hearingDistance = 1f)
    {
        Modifiers = new EFTStatModifiersClass
        {
            PrecicingSpeedCoef = precision,
            AccuratySpeedCoef = accuracySpeed,
            RuntimeVisionEffectK = gainSight,
            ScatteringCoef = scatter,
            PriorityScatteringCoef = priorityScatter,
            VisibleDistCoef = visibleDistance,
            HearingDistCoef = hearingDistance
        };
    }

    public EFTStatModifiersClass Modifiers;
}

public class CoreOverrides
{
    public string README = "Dont change anything here unless you know exactly what you are doing. Changes here require game restart! Not all settings do what the name suggests.";
    public bool SCAV_GROUPS_TOGETHER = false;
    public float DIST_NOT_TO_GROUP = 50f;
    public bool CAN_SHOOT_TO_HEAD = true;
    public float SOUND_DOOR_OPEN_METERS = 40f;
    public float SOUND_DOOR_BREACH_METERS = 70f;
    public float JUMP_SPREAD_DIST = 65f;
    public float BASE_WALK_SPEREAD2 = 65f;
    public int GRENADE_PRECISION = 10;
    public float PRONE_POSE = 1f;
    public float MOVE_COEF = 1f;
    public float LOWER_POSE = 1f;
    public float MAX_POSE = 1f;
    public float FLARE_POWER = 1.75f;
    public float FLARE_TIME = 2.5f;
    public float SHOOT_TO_CHANGE_RND_PART_DELTA = 2f;
}

public class EFTCoreSettings
{
    static EFTCoreSettings()
    {
        if (!JsonUtility.Load.LoadObject<CoreOverrides>(out _overrides, nameof(CoreOverrides)))
        {
            _overrides = new CoreOverrides();
            JsonUtility.SaveObjectToJson(_overrides, nameof(CoreOverrides));
        }
    }

    private static CoreOverrides _overrides;

    public static void UpdateCoreSettings()
    {
        try
        {
            var core = EFTCore.Core;
            if (_overrides == null)
            {
                _overrides = new CoreOverrides();
            }

            core.SCAV_GROUPS_TOGETHER = _overrides.SCAV_GROUPS_TOGETHER;
            core.DIST_NOT_TO_GROUP = _overrides.DIST_NOT_TO_GROUP;
            core.DIST_NOT_TO_GROUP_SQR = core.DIST_NOT_TO_GROUP.Sqr();
            core.CAN_SHOOT_TO_HEAD = _overrides.CAN_SHOOT_TO_HEAD;
            core.SOUND_DOOR_OPEN_METERS = _overrides.SOUND_DOOR_OPEN_METERS;
            core.SOUND_DOOR_BREACH_METERS = _overrides.SOUND_DOOR_BREACH_METERS;
            core.JUMP_SPREAD_DIST = _overrides.JUMP_SPREAD_DIST;
            core.BASE_WALK_SPEREAD2 = _overrides.BASE_WALK_SPEREAD2;
            core.GRENADE_PRECISION = _overrides.GRENADE_PRECISION;
            core.PRONE_POSE = _overrides.PRONE_POSE;
            core.MOVE_COEF = _overrides.MOVE_COEF;
            core.LOWER_POSE = _overrides.LOWER_POSE;
            core.MAX_POSE = _overrides.MAX_POSE;
            core.FLARE_POWER = _overrides.FLARE_POWER;
            core.FLARE_TIME = _overrides.FLARE_TIME;
            core.SHOOT_TO_CHANGE_RND_PART_DELTA = _overrides.SHOOT_TO_CHANGE_RND_PART_DELTA;

            ModDetection.UpdateArmorClassCoef();
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    public static void UpdateArmorClassCoef(float coef)
    {
        EFTCore.Core.ARMOR_CLASS_COEF = coef;
    }

    public EFTCore Core;
}

public class EFTBotSettings
{
    [JsonConstructor]
    public EFTBotSettings()
    { }

    public EFTBotSettings(string name, WildSpawnType type, BotDifficulty[] difficulties)
    {
        Name = name;
        WildSpawnType = type;
        foreach (BotDifficulty diff in difficulties)
        {
            Settings.Add(diff, EFTCore.GetSettings(diff, type, true));
        }
    }

    [JsonProperty]
    public string Name;

    [JsonProperty]
    public WildSpawnType WildSpawnType;

    [JsonProperty]
    public Dictionary<BotDifficulty, BotSettingsComponents> Settings = new();
}