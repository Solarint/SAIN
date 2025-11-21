using EFT;
using System.Collections.Generic;

namespace SAIN.Preset.Personalities;

public static class PersonalityDefaultsClass
{
    public static void InitDefaults(PersonalityDictionary Personalities, SAINPresetClass preset)
    {
        if (!Personalities.ContainsKey(EPersonality.Wreckless))
            initWreckless(Personalities, preset);

        if (!Personalities.ContainsKey(EPersonality.SnappingTurtle))
            initSnappingTurtle(Personalities, preset);

        if (!Personalities.ContainsKey(EPersonality.GigaChad))
            initGigaChad(Personalities, preset);

        if (!Personalities.ContainsKey(EPersonality.Chad))
            initChad(Personalities, preset);

        if (!Personalities.ContainsKey(EPersonality.Rat))
            initRat(Personalities, preset);

        if (!Personalities.ContainsKey(EPersonality.Timmy))
            initTimmy(Personalities, preset);

        if (!Personalities.ContainsKey(EPersonality.Coward))
            initCoward(Personalities, preset);

        if (!Personalities.ContainsKey(EPersonality.Normal))
            initNormal(Personalities, preset);
    }

    private static void initGigaChad(PersonalityDictionary Personalities, SAINPresetClass Preset)
    {
        EPersonality personality = EPersonality.GigaChad;
        var settings = new PersonalitySettingsClass(personality);

        var assignment = settings.Assignment;
        assignment.Enabled = true;
        assignment.RandomlyAssignedChance = 3;
        assignment.CanBeRandomlyAssigned = true;
        assignment.MaxChanceIfMeetRequirements = 80;
        assignment.MinLevel = 0;
        assignment.MaxLevel = 100;
        assignment.PowerLevelMin = 250;
        assignment.PowerLevelMax = 1000;
        assignment.PowerLevelScaleStart = 250;
        assignment.PowerLevelScaleEnd = 500;

        var behavior = settings.Behavior;

        behavior.General.KickOpenAllDoors = true;
        behavior.General.AggressionMultiplier = 1;
        behavior.General.HoldGroundBaseTime = 1.25f;
        behavior.General.HoldGroundMaxRandom = 1.5f;
        behavior.General.HoldGroundMinRandom = 0.65f;

        behavior.Cover.CanShiftCoverPosition = true;
        behavior.Cover.ShiftCoverTimeMultiplier = 0.5f;
        behavior.Cover.MoveToCoverHasEnemySpeed = 1f;
        behavior.Cover.MoveToCoverHasEnemyPose = 1f;
        behavior.Cover.MoveToCoverNoEnemySpeed = 1f;
        behavior.Cover.MoveToCoverNoEnemyPose = 1f;

        behavior.Talk.CanTaunt = true;
        behavior.Talk.CanRespondToEnemyVoice = true;
        behavior.Talk.TauntFrequency = 8;
        behavior.Talk.TauntChance = 45;
        behavior.Talk.TauntMaxDistance = 65f;
        behavior.Talk.ConstantTaunt = true;
        behavior.Talk.FrequentTaunt = true;
        behavior.Talk.CanFakeDeathRare = true;
        behavior.Talk.FakeDeathChance = 3;

        behavior.Search.WillSearchForEnemy = true;
        behavior.Search.WillSearchFromAudio = true;
        behavior.Search.WillChaseDistantGunshots = true;
        behavior.Search.SearchBaseTime = 6;
        behavior.Search.SprintWhileSearchChance = 75;
        behavior.Search.SearchWaitMultiplier = 3f;
        behavior.Search.HeardFromPeaceBehavior = EHeardFromPeaceBehavior.SearchNow;
        behavior.Search.SlowAtCorners = false;

        behavior.Rush.CanRushEnemyReloadHeal = true;
        behavior.Rush.CanJumpCorners = true;
        behavior.Rush.JumpCornerChance = 40f;
        behavior.Rush.CanBunnyHop = true;
        behavior.Rush.BunnyHopChance = 5;

        addPMCs(settings.Assignment.AllowedTypes);
        Personalities.Add(personality, settings);
        if (Preset.Info.IsCustom == true)
        {
            SAINPresetClass.Export(settings, Preset.Info.Name, personality.ToString(), nameof(Personalities));
        }
    }

