using SAIN.Attributes;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings.Categories;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset.GlobalSettings
{
    public class GlobalSettingsClass
    {
        public static GlobalSettingsClass ImportGlobalSettings(SAINPresetDefinition Preset)
        {
            string fileName = FileAndFolderNames[JsonEnum.GlobalSettings];
            string presetsFolder = FileAndFolderNames[JsonEnum.Presets];

            if (!Load.LoadObject(out GlobalSettingsClass result, fileName, presetsFolder, Preset.Name))
            {
                result = new GlobalSettingsClass
                {
                    EFTCoreSettings = EFTCoreSettings.GetCore(),
                    //BigBrain = new BigBrainSettings(BigBrainSettings.DefaultBrains)
                };
                SaveObjectToJson(result, fileName, presetsFolder, Preset.Name);
            }

            EFTCoreSettings.UpdateCoreSettings(result.EFTCoreSettings);

            //var brainSettings = result.BigBrain.BrainSettings;
            //if (brainSettings == null || brainSettings.Count == 0)
            //{
            //result.BigBrain = new BigBrainSettings(BigBrainSettings.DefaultBrains);
            //SaveObjectToJson(result, fileName, presetsFolder, Preset.Value);
            //}

            SaveObjectToJson(result, fileName, presetsFolder, Preset.Name);

            return result;
        }

        [Name("Global Aiming Settings")]
        public AimSettings Aiming = new AimSettings();

        [Name("Global Cover Settings")]
        public CoverSettings Cover = new CoverSettings();

        [Name("Global Extract Settings")]
        public ExtractSettings Extract = new ExtractSettings();

        [Name("Global Flashlight Settings")]
        public FlashlightSettings Flashlight = new FlashlightSettings();

        [Name("Global General Settings")]
        public GeneralSettings General = new GeneralSettings();

        [Name("Global Personality Settings")]
        public PersonalitySettings Personality = new PersonalitySettings();

        [Name("Global Mind Settings")]
        public MindSettings Mind = new MindSettings();

        [Name("Global Hearing Settings")]
        public HearingSettings Hearing = new HearingSettings();

        [Name("Global Shoot Settings")]
        public ShootSettings Shoot = new ShootSettings();

        [Name("Global Look Settings")]
        public LookSettings Look = new LookSettings();

        //[Advanced(AdvancedEnum.Hidden)]
        //public BigBrainSettings BigBrain = new BigBrainSettings();

        [Advanced(AdvancedEnum.Hidden)]
        public EFTCoreSettings EFTCoreSettings = new EFTCoreSettings();
    }
}