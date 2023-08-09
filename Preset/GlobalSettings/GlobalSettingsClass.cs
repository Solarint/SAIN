using SAIN.Attributes;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings.Categories;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset.GlobalSettings
{
    public class GlobalSettingsClass
    {
        public static GlobalSettingsClass LoadGlobal(SAINPresetDefinition Preset)
        {
            string fileName = FileAndFolderNames[JsonUtilityEnum.GlobalSettings];
            string presetsFolder = FileAndFolderNames[JsonUtilityEnum.Presets];

            if (!Load.LoadObject(out GlobalSettingsClass result, fileName, presetsFolder, Preset.Name))
            {
                result = new GlobalSettingsClass
                {
                    EFTCoreSettings = EFTCoreSettings.GetCore(),
                    BigBrain = new BigBrainSettings(BigBrainSettings.DefaultBrains)
                };
            }

            EFTCoreSettings.UpdateCoreSettings(result.EFTCoreSettings);

            var brainSettings = result.BigBrain.BrainSettings;
            if (brainSettings == null || brainSettings.Count == 0)
            {
                result.BigBrain = new BigBrainSettings(BigBrainSettings.DefaultBrains);
            }

            SaveObjectToJson(result, fileName, presetsFolder, Preset.Name);

            return result;
        }

        public AimSettings Aiming = new AimSettings();

        public CoverSettings Cover = new CoverSettings();

        public ExtractSettings Extract = new ExtractSettings();

        public FlashlightSettings Flashlight = new FlashlightSettings();

        public GeneralSettings General = new GeneralSettings();

        public PersonalitySettings Personality = new PersonalitySettings();

        public MindSettings Mind = new MindSettings();

        public HearingSettings Hearing = new HearingSettings();

        public ShootSettings Shoot = new ShootSettings();

        [Advanced(AdvancedEnum.Hidden)]
        public VisionSettings Vision = new VisionSettings();

        [Advanced(AdvancedEnum.Hidden)]
        public BigBrainSettings BigBrain = new BigBrainSettings();

        [Advanced(AdvancedEnum.Hidden)]
        public EFTCoreSettings EFTCoreSettings = new EFTCoreSettings();
    }
}