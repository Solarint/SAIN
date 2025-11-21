using EFT;
using SAIN.Helpers;
using SAIN.Preset;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Plugin;

internal static class SAINDifficultyClass
{
    private const string PresetNameEasy = "Baby Bots";
    private const string PresetNameNormal = "Less Difficult";
    private const string PresetNameHard = "Default";
    private const string PresetNameHarderPMCs = "Default with Harder PMCs";
    private const string DefaultPresetDescription = "Bots are difficult but fair, the way SAIN was meant to played.";
    private const string PresetNameVeryHard = "I Like Pain";
    private const string PresetNameImpossible = "Death Wish";

    public static readonly Dictionary<SAINDifficulty, SAINPresetDefinition> DefaultPresetDefinitions = new();

    static SAINDifficultyClass()
    {
        DefaultPresetDefinitions.Add(
            SAINDifficulty.easy,
            SAINPresetDefinition.CreateDefaultDefinition(
                PresetNameEasy,
                SAINDifficulty.easy,
                "Bots react slowly and are incredibly inaccurate."));

        DefaultPresetDefinitions.Add(
            SAINDifficulty.lesshard,
            SAINPresetDefinition.CreateDefaultDefinition(
                PresetNameNormal,
                SAINDifficulty.lesshard,
                "Bots react more slowly, and are less accurate than usual."));

        DefaultPresetDefinitions.Add(
            SAINDifficulty.hard,
            SAINPresetDefinition.CreateDefaultDefinition(
                PresetNameHard,
                SAINDifficulty.hard,
                DefaultPresetDescription));

        DefaultPresetDefinitions.Add(
            SAINDifficulty.harderpmcs,
            SAINPresetDefinition.CreateDefaultDefinition(
                PresetNameHarderPMCs,
                SAINDifficulty.harderpmcs,
                "Default Settings, but PMCs are harder than normal."));

        DefaultPresetDefinitions.Add(
            SAINDifficulty.veryhard,
            SAINPresetDefinition.CreateDefaultDefinition(
                PresetNameVeryHard,
                SAINDifficulty.veryhard,
                "Bots react faster, are more accurate, and can see further."));

        DefaultPresetDefinitions.Add(
            SAINDifficulty.deathwish,
            SAINPresetDefinition.CreateDefaultDefinition(
                PresetNameImpossible,
                SAINDifficulty.deathwish,
                "Prepare To Die. Bots have almost no scatter, get less recoil from their weapon while shooting, are more accurate, and react deadly fast."));
    }

    public static SAINPresetClass GetDefaultPreset(SAINDifficulty difficulty)
    {
        SAINPresetClass result;
        switch (difficulty)
        {
            case SAINDifficulty.easy:
                result = SAINDifficultyClass.CreateEasyPreset();
                break;

            case SAINDifficulty.lesshard:
                result = SAINDifficultyClass.CreateNormalPreset();
                break;

            case SAINDifficulty.hard:
                result = SAINDifficultyClass.CreateBasePreset(SAINDifficulty.hard);
                break;

            case SAINDifficulty.harderpmcs:
                result = SAINDifficultyClass.CreateHarderPMCsPreset();
                break;

            case SAINDifficulty.veryhard:
                result = SAINDifficultyClass.CreateVeryHardPreset();
                break;

            case SAINDifficulty.deathwish:
                result = SAINDifficultyClass.CreateImpossiblePreset();
                break;

            default:
                return null;
        }
        return result;
    }

