using SAIN.Helpers;

namespace SAIN.Preset.Personalities;

public class PersonalityManagerClass : BasePreset
{
    public PersonalityManagerClass(SAINPresetClass preset) : base(preset)
    {
        import();
        PersonalityDefaultsClass.InitDefaults(PersonalityDictionary, Preset);
    }

    public void Init()
    {
        foreach (var settings in PersonalityDictionary.Values)
        {
            settings.Init();
        }
    }

    public void UpdateDefaults(PersonalityManagerClass replacementClass = null)
    {
        foreach (var settings in PersonalityDictionary)
        {
            var replacementSettings = replacementClass?.PersonalityDictionary[settings.Key];
            settings.Value.UpdateDefaults(replacementSettings);
        }
    }

    public void Update()
    {
        foreach (var settings in PersonalityDictionary.Values)
        {
            settings.Update();
        }
    }

    private void import()
    {
        if (!Preset.Info.IsCustom)
        {
            return;
        }

        foreach (var item in EnumValues.Personalities)
        {
            if (SAINPresetClass.Import(out PersonalitySettingsClass personality, Preset.Info.Name, item.ToString(), nameof(Personalities)))
            {
                PersonalityDictionary.Add(item, personality);
            }
        }
    }

    public void ResetAllToDefaults()
    {
        PersonalityDictionary.Remove(EPersonality.Wreckless);
        PersonalityDictionary.Remove(EPersonality.SnappingTurtle);
        PersonalityDictionary.Remove(EPersonality.GigaChad);
        PersonalityDictionary.Remove(EPersonality.Chad);
        PersonalityDictionary.Remove(EPersonality.Rat);
        PersonalityDictionary.Remove(EPersonality.Coward);
        PersonalityDictionary.Remove(EPersonality.Timmy);
        PersonalityDictionary.Remove(EPersonality.Normal);
        PersonalityDefaultsClass.InitDefaults(PersonalityDictionary, Preset);
    }

    public void ResetToDefault(EPersonality personality)
    {
        PersonalityDictionary.Remove(personality);
        PersonalityDefaultsClass.InitDefaults(PersonalityDictionary, Preset);
    }

    public PersonalityDictionary PersonalityDictionary = new();
}