using SAIN.Editor.Abstract;
using System;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Helpers.Reflection;
using static SAIN.SAINPreset.Attributes.GetAttributeValue.AttributesGUI;

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

            var pers = SAINPlugin.LoadedPreset.GlobalSettings.Personality;
            Type type = pers.GetType();

            pers.AllGigaChads = EditValue(pers.AllGigaChads, GetField(type, nameof(pers.AllGigaChads)));
            if (pers.AllGigaChads)
            {
                pers.AllChads = false;
                pers.AllRats = false;
            }
            pers.AllChads = EditValue(pers.AllChads, GetField(type, nameof(pers.AllChads)));
            if (pers.AllChads)
            {
                pers.AllGigaChads = false;
                pers.AllRats = false;
            }
            pers.AllRats = EditValue(pers.AllRats, GetField(type, nameof(pers.AllRats)));
            if (pers.AllRats)
            {
                pers.AllGigaChads = false;
                pers.AllChads = false;
            }

            Builder.Space(25f);
            PersonScroll = Builder.BeginScrollView(PersonScroll);

            foreach (var personality in SAINPlugin.LoadedPreset.PersonalityManager.Personalities.Values)
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

                personality.CanJumpCorners = EditValue(personality.CanJumpCorners, GetField(type, nameof(personality.CanJumpCorners)));
                personality.CanTaunt = EditValue(personality.CanTaunt, GetField(type, nameof(personality.CanTaunt)));
                personality.TauntFrequency = EditValue(personality.TauntFrequency, GetField(type, nameof(personality.TauntFrequency)));
                personality.TauntMaxDistance = EditValue(personality.TauntMaxDistance, GetField(type, nameof(personality.TauntMaxDistance)));
                personality.HoldGroundBaseTime = EditValue(personality.HoldGroundBaseTime, GetField(type, nameof(personality.HoldGroundBaseTime)));
                personality.SearchBaseTime = EditValue(personality.SearchBaseTime, GetField(type, nameof(personality.SearchBaseTime)));
                personality.PowerLevelMin = EditValue(personality.PowerLevelMin, GetField(type, nameof(personality.PowerLevelMin)));
                personality.PowerLevelMax = EditValue(personality.PowerLevelMax, GetField(type, nameof(personality.PowerLevelMax)));
                personality.TrueRandomChance = EditValue(personality.TrueRandomChance, GetField(type, nameof(personality.TrueRandomChance)));
                personality.RandomChanceIfMeetRequirements = EditValue(personality.RandomChanceIfMeetRequirements, GetField(type, nameof(personality.RandomChanceIfMeetRequirements)));
            }
            Builder.EndScrollView();
        }

        private Dictionary<string, bool> OpenPersMenus = new Dictionary<string, bool>();
        private Vector2 PersonScroll = Vector2.zero;
    }
}