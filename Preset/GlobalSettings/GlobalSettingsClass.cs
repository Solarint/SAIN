using SAIN.Helpers;

namespace SAIN.Preset.GlobalSettings
{
    public class GlobalSettingsClass
    {
        public AimSettings Aiming = new AimSettings();

        public CoverSettings Cover = new CoverSettings();

        public ExtractSettings Extract = new ExtractSettings();

        public FlashlightSettings Flashlight = new FlashlightSettings();

        public GeneralSettings General = new GeneralSettings();

        public PersonalitySettings Personality = new PersonalitySettings();

        public HearingSettings Hearing = new HearingSettings();

        public ShootSettings Shoot = new ShootSettings();

        public VisionSettings Vision = new VisionSettings();

        public EFTCoreSettings EFTCoreSettings = new EFTCoreSettings();
    }
}