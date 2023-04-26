using Aki.Reflection.Patching;
using EFT;
using System.Reflection;

namespace SAIN_Audio.Patches
{
    public class StepSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("MovementContext")?.PropertyType?.GetMethod("method_1");
        }
        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return true;
        }
    }

    public class JumpSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("MovementContext")?.PropertyType?.GetMethod("method_2");
        }
        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return true;
        }
    }

    public class DoorSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("MovementContext")?.PropertyType?.GetMethod("method_17");
        }
        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return true;
        }
    }

    public class DoorBreachSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("MovementContext")?.PropertyType?.GetMethod("PlayBreachSound");
        }
        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return true;
        }
    }
}
