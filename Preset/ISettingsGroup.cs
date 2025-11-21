using SAIN.Preset.GlobalSettings;
using System.Collections.Generic;

namespace SAIN.Preset.Personalities;

public interface ISettingsGroup
{
    void Init();
    void Update();
    List<ISAINSettings> SettingsList { get; }
    void InitList();
    void CreateDefaults();
    void UpdateDefaults(ISettingsGroup replacementValues = null);
}