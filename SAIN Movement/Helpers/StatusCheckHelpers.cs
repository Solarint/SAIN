using EFT;

namespace SAIN.Helpers
{
    public class CheckSelf
    {
        public static void Hurt(BotOwner bot)
        {

        }
        public static void Ammo(BotOwner bot)
        {

        }
    }
    public class CheckEnemy
    {
        //static float hitCount = 0.0f;
        //static bool isEnemyHurt = false;
        //static string currentEnemyID;
        public static void EnemyHurt(BotOwner bot)
        {
            //bool lowAmmo = MagCurrent / MagMax < 0.2f;
            //bool isHurt = ___botOwner_0.GetPlayer.HealthStatus == ETagStatus.BadlyInjured;
        }
        public static void EnemyPos(BotOwner bot)
        {

        }
    }
}

//COPYPASTE
/*
//CheckForCalcPath for new contact
if (___botOwner_0.Memory.GoalEnemy.ProfileId != currentEnemyID)
{
    isEnemyHurt = false;
    hitCount = 0.0f;
    if (Core.DebugDogFighter.Value == true)
    {
        Logger.LogInfo($"Dogfighter {___botOwner_0.name}: Contact! New Enemy: {___botOwner_0.Memory.GoalEnemy.Nickname} Old Enemy: {___botOwner_0.Memory.Enemy_Last_Id.Nickname}");
    }
    currentEnemyID = ___botOwner_0.Memory.GoalEnemy.ProfileId;
}
//CheckForCalcPath if still falling back PLACEHOLDER
if (fallbackTime > Time.time + 3.0f)
{
    isFallingBack = false;
}
//CheckForCalcPath if the enemy has been hit a few times
if (___botOwner_0.Memory.GoalEnemy.LastHitTime < Time.time + 3.0f)
{
    hitCount += 1.0f;
    if ((hitCount >= 3.0f) && !isEnemyHurt)
    {
        isEnemyHurt = true;
        if (Core.DebugDogFighter.Value == true)
        {
            Logger.LogInfo($"Dogfighter {___botOwner_0.name}: Enemy is Hurtin! Hitcount: {hitCount}");
        }
    }
}*/