    private static SAINPresetClass CreateEasyPreset()
    {
        var preset = CreateBasePreset(SAINDifficulty.easy);

        var global = preset.GlobalSettings;
        global.Shoot.BOT_RECOIL_COEF = 3f;
        global.Difficulty.ScatteringCoef = 3f;
        global.Difficulty.PRECISION_SPEED_COEF = 0.33f;
        global.Difficulty.ACCURACY_SPEED_COEF = 3f;
        global.Difficulty.HearingDistanceCoef = 0.4f;
        global.Aiming.FasterCQBReactionsGlobal = false;
        global.Difficulty.VisibleDistCoef = 0.5f;
        global.Difficulty.GainSightCoef = 0.33f;

        foreach (var bot in preset.BotSettings.SAINSettings)
        {
            bot.Value.DifficultyModifier = Mathf.Clamp(bot.Value.DifficultyModifier * 0.5f, 0.01f, 2f).Round100();
            foreach (var setting in bot.Value.Settings)
            {
                setting.Value.Core.VisibleAngle = 120f;
                setting.Value.Shoot.FireratMulti *= 0.4f;
                setting.Value.Shoot.BurstMulti *= 0.5f;
                if (setting.Value.Aiming.MAX_AIM_TIME < 2f)
                {
                    setting.Value.Aiming.MAX_AIM_TIME = 2f;
                }
                if (setting.Value.Aiming.MAX_AIMING_UPGRADE_BY_TIME < 0.4f)
                {
                    setting.Value.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.4f;
                }
            }
        }
        foreach (var botsetting in preset.BotSettings.SAINSettings)
        {
            if (botsetting.Key.IsBossOrFollower())
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.4f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.5f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.55f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.65f;
            }
            if (botsetting.Key.IsPMC() || botsetting.Key == WildSpawnType.exUsec || botsetting.Key == WildSpawnType.pmcBot || botsetting.Key == WildSpawnType.arenaFighter || botsetting.Key == WildSpawnType.arenaFighterEvent)
            {
                var pmcSettings = botsetting.Value.Settings;

                var easy = pmcSettings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.4f;

                var normal = pmcSettings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.5f;

                var hard = pmcSettings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.55f;

                var impossible = pmcSettings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.75f;
            }
            if (botsetting.Key == WildSpawnType.assault || botsetting.Key == WildSpawnType.assaultGroup)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.4f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.45f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.45f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.5f;
            }
        }
        return preset;
    }

    private static SAINPresetClass CreateNormalPreset()
    {
        var preset = CreateBasePreset(SAINDifficulty.lesshard);

        var global = preset.GlobalSettings;
        global.Shoot.BOT_RECOIL_COEF = 1.6f;
        global.Difficulty.ScatteringCoef = 1.25f;
        global.Difficulty.PRECISION_SPEED_COEF = 0.75f;
        global.Difficulty.ACCURACY_SPEED_COEF = 1.25f;
        global.Difficulty.VisibleDistCoef = 0.75f;
        global.Difficulty.GainSightCoef = 0.75f;
        global.Difficulty.HearingDistanceCoef = 0.66f;
        global.Aiming.FasterCQBReactionsGlobal = false;

        foreach (var bot in preset.BotSettings.SAINSettings)
        {
            bot.Value.DifficultyModifier = Mathf.Clamp(bot.Value.DifficultyModifier * 0.85f, 0.01f, 2f).Round100();
            foreach (var setting in bot.Value.Settings)
            {
                setting.Value.Core.VisibleAngle = 150f;
                setting.Value.Shoot.FireratMulti *= 0.8f;
            }
        }

        foreach (var botsetting in preset.BotSettings.SAINSettings)
        {
            if (botsetting.Key.IsBossOrFollower())
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.5f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.65f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.8f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;

                easy.Aiming.AimCenterMass = false;
                normal.Aiming.AimCenterMass = false;
                hard.Aiming.AimCenterMass = false;
                impossible.Aiming.AimCenterMass = false;
            }
            if (botsetting.Key.IsPMC() || botsetting.Key == WildSpawnType.exUsec || botsetting.Key == WildSpawnType.pmcBot || botsetting.Key == WildSpawnType.arenaFighter || botsetting.Key == WildSpawnType.arenaFighterEvent)
            {
                var pmcSettings = botsetting.Value.Settings;

                var easy = pmcSettings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.4f;

                var normal = pmcSettings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.6f;

                var hard = pmcSettings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.7f;

                var impossible = pmcSettings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.9f;
            }
            if (botsetting.Key == WildSpawnType.assault || botsetting.Key == WildSpawnType.assaultGroup)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.35f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.45f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.5f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.65f;

                easy.Move.LEAN_TOGGLE = false;
                normal.Move.LEAN_TOGGLE = false;
                hard.Move.LEAN_TOGGLE = false;
                impossible.Move.LEAN_TOGGLE = false;
            }
        }
        return preset;
    }

    private static SAINPresetClass CreateBasePreset(SAINDifficulty difficulty)
    {
        var preset = new SAINPresetClass(difficulty);
        foreach (var botsetting in preset.BotSettings.SAINSettings)
        {
            var settings = botsetting.Value.Settings;
            var easy = settings[BotDifficulty.easy];
            var normal = settings[BotDifficulty.normal];
            var hard = settings[BotDifficulty.hard];
            var impossible = settings[BotDifficulty.impossible];

            easy.Move.LEAN_TOGGLE = true;
            normal.Move.LEAN_TOGGLE = true;
            hard.Move.LEAN_TOGGLE = true;
            impossible.Move.LEAN_TOGGLE = true;

            easy.Move.STRAFE_SPEED = 0.55f;
            normal.Move.STRAFE_SPEED = 0.75f;
            hard.Move.STRAFE_SPEED = 0.8f;
            impossible.Move.STRAFE_SPEED = 1f;

            easy.Core.VisibleAngle = 120f;
            normal.Core.VisibleAngle = 150f;
            hard.Core.VisibleAngle = 170f;
            impossible.Core.VisibleAngle = 180f;

            easy.Core.VisibleDistance = 150f;
            normal.Core.VisibleDistance = 225f;
            hard.Core.VisibleDistance = 250f;
            impossible.Core.VisibleDistance = 275f;

            easy.Aiming.AimCenterMass = true;
            normal.Aiming.AimCenterMass = true;
            hard.Aiming.AimCenterMass = true;
            impossible.Aiming.AimCenterMass = false;

            easy.Aiming.FasterCQBReactions = false;
            normal.Aiming.FasterCQBReactions = true;
            hard.Aiming.FasterCQBReactions = true;
            impossible.Aiming.FasterCQBReactions = true;

            easy.Aiming.AimForHead = false;
            normal.Aiming.AimForHead = false;
            hard.Aiming.AimForHead = false;
            impossible.Aiming.AimForHead = false;

            easy.Aiming.AimForHeadChance = 1;
            normal.Aiming.AimForHeadChance = 10f;
            hard.Aiming.AimForHeadChance = 33f;
            impossible.Aiming.AimForHeadChance = 50f;

            switch (botsetting.Key)
            {
                case WildSpawnType.gifter:
                    easy.Move.STRAFE_SPEED = 1f;
                    normal.Move.STRAFE_SPEED = 1f;
                    hard.Move.STRAFE_SPEED = 1f;
                    impossible.Move.STRAFE_SPEED = 1f;
                    easy.Aiming.AimForHead = true;
                    normal.Aiming.AimForHead = true;
                    hard.Aiming.AimForHead = true;
                    impossible.Aiming.AimForHead = true;
                    break;

                case WildSpawnType.pmcBEAR:
                case WildSpawnType.pmcUSEC:

                    easy.Move.LEAN_TOGGLE = true;
                    normal.Move.LEAN_TOGGLE = true;
                    hard.Move.LEAN_TOGGLE = true;
                    impossible.Move.LEAN_TOGGLE = true;

                    easy.Move.STRAFE_SPEED = 0.55f;
                    normal.Move.STRAFE_SPEED = 0.75f;
                    hard.Move.STRAFE_SPEED = 0.8f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    easy.Core.VisibleAngle = 150f;
                    normal.Core.VisibleAngle = 160f;
                    hard.Core.VisibleAngle = 170f;
                    impossible.Core.VisibleAngle = 180f;

                    easy.Core.VisibleDistance = 200f;
                    normal.Core.VisibleDistance = 225f;
                    hard.Core.VisibleDistance = 250f;
                    impossible.Core.VisibleDistance = 275f;

                    easy.Aiming.AimCenterMass = true;
                    normal.Aiming.AimCenterMass = true;
                    hard.Aiming.AimCenterMass = true;
                    impossible.Aiming.AimCenterMass = false;

                    break;

                case WildSpawnType.assault:
                case WildSpawnType.assaultGroup:
                case WildSpawnType.cursedAssault:
                case WildSpawnType.test:
                case WildSpawnType.crazyAssaultEvent:

                    easy.Move.STRAFE_SPEED = 0.5f;
                    normal.Move.STRAFE_SPEED = 0.55f;
                    hard.Move.STRAFE_SPEED = 0.6f;
                    impossible.Move.STRAFE_SPEED = 0.65f;

                    easy.Core.GainSightCoef = 1f;
                    normal.Core.GainSightCoef = 1f;
                    hard.Core.GainSightCoef = 1f;
                    impossible.Core.GainSightCoef = 1f;

                    easy.Core.VisibleAngle = 120f;
                    normal.Core.VisibleAngle = 135f;
                    hard.Core.VisibleAngle = 140f;
                    impossible.Core.VisibleAngle = 150f;

                    easy.Core.VisibleDistance = 100f;
                    normal.Core.VisibleDistance = 125f;
                    hard.Core.VisibleDistance = 150f;
                    impossible.Core.VisibleDistance = 200f;

                    easy.Difficulty.HearingDistanceCoef = 0.5f;
                    normal.Difficulty.HearingDistanceCoef = 0.65f;
                    hard.Difficulty.HearingDistanceCoef = 0.75f;
                    impossible.Difficulty.HearingDistanceCoef = 1f;

                    easy.Aiming.FasterCQBReactions = false;
                    normal.Aiming.FasterCQBReactions = false;
                    hard.Aiming.FasterCQBReactions = false;
                    impossible.Aiming.FasterCQBReactions = false;

                    easy.Move.LEAN_TOGGLE = false;
                    normal.Move.LEAN_TOGGLE = false;
                    hard.Move.LEAN_TOGGLE = false;
                    impossible.Move.LEAN_TOGGLE = false;

                    easy.Aiming.AimCenterMass = true;
                    normal.Aiming.AimCenterMass = true;
                    hard.Aiming.AimCenterMass = true;
                    impossible.Aiming.AimCenterMass = true;

                    break;

                case WildSpawnType.arenaFighter:
                case WildSpawnType.arenaFighterEvent:
                case WildSpawnType.pmcBot:
                case WildSpawnType.exUsec:

                    easy.Move.STRAFE_SPEED = 0.55f;
                    normal.Move.STRAFE_SPEED = 0.75f;
                    hard.Move.STRAFE_SPEED = 0.8f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    easy.Aiming.AimCenterMass = false;
                    normal.Aiming.AimCenterMass = false;
                    hard.Aiming.AimCenterMass = false;
                    impossible.Aiming.AimCenterMass = false;

                    break;

                case WildSpawnType.bossTest:
                case WildSpawnType.bossBoar:
                case WildSpawnType.bossBoarSniper:
                case WildSpawnType.bossSanitar:
                case WildSpawnType.bossGluhar:
                case WildSpawnType.bossBully:

                    easy.Move.STRAFE_SPEED = 0.55f;
                    normal.Move.STRAFE_SPEED = 0.75f;
                    hard.Move.STRAFE_SPEED = 0.8f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    easy.Aiming.AimCenterMass = false;
                    normal.Aiming.AimCenterMass = false;
                    hard.Aiming.AimCenterMass = false;
                    impossible.Aiming.AimCenterMass = false;

                    break;

                case WildSpawnType.bossKilla:
                case WildSpawnType.bossKillaAgro:
                case WildSpawnType.bossTagilla:
                case WildSpawnType.bossTagillaAgro:
                case WildSpawnType.bossKolontay:
                    
                    easy.Aiming.AimCenterMass = false;
                    normal.Aiming.AimCenterMass = false;
                    hard.Aiming.AimCenterMass = false;
                    impossible.Aiming.AimCenterMass = false;

                    easy.Shoot.FireratMulti = 2f;
                    normal.Shoot.FireratMulti = 2f;
                    hard.Shoot.FireratMulti = 2f;
                    impossible.Shoot.FireratMulti = 2f;

                    easy.Difficulty.ScatteringCoef = 0.5f;
                    normal.Difficulty.ScatteringCoef = 0.5f;
                    hard.Difficulty.ScatteringCoef = 0.5f;
                    impossible.Difficulty.ScatteringCoef = 0.5f;

                    break;

                case WildSpawnType.bossKojaniy:
                case WildSpawnType.bossPartisan:
                    
                    easy.Aiming.AimCenterMass = false;
                    normal.Aiming.AimCenterMass = false;
                    hard.Aiming.AimCenterMass = false;
                    impossible.Aiming.AimCenterMass = false;
                    easy.Difficulty.ScatteringCoef = 0.5f;
                    normal.Difficulty.ScatteringCoef = 0.5f;
                    hard.Difficulty.ScatteringCoef = 0.5f;
                    impossible.Difficulty.ScatteringCoef = 0.5f;

                    break;

                case WildSpawnType.followerTest:
                case WildSpawnType.followerBully:
                case WildSpawnType.followerGluharSnipe:
                case WildSpawnType.followerGluharScout:
                case WildSpawnType.followerGluharSecurity:
                case WildSpawnType.followerGluharAssault:
                case WildSpawnType.followerSanitar:
                case WildSpawnType.followerTagilla:
                case WildSpawnType.tagillaHelperAgro:
                case WildSpawnType.followerKojaniy:
                case WildSpawnType.followerBoar:
                case WildSpawnType.followerBoarClose1:
                case WildSpawnType.followerBoarClose2:
                case WildSpawnType.followerKolontayAssault:
                case WildSpawnType.followerKolontaySecurity:

                    easy.Move.STRAFE_SPEED = 0.55f;
                    normal.Move.STRAFE_SPEED = 0.75f;
                    hard.Move.STRAFE_SPEED = 0.8f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    easy.Aiming.AimCenterMass = false;
                    normal.Aiming.AimCenterMass = false;
                    hard.Aiming.AimCenterMass = false;
                    impossible.Aiming.AimCenterMass = false;

                    break;

                case WildSpawnType.sectantWarrior:
                case WildSpawnType.sectantPriest:
                case WildSpawnType.sectactPriestEvent:
                    easy.Move.STRAFE_SPEED = 1f;
                    normal.Move.STRAFE_SPEED = 1f;
                    hard.Move.STRAFE_SPEED = 1f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    easy.Aiming.AimCenterMass = false;
                    normal.Aiming.AimCenterMass = false;
                    hard.Aiming.AimCenterMass = false;
                    impossible.Aiming.AimCenterMass = false;
                    break;

                case WildSpawnType.bossKnight:
                case WildSpawnType.followerBigPipe:
                case WildSpawnType.followerBirdEye:

                    easy.Move.STRAFE_SPEED = 1f;
                    normal.Move.STRAFE_SPEED = 1f;
                    hard.Move.STRAFE_SPEED = 1f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    easy.Aiming.AimCenterMass = false;
                    normal.Aiming.AimCenterMass = false;
                    hard.Aiming.AimCenterMass = false;
                    impossible.Aiming.AimCenterMass = false;

                    easy.Aiming.AimForHead = true;
                    normal.Aiming.AimForHead = true;
                    hard.Aiming.AimForHead = true;
                    impossible.Aiming.AimForHead = true;

                    easy.Aiming.FasterCQBReactions = true;
                    normal.Aiming.FasterCQBReactions = true;
                    hard.Aiming.FasterCQBReactions = true;
                    impossible.Aiming.FasterCQBReactions = true;

                    easy.Difficulty.ScatteringCoef = 0.5f;
                    normal.Difficulty.ScatteringCoef = 0.5f;
                    hard.Difficulty.ScatteringCoef = 0.5f;
                    impossible.Difficulty.ScatteringCoef = 0.5f;

                    easy.Difficulty.AggressionCoef = 2f;
                    normal.Difficulty.AggressionCoef = 2f;
                    hard.Difficulty.AggressionCoef = 2f;
                    impossible.Difficulty.AggressionCoef = 2f;

                    easy.Aiming.AimForHeadChance = 15;
                    normal.Aiming.AimForHeadChance = 25f;
                    hard.Aiming.AimForHeadChance = 35f;
                    impossible.Aiming.AimForHeadChance = 50f;
                    break;

                case WildSpawnType.marksman:
                case WildSpawnType.bossZryachiy:
                case WildSpawnType.followerZryachiy:
                case WildSpawnType.peacefullZryachiyEvent:
                case WildSpawnType.ravangeZryachiyEvent:
                case WildSpawnType.shooterBTR:
                case WildSpawnType.spiritWinter:
                case WildSpawnType.spiritSpring:
                case WildSpawnType.peacemaker:
                case WildSpawnType.skier:
                case WildSpawnType.sectantPredvestnik:
                case WildSpawnType.sectantPrizrak:
                case WildSpawnType.sectantOni:
                case WildSpawnType.infectedAssault:
                case WildSpawnType.infectedPmc:
                case WildSpawnType.infectedCivil:
                case WildSpawnType.infectedLaborant:
                case WildSpawnType.infectedTagilla:
                    break;

                default:
                    break;
            }
        }
        return preset;
    }

    private static SAINPresetClass CreateHarderPMCsPreset()
    {
        var preset = CreateBasePreset(SAINDifficulty.harderpmcs);
        ApplyHarderPMCs(preset);
        return preset;
    }

    private static void ApplyHarderPMCs(SAINPresetClass preset)
    {
        var botSettings = preset.BotSettings;
        foreach (var botsetting in botSettings.SAINSettings)
        {
            if (botsetting.Key == WildSpawnType.pmcUSEC || botsetting.Key == WildSpawnType.pmcBEAR)
            {
                var pmcSettings = botsetting.Value.Settings;

                // Set for all difficulties
                foreach (var diff in pmcSettings.Values)
                {
                    //diff.Core.ScatteringPerMeter = 0.03f;
                    //diff.Core.ScatteringClosePerMeter = 0.080f;
                    diff.Mind.WeaponProficiency = 0.75f;
                    diff.Difficulty.ScatteringCoef = 0.66f;
                    diff.Difficulty.PRECISION_SPEED_COEF = 1.33f;
                    diff.Difficulty.ACCURACY_SPEED_COEF = 0.66f;
                    diff.Difficulty.GainSightCoef = 1.25f;
                    diff.Difficulty.VisibleDistCoef = 1.25f;
                    diff.Difficulty.AggressionCoef = 1.2f;
                }

                var easy = pmcSettings[BotDifficulty.easy];
                easy.Aiming.FasterCQBReactionsDistance = 20f;
                easy.Aiming.FasterCQBReactionsMinimum = 0.3f;
                easy.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.35f;
                easy.Aiming.MAX_AIM_TIME = 1.5f;
                easy.Aiming.BASE_HIT_AFFECTION_DELAY_SEC = 0.65f;
                easy.Core.VisibleDistance = 200f;

                var normal = pmcSettings[BotDifficulty.normal];
                normal.Aiming.FasterCQBReactionsDistance = 35f;
                normal.Aiming.FasterCQBReactionsMinimum = 0.25f;
                normal.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.4f;
                normal.Aiming.MAX_AIM_TIME = 1.35f;
                normal.Aiming.BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;
                normal.Core.VisibleDistance = 225f;

                var hard = pmcSettings[BotDifficulty.hard];
                hard.Aiming.FasterCQBReactionsDistance = 50f;
                hard.Aiming.FasterCQBReactionsMinimum = 0.2f;
                hard.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.2f;
                hard.Aiming.MAX_AIM_TIME = 1.15f;
                hard.Aiming.BASE_HIT_AFFECTION_DELAY_SEC = 0.35f;
                hard.Core.VisibleDistance = 250f;

                var impossible = pmcSettings[BotDifficulty.impossible];
                impossible.Aiming.FasterCQBReactionsDistance = 60f;
                impossible.Aiming.FasterCQBReactionsMinimum = 0.15f;
                impossible.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.15f;
                impossible.Aiming.MAX_AIM_TIME = 1.0f;
                impossible.Aiming.BASE_HIT_AFFECTION_DELAY_SEC = 0.25f;
                impossible.Core.VisibleDistance = 275f;

                easy.Aiming.AimCenterMass = false;
                normal.Aiming.AimCenterMass = false;
                hard.Aiming.AimCenterMass = false;
                impossible.Aiming.AimCenterMass = false;
            }
        }
    }

    private static SAINPresetClass CreateVeryHardPreset()
    {
        var preset = CreateBasePreset(SAINDifficulty.veryhard);

        var global = preset.GlobalSettings;
        global.Shoot.BOT_RECOIL_COEF = 0.75f;
        global.Difficulty.ScatteringCoef = 0.75f;
        global.Aiming.AimCenterMassGlobal = false;
        global.Difficulty.VisibleDistCoef = 1.25f;
        global.Difficulty.GainSightCoef = 1.25f;
        global.Difficulty.PRECISION_SPEED_COEF = 1.25f;
        global.Difficulty.ACCURACY_SPEED_COEF = 0.75f;

        ApplyHarderPMCs(preset);

        foreach (var bot in preset.BotSettings.SAINSettings)
        {
            bot.Value.DifficultyModifier = Mathf.Clamp(bot.Value.DifficultyModifier * 1.33f, 0.01f, 2f).Round100();
            foreach (var setting in bot.Value.Settings)
            {
                setting.Value.Core.VisibleAngle = 170f;
                setting.Value.Shoot.FireratMulti = 1.5f;
                setting.Value.Shoot.BurstMulti = 2f;
                setting.Value.Aiming.AimCenterMass = false;
            }
        }
        foreach (var botsetting in preset.BotSettings.SAINSettings)
        {
            if (botsetting.Key.IsBossOrFollower())
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.75f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.85f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.9f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
            if (botsetting.Key.IsPMC() || botsetting.Key == WildSpawnType.exUsec || botsetting.Key == WildSpawnType.pmcBot || botsetting.Key == WildSpawnType.arenaFighter || botsetting.Key == WildSpawnType.arenaFighterEvent)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.75f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.85f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.9f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
            if (botsetting.Key == WildSpawnType.assault || botsetting.Key == WildSpawnType.assaultGroup)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.65f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.7f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.75f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.9f;
            }
        }
        return preset;
    }

    private static SAINPresetClass CreateImpossiblePreset()
    {
        var preset = CreateBasePreset(SAINDifficulty.deathwish);

        var global = preset.GlobalSettings;
        global.Shoot.BOT_RECOIL_COEF = 0.5f;

        global.Difficulty.ScatteringCoef = 0.01f;
        global.Difficulty.VisibleDistCoef = 2f;
        global.Difficulty.GainSightCoef = 2f;
        global.Difficulty.PRECISION_SPEED_COEF = 3f;
        global.Difficulty.ACCURACY_SPEED_COEF = 0.1f;

        global.Aiming.AimCenterMassGlobal = false;
        global.Look.NotLooking.NotLookingToggle = false;

        foreach (var bot in preset.BotSettings.SAINSettings)
        {
            foreach (var setting in bot.Value.Settings)
            {
                setting.Value.Core.VisibleAngle = 180f;
                setting.Value.Shoot.FireratMulti = 3f;
                setting.Value.Shoot.BurstMulti = 3f;
                setting.Value.Aiming.AimCenterMass = false;
                setting.Value.Core.VisibleAngle = 180;
                setting.Value.Aiming.AimForHead = true;
                setting.Value.Aiming.AimForHeadChance = 66f;
            }
        }
        foreach (var botsetting in preset.BotSettings.SAINSettings)
        {
            if (botsetting.Key.IsBossOrFollower())
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.85f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.9f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 1f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
            if (botsetting.Key.IsPMC() || botsetting.Key == WildSpawnType.exUsec || botsetting.Key == WildSpawnType.pmcBot || botsetting.Key == WildSpawnType.arenaFighter || botsetting.Key == WildSpawnType.arenaFighterEvent)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.75f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.9f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 1.0f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
            if (botsetting.Key == WildSpawnType.assault || botsetting.Key == WildSpawnType.assaultGroup)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[BotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.65f;

                var normal = settings[BotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.75f;

                var hard = settings[BotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.9f;

                var impossible = settings[BotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
        }
        return preset;
    }
}