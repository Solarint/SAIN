using System;
using System.Collections.Generic;

namespace SAIN.Editor;

public static class SettingsContainers
{
    private static readonly Dictionary<Type, SettingsContainer> Containers = new();

    public static SettingsContainer GetContainer(Type containerType, string name = null)
    {
        if (!Containers.ContainsKey(containerType))
        {
            Containers.Add(containerType, new SettingsContainer(containerType, name));
        }
        return Containers[containerType];
    }
}