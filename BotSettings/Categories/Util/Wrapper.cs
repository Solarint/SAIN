using SAIN.Editor;
using System;
using UnityEngine;

namespace SAIN.BotSettings.Categories.Util
{
    public class SettingsWrapper
    {
        public bool Hidden = false;
        public Type Type;
        public string Key;
        public string DisplayName;
        public string Description;
        public object Value;
        public object EFTDefault;
        public object SAINDefault;
        public object Min;
        public object Max;
        public float? Rounding;

        public void GUIModify()
        {
            ModifySettings.GUIModify(this);
        }
    }
}
