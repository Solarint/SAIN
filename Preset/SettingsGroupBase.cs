using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;
using System.Collections.Generic;

namespace SAIN.Preset.Personalities;

public abstract class SettingsGroupBase<T> : ISettingsGroup
{
    [JsonIgnore]
    [Hidden]
    public List<ISAINSettings> SettingsList { get; } = new List<ISAINSettings>();

    public virtual void InitList()
    {
        if (!initialized)
        {
            initialized = true;
        }
    }

    public virtual void Init()
    {
        InitList();
        CreateDefaults();
        Update();
    }

    public void Update()
    {
        foreach (var item in SettingsList)
        {
            item.Update();
        }
    }

    protected bool initialized;

    public void CreateDefaults()
    {
        foreach (var item in SettingsList)
        {
            item.CreateDefault();
        }
    }

    public void UpdateDefaults(ISettingsGroup replacementGroup = null)
    {
        if (replacementGroup == null)
        {
            foreach (var item in SettingsList)
            {
                item.UpdateDefaults(item);
            }
            return;
        }

        replacementGroup.InitList();
        for (int i = 0; i < SettingsList.Count; i++)
        {
            var item = SettingsList[i];
            var replacement = replacementGroup.SettingsList[i];
            item.UpdateDefaults(replacement);
        }
    }
}