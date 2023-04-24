using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using static SAIN.Movement.Config.DogFighterConfig;

namespace SAIN.Movement.Patches
{
    public class MovementSpeed : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // AccessTools lets us get methods easily
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
/*
    public class MovementSpeedPatch1 : ModulePatch
    {
        static float VisibleTimer = 0f;
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass406)?.GetMethod("GoToByWay", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner ___botOwner_0)
        {
            if (VisibleTimer < Time.time)
            {
                VisibleTimer = Time.time + 0.25f;

                if (___botOwner_0.Memory.HaveEnemy && !___botOwner_0.Memory.GoalEnemy.CanShoot && ___botOwner_0.Memory.GoalEnemy.PersonalLastSeenTime > Time.time + 5f)
                {
                    ___botOwner_0.SetTargetMoveSpeed(0.1f);
                    ___botOwner_0.Mover.SetTargetMoveSpeed(0.1f);
                    ___botOwner_0.SetPose(0.1f);
                    ___botOwner_0.GetPlayer.ChangePose(0.1f);
                    ___botOwner_0.GetPlayer.MovementContext.SetAimingSlowdown(false, 1f);
                    //Logger.LogInfo($"Movement: [{___botOwner_0.name}] Enemy Visible but not Shootable, Rat mode engaged");
                    return;
                }
                //Move speed is set to 0.6 by default, this changes that to 1.0 (max walk speed)
                if (___botOwner_0.AimingData.LastDist2Target < 20f || !___botOwner_0.WeaponManager.HaveBullets)
                {
                    ___botOwner_0.SetTargetMoveSpeed(1.0f);
                    ___botOwner_0.Mover.SetTargetMoveSpeed(1.0f);
                    ___botOwner_0.SetPose(1.0f);
                    ___botOwner_0.GetPlayer.ChangePose(1.0f);
                    ___botOwner_0.GetPlayer.MovementContext.SetAimingSlowdown(false, 1f);
                    //Logger.LogInfo($"Movement: [{___botOwner_0.name}] Enemy Close!");
                    return;
                }
                if (___botOwner_0.AimingData.LastDist2Target > 40f && ___botOwner_0.Memory.GoalEnemy.CanShoot)
                {
                    ___botOwner_0.SetTargetMoveSpeed(0.6f);
                    ___botOwner_0.Mover.SetTargetMoveSpeed(0.6f);
                    ___botOwner_0.SetPose(0.6f);
                    ___botOwner_0.GetPlayer.ChangePose(0.6f);
                    ___botOwner_0.GetPlayer.MovementContext.SetAimingSlowdown(true, 0.6f);
                    //Logger.LogInfo($"Movement: [{___botOwner_0.name}] Enemy Visible but is far away, so I'm aiming");
                    return;
                }
            }
        }
    }
    public class MovementSpeedPatch2 : ModulePatch
    {
        static float VisibleTimer = 0f;
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass327)?.GetMethod("ManualUpdate", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner ___botOwner_0)
        {
            if (VisibleTimer < Time.time)
            {
                VisibleTimer = Time.time + 0.25f;
                if (___botOwner_0.Memory.HaveEnemy && !___botOwner_0.Memory.GoalEnemy.CanShoot && ___botOwner_0.Memory.GoalEnemy.PersonalLastSeenTime > Time.time + 5f)
                {
                    ___botOwner_0.SetTargetMoveSpeed(0.1f);
                    ___botOwner_0.Mover.SetTargetMoveSpeed(0.1f);
                    ___botOwner_0.SetPose(0.1f);
                    ___botOwner_0.GetPlayer.ChangePose(0.1f);
                    ___botOwner_0.GetPlayer.MovementContext.SetAimingSlowdown(false, 1f);
                    //Logger.LogInfo($"Movement: [{___botOwner_0.name}] Enemy Visible but not Shootable, Rat mode engaged");
                    return;
                }
                //Move speed is set to 0.6 by default, this changes that to 1.0 (max walk speed)
                if (___botOwner_0.AimingData.LastDist2Target < 20f || !___botOwner_0.WeaponManager.HaveBullets)
                {
                    ___botOwner_0.SetTargetMoveSpeed(1.0f);
                    ___botOwner_0.Mover.SetTargetMoveSpeed(1.0f);
                    ___botOwner_0.SetPose(1.0f);
                    ___botOwner_0.GetPlayer.ChangePose(1.0f);
                    ___botOwner_0.GetPlayer.MovementContext.SetAimingSlowdown(false, 1f);
                    //Logger.LogInfo($"Movement: [{___botOwner_0.name}] Enemy Close!");
                    return;
                }
                if (___botOwner_0.AimingData.LastDist2Target > 30f && ___botOwner_0.Memory.GoalEnemy.CanShoot)
                {
                    ___botOwner_0.SetTargetMoveSpeed(0.6f);
                    ___botOwner_0.Mover.SetTargetMoveSpeed(0.6f);
                    ___botOwner_0.SetPose(0.6f);
                    ___botOwner_0.GetPlayer.ChangePose(0.6f);
                    ___botOwner_0.GetPlayer.MovementContext.SetAimingSlowdown(true, 0.6f);
                    //Logger.LogInfo($"Movement: [{___botOwner_0.name}] Enemy Visible but is far away, so I'm aiming");
                    return;
                }
            }
        }
    }
    public class MovementSpeedPatch3 : ModulePatch
    {
        static float VisibleTimer = 0f;
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass473)?.GetMethod("CheckLookEnemy", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner ___botOwner_0)
        {
            if (VisibleTimer < Time.time)
            {
                VisibleTimer = Time.time + 0.25f;
                if (___botOwner_0.Memory.HaveEnemy && !___botOwner_0.Memory.GoalEnemy.CanShoot && ___botOwner_0.Memory.GoalEnemy.PersonalLastSeenTime > Time.time + 5f)
                {
                    ___botOwner_0.SetTargetMoveSpeed(0.1f);
                    ___botOwner_0.Mover.SetTargetMoveSpeed(0.1f);
                    ___botOwner_0.SetPose(0.1f);
                    ___botOwner_0.GetPlayer.ChangePose(0.1f);
                    ___botOwner_0.GetPlayer.MovementContext.SetAimingSlowdown(false, 1f);
                    //Logger.LogInfo($"Movement: [{___botOwner_0.name}] Enemy Visible but not Shootable, Rat mode engaged");
                    return;
                }
                //Move speed is set to 0.6 by default, this changes that to 1.0 (max walk speed)
                if (___botOwner_0.AimingData.LastDist2Target < 20f || !___botOwner_0.WeaponManager.HaveBullets)
                {
                    ___botOwner_0.SetTargetMoveSpeed(1.0f);
                    ___botOwner_0.Mover.SetTargetMoveSpeed(1.0f);
                    ___botOwner_0.SetPose(1.0f);
                    ___botOwner_0.GetPlayer.ChangePose(1.0f);
                    ___botOwner_0.GetPlayer.MovementContext.SetAimingSlowdown(false, 1f);
                    //Logger.LogInfo($"Movement: [{___botOwner_0.name}] Enemy Close!");
                    return;
                }
                if (___botOwner_0.AimingData.LastDist2Target > 30f && ___botOwner_0.Memory.GoalEnemy.CanShoot)
                {
                    ___botOwner_0.SetTargetMoveSpeed(0.6f);
                    ___botOwner_0.Mover.SetTargetMoveSpeed(0.6f);
                    ___botOwner_0.SetPose(0.6f);
                    ___botOwner_0.GetPlayer.ChangePose(0.6f);
                    ___botOwner_0.GetPlayer.MovementContext.SetAimingSlowdown(true, 0.6f);
                    //Logger.LogInfo($"Movement: [{___botOwner_0.name}] Enemy Visible but is far away, so I'm aiming");
                    return;
                }
            }
        }
    }*/