
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
            new Patches.Generic.InitHelper().Enable();
            new Patches.Generic.GetDefaultBotController().Enable();
            new Patches.Generic.GrenadeThrownActionPatch().Enable();
            new Patches.Generic.GrenadeExplosionActionPatch().Enable();
            //new Patches.AddEnemyToAllGroupsInBotZonePatch().Enable();
            //new Patches.AddEnemyToAllGroupsPatch().Enable();
        }

        private static void ShootPatches()
        {
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
            new Patches.Movement.SteerDisablePatch().Enable();
            new Patches.Movement.StopMoveDisable().Enable();
            new Patches.Movement.KickPatch().Enable();
            new Patches.Movement.SprintDisable().Enable();
            new Patches.Movement.BotRunDisable().Enable();
            new Patches.Movement.TargetSpeedDisable1().Enable();
            new Patches.Movement.TargetSpeedDisable2().Enable();
            //new Patches.Movement.SetPoseDisable1().Enable();
            //new Patches.Movement.SetPoseDisable2().Enable();
            //new Patches.Movement.HoldOrCoverDisable1().Enable();
            //new Patches.Movement.HoldOrCoverDisable2().Enable();
        }

        private static void ComponentPatches()
        {
            new Patches.Components.AddComponentPatch().Enable();
            new Patches.Components.DisposeComponentPatch().Enable();
        }

        private static void VisionPatches()
        {
            new Patches.VisibleDistancePatch().Enable();
            new Patches.GainSightPatch().Enable();
            new Patches.CheckFlashlightPatch().Enable();
            new Patches.DazzlePatch().Enable();
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
            new Patches.BotGlobalLookPatch().Enable();
            new Patches.BotGlobalShootPatch().Enable();
            new Patches.BotGlobalGrenadePatch().Enable();
            new Patches.BotGlobalMindPatch().Enable();
            new Patches.BotGlobalMovePatch().Enable();
            new Patches.BotGlobalCorePatch().Enable();
            new Patches.BotGlobalAimPatch().Enable();
            new Patches.BotGlobalScatterPatch().Enable();
        }
    }
}
