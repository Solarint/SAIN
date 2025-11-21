using SAIN.Plugin;
using SAIN.Preset;
using System;

namespace SAIN.SAINComponent;

// this purely exists to avoid rewriting code 100 times
public class PresetAutoUpdater
{
    public void Subscribe(Action<SAINPresetClass> func)
    {
        if (func != null)
        {
            Subscribed = true;
            _func = func;
            PresetHandler.OnPresetUpdated += func;
        }
    }

    public void UnSubscribe()
    {
        if (Subscribed && _func != null)
        {
            Subscribed = false;
            PresetHandler.OnPresetUpdated -= _func;
        }
    }

    public bool Subscribed { get; private set; }

    private Action<SAINPresetClass> _func;
}