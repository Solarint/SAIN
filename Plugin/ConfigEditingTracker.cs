using SAIN.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SAIN.Plugin;

internal static class ConfigEditingTracker
{
    public static void Update()
    {
        SettingChangedThisFrame = false;
    }

    public static bool SettingChangedThisFrame { get; private set; }

    public static void Add(string name, object value)
    {
        AddConfigValue(name, value);
        ClearAndCreateStringBuilder();
        SettingChangedThisFrame = true;
    }

    private static void AddConfigValue(string name, object value)
    {
        if (EditedConfigValues.ContainsKey(name))
        {
            EditedConfigValues[name] = value;
            return;
        }
        EditedConfigValues.Add(name, value);
    }

    public static void Remove(ConfigInfoClass info)
    {
        EditedConfigValues.Remove(info.Name);
        ClearAndCreateStringBuilder();
    }

    public static bool WasEdited(ConfigInfoClass info)
    {
        return EditedConfigValues.ContainsKey(info.Name);
    }

    public static void Clear()
    {
        EditedConfigValues.Clear();
        _stringBuilder.Clear();
        _unsavedValues = string.Empty;
    }

    public static string GetUnsavedValuesString()
    {
        return _unsavedValues;
    }

    private static readonly Type _float = typeof(float);
    private static readonly Type _bool = typeof(bool);

    private static void AddToStringBuilder(string name, object value)
    {
        string _string;
        Type type = value.GetType();
        if (type == _float || type == _bool)
        {
            _string = $"{name}: {value}";
        }
        else
        {
            _string = $"{name}";
        }
        _stringBuilder.AppendLine(_string);
        _unsavedValues = _stringBuilder.ToString();
    }

    private static void ClearAndCreateStringBuilder()
    {
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"Unsaved Config Options: Count: [{EditedConfigValues.Count}]");
        foreach (var item in EditedConfigValues)
        {
            AddToStringBuilder(item.Key, item.Value);
        }
        _unsavedValues = _stringBuilder.ToString();
    }

    private static string _unsavedValues = string.Empty;
    private static readonly StringBuilder _stringBuilder = new();

    public static bool UnsavedChanges => EditedConfigValues.Count > 0;

    public static readonly Dictionary<string, object> EditedConfigValues = new();
}