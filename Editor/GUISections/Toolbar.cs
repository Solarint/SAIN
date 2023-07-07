using static SAIN.Editor.Buttons;
using static SAIN.Editor.RectLayout;
using static SAIN.Editor.Styles;
using UnityEngine;
using System.Windows.Forms.VisualStyles;

namespace SAIN.Editor.GUISections
{
    internal class Toolbar
    {
        public static void CreateTabSelection()
        {
            int size = GUI.skin.button.fontSize;
            GUI.skin.button.fontSize = size + 15;
            GUI.skin.button.fontStyle = FontStyle.Bold;
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
            SelectedTab = GUILayout.SelectionGrid(SelectedTab, Tabs, Tabs.Length, GUILayout.Height(40));
            GUI.skin.button.fontSize = size;
        }

        public static bool IsTabSelected(string tab)
        {
            return Tabs[SelectedTab] == tab;
        }

        public static readonly string Home = nameof(Home);
        public static readonly string Hearing = nameof(Hearing);
        public static readonly string Personalities = nameof(Personalities);
        public static readonly string Advanced = nameof(Advanced);
        public static readonly string[] Tabs = { Home, Hearing, Personalities, Advanced };
    }
}
