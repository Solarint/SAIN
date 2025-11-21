using EFT;
using UnityEngine;

namespace SAIN.Components.Helpers;

public class SAINSoundTypeHandler
{
    public static void AISoundFileChecker(string sound, Player player)
    {
        if (player == null || player.HealthController?.IsAlive == false)
        {
            return;
        }

        SAINSoundType soundType = SAINSoundType.None;
        var Item = player.HandsController.Item;
        float soundDist = 20f;

        if (Item != null)
        {
            if (Item is ThrowWeapItemClass)
            {
                if (sound == "Pin")
                {
                    soundType = SAINSoundType.GrenadePin;
                    soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_GrenadePinDraw;
                }
                if (sound == "Draw")
                {
                    soundType = SAINSoundType.GrenadeDraw;
                    soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_GrenadePinDraw;
                }
            }
            else if (Item is MedsItemClass)
            {
                soundType = SAINSoundType.Heal;
                soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Healing;
                if (sound == "CapRemove" || sound == "Inject")
                {
                    soundDist *= 0.5f;
                }
            }
            else
            {
                soundType = SAINSoundType.Reload;
                soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Reload;
            }
        }

        BotManagerComponent.Instance?.BotHearing.PlayAISound(player.ProfileId, soundType, player.Position + Vector3.up, soundDist, 1f);
    }
}