
namespace SAIN
{
    internal class EFTPatches
    {
        public static void Init()
        {
            GenericPatches();
            MovementPatches();
            ComponentPatches();
            HearingPatches();
            TalkPatches();
            VisionPatches();
            GlobalSettingsPatches();
            ShootPatches();
        }

        private static void GenericPatches()
        {
            new Patches.BetterAudioPatch().Enable();
            new Patches.Generic.InitHelper().Enable();
            new Patches.Generic.GetBotController().Enable();
            new Patches.Generic.GetBotSpawnerClass().Enable();
            new Patches.Generic.GrenadeThrownActionPatch().Enable();
            new Patches.Generic.GrenadeExplosionActionPatch().Enable();
        }

        private static void ShootPatches()
        {
            new Patches.AimTimePatch().Enable();
            new Patches.AimOffsetPatch().Enable();
            new Patches.RecoilPatch().Enable();
            new Patches.LoseRecoilPatch().Enable();
            new Patches.EndRecoilPatch().Enable();
            new Patches.FullAutoPatch().Enable();
            new Patches.SemiAutoPatch().Enable();
            new Patches.FiremodePatch().Enable();
        }

        private static void MovementPatches()
        {
            new Patches.Movement.KickPatch().Enable();
        }

        private static void ComponentPatches()
        {
            //new Patches.Components.AddComponentPatch().Enable();
            //new Patches.Components.DisposeComponentPatch().Enable();
        }

        private static void VisionPatches()
        {
            //new Patches.LookDisablePatch1().Enable();
            //new Patches.LookDisablePatch2().Enable();
            new Patches.VisibleDistancePatch().Enable();
            //new Patches.GainSightPatch().Enable();
            new Patches.CheckFlashlightPatch().Enable();
        }

        private static void HearingPatches()
        {
            new Patches.InitiateShotPatch().Enable();
            new Patches.TryPlayShootSoundPatch().Enable();
            new Patches.HearingSensorPatch().Enable();
        }

        private static void TalkPatches()
        {
            new Patches.PlayerTalkPatch().Enable();
            new Patches.TalkDisablePatch1().Enable();
            new Patches.TalkDisablePatch2().Enable();
            new Patches.TalkDisablePatch3().Enable();
            new Patches.TalkDisablePatch4().Enable();
        }

        private static void GlobalSettingsPatches()
        {
            new Patches.BotGlobalCore1().Enable();
            new Patches.BotGlobalLookPatch().Enable();
            new Patches.BotGlobalShootPatch().Enable();
            new Patches.BotGlobalGrenadePatch().Enable();
            new Patches.BotGlobalMindPatch().Enable();
            new Patches.BotGlobalMovePatch().Enable();
            new Patches.BotGlobalCore2Patch().Enable();
            new Patches.BotGlobalAimPatch().Enable();
            new Patches.BotGlobalScatterPatch().Enable();
        }
    }
}
