using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories;

public class SAINMoveSettings : SAINSettingsBase<SAINMoveSettings>, ISAINSettings
{
    [Name("Strafe Speed")]
    [Description("How fast a bot will strafe when fighting an enemy at close range")]
    [MinMax(0, 1, 100)]
    public float STRAFE_SPEED = 0.5f;

    [Name("Can Lean")]
    [Description("Can this bot lean while peeking and while outside of cover?")]
    [Category("Movement Option Toggles")]
    public bool LEAN_TOGGLE = true;

    [Name("Can Lean in Cover")]
    [Description("Can this bot lean while in cover?")]
    [Category("Movement Option Toggles")]
    public bool LEAN_INCOVER_TOGGLE = true;

    [Name("Can Jump")]
    [Description("Can this bot Jump?")]
    [Category("Movement Option Toggles")]
    public bool JUMP_TOGGLE = true;

    [Name("Can Auto Pose")]
    [Description("Does this bot automatically adjust their crouch height depending on objects between them and their enemy?")]
    [Category("Movement Option Toggles")]
    public bool AUTOCROUCH_TOGGLE = true;

    [Name("Can Go Prone")]
    [Description("Can this bot go Prone at all?")]
    [Category("Movement Option Toggles")]
    public bool PRONE_TOGGLE = true;

    [Name("Can Go Prone from Suppression")]
    [Description("Can this bot go Prone as a panic response to being suppressed?")]
    [Category("Movement Option Toggles")]
    public bool PRONE_SUPPRESS_TOGGLE = true;

    [Name("Can Vault")]
    [Description("Can this bot Vault?")]
    [Category("Movement Option Toggles")]
    public bool VAULT_TOGGLE = true;

    [Name("Can Vault to get Unstuck")]
    [Description("Can this bot Vault if they are stuck on map geometry?")]
    [Category("Movement Option Toggles")]
    public bool VAULT_UNSTUCK_TOGGLE = true;
}