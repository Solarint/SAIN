using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories;

public class SAINMindSettings : SAINSettingsBase<SAINMindSettings>, ISAINSettings
{
    [Category("Personality")]
    [Name("Global Aggression Multiplier")]
    [Description("How quickly bots will move to search for enemies after losing sight, and how carefully they will search. Higher number equals higher aggression.")]
    [MinMax(0.01f, 3f, 10f)]
    public float Aggression = 1f;

    [Category("Weapon Control")]
    [Name("Weapon Proficiency")]
    [Description("How Well this bot can fire any weapon type, affects recoil, fire-rate, and burst length. Higher number equals harder bots.")]
    [Percentage01to99]
    public float WeaponProficiency = 0.5f;

    [Name("Suppression Resistance")]
    [Description("Higher = Less affected by suppression. A Value of 0 means No Resistance. " +
        "A Value of 1 means Full Resistance. " +
        "The final resistance number is the mid-point between their personality and bot type resistance. " +
        "So a value of 0.25 for personality and a value of 0.75 for bot type would result in 0.5")]
    [MinMax(0.0f, 1f, 100)]
    public float SuppressionResistance = 0f;

    [Category("Talk")]
    [Name("Talk Frequency")]
    [Description("How often to check if a bot wants to talk. Higher = More Delay between Talking.")]
    [MinMax(0f, 30f)]
    public float TalkFrequency = 1f;

    [Category("Talk")]
    public bool CanTalk = true;

    [Category("Talk")]
    public bool BotTaunts = true;

    [Category("Talk")]
    public bool SquadTalk = true;

    [Category("Talk")]
    [Name("Squad Talk Frequency. Higher = More Delay between Talking.")]
    [MinMax(0f, 60f)]
    public float SquadMemberTalkFreq = 3f;

    [Category("Talk")]
    [Name("Squad Leader Talk Frequency. Higher = More Delay between Talking.")]
    [MinMax(0f, 60f)]
    public float SquadLeadTalkFreq = 3f;

    [Category("Extract")]
    [Name("Enable Extracts")]
    public bool EnableExtracts = true;

    [Category("Extract")]
    [Name("Max Raid Percentage before Extract")]
    [Description("The longest possible time before this bot can decide to move to extract. Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent")]
    [MinMax(0f, 100f)]
    public float MaxExtractPercentage = 30f;

    [Category("Extract")]
    [Name("Min Raid Percentage before Extract")]
    [Description("The longest possible time before this bot can decide to move to extract. Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent")]
    [MinMax(0f, 100f)]
    public float MinExtractPercentage = 5f;

    public override void Apply(BotSettingsComponents settings)
    {
        settings.Mind.UNDER_FIRE_PERIOD = 5f;
        settings.Mind.CHANCE_FUCK_YOU_ON_CONTACT_100 = 0f;
        //settings.Mind.PART_PERCENT_TO_HEAL = 0.6f;
        //settings.Mind.SURGE_KIT_ONLY_SAFE_CONTAINER = false;
        settings.Mind.FOOD_DRINK_DELAY_SEC = 240f;
        settings.Mind.CAN_USE_MEDS = true;
        settings.Mind.CAN_USE_FOOD_DRINK = true;
        settings.Mind.HIT_DELAY_WHEN_PEACE = 0.4f;
        settings.Mind.HIT_DELAY_WHEN_HAVE_SMT = 0.1f;
    }
}