    private static void initWreckless(PersonalityDictionary Personalities, SAINPresetClass Preset)
    {
        EPersonality personality = EPersonality.Wreckless;
        var settings = new PersonalitySettingsClass(personality);

        var assignment = settings.Assignment;
        assignment.Enabled = true;
        assignment.RandomlyAssignedChance = 1;
        assignment.CanBeRandomlyAssigned = true;
        assignment.MaxChanceIfMeetRequirements = 5;
        assignment.MinLevel = 0;
        assignment.MaxLevel = 100;
        assignment.PowerLevelMin = 250;
        assignment.PowerLevelMax = 1000;
        assignment.PowerLevelScaleStart = 250;
        assignment.PowerLevelScaleEnd = 500;

        var behavior = settings.Behavior;

        behavior.General.KickOpenAllDoors = true;
        behavior.General.AggressionMultiplier = 1;
        behavior.General.HoldGroundBaseTime = 2f;
        behavior.General.HoldGroundMaxRandom = 2.5f;
        behavior.General.HoldGroundMinRandom = 0.75f;
        behavior.General.TARGET_SUPPRESS_TOGGLE = true;
        behavior.General.TimeSinceSeenToSuppress = 30f;
        behavior.General.TimeSinceShotAtToSuppress = 30f;
        behavior.General.TimeSinceShotToSuppress = 30f;

        behavior.Cover.CanShiftCoverPosition = true;
        behavior.Cover.ShiftCoverTimeMultiplier = 0.5f;
        behavior.Cover.MoveToCoverHasEnemySpeed = 1f;
        behavior.Cover.MoveToCoverHasEnemyPose = 1f;
        behavior.Cover.MoveToCoverNoEnemySpeed = 1f;
        behavior.Cover.MoveToCoverNoEnemyPose = 1f;

        behavior.Talk.CanTaunt = true;
        behavior.Talk.CanRespondToEnemyVoice = true;
        behavior.Talk.TauntFrequency = 4;
        behavior.Talk.TauntChance = 33;
        behavior.Talk.TauntMaxDistance = 75f;
        behavior.Talk.ConstantTaunt = true;
        behavior.Talk.FrequentTaunt = true;
        behavior.Talk.CanFakeDeathRare = true;
        behavior.Talk.FakeDeathChance = 6;

        behavior.Search.WillSearchForEnemy = true;
        behavior.Search.WillSearchFromAudio = true;
        behavior.Search.WillChaseDistantGunshots = true;
        behavior.Search.SearchBaseTime = 0.1f;
        behavior.Search.SprintWhileSearchChance = 90;
        behavior.Search.SearchWaitMultiplier = 1f;
        behavior.Search.HeardFromPeaceBehavior = EHeardFromPeaceBehavior.Charge;
        behavior.Search.SlowAtCorners = false;

        behavior.Rush.CanRushEnemyReloadHeal = true;
        behavior.Rush.CanJumpCorners = true;
        behavior.Rush.JumpCornerChance = 80f;
        behavior.Rush.CanBunnyHop = true;
        behavior.Rush.BunnyHopChance = 40;

        addAllTypes(settings.Assignment.AllowedTypes);
        Personalities.Add(personality, settings);
        if (Preset.Info.IsCustom == true)
        {
            SAINPresetClass.Export(settings, Preset.Info.Name, personality.ToString(), nameof(Personalities));
        }
    }

