using Comfort.Common;
using EFT.UI;
using HarmonyLib;
using UnityEngine;

namespace SAIN.Editor;

internal class Sounds
{
    private static GUISounds GUISounds => Singleton<GUISounds>.Instance;
    private static UISoundsWrapper _soundsWrapper;
    private static AudioSource _audioSource;

    private static void getWrapper()
    {
        _soundsWrapper = AccessTools.Field(typeof(GUISounds), "uisoundsWrapper_0").GetValue(GUISounds) as UISoundsWrapper;
        _audioSource = AccessTools.Field(typeof(GUISounds), "audioSource_0").GetValue(GUISounds) as AudioSource;
    }

    public static void PlaySound(EUISoundType soundType, float volume = 1f)
    {
        volume = Mathf.Clamp(volume, 0f, 1f);
        if (_soundsWrapper == null)
        {
            getWrapper();
        }
        if (SoundLimiter < Time.time)
        {
            SoundLimiter = Time.time + 0.05f;
            playSound(soundType, volume);
        }
    }

    private static void playSound(EUISoundType soundType, float volume)
    {
        if (_soundsWrapper == null || _audioSource == null)
        {
#if DEBUG
            Logger.LogWarning($"null");
#endif
            Singleton<GUISounds>.Instance.PlayUISound(soundType);
        }
        else
        {
            var clip = _soundsWrapper.GetUIClip(soundType);
            if (clip == null)
            {
                return;
            }
            _audioSource.PlayOneShot(clip, volume);
        }
#if DEBUG
        if (SAINPlugin.DebugMode)
        {
            Logger.LogDebug(soundType);
        }
#endif
    }

    private static float SoundLimiter;
}