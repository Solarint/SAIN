using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.Personalities;

public class PersonalityRushSettings : SAINSettingsBase<PersonalityRushSettings>, ISAINSettings
{
    [Name("Can Rush Healing/Reloading/Grenade-Pulling Enemies")]
    public bool CanRushEnemyReloadHeal = false;

    [Name("Can Jump Push")]
    [Description("Can this personality jump when rushing an enemy?")]
    public bool CanJumpCorners = false;

    [Name("Jump Push Chance")]
    [Description("If a bot can Jump Push, this is the chance they will actually do it.")]
    [Percentage()]
    public float JumpCornerChance = 60f;

    [Name("Can Bunny Hop during Jump Push")]
    [Description("Can this bot hit a clip on you?")]
    public bool CanBunnyHop = false;

    [Name("Bunny Hop Chance")]
    [Description("If a bot can bunny hop, this is the chance they will actually do it.")]
    [Percentage()]
    public float BunnyHopChance = 5f;
}