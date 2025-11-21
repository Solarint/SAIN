using System.Collections.Generic;

namespace SAIN.Preset.Personalities;

public static class PersonalityDescriptionsClass
{
    public static void Import()
    {
    }

    public static Dictionary<EPersonality, string> PersonalityDescriptions { get; private set; } = new Dictionary<EPersonality, string>
    {
        {
                EPersonality.Normal,
                "An Average Tarkov Enjoyer"
            },
        {
                EPersonality.GigaChad,
                "A true alpha threat. Hyper Aggressive and typically wearing high tier equipment."
            },
        {
                EPersonality.Wreckless,
                "This personality tends to sprint at their enemies, and will very frequently scream at everyone - Usually both at the same time. More Aggressive than Gigachads."
            },
        {
                EPersonality.SnappingTurtle,
                "A player who finds the balance between rat and chad, yin and yang. Will rat you out but can spring out at any moment."
            },
        {
                EPersonality.Chad,
                "An aggressive player. Typically wearing high tier equipment, and is more aggressive than usual."
            },
        {
                EPersonality.Rat,
                "Scum of Tarkov. Rarely Seeks out enemies, and when they do - they will crab walk all the way there"
            },
        {
                EPersonality.Timmy,
                "A New Player, terrified of everything."
            },
        {
                EPersonality.Coward,
                "A player who is more passive and afraid than usual. Will never seek out enemies and will hide in a closet until the scary thing goes away."
            },
    };
}