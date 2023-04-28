using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using static SAIN_Grenades.Configs.GrenadeYell;

namespace SAIN_Grenades.Patches
{
    public class PlayerSayPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "Say");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref EPhraseTrigger @event)
        {
            bool allowVoiceChance = UnityEngine.Random.value < (GrenadeYellChance.Value / 100f);

            bool badVoice =
                @event == EPhraseTrigger.OnEnemyGrenade ||
                @event == EPhraseTrigger.OnGrenade;

            if (badVoice == true && allowVoiceChance == false)
                @event = EPhraseTrigger.PhraseNone;
        }
    }
}