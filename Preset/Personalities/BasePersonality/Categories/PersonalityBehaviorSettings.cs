namespace SAIN.Preset.Personalities;

public class PersonalityBehaviorSettings : SettingsGroupBase<PersonalityBehaviorSettings>, ISettingsGroup
{
    public PersonalityGeneralSettings General = new();
    public PersonalitySearchSettings Search = new();
    public PersonalityRushSettings Rush = new();
    public PersonalityCoverSettings Cover = new();
    public PersonalityTalkSettings Talk = new();

    public override void InitList()
    {
        SettingsList.Clear();
        SettingsList.Add(Cover);
        SettingsList.Add(General);
        SettingsList.Add(Rush);
        SettingsList.Add(Search);
        SettingsList.Add(Talk);
    }
}