    private static void initSnappingTurtle(PersonalityDictionary Personalities, SAINPresetClass Preset)
    {
        EPersonality personality = EPersonality.SnappingTurtle;
        var settings = new PersonalitySettingsClass(personality);

        var turtleAssignment = settings.Assignment;
        turtleAssignment.Enabled = true;
        turtleAssignment.RandomlyAssignedChance = 1;
        turtleAssignment.CanBeRandomlyAssigned = true;
        turtleAssignment.MaxChanceIfMeetRequirements = 30;
        turtleAssignment.MinLevel = 15;
        turtleAssignment.MaxLevel = 100;
        turtleAssignment.PowerLevelMin = 150;
        turtleAssignment.PowerLevelMax = 1000;
        turtleAssignment.PowerLevelScaleStart = 150;
        turtleAssignment.PowerLevelScaleEnd = 500;

        var behavior = settings.Behavior;

        behavior.General.AggressionMultiplier = 1;
        behavior.General.HoldGroundBaseTime = 1.5f;
        behavior.General.HoldGroundMaxRandom = 1.2f;
        behavior.General.HoldGroundMinRandom = 0.8f;
        behavior.General.TARGET_SUPPRESS_TOGGLE = false;

        behavior.Cover.CanShiftCoverPosition = true;
        behavior.Cover.ShiftCoverTimeMultiplier = 2f;
        behavior.Cover.MoveToCoverHasEnemySpeed = 1f;
        behavior.Cover.MoveToCoverHasEnemyPose = 1f;
        behavior.Cover.MoveToCoverNoEnemySpeed = 1f;
        behavior.Cover.MoveToCoverNoEnemyPose = 1f;

        behavior.Talk.CanTaunt = true;
        behavior.Talk.CanRespondToEnemyVoice = false;
        behavior.Talk.TauntFrequency = 15;
        behavior.Talk.TauntMaxDistance = 70f;
        behavior.Talk.ConstantTaunt = false;
        behavior.Talk.FrequentTaunt = false;
        behavior.Talk.CanFakeDeathRare = true;
        behavior.Talk.FakeDeathChance = 10;

        behavior.Search.WillSearchForEnemy = true;
        behavior.Search.WillSearchFromAudio = true;
        behavior.Search.WillChaseDistantGunshots = false;
        behavior.Search.SearchBaseTime = 90f;
        behavior.Search.SprintWhileSearchChance = 40f;
        behavior.Search.Sneaky = true;
        behavior.Search.SneakyPose = 1f;
        behavior.Search.SneakySpeed = 0.33f;
        behavior.Search.SearchWaitMultiplier = 3f;
        behavior.Search.HeardFromPeaceBehavior = EHeardFromPeaceBehavior.Freeze;

        behavior.Rush.CanRushEnemyReloadHeal = true;
        behavior.Rush.CanJumpCorners = true;
        behavior.Rush.JumpCornerChance = 100f;
        behavior.Rush.CanBunnyHop = true;
        behavior.Rush.BunnyHopChance = 20;

        addPMCs(settings.Assignment.AllowedTypes);
        Personalities.Add(personality, settings);
        if (Preset.Info.IsCustom == true)
        {
            SAINPresetClass.Export(settings, Preset.Info.Name, personality.ToString(), nameof(Personalities));
        }
    }

    private static void initChad(PersonalityDictionary Personalities, SAINPresetClass Preset)
    {
        EPersonality personality = EPersonality.Chad;
        var settings = new PersonalitySettingsClass(personality);

        var assignment = settings.Assignment;
        assignment.Enabled = true;
        assignment.RandomlyAssignedChance = 8;
        assignment.CanBeRandomlyAssigned = true;
        assignment.MaxChanceIfMeetRequirements = 80;
        assignment.MinLevel = 0;
        assignment.MaxLevel = 100;
        assignment.PowerLevelMin = 100;
        assignment.PowerLevelMax = 1000;
        assignment.PowerLevelScaleStart = 100;
        assignment.PowerLevelScaleEnd = 400;

        var behavior = settings.Behavior;

        behavior.General.AggressionMultiplier = 1;
        behavior.General.HoldGroundBaseTime = 1.5f;
        behavior.General.HoldGroundMaxRandom = 1.5f;
        behavior.General.HoldGroundMinRandom = 0.75f;

        behavior.Cover.CanShiftCoverPosition = true;
        behavior.Cover.ShiftCoverTimeMultiplier = 1f;
        behavior.Cover.MoveToCoverHasEnemySpeed = 1f;
        behavior.Cover.MoveToCoverHasEnemyPose = 1f;
        behavior.Cover.MoveToCoverNoEnemySpeed = 1f;
        behavior.Cover.MoveToCoverNoEnemyPose = 1f;

        behavior.Talk.CanTaunt = true;
        behavior.Talk.CanRespondToEnemyVoice = false;
        behavior.Talk.TauntFrequency = 20;
        behavior.Talk.TauntChance = 60;
        behavior.Talk.TauntMaxDistance = 50f;
        behavior.Talk.FrequentTaunt = true;
        behavior.Talk.ConstantTaunt = false;
        behavior.Talk.CanFakeDeathRare = false;
        behavior.Talk.FakeDeathChance = 0;

        behavior.Search.WillSearchForEnemy = true;
        behavior.Search.WillSearchFromAudio = true;
        behavior.Search.WillChaseDistantGunshots = true;
        behavior.Search.SearchBaseTime = 16f;
        behavior.Search.SprintWhileSearchChance = 60f;
        behavior.Search.Sneaky = false;
        behavior.Search.SneakyPose = 0f;
        behavior.Search.SneakySpeed = 0f;
        behavior.Search.SearchWaitMultiplier = 1f;
        behavior.Search.HeardFromPeaceBehavior = EHeardFromPeaceBehavior.Freeze;
        behavior.Search.SlowAtCorners = false;

        behavior.Rush.CanRushEnemyReloadHeal = true;
        behavior.Rush.CanJumpCorners = false;
        behavior.Rush.JumpCornerChance = 0f;
        behavior.Rush.CanBunnyHop = false;
        behavior.Rush.BunnyHopChance = 0f;

        addPMCs(settings.Assignment.AllowedTypes);
        Personalities.Add(personality, settings);
        if (Preset.Info.IsCustom == true)
        {
            SAINPresetClass.Export(settings, Preset.Info.Name, personality.ToString(), nameof(Personalities));
        }
    }

