using System;

namespace SAIN.Preset
{
    public sealed class SAINPresetDefinition
    {
        public string Name;
        public string Description;
        public string Creator;
        public readonly string SAINVersion = AssemblyInfo.SAINVersion;
        public readonly string DateCreated = DateTime.Today.ToString();
    }
}