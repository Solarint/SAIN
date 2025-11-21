using SAIN.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Editor;

public sealed class SettingsContainer
{
    public SettingsContainer(Type settingsType, string name = null)
    {
        Name = name ?? settingsType.Name;
        foreach (FieldInfo field in settingsType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            ConfigInfoClass attributes = new(field);
            if (!attributes.Hidden)
            {
                var category = new Category(attributes);
                category.OptionCount(out int realCount);
                if (realCount > 0)
                {
                    Categories.Add(category);
                }
            }
        }
    }

    public readonly string Name;

    public readonly List<Category> Categories = new();
    public readonly List<Category> SelectedCategories = new();

    public string SearchPattern = string.Empty;

    public bool Open = false;
    public bool SecondOpen = false;
    public Vector2 Scroll = Vector2.zero;
    public Vector2 SecondScroll = Vector2.zero;
}