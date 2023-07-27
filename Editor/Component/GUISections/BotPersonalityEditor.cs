using SAIN.Classes;
using SAIN.Editor.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Editor.GUISections
{
    public class BotPersonalityEditor : EditorAbstract
    {
        public BotPersonalityEditor(SAINEditor editor) : base(editor)
        {
        }

        public void PersonalityMenu()
        {
            Box("Personality Settings");
            Box("For The Memes. Recommended not to use these during normal gameplay!");
            Box("Bots will be more predictable and exploitable.");

            if (Builder.CreateButtonOption(AllGigaChads))
            {
                AllChads.Value = false;
                AllRats.Value = false;
            }
            if (Builder.CreateButtonOption(AllChads))
            {
                AllGigaChads.Value = false;
                AllRats.Value = false;
            }
            if (Builder.CreateButtonOption(AllRats))
            {
                AllGigaChads.Value = false;
                AllChads.Value = false;
            }

            Builder.Space(25f);
            PersonScroll = Builder.BeginScrollView(PersonScroll);

            foreach (var personality in PersonalityManager.Personalities.Values)
            {
                string name = personality.Name;
                if (!OpenPersMenus.ContainsKey(name))
                {
                    OpenPersMenus.Add(name, false);
                }
                OpenPersMenus[name] = Builder.ExpandableMenu(name, OpenPersMenus[name], personality.Description);
                if (!OpenPersMenus[name])
                {
                    continue;
                }

                personality.CanJumpCorners = Toggle(personality.CanJumpCorners, nameof(personality.CanJumpCorners));
                personality.CanTaunt = Toggle(personality.CanTaunt, nameof(personality.CanTaunt));

                Builder.BeginHorizontal();
                Builder.Label(nameof(personality.TauntFrequency), GUILayout.Width(250f));
                personality.TauntFrequency = Builder.CreateSlider(personality.TauntFrequency, 0f, 60f, 1f, 35f);
                Builder.Box(personality.TauntFrequency.ToString(), GUILayout.Width(100f));
                Builder.EndHorizontal();

                Builder.BeginHorizontal();
                Builder.Label(nameof(personality.TauntMaxDistance), GUILayout.Width(250f));
                personality.TauntMaxDistance = Builder.CreateSlider(personality.TauntMaxDistance, 5f, 100f, 1f, 35f);
                Builder.Box(personality.TauntMaxDistance.ToString(), GUILayout.Width(100f));
                Builder.EndHorizontal();

                Builder.BeginHorizontal();
                Builder.Label(nameof(personality.HoldGroundBaseTime), GUILayout.Width(250f));
                personality.HoldGroundBaseTime = Builder.CreateSlider(personality.HoldGroundBaseTime, 0f, 3f, 100f, 35f);
                Builder.Box(personality.HoldGroundBaseTime.ToString(), GUILayout.Width(100f));
                Builder.EndHorizontal();

                Builder.BeginHorizontal();
                Builder.Label(nameof(personality.SearchBaseTime), GUILayout.Width(250f));
                personality.SearchBaseTime = Builder.CreateSlider(personality.SearchBaseTime, 0f, 1000f, 10f, 35f);
                Builder.Box(personality.SearchBaseTime.ToString(), GUILayout.Width(100f));
                Builder.EndHorizontal();

                Builder.BeginHorizontal();
                Builder.Label(nameof(personality.PowerLevelMin), GUILayout.Width(250f));
                personality.PowerLevelMin = Builder.CreateSlider(personality.PowerLevelMin, 1f, 500f, 1f, 35f);
                Builder.Box(personality.PowerLevelMin.ToString(), GUILayout.Width(100f));
                Builder.EndHorizontal();

                Builder.BeginHorizontal();
                Builder.Label(nameof(personality.PowerLevelMax), GUILayout.Width(250f));
                personality.PowerLevelMax = Builder.CreateSlider(personality.PowerLevelMax, 1f, 500f, 1f, 35f);
                Builder.Box(personality.PowerLevelMax.ToString(), GUILayout.Width(100f));
                Builder.EndHorizontal();

                Builder.BeginHorizontal();
                Builder.Label(nameof(personality.TrueRandomChance), GUILayout.Width(250f));
                personality.TrueRandomChance = Builder.CreateSlider(personality.TrueRandomChance, 0f, 100f, 1f, 35f);
                Builder.Box(personality.TrueRandomChance.ToString(), GUILayout.Width(100f));
                Builder.EndHorizontal();

                Builder.BeginHorizontal();
                Builder.Label(nameof(personality.RandomChanceIfMeetRequirements), GUILayout.Width(250f));
                personality.RandomChanceIfMeetRequirements = Builder.CreateSlider(personality.RandomChanceIfMeetRequirements, 0f, 100f, 1f, 35f);
                Builder.Box(personality.RandomChanceIfMeetRequirements.ToString(), GUILayout.Width(100f));
                Builder.EndHorizontal();
            }
            Builder.EndScrollView();
        }

        Dictionary<string, bool> OpenPersMenus = new Dictionary<string, bool>();
        Vector2 PersonScroll = Vector2.zero;

    }
}
