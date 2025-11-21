using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class NotLookingSettings : SAINSettingsBase<NotLookingSettings>, ISAINSettings
{
    [Name("Bot Reaction and Accuracy Changes Toggle - Experimental")]
    [Section("Unseen Bot")]
    [Experimental]
    [Description("Experimental: Bots will have slightly reduced accuracy and vision speed if you are not looking in their direction. " +
        "So if a bot notices and starts shooting you while your back is turned, they will be less accurate and notice you more slowly.")]
    public bool NotLookingToggle = true;

    [Name("Bot Reaction and Accuracy Changes Time Limit")]
    [Section("Unseen Bot")]
    [Experimental]
    [Description("The Maximum Time that a bot can be shooting at you before the reduced spread not longer has an affect. " +
        "So if a bot is shooting at you from the back for X seconds, after that time it will no longer reduce their accuracy to give you a better chance to react.")]
    [MinMax(0.5f, 20f, 100f)]
    [Advanced]
    public float NotLookingTimeLimit = 4f;

    [Name("Bot Reaction and Accuracy Changes Angle")]
    [Section("Unseen Bot")]
    [Experimental]
    [Advanced]
    [Description("The Maximum Angle for the player to be considered looking at a bot.")]
    [MinMax(5f, 45f, 1f)]
    public float NotLookingAngle = 45f;

    [Name("Bot Reaction Multiplier When Out of Sight")]
    [Section("Unseen Bot")]
    [Experimental]
    [Description("How much to multiply bot vision speed by if you aren't looking at them when they notice you. Higher = More time before reacting.")]
    [MinMax(1f, 2f, 100f)]
    [Advanced]
    public float NotLookingVisionSpeedModifier = 1.1f;

    [Name("Bot Accuracy and Spread Increase When Out of Sight")]
    [Section("Unseen Bot")]
    [Experimental]
    [Description("How much additional random Spread to add to a bot's aim if the player isn't look at them." +
        " 1 means it will randomize in a 1 meter sphere around their original aim target in addition to existing random spread." +
        " Higher = More spread and less accurate bots.")]
    [MinMax(0.1f, 1.5f, 100f)]
    [Advanced]
    public float NotLookingAccuracyAmount = 0.33f;
}