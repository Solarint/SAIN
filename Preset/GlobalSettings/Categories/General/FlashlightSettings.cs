using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class FlashlightSettings : SAINSettingsBase<FlashlightSettings>, ISAINSettings
{
    [MinMax(0.25f, 10f, 100f)]
    public float DazzleEffectiveness = 3f;

    [MinMax(0f, 60f)]
    public float MaxDazzleRange = 40f;

    public bool AllowLightOnForDarkBuildings = true;

    public bool TurnLightOffNoEnemyPMC = true;

    public bool TurnLightOffNoEnemySCAV = false;

    public bool TurnLightOffNoEnemyGOONS = true;

    public bool TurnLightOffNoEnemyBOSS = false;

    public bool TurnLightOffNoEnemyFOLLOWER = false;

    public bool TurnLightOffNoEnemyRAIDERROGUE = false;

    [Advanced]
    public bool DebugFlash = false;

    public bool SillyMode = false;
}