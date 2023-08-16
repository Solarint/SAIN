using System;

namespace SAIN.Preset
{
    public sealed class SAINPresetDefinition
    {
        public string Name;
        public string Description;
        public string Creator;
        public string SAINVersion;
        public string DateCreated;

        public SAINPresetDefinition Clone()
        {
            return new SAINPresetDefinition()
            {
                Name = Name,
                Description = Description,
                Creator = Creator,
                SAINVersion = SAINVersion,
                DateCreated = DateCreated
            };
        }
    }
}