using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using static Movement.UserSettings.DogFightConfig;

namespace Movement.Patches
{
    public class MovementSpeed : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), "UpdateManual");
        }
        [PatchPostfix]
        public static void PatchPostfix(BotOwner __instance)
        {
            if (!MoveSpeedToggle.Value)
            {
                return;
            }

            BotOwner bot = __instance;

            float Speed;
            float Pose;
            float AimMove;

            // Bots Walk around slower if not in combat
            if (bot.Memory.IsPeace)
            {
                Speed = 0.75f;
                Pose = 1.0f;
                AimMove = 0.65f;

                // Slows down Scavs Even More
                if (bot.IsRole(WildSpawnType.assault) && ScavMoveSlowerToggle.Value)
                {
                    Speed = 0.5f;
                    Pose = 1.0f;
                    AimMove = 0.5f;
                }
            }
            else
            {
                Speed = 0.9f;
                Pose = 1.0f;
                AimMove = 0.75f;

                // Speed bots up in close quarters
                if (bot.Memory.GoalEnemy.Distance <= 20f)
                {
                    Speed = 1.0f;
                    Pose = 1.0f;
                    AimMove = 0.85f;
                }
            }

            // Bot Commands
            bot.SetTargetMoveSpeed(Speed);
            bot.Mover.SetTargetMoveSpeed(Speed);

            bot.GetPlayer.MovementContext.SetAimingSlowdown(false, AimMove);

            bot.SetPose(Pose);
            bot.GetPlayer.ChangePose(Pose);
        }
    }
}