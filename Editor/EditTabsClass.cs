using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SAIN.Editor.RectLayout;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SAIN.Editor
{
    internal class EditTabsClass
    {
        static EditTabsClass()
        {
            TabClasses = new Dictionary<EditorTabs, EditorTabClass> 
            {
                {
                    EditorTabs.None, new EditorTabClass
                    {
                        Name = "None",
                        ToolTip = "",
                        Enum = EditorTabs.None,
                    } 
                },
                {
                    EditorTabs.Home, new EditorTabClass
                    {
                        Name = "Home",
                        ToolTip = "Select Presets and check which mods are detected that can affect SAIN.",
                        Enum = EditorTabs.Home,
                    }
                }, 
                {
                    EditorTabs.GlobalSettings, new EditorTabClass
                    {
                        Name = "Global Settings",
                        ToolTip = "Modify settings that are the same between all bots.",
                        Enum = EditorTabs.GlobalSettings,
                    }
                },
                {
                    EditorTabs.BotSettings, new EditorTabClass
                    {
                        Name = "Bot Settings",
                        ToolTip = "Modify Settings that are unique to particular bot types for individual difficulties. Difficulty is determined on spawn by EFT, and is changed by selecting the Difficulty value when starting a raid. As Online is a mix of all difficulties.",
                        Enum = EditorTabs.BotSettings,
                    }
                },
                { 
                    EditorTabs.Personalities, new EditorTabClass
                    {
                        Name = "Personalities",
                        ToolTip = "Modify Individual Personality settings for how they are assigned to bots, and what each personality does for a bot's behavior.",
                        Enum = EditorTabs.Personalities,
                    }
                },
                { 
                    EditorTabs.Advanced, new EditorTabClass
                    {
                        Name = "Advanced Options",
                        ToolTip = "Edit at your own risk. Enable additional advanced config options here",
                        Enum = EditorTabs.Advanced,
                    }
                },
            };

            List<string> tabs = new List<string>();
            foreach (var tab in TabClasses.Values)
            {
                tabs.Add(tab.Name);
            }
            Tabs = tabs.ToArray();
        }

        private const float TabMenuHeight = 50f;
        private const float TabMenuVerticalMargin = 5f;

        private static void CreateNewTabMenuRects()
        {
            TabMenuRect = new Rect(0, ExitRect.height + TabMenuVerticalMargin, MainWindow.width, TabMenuHeight);
            TabRects = Builder.HorizontalGridRects(TabMenuRect, Tabs.Length, 15f);
        }

        private static BuilderClass Builder => SAINPlugin.Editor.Builder;

        public static void TabSelectMenu(float minHeight = 15, float speed = 3, float closeSpeedMulti = 0.66f)
        {
            if (TabMenuRect == null || TabRects == null)
            {
                CreateNewTabMenuRects();
            }

            string openTabString = Builder.SelectionGridExpandHeight(TabMenuRect, Tabs, SelectedTab.ToString(), TabRects, minHeight, speed, closeSpeedMulti);
            SelectedTab = (EditorTabs)Enum.Parse(typeof(EditorTabs), openTabString);
        }

        private static Rect[] TabRects;
        private static Rect TabMenuRect;
        private static Rect OpenTabRect;

        public static void BeginScrollView()
        {
            TabClasses[SelectedTab].Scroll = GUILayout.BeginScrollView(TabClasses[SelectedTab].Scroll);
        }

        public static void EndScrollView()
        {
            GUILayout.EndScrollView();
        }

        public static bool IsTabSelected(EditorTabs tab, string selected)
        {
            return TabClasses[tab].Name == selected;
        }

        public static EditorTabs SelectedTab = EditorTabs.None;
        public static readonly string[] Tabs;
        public static readonly Dictionary<EditorTabs, EditorTabClass> TabClasses;
    }

    public sealed class EditorTabClass
    {
        public string Name;
        public string ToolTip;
        public Vector2 Scroll = Vector2.zero;
        public EditorTabs Enum = EditorTabs.None;
    }

    public enum EditorTabs
    {
        None,
        Home,
        GlobalSettings,
        BotSettings,
        Personalities,
        Advanced
    }
}
