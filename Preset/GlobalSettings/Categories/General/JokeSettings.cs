using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class JokeSettings : SAINSettingsBase<JokeSettings>, ISAINSettings
{
    [Name("Random Cheater AI")]
    [Description("Emulate the real Live-Like experience! 1% of bots will be a cheater. They will move faster than they should, have 0 recoil, and perfect aim, always shoot full auto at any range if their weapon supports it, and always fire as fast as possible if they have a semi-auto weapon.")]
    public bool RandomCheaters = false;

    [Name("Random Speed Hacker Chance")]
    [Description("If for some reason you enabled random cheaters, this is the chance they will be assigned as one.")]
    [Percentage]
    public float RandomCheaterChance = 1f;
}