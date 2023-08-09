using EFT;
using EFT.UI;
using SAIN.Editor.Abstract;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings.Categories;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Editor.Util
{
    public class ModifyLists : EditorAbstract
    {
        public ModifyLists(SAINEditor editor) : base(editor)
        {
        }

        public List<BigBrainConfigClass> AddOrRemove(List<BigBrainConfigClass> list, int optionsPerLine = 4)
        {
            if (list != null)
            {
                int i = StartListEdit(optionsPerLine, out var options);
                foreach (BigBrainConfigClass brain in BigBrainSettings.DefaultBrains)
                {
                    list = AddOrRemove(brain, list, null, null, options);
                    i = ListSpacing(i, optionsPerLine);
                }
                EndListEdit();
            }
            return list;
        }

        public List<WildSpawnType> AddOrRemove(List<WildSpawnType> list, int optionsPerLine = 4)
        {
            if (list != null)
            {
                int i = StartListEdit(optionsPerLine, out var options);
                foreach (var botType in BotTypeDefinitions.BotTypes.Values)
                {
                    list = AddOrRemove(botType.WildSpawnType, list, botType.Name, botType.Description, options);
                    i = ListSpacing(i, optionsPerLine);
                }
                EndListEdit();
            }
            return list;
        }

        public List<BotDifficulty> AddOrRemove(List<BotDifficulty> list, int optionsPerLine = 4)
        {
            if (list != null)
            {
                int i = StartListEdit(optionsPerLine, out var options);
                foreach (var dificulty in EnumValues.Difficulties)
                {
                    list = AddOrRemove(dificulty, list, null, null, options);
                    i = ListSpacing(i, optionsPerLine);
                }
                EndListEdit();
            }
            return list;
        }

        public List<BotType> AddOrRemove(List<BotType> list, int optionsPerLine = 4)
        {
            if (list != null)
            {
                int i = StartListEdit(optionsPerLine, out var options);
                foreach (var botType in BotTypeDefinitions.BotTypes.Values)
                {
                    list = AddOrRemove(botType, list, botType.Name, botType.Description, options);
                    i = ListSpacing(i, optionsPerLine);
                }
                EndListEdit();
            }
            return list;
        }

        private List<T> AddOrRemove<T>(T value, List<T> list, string name = null, string description = null, params GUILayoutOption[] options)
        {
            if (list != null)
            {
                bool inList = list.Contains(value);
                if (Builder.Toggle(inList, name ?? value.ToString(), description, EUISoundType.MenuCheckBox, options))
                {
                    if (inList)
                    {
                        list.Remove(value);
                    }
                    else
                    {
                        list.Add(value);
                    }
                }
            }
            return list;
        }

        private int StartListEdit(int optionsPerLine, out GUILayoutOption[] dimensions)
        {
            BeginVertical();
            BeginHorizontal();
            FlexibleSpace();

            float width = Mathf.Round(optionsPerLine / 1800f);
            float height = 22.5f;
            dimensions = new GUILayoutOption[]
            {
                Height(height), Width(width),
            };
            return 0;
        }

        private int ListSpacing(int i, int max)
        {
            i++;
            if (i >= max)
            {
                i = 0;

                FlexibleSpace();
                EndHorizontal();
                BeginHorizontal();
                FlexibleSpace();
            }
            return i;
        }

        private void EndListEdit()
        {
            FlexibleSpace();
            EndHorizontal();
            EndVertical();
        }
    }
}