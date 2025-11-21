using SAIN.Preset.BotSettings.SAINSettings.Categories;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.Personalities;

namespace SAIN.Preset.BotSettings.SAINSettings;

public class SAINSettingsClass : SettingsGroupBase<SAINSettingsClass>
{
    public DifficultySettings Difficulty = new();
    public SAINCoreSettings Core = new();
    public SAINAimingSettings Aiming = new();
    public SAINBossSettings Boss = new();
    public SAINChangeSettings Change = new();
    public SAINGrenadeSettings Grenade = new();
    public SAINHearingSettings Hearing = new();
    public SAINLaySettings Lay = new();
    public SAINLookSettings Look = new();
    public SAINMindSettings Mind = new();
    public SAINMoveSettings Move = new();
    public SAINPatrolSettings Patrol = new();
    public SAINScatterSettings Scattering = new();
    public SAINShootSettings Shoot = new();

    public override void InitList()
    {
        SettingsList.Clear();
        SettingsList.Add(Difficulty);
        SettingsList.Add(Core);
        SettingsList.Add(Aiming);
        SettingsList.Add(Boss);
        SettingsList.Add(Change);
        SettingsList.Add(Grenade);
        SettingsList.Add(Hearing);
        SettingsList.Add(Lay);
        SettingsList.Add(Look);
        SettingsList.Add(Mind);
        SettingsList.Add(Patrol);
        SettingsList.Add(Scattering);
        SettingsList.Add(Shoot);
    }
}