using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.Personalities;

public class PersonalityTalkSettings : SAINSettingsBase<PersonalityTalkSettings>, ISAINSettings
{
    [Name("Can Yell Taunts")]
    [Description("Hey you...yeah YOU! FUCK YOU! You heard?")]
    public bool CanTaunt = false;

    [Name("Can Yell Taunts Frequently")]
    [Description("HEY COCKSUCKAAAA")]
    public bool FrequentTaunt = false;

    [Name("Can Yell Taunts Constantly")]
    [Description("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public bool ConstantTaunt = false;

    [Description("Will this personality yell back at enemies taunting them")]
    public bool CanRespondToEnemyVoice = true;

    [Advanced]
    [MinMax(0.1f, 100f, 100f)]
    public float TauntFrequency = 15f;

    [Advanced]
    [MinMax(0f, 100f, 1f)]
    public float TauntChance = 50f;

    [Advanced]
    [MinMax(0.1f, 150f, 100f)]
    public float TauntMaxDistance = 50f;

    [Advanced]
    public bool CanFakeDeathRare = false;

    [Advanced]
    public float FakeDeathChance = 2f;

    [Advanced]
    public bool CanBegForLife = false;
}