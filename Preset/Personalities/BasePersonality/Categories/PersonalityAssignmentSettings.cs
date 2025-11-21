using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;
using System.Collections.Generic;

namespace SAIN.Preset.Personalities;

public class PersonalityAssignmentSettings : SAINSettingsBase<PersonalityAssignmentSettings>, ISAINSettings
{
    [JsonIgnore]
    [Hidden]
    private const string PowerLevelDescription = " Power level is a combined number that takes into account " +
        "armor, the class of that armor, " +
        "the attachments a bot has on their weapon, " +
        "whether they have a faceshield, " +
        "and the weapon class that is currently used by a bot." +
        " Power Level usually falls within 0 to 250 on average, and almost never goes above 500";

    [Name("Personality Enabled")]
    [Description("Enables or Disables this Personality, if a All Chads, All GigaChads, or AllRats is enabled in global settings, this value is ignored")]
    public bool Enabled = true;

    [NameAndDescription("Can Be Randomly Assigned", "A percentage chance that this personality can be applied to any bot, regardless of bot stats, power, player level, or anything else.")]
    public bool CanBeRandomlyAssigned = true;

    [NameAndDescription("Randomly Assigned Chance", "If personality can be randomly assigned, this is the chance that will happen")]
    [MinMax(0, 100)]
    public float RandomlyAssignedChance = 3;

    [NameAndDescription("Minimum Level", "The min level that a bot can be to be eligible for this personality.")]
    [Percentage]
    public float MinLevel = 0;

    [NameAndDescription("Max Level", "The max level that a bot can be to be eligible for this personality.")]
    [Percentage]
    public float MaxLevel = 100;

    [Name("Power Level Scale Start")]
    [Description("When a bot is at, or above this power level, they will start to have a chance to be assigned this personality.")]
    [MinMax(0, 1000, 1)]
    public float PowerLevelScaleStart = 0f;

    [Name("Power Level Scale End")]
    [Description("When a bot is at, or above this power level, they will have the full percentage chance to be assigned this personality.")]
    [MinMax(0, 1000, 1)]
    public float PowerLevelScaleEnd = 500f;

    [Description("The lower the power level, the higher the chance")]
    public bool InverseScale = false;

    [NameAndDescription("Power Level Minimum", "Minimum Power level for a bot to use this personality." + PowerLevelDescription)]
    [MinMax(0, 1000, 1)]
    public float PowerLevelMin = 0;

    [NameAndDescription("Power Level Maximum", "Maximum Power level for a bot to use this personality." + PowerLevelDescription)]
    [MinMax(0, 1000, 1)]
    public float PowerLevelMax = 800;

    [Name("Maximum Chance If Meets Requirements")]
    [Description("If the bot meets all conditions for this personality, this is the chance the personality will actually be assigned. " +
        "The percentage chance to be assigned scales if a bots power level falls between Power Level Scale Start and Power Level Scale End, " +
        "so if they fall right in the middle, and the value here is 60%, they will have a 30% chance to be assigned.")]
    [MinMax(0, 100, 1)]
    public float MaxChanceIfMeetRequirements = 50;

    [Name("Allowed Bot Types")]
    [Advanced]
    [Hidden]
    public List<WildSpawnType> AllowedTypes = new();
}