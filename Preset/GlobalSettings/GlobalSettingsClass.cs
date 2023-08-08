using SAIN.Helpers;
using SAIN.Preset.GlobalSettings.Categories;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset.GlobalSettings
{
    public class GlobalSettingsClass
    {
        public static GlobalSettingsClass LoadGlobalSettings(SAINPresetDefinition Preset)
        {
            string fileName = "GlobalSettings";
            string presetsFolder = "Presets";
            string presetNameFolder = Preset.Name;
            if (!Load.LoadObject(out GlobalSettingsClass result, fileName, presetsFolder, presetNameFolder))
            {
                result = new GlobalSettingsClass
                {
                    EFTCoreSettings = EFTCoreSettings.GetCore()
                };

                Save.SaveJson(result, fileName, presetsFolder, presetNameFolder);
            }
            else
            {
                EFTCoreSettings.UpdateCoreSettings(result.EFTCoreSettings);
            }

            return result;
        }

        public AimSettings Aiming = new AimSettings();

        public BigBrainSettings BigBrain = new BigBrainSettings();

        public CoverSettings Cover = new CoverSettings();

        public ExtractSettings Extract = new ExtractSettings();

        public FlashlightSettings Flashlight = new FlashlightSettings();

        public GeneralSettings General = new GeneralSettings();

        public PersonalitySettings Personality = new PersonalitySettings();

        public MindSettings Mind = new MindSettings();

        public HearingSettings Hearing = new HearingSettings();

        public ShootSettings Shoot = new ShootSettings();

        public VisionSettings Vision = new VisionSettings();

        public EFTCoreSettings EFTCoreSettings = new EFTCoreSettings();
    }
}