    private static void initRat(PersonalityDictionary Personalities, SAINPresetClass Preset)
    {
        EPersonality personality = EPersonality.Rat;
        var settings = new PersonalitySettingsClass(personality);

        var assignment = settings.Assignment;
        assignment.Enabled = true;
        assignment.RandomlyAssignedChance = 10;
        assignment.CanBeRandomlyAssigned = true;
        assignment.MaxChanceIfMeetRequirements = 60;
        assignment.MinLevel = 0;
        assignment.MaxLevel = 100;
        assignment.PowerLevelMin = 0;
        assignment.PowerLevelMax = 200;
        assignment.PowerLevelScaleStart = 0;
        assignment.PowerLevelScaleEnd = 200;
        assignment.InverseScale = true;

        var behavior = settings.Behavior;

        behavior.General.AggressionMultiplier = 1;
        behavior.General.HoldGroundBaseTime = 1f;
        behavior.General.HoldGroundMaxRandom = 1.5f;
        behavior.General.HoldGroundMinRandom = 0.75f;
        behavior.General.TARGET_SUPPRESS_TOGGLE = false;

        behavior.Cover.CanShiftCoverPosition = false;
        behavior.Cover.ShiftCoverTimeMultiplier = 1f;
        behavior.Cover.MoveToCoverHasEnemySpeed = 0.5f;
        behavior.Cover.MoveToCoverHasEnemyPose = 0.5f;
        behavior.Cover.MoveToCoverNoEnemySpeed = 0.5f;
        behavior.Cover.MoveToCoverNoEnemyPose = 1f;

        behavior.Talk.CanTaunt = false;
        behavior.Talk.CanRespondToEnemyVoice = false;
        behavior.Talk.TauntFrequency = 10;
        behavior.Talk.TauntChance = 0;
        behavior.Talk.TauntMaxDistance = 70f;
        behavior.Talk.FrequentTaunt = false;
        behavior.Talk.ConstantTaunt = false;
        behavior.Talk.CanFakeDeathRare = false;
        behavior.Talk.FakeDeathChance = 0;

        behavior.Search.WillSearchForEnemy = true;
        behavior.Search.WillSearchFromAudio = true;
        behavior.Search.WillChaseDistantGunshots = false;
        behavior.Search.SearchBaseTime = 240f;
        behavior.Search.SprintWhileSearchChance = 0f;
        behavior.Search.Sneaky = true;
        behavior.Search.SneakyPose = 0f;
        behavior.Search.SneakySpeed = 0f;
        behavior.Search.SearchWaitMultiplier = 1f;
        behavior.Search.HeardFromPeaceBehavior = EHeardFromPeaceBehavior.Freeze;

        behavior.Rush.CanRushEnemyReloadHeal = false;
        behavior.Rush.CanJumpCorners = false;
        behavior.Rush.JumpCornerChance = 0f;
        behavior.Rush.CanBunnyHop = false;
        behavior.Rush.BunnyHopChance = 0f;

        var allowedTypes = settings.Assignment.AllowedTypes;
        addAllTypes(allowedTypes);

        allowedTypes.Remove(WildSpawnType.arenaFighter);
        allowedTypes.Remove(WildSpawnType.exUsec);
        allowedTypes.Remove(WildSpawnType.pmcBot);

        Personalities.Add(personality, settings);
        if (Preset.Info.IsCustom == true)
        {
            SAINPresetClass.Export(settings, Preset.Info.Name, personality.ToString(), nameof(Personalities));
        }
    }

