using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Movement.Helpers;
using System.Reflection;
using UnityEngine;
using static SAIN.Movement.Config.DebugConfig;
using static SAIN.Movement.Config.DogFighterConfig;

namespace SAIN.Movement.Patches
{
    public class DodgePatch : ModulePatch
    {
        private static MethodInfo _DogFightStateSetter;
        protected override MethodBase GetTargetMethod()
        {
            _DogFightStateSetter = AccessTools.PropertySetter(typeof(GClass328), "DogFightState");
            return AccessTools.Method(typeof(GClass328), "Fight");
        }
        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float ___float_1)
        {
            BotOwner bot = ___botOwner_0;
            SainMemory sain = bot.gameObject.GetComponent<SainMemory>();

            if (!DodgeToggle.Value || bot.Memory.GoalEnemy == null)
            {
                return true;
            }

            //Check if Scav Can Dodge value is set to true, need to expand and do this better
            if (!ScavDodgeToggle.Value && bot.IsRole(WildSpawnType.assault))
            {
                return true;
            }

            if (sain.DodgeTimer < Time.time)
            {
                sain.DodgeTimer = Time.time + UnityEngine.Random.Range(0.5f, 1f);

                if (bot.Memory.GoalEnemy.CanShoot)
                {
                    if (Dodge.ExecuteDodge(bot))
                    {
                        if (DebugDodge.Value) Logger.LogInfo($"[{bot.name}] Dodged and is shooting");

                        _DogFightStateSetter.Invoke(bot, new object[] { BotDogFightStatus.shootFromPlace });

                        return false;
                    }
                }
            }
            // Faster Reaction in default dog fight mode // Default is 1 second
            if (___float_1 > 0.25f) ___float_1 = 0.25f;
            return true;
        }
    }
}
