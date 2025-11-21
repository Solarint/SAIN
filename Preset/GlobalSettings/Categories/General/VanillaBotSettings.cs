using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class VanillaBotSettings : SAINSettingsBase<VanillaBotSettings>, ISAINSettings
{
    [Name("Vanilla Scavs")]
    [Description("REQUIRES RESTART OF GAME. Non Player-Scavs will have vanilla ai behavior. Disabling sain for player scavs is not currently possible.")]
    public bool VanillaScavs = false;

    [Name("Vanilla Bosses")]
    [Description("REQUIRES RESTART OF GAME. Bosses other than the goons will have vanilla ai behavior.")]
    public bool VanillaBosses = false;

    [Name("Vanilla Boss Followers")]
    [Description("REQUIRES RESTART OF GAME. Boss Followers other than the goons will have vanilla ai behavior.")]
    public bool VanillaFollowers = false;

    [Name("Vanilla Goons")]
    [Description("REQUIRES RESTART OF GAME. Goons will have vanilla behavior. This disables custom personality edits specially made for the goons and I will be very sad.")]
    public bool VanillaGoons = false;

    [Name("Vanilla Bloodhounds")]
    [Description("REQUIRES RESTART OF GAME")]
    public bool VanillaBloodHounds = false;

    [Name("Vanilla Rogues")]
    [Description("REQUIRES RESTART OF GAME")]
    public bool VanillaRogues = false;

    [Name("Vanilla Cultists")]
    [Description("REQUIRES RESTART OF GAME")]
    public bool VanillaCultists = false;
}