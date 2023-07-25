
namespace SAIN
{
    internal class EFTPatches
    {
        public static void Init()
        {
            GenericPatches();
            HearingPatches();
            TalkPatches();
            VisionPatches();
            GlobalSettingsPatches();
            ShootPatches();
        }

        private static void GenericPatches()
        {
            new Patches.Movement.KickPatch().Enable();
            new Patches.BetterAudioPatch().Enable();

            new Patches.Generic.InitHelper().Enable();
            new Patches.Generic.GetBotController().Enable();
            new Patches.Generic.GetBotSpawnerClass().Enable();
            new Patches.Generic.GrenadeThrownActionPatch().Enable();
            new Patches.Generic.GrenadeExplosionActionPatch().Enable();

            new Patches.Generic.BotGroupAddEnemyPatch().Enable();
            new Patches.Generic.BotMemoryAddEnemyPatch().Enable();
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
        }

        private static void VisionPatches()
        {
            new Patches.NoAIESPPatch().Enable();
            new Patches.VisionSpeedPatch().Enable();
            new Patches.VisibleDistancePatch().Enable();
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
            new Patches.BotGlobalCore2Patch().Enable();
            new Patches.BotGlobalLookPatch().Enable();
            new Patches.BotGlobalShootPatch().Enable();
            new Patches.BotGlobalGrenadePatch().Enable();
            new Patches.BotGlobalMindPatch().Enable();
            new Patches.BotGlobalMovePatch().Enable();
            new Patches.BotGlobalAimPatch().Enable();
            new Patches.BotGlobalScatterPatch().Enable();
        }
    }
}
