using EFT;
using SAIN.Helpers;

namespace SAIN.BotPresets
{
    public sealed class BotType
    {
        public BotType(WildSpawnType type, string name, string section, string description)
        {
            OriginalName = type.ToString();
            Name = name;
            Description = description;
            Section = section;
            WildSpawnType = type;

            Preset = PresetHandler(WildSpawnType);
        }

        public BotType(string originalName, string name, string section, string description)
        {
            OriginalName = originalName;
            Name = name;
            Description = description;
            Section = section;
            WildSpawnType = PresetManager.GetType(originalName);

            Preset = PresetHandler(WildSpawnType);
        }

        private static BotPreset PresetHandler(WildSpawnType wildType)
        {
            BotPreset Preset = JsonUtility.Load.BotPreset(wildType);
            if (Preset == null)
            {
                Preset = new BotPreset(wildType);
                JsonUtility.Save.SavePreset(Preset);
            }
            return Preset;
        }

        public readonly BotPreset Preset;

        public string OriginalName { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Section { get; private set; }

        public WildSpawnType WildSpawnType { get; private set; }
    }
}