    private static void initTimmy(PersonalityDictionary Personalities, SAINPresetClass Preset)
    {
        EPersonality personality = EPersonality.Timmy;
        var settings = new PersonalitySettingsClass(personality);

        var assignment = settings.Assignment;
        assignment.Enabled = true;
        assignment.RandomlyAssignedChance = 0;
        assignment.CanBeRandomlyAssigned = false;
        assignment.MaxChanceIfMeetRequirements = 60f;
        assignment.MinLevel = 0;
        assignment.MaxLevel = 15;
        assignment.PowerLevelMin = 0;
        assignment.PowerLevelMax = 150f;
        assignment.PowerLevelScaleStart = 0;
        assignment.PowerLevelScaleEnd = 150;
        assignment.InverseScale = true;

        var behavior = settings.Behavior;

        behavior.General.AggressionMultiplier = 1;
        behavior.General.HoldGroundBaseTime = 0.5f;
        behavior.General.HoldGroundMaxRandom = 1.5f;
        behavior.General.HoldGroundMinRandom = 0.75f;
        behavior.General.TARGET_SUPPRESS_TOGGLE = true;
        behavior.General.TimeSinceSeenToSuppress = 0.5f;
        behavior.General.TimeSinceShotAtToSuppress = 30f;
        behavior.General.TimeSinceShotToSuppress = 60f;

        behavior.Cover.CanShiftCoverPosition = false;
        behavior.Cover.ShiftCoverTimeMultiplier = 0.5f;
        behavior.Cover.MoveToCoverHasEnemySpeed = 0.5f;
        behavior.Cover.MoveToCoverHasEnemyPose = 0.5f;
        behavior.Cover.MoveToCoverNoEnemySpeed = 0.5f;
        behavior.Cover.MoveToCoverNoEnemyPose = 1f;

        behavior.Talk.CanTaunt = false;
        behavior.Talk.CanRespondToEnemyVoice = false;
        behavior.Talk.TauntFrequency = 10;
        behavior.Talk.TauntMaxDistance = 70f;
        behavior.Talk.FrequentTaunt = false;
        behavior.Talk.ConstantTaunt = false;
        behavior.Talk.CanFakeDeathRare = false;
        behavior.Talk.FakeDeathChance = 0;
        behavior.Talk.CanBegForLife = true;

        behavior.Search.WillSearchForEnemy = true;
        behavior.Search.WillSearchFromAudio = false;
        behavior.Search.WillChaseDistantGunshots = false;
        behavior.Search.SearchBaseTime = 90f;
        behavior.Search.SprintWhileSearchChance = 20f;
        behavior.Search.Sneaky = false;
        behavior.Search.SneakyPose = 0f;
        behavior.Search.SneakySpeed = 0f;
        behavior.Search.SearchWaitMultiplier = 0.5f;
        behavior.Search.HeardFromPeaceBehavior = EHeardFromPeaceBehavior.Freeze;

        behavior.Rush.CanRushEnemyReloadHeal = false;
        behavior.Rush.CanJumpCorners = false;
        behavior.Rush.JumpCornerChance = 0f;
        behavior.Rush.CanBunnyHop = false;
        behavior.Rush.BunnyHopChance = 0f;

        var allowedTypes = settings.Assignment.AllowedTypes;
        addAllTypes(allowedTypes);

        allowedTypes.Remove(WildSpawnType.arenaFighter);
        allowedTypes.Remove(WildSpawnType.exUsec);
        allowedTypes.Remove(WildSpawnType.pmcBot);

        Personalities.Add(personality, settings);
        if (Preset.Info.IsCustom == true)
        {
            SAINPresetClass.Export(settings, Preset.Info.Name, personality.ToString(), nameof(Personalities));
        }
    }

