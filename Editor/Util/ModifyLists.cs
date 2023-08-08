using EFT.UI;
using EFT;
using SAIN.Editor.Abstract;
using SAIN.Preset.GlobalSettings.Categories;
using SAIN.Preset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAIN.Helpers;
using System.Reflection;

namespace SAIN.Editor.Util
{
    public class ModifyLists : EditorAbstract
    {
        public ModifyLists(SAINEditor editor) : base(editor) { }

        public void AddOrRemove(List<WildSpawnType> list, int optionsPerLine = 4)
        {
            int i = StartListEdit();
            foreach (var botType in BotTypeDefinitions.BotTypes.Values)
            {
                AddOrRemove(botType.WildSpawnType, list, botType.Name, botType.Description);
                i = ListSpacing(i, optionsPerLine);
            }
            EndListEdit();
        }

        public void AddOrRemove(List<BotDifficulty> list, int optionsPerLine = 4)
        {
            int i = StartListEdit();
            foreach (var dificulty in EnumValues.Difficulties)
            {
                AddOrRemove(dificulty, list);
                i = ListSpacing(i, optionsPerLine);
            }
            EndListEdit();
        }

        public void AddOrRemove(List<BotType> list, int optionsPerLine = 4)
        {
            int i = StartListEdit();
            foreach (var botType in BotTypeDefinitions.BotTypes.Values)
            {
                AddOrRemove(botType, list, botType.Name, botType.Description);
                i = ListSpacing(i, optionsPerLine);
            }
            EndListEdit();
        }

        public void AddOrRemove(List<string> list, int optionsPerLine = 4)
        {
            int i = StartListEdit();
            foreach (var brain in BigBrainSettings.AllBotBrains)
            {
                AddOrRemove(brain, list);
                i = ListSpacing(i, optionsPerLine);
            }
            EndListEdit();
        }

        private void AddOrRemove<T>(T value, List<T> list, string name = null, string description = null)
        {
            bool inList = list.Contains(value);
            if (Builder.Toggle(inList, name ?? value.ToString(), description, EUISoundType.MenuCheckBox))
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

        private int StartListEdit()
        {
            BeginVertical();
            BeginHorizontal();
            return 0;
        }

        private int ListSpacing(int i, int max)
        {
            i++;
            if (i >= max)
            {
                i = 0;
                EndHorizontal();
                BeginHorizontal();
            }
            return i;
        }

        private void EndListEdit()
        {
            EndHorizontal();
            EndVertical();
        }
    }
}
