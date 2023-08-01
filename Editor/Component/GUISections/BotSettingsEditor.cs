using EFT;
using SAIN.BotPresets;
using SAIN.Classes;
using SAIN.BotSettings;
using SAIN.Editor.Abstract;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SAIN.BotSettings.Categories;
using SAIN.SAINPreset.Attributes;

namespace SAIN.Editor.GUISections
{
    public class BotSettingsEditor : EditorAbstract
    {
        public BotSettingsEditor(SAINEditor editor) : base(editor)
        {
        }

        public void EditMenu()
        {
            SAINSettings testSettings = new SAINSettings();
            FieldInfo[] Fields = SAINAimingSettings.Fields;
            BeginVertical();
            foreach (FieldInfo field in Fields)
            {
                object value = field.GetValue(testSettings.Aiming);
                if (value != null)
                {
                    GetAttributeValue.GUI gui = new GetAttributeValue.GUI(field);
                    value = gui.EditValue(value);
                    field.SetValue(testSettings.Aiming, value);
                }
            }
            EndVertical();
        }
    }
}