    private static void initCoward(PersonalityDictionary Personalities, SAINPresetClass Preset)
    {
        EPersonality personality = EPersonality.Coward;
        var settings = new PersonalitySettingsClass(personality);

        var assignment = settings.Assignment;
        assignment.Enabled = true;
        assignment.RandomlyAssignedChance = 5;
        assignment.CanBeRandomlyAssigned = true;
        assignment.MaxChanceIfMeetRequirements = 30f;
        assignment.MinLevel = 0;
        assignment.MaxLevel = 100;
        assignment.PowerLevelMin = 0;
        assignment.PowerLevelMax = 250f;
        assignment.PowerLevelScaleStart = 0;
        assignment.PowerLevelScaleEnd = 250f;
        assignment.InverseScale = true;

        var behavior = settings.Behavior;

        behavior.General.AggressionMultiplier = 1;
        behavior.General.HoldGroundBaseTime = 0.25f;
        behavior.General.HoldGroundMaxRandom = 1.5f;
        behavior.General.HoldGroundMinRandom = 0.75f;
        behavior.General.TARGET_SUPPRESS_TOGGLE = true;
        behavior.General.TimeSinceSeenToSuppress = 2f;
        behavior.General.TimeSinceShotAtToSuppress = 2f;
        behavior.General.TimeSinceShotToSuppress = 2f;

        behavior.Cover.CanShiftCoverPosition = false;
        behavior.Cover.MoveToCoverHasEnemySpeed = 0.5f;
        behavior.Cover.MoveToCoverHasEnemyPose = 0.5f;
        behavior.Cover.MoveToCoverNoEnemySpeed = 0.5f;
        behavior.Cover.MoveToCoverNoEnemyPose = 1f;

        behavior.Talk.CanBegForLife = true;

        behavior.Search.WillSearchForEnemy = false;
        behavior.Search.WillSearchFromAudio = false;
        behavior.Search.WillChaseDistantGunshots = false;
        behavior.Search.HeardFromPeaceBehavior = EHeardFromPeaceBehavior.Freeze;

        var allowedTypes = settings.Assignment.AllowedTypes;
        addAllTypes(allowedTypes);

        allowedTypes.Remove(WildSpawnType.arenaFighter);
        allowedTypes.Remove(WildSpawnType.exUsec);
        allowedTypes.Remove(WildSpawnType.pmcBot);

        Personalities.Add(personality, settings);
        if (Preset.Info.IsCustom == true)
        {
            SAINPresetClass.Export(settings, Preset.Info.Name, personality.ToString(), nameof(Personalities));
        }
    }

    private static void initNormal(PersonalityDictionary Personalities, SAINPresetClass Preset)
    {
        EPersonality personality = EPersonality.Normal;
        var settings = new PersonalitySettingsClass(personality);

        var assignment = settings.Assignment;
        assignment.Enabled = true;
        assignment.RandomlyAssignedChance = 0;
        assignment.CanBeRandomlyAssigned = false;
        assignment.MaxChanceIfMeetRequirements = 50f;
        assignment.MinLevel = 0;
        assignment.MaxLevel = 100;
        assignment.PowerLevelMin = 0;
        assignment.PowerLevelMax = 1000f;
        assignment.PowerLevelScaleStart = 0;
        assignment.PowerLevelScaleEnd = 1000f;
        assignment.InverseScale = true;

        var behavior = settings.Behavior;

        behavior.General.AggressionMultiplier = 1;
        behavior.General.HoldGroundBaseTime = 1f;
        behavior.General.HoldGroundMaxRandom = 1.5f;
        behavior.General.HoldGroundMinRandom = 0.5f;

        behavior.Cover.CanShiftCoverPosition = true;
        behavior.Cover.ShiftCoverTimeMultiplier = 1f;
        behavior.Cover.MoveToCoverHasEnemySpeed = 0.75f;
        behavior.Cover.MoveToCoverHasEnemyPose = 1f;
        behavior.Cover.MoveToCoverNoEnemySpeed = 0.75f;
        behavior.Cover.MoveToCoverNoEnemyPose = 1f;

        behavior.Talk.CanRespondToEnemyVoice = true;
        behavior.Talk.TauntFrequency = 10;
        behavior.Talk.TauntMaxDistance = 50f;

        behavior.Search.WillSearchForEnemy = true;
        behavior.Search.WillSearchFromAudio = true;
        behavior.Search.WillChaseDistantGunshots = false;
        behavior.Search.SearchBaseTime = 60f;
        behavior.Search.SprintWhileSearchChance = 10f;
        behavior.Search.SearchWaitMultiplier = 1f;
        behavior.Search.HeardFromPeaceBehavior = EHeardFromPeaceBehavior.Freeze;

        var allowedTypes = settings.Assignment.AllowedTypes;
        addAllTypes(allowedTypes);

        Personalities.Add(personality, settings);
        if (Preset.Info.IsCustom == true)
        {
            SAINPresetClass.Export(settings, Preset.Info.Name, personality.ToString(), nameof(Personalities));
        }
    }

    private static void addPMCs(List<WildSpawnType> allowedTypes)
    {
        allowedTypes.Add(WildSpawnType.pmcUSEC);
        allowedTypes.Add(WildSpawnType.pmcBEAR);
    }

    private static void addAllTypes(List<WildSpawnType> allowedTypes)
    {
        allowedTypes.Clear();
        foreach (var botType in BotTypeDefinitions.BotTypes)
        {
            allowedTypes.Add(botType.Key);
        }
    }
}