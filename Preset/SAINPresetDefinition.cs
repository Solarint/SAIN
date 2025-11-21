using SAIN.Plugin;
using System;

namespace SAIN.Preset;

public sealed class SAINPresetDefinition
{
    public string Name;
    public string Description;
    public string Creator;
    public string SAINVersion;
    public string SAINPresetVersion;
    public string DateCreated;
    public SAINDifficulty BaseSAINDifficulty = SAINDifficulty.hard;
    public bool IsCustom = true;
    public bool CanEditName = true;

    public SAINPresetDefinition Clone()
    {
        return new SAINPresetDefinition()
        {
            Name = Name,
            Description = Description,
            Creator = "None",
            SAINVersion = AssemblyInfoClass.SAINVersion,
            SAINPresetVersion = AssemblyInfoClass.SAINPresetVersion,
            DateCreated = DateTime.Now.ToString(),
            IsCustom = true,
            BaseSAINDifficulty = BaseSAINDifficulty,
        };
    }

    public static SAINPresetDefinition CreateDefaultDefinition(string difficulty, SAINDifficulty baseDifficulty, string description = null)
    {
        return new SAINPresetDefinition
        {
            Name = difficulty,
            Description = description ?? $"The Default {difficulty} SAIN Preset.",
            Creator = "Solarint",
            SAINVersion = AssemblyInfoClass.SAINVersion,
            SAINPresetVersion = AssemblyInfoClass.SAINPresetVersion,
            DateCreated = DateTime.Now.ToString(),
            IsCustom = false,
            CanEditName = false,
            BaseSAINDifficulty = baseDifficulty,
        };
    }
}