using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings;

public class LocationSettingsClass : SAINSettingsBase<LocationSettingsClass>, ISAINSettings
{
    [JsonConstructor]
    public LocationSettingsClass()
    {
        addNewLocations();
    }

    private void addNewLocations()
    {
        foreach (var type in EnumValues.GetEnum<ELocation>())
        {
            if (LocationSettings.ContainsKey(type))
                continue;

            if (type == ELocation.None || type == ELocation.Terminal || type == ELocation.Town)
            {
                continue;
            }
            LocationSettings.Add(type, new DifficultySettings());
        }
    }

    public DifficultySettings Current()
    {
        var gameworld = GameWorldComponent.Instance;
        if (gameworld == null || gameworld.Location == null)
        {
#if DEBUG
            Logger.LogError($"gameworld or location class null");
#endif
            return null;
        }
        if (LocationSettings.TryGetValue(gameworld.Location.Location, out var settings))
        {
            return settings;
        }
#if DEBUG
        Logger.LogError($"no settings for {gameworld.Location.Location}");
#endif
        return null;
    }

    [Name("Location Specific Modifiers")]
    [Description("These modifiers only apply to bots on the location they are assigned to. Applies to all bots equally.")]
    [MinMax(0.01f, 5f, 100f)]
    public Dictionary<ELocation, DifficultySettings> LocationSettings = new();

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}